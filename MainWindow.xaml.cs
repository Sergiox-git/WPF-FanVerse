using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FanVerse.Models;
using FanVerse.Services;
using FanVerse.Views;

namespace FanVerse
{
    public partial class MainWindow : Window
    {
        private readonly SesionUsuario _sesion;
        private readonly CatalogoService _catalogo = new();

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

            await CargarCatalogoAsync();
        }

        private async Task CargarCatalogoAsync(string? contenidoId = null)
        {
            BtnCatalogo.IsEnabled = false;
            EstadoCatalogoTextBlock.Text = "Cargando catálogo…";

            try
            {
                var contenidos = await _catalogo.ObtenerAsync(_sesion.IdToken);
                CatalogoListBox.ItemsSource = contenidos;

                // Después de un alta, seleccionamos el registro por su ID.
                if (contenidoId is not null)
                {
                    CatalogoListBox.SelectedValue = contenidoId;
                }

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
                EstadoCatalogoTextBlock.Text =
                    "Firebase tardó demasiado. Vuelve a intentarlo.";
            }
            catch (JsonException)
            {
                EstadoCatalogoTextBlock.Text =
                    "La respuesta de Firebase no tiene el formato esperado.";
            }
            finally
            {
                BtnCatalogo.IsEnabled = true;
            }
        }

        private async void AnadirContenido_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new AltaContenidoWindow(_catalogo, _sesion.IdToken)
            {
                Owner = this
            };

            // El formulario devuelve true cuando Firebase confirma el alta.
            if (ventana.ShowDialog() == true)
            {
                await CargarCatalogoAsync(ventana.ContenidoId);
            }
        }
        private async void EditarContenido_Click(object sender, RoutedEventArgs e)
        {
            if (CatalogoListBox.SelectedItem is not KeyValuePair<string, Contenido> seleccionado)
            {
                EstadoCatalogoTextBlock.Text = "Selecciona una película o serie para editar.";
                return;
            }

            var ventana = new AltaContenidoWindow(
                _catalogo,
                _sesion.IdToken,
                seleccionado.Key,
                seleccionado.Value)
            {
                Owner = this
            };

            if (ventana.ShowDialog() == true)
            {
                await CargarCatalogoAsync(seleccionado.Key);
            }
        }

        private void Resenas_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 2;
            ActualizarMenu();
        }

        private void ActualizarMenu()
        {
            BtnInicio.Background = Brushes.Transparent;
            BtnCatalogo.Background = Brushes.Transparent;
            BtnResenas.Background = Brushes.Transparent;

            switch (MainTabs.SelectedIndex)
            {
                case 0:
                    BtnInicio.Background = (Brush)FindResource("RojoBrush");
                    break;
                case 1:
                    BtnCatalogo.Background = (Brush)FindResource("RojoBrush");
                    break;
                case 2:
                    BtnResenas.Background = (Brush)FindResource("RojoBrush");
                    break;
            }
        }
    }
}