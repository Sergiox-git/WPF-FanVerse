
using System.Windows;
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

        private void Resenas_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 2;
            ActualizarMenu();
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
