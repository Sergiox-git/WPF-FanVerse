namespace FanVerse.Models;

public record ResultadoTmdb(
    int TmdbId,
    string Tipo,
    string Titulo,
    string Sinopsis,
    string? FechaEstreno,
    string? PosterPath)
{
    public double? Valoracion { get; init; }

    public Uri? PosterUrl => ConstruirPosterUrl(PosterPath);

    internal static Uri? ConstruirPosterUrl(string? posterPath)
    {
        if (string.IsNullOrWhiteSpace(posterPath))
        {
            return null;
        }

        string archivo = Uri.EscapeDataString(posterPath.TrimStart('/'));
        return new Uri($"https://image.tmdb.org/t/p/w500/{archivo}");
    }
}