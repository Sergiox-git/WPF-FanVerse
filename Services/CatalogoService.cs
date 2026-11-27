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

    private sealed class AltaCatalogoRespuesta
    {
        public string? Name { get; init; }
    }

    private static string ConstruirUrlCatalogo(string idToken)
    {
        string? urlBase = Environment.GetEnvironmentVariable("FIREBASE_DB_URL");

        if (string.IsNullOrWhiteSpace(urlBase))
        {
            throw new InvalidOperationException(
                "Configura FIREBASE_DB_URL antes de iniciar FanVerse.");
        }

        return $"{urlBase.Trim().TrimEnd('/')}/catalogo.json"
            + $"?auth={Uri.EscapeDataString(idToken)}";
    }
}