using System.Windows;

namespace FanVerse.Views;

public partial class ConfirmarEliminacionWindow : Window
{
    public ConfirmarEliminacionWindow(string titulo)
    {
        InitializeComponent();
        MensajeTextBlock.Text = $"«{titulo}» se eliminará de Mi catálogo.";
    }

    private void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
