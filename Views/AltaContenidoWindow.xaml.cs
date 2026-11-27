using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using FanVerse.Models;
using FanVerse.Services;
using FanVerse.Views;

namespace FanVerse.Views;

public partial class AltaContenidoWindow : Window
{
    private readonly CatalogoService _catalogo;
    private readonly string _idToken;
    private bool _guardando;

    public string? ContenidoId { get; private set; }

    public AltaContenidoWindow(CatalogoService catalogo, string idToken)
    {
        InitializeComponent();
        _catalogo = catalogo;
        _idToken = idToken;
    }

    private async void Guardar_Click(object sender, RoutedEventArgs e)
    {
        string titulo = TituloTextBox.Text.Trim();

        if (titulo.Length == 0)
        {
            EstadoTextBlock.Text = "Escribe un título.";
            TituloTextBox.Focus();
            return;
        }

        if (TipoComboBox.SelectedValue is not string tipo)
        {
            EstadoTextBlock.Text = "Selecciona película o serie.";
            TipoComboBox.Focus();
            return;
        }

        var contenido = new Contenido
        {
            Titulo = titulo,
            Tipo = tipo,
            Sinopsis = SinopsisTextBox.Text.Trim(),
            Origen = "manual"
        };

        _guardando = true;
        GuardarButton.IsEnabled = false;
        EstadoTextBlock.Text = "Guardando en Firebase...";

        try
        {
            ContenidoId = await _catalogo.CrearAsync(contenido, _idToken);
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            EstadoTextBlock.Text =
                "Firebase denegó el alta. Comprueba la sesión y el permiso de editor.";
        }
        catch (Exception ex) when (
            ex is HttpRequestException or TaskCanceledException
                or JsonException or InvalidOperationException)
        {
            EstadoTextBlock.Text =
                "No se pudo confirmar el alta. Revisa el catálogo antes de volver a guardar.";
        }
        finally
        {
            _guardando = false;
            GuardarButton.IsEnabled = true;
        }

        if (ContenidoId is not null)
        {
            DialogResult = true;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (_guardando)
        {
            e.Cancel = true;
        }
    }
}