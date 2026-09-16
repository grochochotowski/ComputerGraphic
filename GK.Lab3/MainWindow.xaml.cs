using System.Windows;
using System.Windows.Media;
using GK.Lab3.Views;

namespace GK.Lab3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadConversionView();
        }

        // Load the ConversionView into the ContentArea and set up the event handler
        private void LoadConversionView()
        {
            var view = new ConversionView();
            view.ColorChanged += View_ColorChanged;
            ContentArea.Content = view;
        }

        // Update MENU on color change in ConversionView
        private void View_ColorChanged(Color color)
        {
            MenuPanel.Background = new SolidColorBrush(Color.FromArgb(200, color.R, color.G, color.B));
        }

        // Switch view to Conversion
        private void Conversion_Click(object sender, RoutedEventArgs e)
        {
            LoadConversionView();
        }

        // Switch view to RgbCube
        private void Cube_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new RgbCubeView();
            MenuPanel.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50));
        }
    }
}