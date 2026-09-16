using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GK.Lab6.Algorithms;
using GK.Lab6.Models;

namespace GK.Lab6
{
    /// <summary>
    /// Main window for the Bézier curve editor.
    /// Handles mouse interactions for adding, moving, and removing control points.
    /// Automatically redraws the curve when points change.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<ControlPoint> points;
        public ObservableCollection<ControlPoint> Points
        {
            get => points;
            private set { points = value; OnPropertyChanged(nameof(Points)); }
        }

        private int draggedPtId;
        public const int POINT_WIDTH = 10, POINT_HEIGHT = 10;

        private double deltaT;

        // Delta T property with validation
        public string DeltaT
        {
            get => deltaT.ToString();
            set
            {
                if (!double.TryParse(value, out double val))
                {
                    MessageBox.Show("Please enter a valid T increment.");
                    return;
                }
                if (!(val > 0 && val <= 1))
                {
                    MessageBox.Show($"Please enter T increment in range ({0},{1}>.");
                    return;
                }
                Cover();
                deltaT = val;
                Draw();
                OnPropertyChanged(nameof(DeltaT));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // Create drawing canvas
            Image.Source = new WriteableBitmap(670, 400, 96, 96, PixelFormats.Bgra32, null);

            // Initially no point is being dragged
            draggedPtId = -1;

            // Empty collection of points
            Points = new ObservableCollection<ControlPoint>();

            // Default deltaT = 1/4096
            DeltaT = (1.0 / 4096.0).ToString();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Add button click handler
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var bmp = (WriteableBitmap)Image.Source;

            // Validate X
            if (!double.TryParse(XTextBox.Text, out double x))
            {
                MessageBox.Show("Please enter a valid X coordinate.");
                return;
            }
            if (!(x >= 0 && x + POINT_WIDTH < bmp.PixelWidth))
            {
                MessageBox.Show($"Please enter X coordinate in range <{0}," +
                    $"{bmp.PixelWidth - POINT_WIDTH}).");
                return;
            }

            // Validate Y
            if (!double.TryParse(YTextBox.Text, out double y))
            {
                MessageBox.Show("Please enter a valid Y coordinate.");
                return;
            }
            if (!(y >= 0 && y + POINT_HEIGHT < bmp.PixelHeight))
            {
                MessageBox.Show($"Please enter Y coordinate in range <{0}," +
                    $"{bmp.PixelHeight - POINT_HEIGHT}).");
                return;
            }

            // Add the point and redraw
            Cover();
            Points.Add(new ControlPoint(x, y, this));
            Draw();
        }

        // Left click down handler
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If already dragging, ignore
            if (draggedPtId != -1) return;

            Point mse = e.GetPosition(Image);
            int mseX = (int)mse.X, mseY = (int)mse.Y;

            // Check if clicking on a point
            int id = 0;
            foreach (var p in points)
            {
                int pX = (int)p.X, pY = (int)p.Y;
                if (mseX >= pX && mseX < pX + POINT_WIDTH &&
                    mseY >= pY && mseY < pY + POINT_HEIGHT)
                {
                    // Start dragging
                    draggedPtId = id;
                    break;
                }
                ++id;
            }

            // If not clicking on any control point, add a new one
            if (draggedPtId == -1)
            {
                Cover();
                Points.Add(new ControlPoint(mse.X, mse.Y, this));
                Draw();
            }
        }

        // Mouse move handler
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            Point mse = e.GetPosition(Image);
            int mseX = (int)mse.X, mseY = (int)mse.Y;

            // If not dragging, just highlight points under cursor
            if (draggedPtId == -1)
            {
                int id = 0;
                foreach (var p in points)
                {
                    int pX = (int)p.X, pY = (int)p.Y;
                    if (mseX >= pX && mseX < pX + POINT_WIDTH &&
                        mseY >= pY && mseY < pY + POINT_HEIGHT)
                        p.IsHighlighted = true;
                    else
                        p.IsHighlighted = false;
                    ++id;
                }
                return;
            }

            // If dragging, move the point to cursor position
            ControlPoint draggedPt = points[draggedPtId];
            var bmp = (WriteableBitmap)Image.Source;

            // Ensure the point stays within canvas bounds
            if (mseX >= 0 && mseX + POINT_WIDTH < bmp.PixelWidth &&
                mseY >= 0 && mseY + POINT_HEIGHT < bmp.PixelHeight)
            {
                Cover();
                draggedPt.X = mse.X;
                draggedPt.Y = mse.Y;
                Draw();
            }
        }

        // Left click release handler
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            draggedPtId = -1;
        }

        // Right click handler
        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Don't remove while dragging
            if (draggedPtId != -1) return;

            Point mse = e.GetPosition(Image);
            int mseX = (int)mse.X, mseY = (int)mse.Y;

            // Find and remove the point under cursor
            int id = 0;
            foreach (var p in points)
            {
                int pX = (int)p.X, pY = (int)p.Y;
                if (mseX >= pX && mseX < pX + POINT_WIDTH &&
                    mseY >= pY && mseY < pY + POINT_HEIGHT)
                {
                    Cover();
                    points.RemoveAt(id);
                    Draw();
                    return;
                }
                ++id;
            }
        }

        // Redraws the entire canvas with given color for points and curve
        private void Redraw(int color)
        {
            // Draw all control points as rectangles
            foreach (var p in points)
                DrawPointRectangle(p, color);

            // Draw the Bézier curve if we have at least 2 points
            if (points.Count >= 2)
                BezierCurve.DrawWithLines((WriteableBitmap)Image.Source, points, deltaT, color);
        }

        // Clear
        public void Cover()
        {
            var bmp = (WriteableBitmap)Image.Source;
            bmp.Lock();
            Redraw((255 << 24) | (255 << 16) | (255 << 8) | 255);
        }

        // Draws
        public void Draw()
        {
            var bmp = (WriteableBitmap)Image.Source;
            Redraw((255 << 24) | (0 << 16) | (0 << 8) | 0);
            bmp.Unlock();
        }

        // Draws a filled rectangle for a control point at its (X,Y) position.
        private unsafe void DrawPointRectangle(ControlPoint point, int color)
        {
            int xTopLeft = (int)point.X, yTopLeft = (int)point.Y;
            var bmp = (WriteableBitmap)Image.Source;

            unsafe
            {
                int* p = (int*)bmp.BackBuffer;
                int bmpWid = bmp.PixelWidth, bmpHei = bmp.PixelHeight;
                int yLimit = yTopLeft + POINT_HEIGHT, xLimit = xTopLeft + POINT_WIDTH;

                // Fill the rectangle
                for (int y = yTopLeft; y < yLimit; ++y)
                    for (int x = xTopLeft; x < xLimit; ++x)
                        *(p + x + bmpWid * y) = color;
            }

            // Mark the rectangle area as dirty for rendering
            bmp.AddDirtyRect(new Int32Rect(xTopLeft, yTopLeft, POINT_WIDTH, POINT_HEIGHT));
        }
    }
}