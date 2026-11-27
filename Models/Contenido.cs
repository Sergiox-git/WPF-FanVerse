using System.Text.Json.Serialization;

namespace FanVerse.Models;

public class Contenido
{
    [JsonPropertyName("titulo")]
    public string Titulo { get; init; } = "";

    [JsonPropertyName("tipo")]
    public string Tipo { get; init; } = "";

    [JsonPropertyName("sinopsis")]
    public string Sinopsis { get; init; } = "";

    [JsonPropertyName("origen")]
    public string Origen { get; init; } = "";

    [JsonPropertyName("tmdbId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TmdbId { get; init; }

    [JsonPropertyName("tipoTmdb")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TipoTmdb { get; init; }

    [JsonPropertyName("fechaEstreno")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FechaEstreno { get; init; }

    [JsonPropertyName("posterPath")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PosterPath { get; init; }

    [JsonIgnore]
    public Uri? PosterUrl => ResultadoTmdb.ConstruirPosterUrl(PosterPath);
}