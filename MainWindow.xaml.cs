
using System.Windows;
using System.Windows.Media;
namespace FanVerse
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ActualizarMenu();
        }


        private void Inicio_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 0;
            ActualizarMenu();
        }

        private void Catalogo_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 1;
            ActualizarMenu();
        }

        private void Resenas_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 2;
            ActualizarMenu();
        }

        private void ActualizarMenu()
        {
            // Quitamos la selección de todos los botones
            BtnInicio.Background = Brushes.Transparent;
            BtnCatalogo.Background = Brushes.Transparent;
            BtnResenas.Background = Brushes.Transparent;

            // Destacamos el botón seleccionado
            switch (MainTabs.SelectedIndex)
            {
                case 0:
                    BtnInicio.Background =
                        (Brush)FindResource("RojoBrush");
                    break;

                case 1:
                    BtnCatalogo.Background =
                        (Brush)FindResource("RojoBrush");
                    break;

                case 2:
                    BtnResenas.Background =
                        (Brush)FindResource("RojoBrush");
                    break;
            }
        }

    }
}
