using System.Text.Json.Serialization;

namespace FanVerse.Models;

public class Resena
{
    [JsonPropertyName("puntuacion")]
    public int Puntuacion { get; set; }

    [JsonPropertyName("comentario")]
    public string Comentario { get; set; } = "";

    [JsonPropertyName("tituloAlCrear")]
    public string TituloAlCrear { get; set; } = "";

    [JsonPropertyName("creadaEn")]
    public string? CreadaEn { get; set; }

    [JsonPropertyName("actualizadaEn")]
    public string? ActualizadaEn { get; set; }
}