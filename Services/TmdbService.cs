using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FanVerse.Models;

namespace FanVerse.Services;

public class TmdbService
{
    private static readonly HttpClient Cliente = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public async Task<IReadOnlyList<ResultadoTmdb>> BuscarAsync(
        string consulta, string tipo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consulta);

        string ruta = tipo switch
        {
            "pelicula" => "movie",
            "serie" => "tv",
            _ => throw new ArgumentException("Selecciona película o serie.", nameof(tipo))
        };

        string? token = Environment.GetEnvironmentVariable("TMDB_READ_TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Configura TMDB_READ_TOKEN antes de iniciar FanVerse.");
        }

        string query = Uri.EscapeDataString(consulta.Trim());
        string url = $"https://api.themoviedb.org/3/search/{ruta}"
            + $"?query={query}&language=es-ES";

        using var peticion = new HttpRequestMessage(HttpMethod.Get, url);
        peticion.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Trim());

        using HttpResponseMessage respuesta = await Cliente.SendAsync(peticion);
        respuesta.EnsureSuccessStatusCode();

        var pagina = await respuesta.Content.ReadFromJsonAsync<PaginaTmdb>();
        var resultados = new List<ResultadoTmdb>();

        foreach (var item in pagina?.Results ?? [])
        {
            string? titulo = tipo == "pelicula" ? item.Title : item.Name;
            string? fecha = tipo == "pelicula" ? item.ReleaseDate : item.FirstAirDate;

            if (item.Id <= 0 || string.IsNullOrWhiteSpace(titulo))
            {
                continue;
            }

            resultados.Add(new ResultadoTmdb(
                item.Id,
                tipo,
                titulo,
                item.Overview ?? "",
                string.IsNullOrWhiteSpace(fecha) ? null : fecha,
                string.IsNullOrWhiteSpace(item.PosterPath) ? null : item.PosterPath));
        }

        return resultados;
    }

    private sealed class PaginaTmdb
    {
        public List<ItemTmdb>? Results { get; init; }
    }

    private sealed class ItemTmdb
    {
        public int Id { get; init; }
        public string? Title { get; init; }
        public string? Name { get; init; }
        public string? Overview { get; init; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; init; }

        [JsonPropertyName("first_air_date")]
        public string? FirstAirDate { get; init; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; init; }
    }
}