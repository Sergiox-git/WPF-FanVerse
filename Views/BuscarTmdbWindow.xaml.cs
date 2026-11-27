using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using FanVerse.Models;
using FanVerse.Services;

namespace FanVerse.Views;

public partial class BuscarTmdbWindow : Window
{
    private static readonly CompareInfo ComparadorTitulos =
        CultureInfo.GetCultureInfo("es-ES").CompareInfo;

    private readonly CatalogoService _catalogo;
    private readonly TmdbService _tmdb = new();
    private readonly string _idToken;
    private readonly bool _soloCatalogo;
    private readonly HashSet<string> _importados = new(StringComparer.Ordinal);
    private Dictionary<string, Contenido>? _contenidos;
    private bool _importando;

    public string? ContenidoImportadoId { get; private set; }

    public BuscarTmdbWindow(CatalogoService catalogo, string idToken, bool soloCatalogo)
    {
        _catalogo = catalogo;
        _idToken = idToken;
        _soloCatalogo = soloCatalogo;
        InitializeComponent();

        Title = soloCatalogo ? "Buscar en mi catálogo" : "Buscar en TMDB";
        TituloVentanaTextBlock.Text = Title;
        AyudaTextBlock.Text = soloCatalogo
            ? "Películas y series guardadas en FanVerse."
            : "Explora TMDB y añade títulos a FanVerse.";

        if (soloCatalogo)
        {
            Loaded += async (_, _) => await BuscarAsync();
        }
        else
        {
            TipoComboBox.Items.Remove(TodosItem);
        }

        TipoComboBox.SelectedIndex = 0;
        ActualizarAccion();
    }

    private async void Buscar_Click(object sender, RoutedEventArgs e)
    {
        await BuscarAsync();
    }

    private async Task BuscarAsync()
    {
        if (!BuscarButton.IsEnabled || _importando)
        {
            return;
        }

        string consulta = ConsultaTextBox.Text.Trim();

        if (!_soloCatalogo && consulta.Length == 0)
        {
            EstadoTextBlock.Text = "Escribe un título para buscar en TMDB.";
            ConsultaTextBox.Focus();
            return;
        }

        if (TipoComboBox.SelectedValue is not string tipo)
        {
            return;
        }

        BuscarButton.IsEnabled = false;
        ResultadosListBox.ItemsSource = null;
        EstadoTextBlock.Text = _soloCatalogo ? "Consultando FanVerse…" : "Buscando en TMDB…";

        bool leyendoCatalogo = true;

        try
        {
            _contenidos ??= await _catalogo.ObtenerAsync(_idToken);
            leyendoCatalogo = false;

            List<FichaBusqueda> fichas;

            if (_soloCatalogo)
            {
                fichas = _contenidos.Values
                    .Where(contenido =>
                        (tipo == "todos" || contenido.Tipo == tipo) &&
                        (consulta.Length == 0 || ComparadorTitulos.IndexOf(
                            contenido.Titulo ?? "", consulta,
                            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0))
                    .OrderBy(contenido => contenido.Titulo, StringComparer.CurrentCultureIgnoreCase)
                    .Select(contenido => new FichaBusqueda(
                        contenido.Titulo, contenido.Tipo, contenido.Sinopsis, contenido.FechaEstreno,
                        contenido.PosterUrl, contenido.Origen, null, null))
                    .ToList();
            }
            else
            {
                var resultados = await _tmdb.BuscarAsync(consulta, tipo);
                fichas = resultados.Select(resultado => new FichaBusqueda(
                    resultado.Titulo, resultado.Tipo, resultado.Sinopsis, resultado.FechaEstreno,
                    resultado.PosterUrl, "tmdb", resultado.Valoracion, resultado)).ToList();
            }

            ResultadosListBox.ItemsSource = fichas;
            if (fichas.Count > 0)
            {
                ResultadosListBox.SelectedIndex = 0;
            }

            EstadoTextBlock.Text = fichas.Count == 0
                ? "No se encontraron títulos."
                : fichas.Count == 1 ? "1 título encontrado." : $"{fichas.Count} títulos encontrados.";
        }
        catch (InvalidOperationException ex)
        {
            EstadoTextBlock.Text = ex.Message;
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            EstadoTextBlock.Text = leyendoCatalogo
                ? "Firebase rechazó la lectura del catálogo. Comprueba la sesión."
                : "TMDB rechazó el acceso. Revisa TMDB_READ_TOKEN.";
        }
        catch (Exception ex) when (
            ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            EstadoTextBlock.Text = leyendoCatalogo
                ? "No se pudo consultar el catálogo. Pulsa Buscar para reintentar."
                : "No se pudo completar la búsqueda en TMDB. Vuelve a intentarlo.";
        }
        finally
        {
            BuscarButton.IsEnabled = true;
            ActualizarAccion();
        }
    }

    private void ResultadosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ActualizarAccion();
    }

