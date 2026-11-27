using System.Text.Json.Serialization;

namespace FanVerse.Models;

public class Contenido
{
    [JsonPropertyName("titulo")]
    public string Titulo { get; init; } = "";
}