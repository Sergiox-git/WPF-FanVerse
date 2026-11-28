using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FanVerse.Models;

namespace FanVerse.Services;

public class ResenaYaExisteException : Exception
{
    public ResenaYaExisteException()
        : base("Ya existe una reseña para este contenido.") { }
}

public class ResenasService
{
    private static readonly HttpClient Cliente = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private static string UrlBase()
    {
        string? urlBase = Environment.GetEnvironmentVariable("FIREBASE_DB_URL");

        if (string.IsNullOrWhiteSpace(urlBase))
        {
            throw new InvalidOperationException(
                "Configura FIREBASE_DB_URL antes de iniciar FanVerse.");
        }

        return urlBase.TrimEnd('/');
    }

    public async Task CrearAsync(SesionUsuario sesion, string contenidoId, Resena resena)
    {
        string url = $"{UrlBase()}/usuarios/{sesion.Uid}/resenas/{contenidoId}.json"
            + $"?auth={Uri.EscapeDataString(sesion.IdToken)}";

        using var peticion = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(resena)
        };
        peticion.Headers.TryAddWithoutValidation("If-Match", "null_etag");

        using HttpResponseMessage respuesta = await Cliente.SendAsync(peticion);

        if (respuesta.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            throw new ResenaYaExisteException();
        }

        respuesta.EnsureSuccessStatusCode();
    }

    public async Task<Dictionary<string, Resena>> ObtenerTodasAsync(SesionUsuario sesion)
    {
        string url = $"{UrlBase()}/usuarios/{sesion.Uid}/resenas.json"
            + $"?auth={Uri.EscapeDataString(sesion.IdToken)}";

        using HttpResponseMessage respuesta = await Cliente.GetAsync(url);
        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content
            .ReadFromJsonAsync<Dictionary<string, Resena>>() ?? new();
    }

    public async Task ActualizarAsync(
        SesionUsuario sesion, string contenidoId, int puntuacion, string comentario)
    {
        string url = $"{UrlBase()}/usuarios/{sesion.Uid}/resenas/{contenidoId}.json"
            + $"?auth={Uri.EscapeDataString(sesion.IdToken)}";

        var cambios = new
        {
            puntuacion,
            comentario,
            actualizadaEn = DateTime.UtcNow.ToString("o")
        };

        using var peticion = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = JsonContent.Create(cambios)
        };

        using HttpResponseMessage respuesta = await Cliente.SendAsync(peticion);
        respuesta.EnsureSuccessStatusCode();
    }

    public async Task EliminarAsync(SesionUsuario sesion, string contenidoId)
    {
        string url = $"{UrlBase()}/usuarios/{sesion.Uid}/resenas/{contenidoId}.json"
            + $"?auth={Uri.EscapeDataString(sesion.IdToken)}";

        using HttpResponseMessage respuesta = await Cliente.DeleteAsync(url);
        respuesta.EnsureSuccessStatusCode();
    }
}