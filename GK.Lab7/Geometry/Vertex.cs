using System.Windows;
using System.Windows.Media.Imaging;

namespace GK.Lab7.Geometry
{
    public class Vertex : ObservableObject
    {
        // Size of the vertex point when drawn
        public const int POINT_WIDTH = 10, POINT_HEIGHT = 10;

        // Coordinates X
        private double x;
        public double X
        {
            get => x;
            set
            {
                var win = MainWindow.Instance;
                win.Cover();
                x = value;
                win.Draw();
                OnPropertyChanged();
            }
        }

        // Coordinates Y
        private double y;
        public double Y
        {
            get => y;
            set
            {
                var win = MainWindow.Instance;
                win.Cover();
                y = value;
                win.Draw();
                OnPropertyChanged();
            }
        }

        // Highlighted state
        private bool isHighlighted;
        public bool IsHighlighted
        {
            get => isHighlighted;
            set { isHighlighted = value; OnPropertyChanged(); }
        }

        // Constructor
        public Vertex(double x, double y)
        {
            this.x = x;
            this.y = y;
            IsHighlighted = false;
        }

        // Draws the vertex as a rectangle on the given WriteableBitmap with the specified color
        public void DrawRectangle(WriteableBitmap bmp, int color)
        {
            int xTopLeft = (int)X, yTopLeft = (int)Y;
            unsafe
            {
                int* p = (int*)bmp.BackBuffer;
                int bmpWid = bmp.PixelWidth, bmpHei = bmp.PixelHeight;
                int yLimit = yTopLeft + POINT_HEIGHT, xLimit = xTopLeft + POINT_WIDTH;
                for (int y = yTopLeft; y < yLimit; ++y)
                    for (int x = xTopLeft; x < xLimit; ++x)
                        if (x >= 0 && x < bmpWid && y >= 0 && y < bmpHei)
                        {
                            *(p + x + bmpWid * y) = color;
                            bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                        }
            }
        }

        // Checks if the point intersects with this vertex's rectangle
        public bool Intersects(Point point)
        {
            double pX = point.X, pY = point.Y;
            return pX >= X && pX < X + POINT_WIDTH &&
                pY >= Y && pY < Y + POINT_HEIGHT;
        }

        // Sets coordinates with redraw
        public void SetXY(double x, double y)
        {
            var win = MainWindow.Instance;
            win.Cover();
            SetXYWithoutRedraw(x, y);
            win.Draw();
        }

        // Sets coordinates without redraw (used during transformations)
        public void SetXYWithoutRedraw(double x, double y)
        {
            this.x = x;
            this.y = y;
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
        }

        // Subtracts a Point from this Vertex and returns the resulting Vector
        public Vector Subtract(Point p)
        {
            return new Vector(X - p.X, Y - p.Y);
        }
    }
}
