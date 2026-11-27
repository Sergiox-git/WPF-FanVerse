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
        string? urlBase = Environment.GetEnvironmentVariable("FIREBASE_DB_URL");

        if (string.IsNullOrWhiteSpace(urlBase))
        {
            throw new InvalidOperationException(
                "Configura FIREBASE_DB_URL antes de iniciar FanVerse.");
        }

        string url = $"{urlBase.Trim().TrimEnd('/')}/catalogo.json"
            + $"?auth={Uri.EscapeDataString(idToken)}";

        using HttpResponseMessage respuesta = await Cliente.GetAsync(url);
        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content
            .ReadFromJsonAsync<Dictionary<string, Contenido>>() ?? new();
    }
}