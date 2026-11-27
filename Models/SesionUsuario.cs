using System.Text.Json.Serialization;

namespace FanVerse.Models;

public class SesionUsuario
{
    [JsonPropertyName("localId")]
    public string Uid { get; init; } = "";

    [JsonPropertyName("email")]
    public string Email { get; init; } = "";

    [JsonPropertyName("idToken")]
    public string IdToken { get; init; } = "";

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; init; } = "";
}