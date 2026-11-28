
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FanVerse.Models;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using FanVerse.Services;

namespace FanVerse
{
    public partial class MainWindow : Window
    {
        private readonly SesionUsuario _sesion;

        private readonly CatalogoService _catalogo = new();
        private readonly ResenasService _resenas = new();

        private Dictionary<string, Contenido> _catalogoCache = new();
        private Dictionary<string, Resena> _misResenas = new();

        // Clave (contenidoId) de la reseña que se está editando. Null = alta nueva.
        private string? _contenidoIdSeleccionado;

        public MainWindow(SesionUsuario sesion)
        {
            _sesion = sesion;
            InitializeComponent();
            ActualizarMenu();
        }


        private void Inicio_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 0;
            ActualizarMenu();
        }

        private async void Catalogo_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 1;
            ActualizarMenu();

            BtnCatalogo.IsEnabled = false;
            EstadoCatalogoTextBlock.Text = "Cargando catálogo…";

            try
            {
                var contenidos = await _catalogo.ObtenerAsync(_sesion.IdToken);
                CatalogoListBox.ItemsSource = contenidos;
                EstadoCatalogoTextBlock.Text = contenidos.Count == 0
                    ? "El catálogo está vacío."
                    : $"Títulos encontrados: {contenidos.Count}";
            }
            catch (InvalidOperationException ex)
            {
                EstadoCatalogoTextBlock.Text = ex.Message;
            }
            catch (HttpRequestException ex) when (
                ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                EstadoCatalogoTextBlock.Text =
                    "Firebase denegó la lectura. Revisa la sesión y las reglas del catálogo.";
            }
            catch (HttpRequestException ex)
            {
                EstadoCatalogoTextBlock.Text = ex.StatusCode is { } estado
                    ? $"Firebase respondió con HTTP {(int)estado}."
                    : "No se pudo conectar con Firebase.";
            }
            catch (TaskCanceledException)
            {
                EstadoCatalogoTextBlock.Text = "Firebase tardó demasiado. Vuelve a intentarlo.";
            }
            catch (JsonException)
            {
                EstadoCatalogoTextBlock.Text = "La respuesta de Firebase no tiene el formato esperado.";
            }
            finally
            {
                BtnCatalogo.IsEnabled = true;
            }
        }

        private async void Resenas_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 2;
            ActualizarMenu();

            if (_catalogoCache.Count == 0)
            {
                await CargarContenidoParaSelectorAsync();
            }

            await CargarMisResenasAsync();
        }

        private async Task CargarContenidoParaSelectorAsync()
        {
            try
            {
                _catalogoCache = await _catalogo.ObtenerAsync(_sesion.IdToken);
                ContenidoComboBox.ItemsSource = _catalogoCache;
            }
            catch (Exception)
            {
                // La pestaña "Mi catálogo" ya informa de errores de lectura del catálogo.
                // Aquí simplemente dejamos el selector vacío si la carga falla.
            }
        }

        private async void ActualizarResenas_Click(object sender, RoutedEventArgs e)
        {
            await CargarMisResenasAsync();
        }

        private async Task CargarMisResenasAsync()
        {
            BtnActualizarResenas.IsEnabled = false;
            EstadoListaResenasTextBlock.Text = "Cargando tus reseñas…";

            try
            {
                _misResenas = await _resenas.ObtenerTodasAsync(_sesion);
                MisResenasListBox.ItemsSource = _misResenas;
                EstadoListaResenasTextBlock.Text = _misResenas.Count == 0
                    ? "Todavía no tienes reseñas."
                    : $"Reseñas guardadas: {_misResenas.Count}";
            }
            catch (InvalidOperationException ex)
            {
                EstadoListaResenasTextBlock.Text = ex.Message;
            }
            catch (HttpRequestException ex) when (
                ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                EstadoListaResenasTextBlock.Text = "Firebase denegó la lectura de tus reseñas.";
            }
            catch (HttpRequestException ex)
            {
                EstadoListaResenasTextBlock.Text = ex.StatusCode is { } estado
                    ? $"Firebase respondió con HTTP {(int)estado}."
                    : "No se pudo conectar con Firebase.";
            }
            catch (TaskCanceledException)
            {
                EstadoListaResenasTextBlock.Text = "Firebase tardó demasiado. Vuelve a intentarlo.";
            }
            finally
            {
                BtnActualizarResenas.IsEnabled = true;
            }
        }

        private void MisResenasListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MisResenasListBox.SelectedValue is not string contenidoId
                || !_misResenas.TryGetValue(contenidoId, out Resena? resena))
            {
                return;
            }

            _contenidoIdSeleccionado = contenidoId;
            ContenidoComboBox.IsEnabled = false;
            PuntuacionTextBox.Text = resena.Puntuacion.ToString();
            ComentarioTextBox.Text = resena.Comentario;

            bool contenidoRetirado = !_catalogoCache.ContainsKey(contenidoId);
            EstadoResenaTextBlock.Text = contenidoRetirado
                ? $"Editando reseña de «{resena.TituloAlCrear}» — Contenido retirado."
                : $"Editando reseña de «{resena.TituloAlCrear}».";
        }

        private void NuevaResena_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            _contenidoIdSeleccionado = null;
            ContenidoComboBox.IsEnabled = true;
            ContenidoComboBox.SelectedIndex = -1;
            PuntuacionTextBox.Text = string.Empty;
            ComentarioTextBox.Text = string.Empty;
            EstadoResenaTextBlock.Text = string.Empty;
            MisResenasListBox.SelectedIndex = -1;
        }

        private async void GuardarResena_Click(object sender, RoutedEventArgs e)
        {
            string? contenidoId = _contenidoIdSeleccionado ?? ContenidoComboBox.SelectedValue as string;

            if (string.IsNullOrWhiteSpace(contenidoId))
            {
                EstadoResenaTextBlock.Text = "Selecciona un contenido antes de guardar.";
                return;
            }

            if (!int.TryParse(PuntuacionTextBox.Text, out int puntuacion)
                || puntuacion < 1 || puntuacion > 10)
            {
                EstadoResenaTextBlock.Text = "La puntuación debe ser un número entero entre 1 y 10.";
                return;
            }

            string comentario = ComentarioTextBox.Text.Trim();
            if (comentario.Length < 1 || comentario.Length > 2000)
            {
                EstadoResenaTextBlock.Text = "El comentario debe tener entre 1 y 2000 caracteres.";
                return;
            }

            BtnGuardarResena.IsEnabled = false;
            EstadoResenaTextBlock.Text = "Guardando…";

            try
            {
                if (_contenidoIdSeleccionado is null)
                {
                    string tituloAlCrear = _catalogoCache.TryGetValue(contenidoId, out Contenido? contenido)
                        ? contenido.Titulo
                        : contenidoId;

                    var resena = new Resena
                    {
                        Puntuacion = puntuacion,
                        Comentario = comentario,
                        TituloAlCrear = tituloAlCrear,
                        CreadaEn = DateTime.UtcNow.ToString("o"),
                        ActualizadaEn = DateTime.UtcNow.ToString("o")
                    };

                    await _resenas.CrearAsync(_sesion, contenidoId, resena);
                    EstadoResenaTextBlock.Text = "Reseña creada correctamente.";
                }
                else
                {
                    await _resenas.ActualizarAsync(_sesion, contenidoId, puntuacion, comentario);
                    EstadoResenaTextBlock.Text = "Reseña actualizada correctamente.";
                }

                await CargarMisResenasAsync();
            }
            catch (ResenaYaExisteException)
            {
                EstadoResenaTextBlock.Text =
                    "Ya tienes una reseña para este contenido. Selecciónala en la lista para editarla.";
            }
            catch (InvalidOperationException ex)
            {
                EstadoResenaTextBlock.Text = ex.Message;
            }
            catch (HttpRequestException ex) when (
                ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                EstadoResenaTextBlock.Text =
                    "Firebase rechazó los datos: revisa la puntuación, el comentario o que el contenido exista.";
            }
            catch (HttpRequestException ex)
            {
                EstadoResenaTextBlock.Text = ex.StatusCode is { } estado
                    ? $"Firebase respondió con HTTP {(int)estado}."
                    : "No se pudo conectar con Firebase.";
            }
            catch (TaskCanceledException)
            {
                EstadoResenaTextBlock.Text = "Firebase tardó demasiado. Vuelve a intentarlo.";
            }
            finally
            {
                BtnGuardarResena.IsEnabled = true;
            }
        }

        private async void EliminarResena_Click(object sender, RoutedEventArgs e)
        {
            if (_contenidoIdSeleccionado is null)
            {
                EstadoResenaTextBlock.Text = "Selecciona una reseña de la lista para eliminarla.";
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show(
                "¿Seguro que quieres eliminar esta reseña? Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
            {
                return;
            }

            BtnEliminarResena.IsEnabled = false;

            try
            {
                await _resenas.EliminarAsync(_sesion, _contenidoIdSeleccionado);
                EstadoResenaTextBlock.Text = "Reseña eliminada.";
                LimpiarFormulario();
                await CargarMisResenasAsync();
            }
            catch (InvalidOperationException ex)
            {
                EstadoResenaTextBlock.Text = ex.Message;
            }
            catch (HttpRequestException ex) when (
                ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                EstadoResenaTextBlock.Text = "Firebase denegó el borrado.";
            }
            catch (HttpRequestException ex)
            {
                EstadoResenaTextBlock.Text = ex.StatusCode is { } estado
                    ? $"Firebase respondió con HTTP {(int)estado}."
                    : "No se pudo conectar con Firebase.";
            }
            finally
            {
                BtnEliminarResena.IsEnabled = true;
            }
        }

        private void ActualizarMenu()
        {
            // Quitamos la selección de todos los botones
            BtnInicio.Background = Brushes.Transparent;
            BtnCatalogo.Background = Brushes.Transparent;
            BtnResenas.Background = Brushes.Transparent;

            // Destacamos el botón seleccionado
            switch (MainTabs.SelectedIndex)
            {
                case 0:
                    BtnInicio.Background =
                        (Brush)FindResource("RojoBrush");
                    break;

                case 1:
                    BtnCatalogo.Background =
                        (Brush)FindResource("RojoBrush");
                    break;

                case 2:
                    BtnResenas.Background =
                        (Brush)FindResource("RojoBrush");
                    break;
            }
        }

    }
}
