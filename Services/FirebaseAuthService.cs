using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FanVerse.Models;

namespace FanVerse.Services;

public class FirebaseAuthService
{
    private static readonly HttpClient Cliente = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public async Task<SesionUsuario?> IniciarSesionAsync(string correo, string contrasena)
    {
        using HttpResponseMessage respuesta =
            await EnviarCredencialesAsync("signInWithPassword", correo, contrasena);

        if (respuesta.StatusCode == HttpStatusCode.BadRequest)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();

        SesionUsuario? sesion = await respuesta.Content.ReadFromJsonAsync<SesionUsuario>();

        if (sesion is null || string.IsNullOrWhiteSpace(sesion.Uid) ||
            string.IsNullOrWhiteSpace(sesion.IdToken))
        {
            throw new InvalidOperationException("Firebase devolvió una sesión incompleta.");
        }

        return sesion;
    }

    public async Task RegistrarAsync(string correo, string contrasena)
    {
        using HttpResponseMessage respuesta =
            await EnviarCredencialesAsync("signUp", correo, contrasena);

        if (respuesta.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.TooManyRequests)
        {
            var error = await respuesta.Content.ReadFromJsonAsync<RespuestaError>();
            throw new InvalidOperationException(TraducirErrorRegistro(error?.Error?.Mensaje));
        }

        respuesta.EnsureSuccessStatusCode();
    }

    private static Task<HttpResponseMessage> EnviarCredencialesAsync(
        string operacion, string correo, string contrasena)
    {
        string? apiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Configura FIREBASE_API_KEY antes de iniciar FanVerse.");
        }

        string url = "https://identitytoolkit.googleapis.com/v1/"
            + $"accounts:{operacion}?key={Uri.EscapeDataString(apiKey.Trim())}";

        return Cliente.PostAsJsonAsync(url, new
        {
            email = correo,
            password = contrasena,
            returnSecureToken = true
        });
    }

    private static string TraducirErrorRegistro(string? mensaje)
    {
        string? codigo = mensaje?.Split(':', 2)[0].Trim();

        return codigo switch
        {
            "EMAIL_EXISTS" => "Ya hay una cuenta con ese correo. Puedes iniciar sesión.",
            "INVALID_EMAIL" => "Introduce un correo electrónico válido.",
            "WEAK_PASSWORD" or "PASSWORD_DOES_NOT_MEET_REQUIREMENTS" =>
                "La contraseña no cumple los requisitos de Firebase. Prueba con una más larga y variada.",
            "OPERATION_NOT_ALLOWED" => "El registro por correo y contraseña está desactivado en Firebase.",
            "TOO_MANY_ATTEMPTS_TRY_LATER" => "Hay demasiados intentos. Espera y vuelve a probar.",
            _ => "Firebase rechazó el registro. Comprueba los datos e inténtalo de nuevo."
        };
    }

    private sealed record RespuestaError(
        [property: JsonPropertyName("error")] DetalleError? Error);

    private sealed record DetalleError(
        [property: JsonPropertyName("message")] string? Mensaje);
}
