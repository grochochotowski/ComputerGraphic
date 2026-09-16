using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GK.Lab6.Models
{
    public class ControlPoint : INotifyPropertyChanged
    {
        private double x;
        public double X
        {
            get => x;
            set
            {
                var bmp = (WriteableBitmap)owner.Image.Source;
                // Ensure the point stays within the canvas width
                if (!(value >= 0 && value + MainWindow.POINT_WIDTH < bmp.PixelWidth))
                {
                    MessageBox.Show($"Please enter X coordinate in range <{0}," +
                        $"{bmp.PixelWidth - MainWindow.POINT_WIDTH}).");
                    return;
                }
                owner.Cover(); // Clear previous drawing
                x = value;
                owner.Draw(); // Redraw curve with new point position
                OnPropertyChanged(nameof(X));
            }
        }

        private double y;
        public double Y
        {
            get => y;
            set
            {
                var bmp = (WriteableBitmap)owner.Image.Source;
                // Ensure the point stays within the canvas height
                if (!(value >= 0 && value + MainWindow.POINT_HEIGHT < bmp.PixelHeight))
                {
                    MessageBox.Show($"Please enter Y coordinate in range <{0}," +
                        $"{bmp.PixelHeight - MainWindow.POINT_HEIGHT}).");
                    return;
                }
                owner.Cover(); // Clear previous drawing
                y = value;
                owner.Draw(); // Redraw curve with new point position
                OnPropertyChanged(nameof(Y));
            }
        }

        private bool isHighlighted;
        public bool IsHighlighted
        {
            get => isHighlighted;
            set { isHighlighted = value; OnPropertyChanged(nameof(IsHighlighted)); }
        }

        private MainWindow owner;
        public ControlPoint(double x, double y, MainWindow owner)
        {
            this.x = x;
            this.y = y;
            IsHighlighted = false;
            this.owner = owner;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