    private bool EstaEnCatalogo(FichaBusqueda ficha)
    {
        if (ficha.ResultadoTmdb is not ResultadoTmdb resultado)
        {
            return true;
        }

        string id = CatalogoService.ObtenerIdentidadTmdb(resultado).ContenidoId;
        return _importados.Contains(id) || _contenidos?.ContainsKey(id) == true;
    }

    private void ActualizarAccion()
    {
        if (ImportarButton is null)
        {
            return;
        }

        var ficha = ResultadosListBox.SelectedItem as FichaBusqueda;
        bool guardado = ficha is not null && EstaEnCatalogo(ficha);

        ImportarButton.Content = _importando
            ? "Añadiendo…"
            : guardado ? "En catálogo" : "Añadir al catálogo";
        ImportarButton.IsEnabled = !_importando && ficha?.ResultadoTmdb is not null && !guardado;
        MarcaCatalogoTextBlock.Text = guardado ? "Guardado en FanVerse" : "";
    }

    private async void Importar_Click(object sender, RoutedEventArgs e)
    {
        if (_importando ||
            ResultadosListBox.SelectedItem is not FichaBusqueda ficha ||
            ficha.ResultadoTmdb is not ResultadoTmdb resultado ||
            EstaEnCatalogo(ficha))
        {
            return;
        }

        _importando = true;
        BuscarButton.IsEnabled = false;
        ResultadosListBox.IsEnabled = false;
        ActualizarAccion();

        try
        {
            var (contenidoId, creado) = await _catalogo.ImportarTmdbAsync(resultado, _idToken);
            _importados.Add(contenidoId);
            ContenidoImportadoId = contenidoId;
            EstadoTextBlock.Text = creado
                ? $"«{resultado.Titulo}» se añadió al catálogo."
                : $"«{resultado.Titulo}» ya estaba en el catálogo.";
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            EstadoTextBlock.Text = "Firebase rechazó el guardado. Comprueba el permiso de editor.";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            EstadoTextBlock.Text = "No se pudo confirmar el guardado. Puedes volver a intentarlo.";
        }
        catch (InvalidOperationException ex)
        {
            EstadoTextBlock.Text = ex.Message;
        }
        finally
        {
            _importando = false;
            BuscarButton.IsEnabled = true;
            ResultadosListBox.IsEnabled = true;
            ActualizarAccion();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (_importando)
        {
            e.Cancel = true;
        }
    }

    private sealed record FichaBusqueda(
        string Titulo,
        string Tipo,
        string? Sinopsis,
        string? FechaEstreno,
        Uri? PosterUrl,
        string Origen,
        double? Valoracion,
        ResultadoTmdb? ResultadoTmdb)
    {
        public string TipoTexto => Tipo switch
        {
            "pelicula" => "Película",
            "serie" => "Serie",
            _ => "Tipo sin especificar"
        };

        public string OrigenTexto => Origen switch
        {
            "tmdb" => "TMDB",
            "manual" => "Alta manual",
            _ => "Sin especificar"
        };

        public string FechaTexto => string.IsNullOrWhiteSpace(FechaEstreno) ? "Sin fecha" : FechaEstreno;
        public string SinopsisTexto => string.IsNullOrWhiteSpace(Sinopsis) ? "Sin sinopsis disponible." : Sinopsis;
        public string? NotaTexto => Valoracion is double nota ? $"Valoración TMDB · {nota:0.0}/10" : null;
    }
}
