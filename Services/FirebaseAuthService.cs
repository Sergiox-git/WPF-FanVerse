using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using FanVerse.Models;

namespace FanVerse.Services;

public class FirebaseAuthService
{
    private static readonly HttpClient Cliente = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public async Task<SesionUsuario?> IniciarSesionAsync(
        string correo, string contrasena)
    {
        string? apiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Configura FIREBASE_API_KEY antes de iniciar FanVerse.");
        }

        string url = "https://identitytoolkit.googleapis.com/v1/"
            + $"accounts:signInWithPassword?key={Uri.EscapeDataString(apiKey)}";

        using HttpResponseMessage respuesta = await Cliente.PostAsJsonAsync(url, new
        {
            email = correo,
            password = contrasena,
            returnSecureToken = true
        });

        if (respuesta.StatusCode == HttpStatusCode.BadRequest)
        {
            return null;
        }

        respuesta.EnsureSuccessStatusCode();

        SesionUsuario? sesion =
            await respuesta.Content.ReadFromJsonAsync<SesionUsuario>();

        if (sesion is null || string.IsNullOrWhiteSpace(sesion.Uid)
                           || string.IsNullOrWhiteSpace(sesion.IdToken))
        {
            throw new InvalidOperationException(
                "Firebase devolvió una sesión incompleta.");
        }

        return sesion;
    }
}