using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FanVerse.Services;

namespace FanVerse.Views;

public partial class BuscadorTmdb : UserControl
{
    private readonly TmdbService _tmdb = new();

    public BuscadorTmdb()
    {
        InitializeComponent();
    }

    public void EnfocarConsulta()
    {
        ConsultaTextBox.Focus();
    }

    private async void Buscar_Click(object sender, RoutedEventArgs e)
    {
        await BuscarAsync();
    }

    private async void Consulta_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await BuscarAsync();
        }
    }

    private async Task BuscarAsync()
    {
        if (!BuscarButton.IsEnabled)
        {
            return;
        }

        string consulta = ConsultaTextBox.Text.Trim();

        if (consulta.Length == 0)
        {
            EstadoTextBlock.Text = "Escribe un título para buscar.";
            ConsultaTextBox.Focus();
            return;
        }

        if (TipoComboBox.SelectedValue is not string tipo)
        {
            return;
        }

        BuscarButton.IsEnabled = false;
        Resultados.ItemsSource = null;
        EstadoTextBlock.Text = "Buscando en TMDB…";

        try
        {
            var resultados = await _tmdb.BuscarAsync(consulta, tipo);
            Resultados.ItemsSource = resultados;

            EstadoTextBlock.Text = resultados.Count == 0
                ? "No se encontraron resultados."
                : $"Resultados de «{consulta}»: {resultados.Count}";
        }
        catch (InvalidOperationException ex)
        {
            EstadoTextBlock.Text = ex.Message;
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            EstadoTextBlock.Text = "TMDB rechazó el acceso. Revisa TMDB_READ_TOKEN.";
        }
        catch (Exception ex) when (
            ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            EstadoTextBlock.Text = "No se pudo completar la búsqueda. Vuelve a intentarlo.";
        }
        finally
        {
            BuscarButton.IsEnabled = true;
        }
    }
}