using System.Net.Http;
using System.Text.Json;
using System.Windows;
using FanVerse.Models;
using FanVerse.Services;

namespace FanVerse.Views;

public partial class LoginWindow : Window
{
    private readonly FirebaseAuthService _auth = new();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void Entrar_Click(object sender, RoutedEventArgs e)
    {
        string correo = CorreoTextBox.Text.Trim();
        string contrasena = ContrasenaPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(correo))
        {
            MensajeTextBlock.Text = "Introduce tu correo electrónico.";
            CorreoTextBox.Focus();
            return;
        }

        if (string.IsNullOrEmpty(contrasena))
        {
            MensajeTextBlock.Text = "Introduce tu contraseña.";
            ContrasenaPasswordBox.Focus();
            return;
        }

        EntrarButton.IsEnabled = false;
        MensajeTextBlock.Text = "Comprobando el acceso…";

        try
        {
            SesionUsuario? sesion =
                await _auth.IniciarSesionAsync(correo, contrasena);

            if (!IsLoaded)
            {
                return;
            }

            if (sesion is null)
            {
                MensajeTextBlock.Text =
                    "Firebase rechazó el acceso. Revisa el correo y la contraseña.";
                ContrasenaPasswordBox.Focus();
                return;
            }

            ContrasenaPasswordBox.Clear();

            var ventanaPrincipal = new MainWindow(sesion);
            Application.Current.MainWindow = ventanaPrincipal;
            ventanaPrincipal.Show();
            Close();
        }
        catch (HttpRequestException)
        {
            MensajeTextBlock.Text = "No se pudo completar la petición a Firebase.";
        }
        catch (TaskCanceledException)
        {
            MensajeTextBlock.Text = "Firebase tardó demasiado. Vuelve a intentarlo.";
        }
        catch (JsonException)
        {
            MensajeTextBlock.Text = "Firebase devolvió una respuesta inesperada.";
        }
        catch (InvalidOperationException ex)
        {
            MensajeTextBlock.Text = ex.Message;
        }
        finally
        {
            EntrarButton.IsEnabled = true;
        }
    }
}