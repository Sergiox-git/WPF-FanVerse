using System.ComponentModel;
using System.Net.Http;
using System.Net.Mail;
using System.Text.Json;
using System.Windows;
using FanVerse.Services;

namespace FanVerse.Views;

public partial class RegistroWindow : Window
{
    private readonly FirebaseAuthService _auth;
    private bool _registrando;

    public string? CorreoRegistrado { get; private set; }

    public RegistroWindow(FirebaseAuthService auth, string correoInicial)
    {
        _auth = auth;
        InitializeComponent();
        CorreoTextBox.Text = correoInicial;
    }

    private async void Registrar_Click(object sender, RoutedEventArgs e)
    {
        if (_registrando)
        {
            return;
        }

        string correo = CorreoTextBox.Text.Trim();
        string contrasena = ContrasenaPasswordBox.Password;

        if (!MailAddress.TryCreate(correo, out MailAddress? direccion) ||
            !string.Equals(correo, direccion.Address, StringComparison.OrdinalIgnoreCase))
        {
            EstadoTextBlock.Text = "Introduce un correo electrónico válido.";
            CorreoTextBox.Focus();
            return;
        }

        if (contrasena.Length < 6)
        {
            EstadoTextBlock.Text = "La contraseña debe tener al menos 6 caracteres.";
            ContrasenaPasswordBox.Focus();
            return;
        }

        if (contrasena != ConfirmacionPasswordBox.Password)
        {
            EstadoTextBlock.Text = "Las contraseñas no coinciden.";
            ConfirmacionPasswordBox.Focus();
            return;
        }

        _registrando = true;
        CamposRegistro.IsEnabled = false;
        RegistrarButton.IsEnabled = false;
        EstadoTextBlock.Text = "Creando tu cuenta…";

        try
        {
            await _auth.RegistrarAsync(correo, contrasena);
            CorreoRegistrado = correo;
        }
        catch (InvalidOperationException ex)
        {
            EstadoTextBlock.Text = ex.Message;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            EstadoTextBlock.Text =
                "No se pudo confirmar el registro. Prueba a iniciar sesión antes de volver a registrarte.";
        }
        finally
        {
            _registrando = false;
            CamposRegistro.IsEnabled = true;
            RegistrarButton.IsEnabled = true;
        }

        if (CorreoRegistrado is not null)
        {
            ContrasenaPasswordBox.Clear();
            ConfirmacionPasswordBox.Clear();
            DialogResult = true;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (_registrando)
        {
            e.Cancel = true;
        }
    }
}
