using System.Windows;
using System.Windows.Controls;

namespace FanVerse.Views;

public partial class BarraVentana : UserControl
{
    public BarraVentana()
    {
        InitializeComponent();
    }

    private void Minimizar_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is Window ventana)
        {
            SystemCommands.MinimizeWindow(ventana);
        }
    }

    private void Maximizar_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is not Window ventana)
        {
            return;
        }

        if (ventana.WindowState == WindowState.Maximized)
        {
            SystemCommands.RestoreWindow(ventana);
        }
        else
        {
            SystemCommands.MaximizeWindow(ventana);
        }
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}
