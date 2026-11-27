namespace FanVerse.Models;

public record ResultadoTmdb(
    int TmdbId,
    string Tipo,
    string Titulo,
    string Sinopsis,
    string? FechaEstreno,
    string? PosterPath);