using Microsoft.Win32;
using GK.Lab7.Geometry;
using GK.Lab7.Tools;
using GK.Lab7.Tools.LiveTransformations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GK.Lab7
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow Instance { get; private set; } = null!;

        private ObservableCollection<Polygon> polygons = null!;
        public ObservableCollection<Polygon> Polygons
        {
            get => polygons;
            private set { polygons = value; OnPropertyChanged(); }
        }

        private int selectedPolygonIndex = -1;
        public int SelectedPolygonIndex
        {
            get => selectedPolygonIndex;
            set
            {
                Cover();
                selectedPolygonIndex = value;
                Draw();
                OnPropertyChanged();
            }
        }

        public Polygon SelectedPolygon { get => Polygons[SelectedPolygonIndex]; }

        private readonly Tool[] tools; // State pattern
        private Tool selectedTool = null!;

        public Vertex TransformationPoint { get; }

        // Transformation point coordinates as strings for data binding
        public string TransformationPointXString
        {
            get
            {
                if (TransformationPoint == null) return string.Empty;
                return TransformationPoint.X.ToString();
            }
            set
            {
                if (!double.TryParse(value, out double val))
                { MessageBox.Show("Enter a valid X coordinate."); return; }
                TransformationPoint.X = val;
                OnPropertyChanged();
            }
        }

        // Transformation point Y coordinate as string for data binding
        public string TransformationPointYString
        {
            get
            {
                if (TransformationPoint == null) return string.Empty;
                return TransformationPoint.Y.ToString();
            }
            set
            {
                if (!double.TryParse(value, out double val))
                { MessageBox.Show("Enter a valid Y coordinate."); return; }
                TransformationPoint.Y = val;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            ImageControl.Source = new WriteableBitmap(670, 400, 96, 96, PixelFormats.Bgra32, null);
            Polygons = new ObservableCollection<Polygon>();
            tools = new Tool[]
            {
                new Tools.Cursor(),
                new Translation(),
                new Tools.LiveTransformations.Rotation(),
                new Scaling()
            };
            selectedTool = tools[0];
            CursorRadioButton.IsChecked = true;
            PointStackPanel.Visibility = Visibility.Collapsed;
            TranslationStackPanel.Visibility = Visibility.Collapsed;
            RotationStackPanel.Visibility = Visibility.Collapsed;
            ScalingStackPanel.Visibility = Visibility.Collapsed;

            // Initialize transformation point outside of visible area
            TransformationPoint = new Vertex(-2 * Vertex.POINT_WIDTH,
                -2 * Vertex.POINT_HEIGHT);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Add a new polygon
        private void AddPolygon_Click(object sender, RoutedEventArgs e)
        {
            Polygons.Add(new Polygon());
        }

        // Delete the selected polygon
        private void DeleteSelectedPolygon_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPolygonIndex == -1) return;
            Cover();
            Polygons.RemoveAt(SelectedPolygonIndex);
            Draw();
        }

        // Add a vertex to the selected polygon
        private void AddVertex_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPolygonIndex == -1) return;
            if (!double.TryParse(XTextBox.Text, out double x))
            {
                MessageBox.Show("Enter a valid X coordinate.");
                return;
            }
            if (!double.TryParse(YTextBox.Text, out double y))
            {
                MessageBox.Show("Enter a valid Y coordinate.");
                return;
            }
            Cover();
            var p = Polygons[SelectedPolygonIndex];
            p.Vertices.Add(new Vertex(x, y));
            Draw();
        }

        // Mouse event handlers that delegate to the selected tool
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedTool.LeftMouseDown(e.GetPosition(ImageControl));
        }

        // Mouse move event
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            selectedTool.MouseMoveTo(e.GetPosition(ImageControl));
        }

        // Mouse left button up event
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            selectedTool.LeftMouseUp(e.GetPosition(ImageControl));
        }

        // Mouse right button down event
        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedTool.RightMouseDown(e.GetPosition(ImageControl));
        }

        // Mouse middle button down event for setting transformation point
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle) return;
            if (CanDrawTransformationPoint())
            {
                var mp = e.GetPosition(ImageControl);
                TransformationPoint.SetXY(mp.X, mp.Y);
                UpdateTransformationPointTextBoxes();
            }
        }

        // Update transformation point text boxes
        public void UpdateTransformationPointTextBoxes()
        {
            OnPropertyChanged(nameof(TransformationPointXString));
            OnPropertyChanged(nameof(TransformationPointYString));
        }

        // Cover the image with white and redraw polygons
        public void Cover()
        {
            if (ImageControl == null) return;
            var bmp = (WriteableBitmap)ImageControl.Source;
            bmp.Lock();
            const int white = (255 << 24) | (255 << 16) | (255 << 8) | 255;
            foreach (var p in Polygons)
                p.DrawOn(bmp, white);
            if (TransformationPoint != null)
                TransformationPoint.DrawRectangle(bmp, white);
        }

        // Draw polygons and transformation point
        public void Draw()
        {
            if (ImageControl == null) return;
            var bmp = (WriteableBitmap)ImageControl.Source;
            for (int i = 0; i < Polygons.Count; ++i)
            {
                var pol = Polygons[i];
                if (i == SelectedPolygonIndex)
                    pol.DrawOn(bmp, (255 << 24) | (0 << 16) | (255 << 8) | 0); // Green
                else
                    pol.DrawOn(bmp, (255 << 24) | (0 << 16) | (0 << 8) | 0); // Black
            }
            if (CanDrawTransformationPoint())
                TransformationPoint.DrawRectangle(bmp,
                    (255 << 24) | (255 << 16) | (0 << 8) | 0); // Red
            bmp.Unlock();
        }

        // Check if the transformation point can be drawn
        private bool CanDrawTransformationPoint()
        {
            return TransformationPoint != null &&
                (selectedTool == tools[2] || selectedTool == tools[3]);
        }

        // JSON structure for saving/loading polygons
        private class JsonPolygon
        {
            public Point[] Vertices { get; set; } = Array.Empty<Point>();

            public JsonPolygon() { }

            public JsonPolygon(Point[] vertices)
            {
                Vertices = vertices;
            }
        }

        // Save polygons to a JSON file
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "Save";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Filter = "JSON (*.json)|*.json";
            dialog.FilterIndex = 1;
            if (dialog.ShowDialog() != true) return;

            var jsonStruct = new JsonPolygon[Polygons.Count];
            for (int p = 0; p < jsonStruct.Length; ++p)
            {
                var verts = Polygons[p].Vertices;
                var points = new Point[verts.Count];
                for (int v = 0; v < verts.Count; ++v)
                    points[v] = new Point(verts[v].X, verts[v].Y);
                jsonStruct[p] = new JsonPolygon(points);
            }

            var json = JsonSerializer.Serialize(jsonStruct, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dialog.FileName, json, Encoding.UTF8);
        }

        // Load polygons from a JSON file
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Load";
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Filter = "JSON (*.json)|*.json";
            if (dialog.ShowDialog() != true) return;

            var name = dialog.FileName;
            if (!File.Exists(name))
            {
                MessageBox.Show($"File {name} does not exist.");
                return;
            }

            var json = File.ReadAllText(name, Encoding.UTF8);
            var list = JsonSerializer.Deserialize<List<JsonPolygon>>(json);

            if (list == null) return;

            Cover();
            Polygons.Clear();
            foreach (var readPol in list)
            {
                var pol = new Polygon();
                var polVerts = pol.Vertices;
                var readVerts = readPol.Vertices;
                foreach (var rv in readVerts)
                    polVerts.Add(new Vertex(rv.X, rv.Y));
                Polygons.Add(pol);
            }
            Draw();
        }

        // Tool selection changed
        private void Tools_Checked(object sender, RoutedEventArgs e)
        {
            var rbs = new RadioButton[] { CursorRadioButton, TranslationRadioButton,
                RotationRadioButton, ScalingRadioButton };
            var sps = new StackPanel[] { PointStackPanel, TranslationStackPanel,
                RotationStackPanel, ScalingStackPanel };

            for (int i = 0; i < sps.Length; ++i)
                sps[i].Visibility = Visibility.Collapsed;
            PerformButton.Visibility = Visibility.Collapsed;
            Cover();

            if (sender == rbs[0])
            {
                selectedTool = tools[0];
                goto cleanup;
            }
            PerformButton.Visibility = Visibility.Visible;
            if (sender == rbs[1])
            {
                selectedTool = tools[1];
                TranslationStackPanel.Visibility = Visibility.Visible;
                goto cleanup;
            }
            PointStackPanel.Visibility = Visibility.Visible;
            for (int i = 2; i < rbs.Length; ++i)
                if (sender == rbs[i])
                {
                    selectedTool = tools[i];
                    sps[i].Visibility = Visibility.Visible;
                    goto cleanup;
                }
        cleanup:
            Draw();
        }

        // Perform the selected transformation
        private void Perform_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPolygonIndex == -1) return;
            if (selectedTool == tools[1]) // Translation
                PerformTranslation();
            else if (selectedTool == tools[2]) // Rotation
                PerformRotation();
            else if (selectedTool == tools[3]) // Scaling
                PerformScaling();
        }

        // Perform translation
        private void PerformTranslation()
        {
            if (!double.TryParse(TranslationVectorXTextBox.Text, out double x))
            {
                MessageBox.Show("Enter a valid X coordinate for the vector.");
                return;
            }
            if (!double.TryParse(TranslationVectorYTextBox.Text, out double y))
            {
                MessageBox.Show("Enter a valid Y coordinate for the vector.");
                return;
            }
            Cover();
            Polygons[SelectedPolygonIndex].Translate(x, y);
            Draw();
        }

        // Perform rotation
        private void PerformRotation()
        {
            if (!double.TryParse(RotationAngleTextBox.Text, out double ang))
            {
                MessageBox.Show("Enter a valid angle.");
                return;
            }
            Cover();
            // Convert degrees to radians
            Polygons[SelectedPolygonIndex].Rotate(TransformationPoint.X,
                TransformationPoint.Y, (ang / 180.0) * Math.PI);
            Draw();
        }

        // Perform scaling
        private void PerformScaling()
        {
            if (!double.TryParse(ScalingCoefficientXTextBox.Text, out double x))
            {
                MessageBox.Show("Enter a valid X scaling coefficient.");
                return;
            }
            if (!double.TryParse(ScalingCoefficientYTextBox.Text, out double y))
            {
                MessageBox.Show("Enter a valid Y scaling coefficient.");
                return;
            }
            Cover();
            Polygons[SelectedPolygonIndex].Scale(TransformationPoint.X,
                TransformationPoint.Y, x, y);
            Draw();
        }
    }
}