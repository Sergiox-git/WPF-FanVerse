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

}