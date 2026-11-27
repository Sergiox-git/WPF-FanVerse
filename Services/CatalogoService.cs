using System.Net.Http;
using System.Net.Http.Json;
using FanVerse.Models;

namespace FanVerse.Services;

public class CatalogoService
{
    private static readonly HttpClient Cliente = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public async Task<Dictionary<string, Contenido>> ObtenerAsync(string idToken)
    {
        string url = ConstruirUrlCatalogo(idToken);

        using HttpResponseMessage respuesta = await Cliente.GetAsync(url);
        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content
            .ReadFromJsonAsync<Dictionary<string, Contenido>>() ?? new();
    }

    public async Task<string> CrearAsync(Contenido contenido, string idToken)
    {
        string url = ConstruirUrlCatalogo(idToken);

        using HttpResponseMessage respuesta =
            await Cliente.PostAsJsonAsync(url, contenido);

        respuesta.EnsureSuccessStatusCode();

        AltaCatalogoRespuesta? alta =
            await respuesta.Content.ReadFromJsonAsync<AltaCatalogoRespuesta>();

        if (string.IsNullOrWhiteSpace(alta?.Name))
        {
            throw new InvalidOperationException(
                "Firebase no devolvió el identificador del contenido.");
        }

        return alta.Name;
    }
    public async Task EditarAsync(
    string contenidoId, Contenido contenido, string idToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contenidoId);

        string url = ConstruirUrlCatalogo(idToken, contenidoId);

        var cambios = new
        {
            titulo = contenido.Titulo,
            tipo = contenido.Tipo,
            sinopsis = contenido.Sinopsis
        };

        using HttpResponseMessage respuesta =
            await Cliente.PatchAsJsonAsync(url, cambios);

        respuesta.EnsureSuccessStatusCode();
    }

    public async Task EliminarAsync(string contenidoId, string idToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contenidoId);

        string url = ConstruirUrlCatalogo(idToken, contenidoId);

        using HttpResponseMessage respuesta = await Cliente.DeleteAsync(url);
        respuesta.EnsureSuccessStatusCode();
    }

    private sealed class AltaCatalogoRespuesta
    {
        public string? Name { get; init; }
    }

    private static string ConstruirUrlCatalogo(
        string idToken, string? contenidoId = null)
    {
        string? urlBase = Environment.GetEnvironmentVariable("FIREBASE_DB_URL");

        if (string.IsNullOrWhiteSpace(urlBase))
        {
            throw new InvalidOperationException(
                "Configura FIREBASE_DB_URL antes de iniciar FanVerse.");
        }

        string ruta = contenidoId is null
            ? "catalogo"
            : $"catalogo/{Uri.EscapeDataString(contenidoId)}";

        return $"{urlBase.Trim().TrimEnd('/')}/{ruta}.json"
            + $"?auth={Uri.EscapeDataString(idToken)}";
    }
}