using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using FanVerse.Models;
using FanVerse.Services;

namespace FanVerse.Views;

public partial class BuscarTmdbWindow : Window
{
    private readonly TmdbService _tmdb = new();

    public BuscarTmdbWindow()
    {
        InitializeComponent();
    }

    public ResultadoTmdb? ResultadoSeleccionado { get; private set; }

    private void Importar_Click(object sender, RoutedEventArgs e)
    {
        if (ResultadosListBox.SelectedItem is not ResultadoTmdb resultado)
        {
            EstadoTextBlock.Text = "Selecciona una película o serie para importar.";
            return;
        }

        ResultadoSeleccionado = resultado;
        DialogResult = true;
    }

    private async void Buscar_Click(object sender, RoutedEventArgs e)
    {
        string consulta = ConsultaTextBox.Text.Trim();

        if (consulta.Length == 0)
        {
            EstadoTextBlock.Text = "Escribe un título para buscar.";
            ConsultaTextBox.Focus();
            return;
        }

        if (TipoComboBox.SelectedValue is not string tipo)
        {
            EstadoTextBlock.Text = "Selecciona película o serie.";
            return;
        }

        BuscarButton.IsEnabled = false;
        ResultadosListBox.ItemsSource = null;
        EstadoTextBlock.Text = "Buscando en TMDB…";

        try
        {
            var resultados = await _tmdb.BuscarAsync(consulta, tipo);
            ResultadosListBox.ItemsSource = resultados;

            EstadoTextBlock.Text = resultados.Count == 0
                ? "No se encontraron resultados."
                : $"Resultados en esta página: {resultados.Count}";
        }
        catch (InvalidOperationException ex)
        {
            EstadoTextBlock.Text = ex.Message;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            EstadoTextBlock.Text = "TMDB rechazó el token. Revisa TMDB_READ_TOKEN.";
        }
        catch (HttpRequestException ex)
        {
            EstadoTextBlock.Text = ex.StatusCode is { } estado
                ? $"TMDB respondió con HTTP {(int)estado}."
                : "No se pudo conectar con TMDB.";
        }
        catch (TaskCanceledException)
        {
            EstadoTextBlock.Text = "TMDB tardó demasiado. Vuelve a intentarlo.";
        }
        catch (JsonException)
        {
            EstadoTextBlock.Text = "La respuesta de TMDB tiene un formato inesperado.";
        }
        finally
        {
            BuscarButton.IsEnabled = true;
        }
    }

    private void ResultadosListBox_SelectionChanged(
        object sender, SelectionChangedEventArgs e)
    {
        if (ResultadosListBox.SelectedItem is ResultadoTmdb resultado)
        {
            TituloTextBlock.Text = resultado.Titulo;
            FechaTextBlock.Text = resultado.FechaEstreno ?? "Fecha desconocida";
            SinopsisTextBlock.Text = string.IsNullOrWhiteSpace(resultado.Sinopsis)
                ? "Sin sinopsis disponible."
                : resultado.Sinopsis;
            return;
        }

        TituloTextBlock.Text = "";
        FechaTextBlock.Text = "";
        SinopsisTextBlock.Text = "";
    }
}