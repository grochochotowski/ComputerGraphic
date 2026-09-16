using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GK.Lab1
{
    public partial class MainWindow : Window
    {
        enum Tool { Select, Line, Rect, Ellipse }

        private Tool _currentTool = Tool.Line;

        // --- DRAWING FIELDS ---
        private Point? _start;
        private Shape? _preview;
        private bool _isDrawing = false;

        // --- SELECTION / MOVE / RESIZE ---
        private Shape? _selected;
        private Ellipse? _resizeHandle;
        private bool _isDragging = false;
        private bool _isResizing = false;
        private Point _dragStartCanvas;
        private double _origLeft, _origTop, _origX1, _origY1, _origX2, _origY2;

        private const double HANDLE_SIZE = 8;

        // --- CONSTRUCTOR ---
        public MainWindow()
        {
            InitializeComponent();
            UpdateParamPanels();
        }

        // --- TOP BAR ---
        private void ToolSelect_Click(object s, RoutedEventArgs e) { _currentTool = Tool.Select; UpdateParamPanels(); }
        private void ToolLine_Click(object s, RoutedEventArgs e) { _currentTool = Tool.Line; UpdateParamPanels(); }
        private void ToolRect_Click(object s, RoutedEventArgs e) { _currentTool = Tool.Rect; UpdateParamPanels(); }
        private void ToolCircle_Click(object s, RoutedEventArgs e) { _currentTool = Tool.Ellipse; UpdateParamPanels(); }
        private void UpdateParamPanels()
        {
            PanelLine.Visibility = Visibility.Visible;
            if (PanelRect != null) PanelRect.Visibility = Visibility.Collapsed;
            if (PanelCircle != null) PanelCircle.Visibility = Visibility.Collapsed;
        }

        // --- MOUSE CONTROL ---
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(DrawCanvas);

            // select
            if (_currentTool == Tool.Select)
            {
                // --- resize handle click ---
                if (_resizeHandle != null && e.OriginalSource == _resizeHandle)
                {
                    _isResizing = true;
                    DrawCanvas.CaptureMouse();
                    return;
                }

                // --- select shape ---
                if (e.OriginalSource is Shape hit && hit != _resizeHandle && DrawCanvas.Children.Contains(hit))
                {
                    SelectShape(hit);
                    _isDragging = true;
                    _dragStartCanvas = pos;

                    if (hit is Line l)
                    {
                        _origX1 = l.X1; _origY1 = l.Y1;
                        _origX2 = l.X2; _origY2 = l.Y2;
                    }
                    else
                    {
                        _origLeft = Canvas.GetLeft(hit);
                        _origTop = Canvas.GetTop(hit);
                        if (double.IsNaN(_origLeft)) _origLeft = 0;
                        if (double.IsNaN(_origTop)) _origTop = 0;
                    }

                    DrawCanvas.CaptureMouse();
                }
                else
                {
                    SelectShape(null);
                }
                return;
            }

            // drawing mode
            if (!_isDrawing)
            {
                // start
                _start = pos;
                _isDrawing = true;

                var strokeBrush = Brushes.Black;
                double thick = 2;

                switch (_currentTool)
                {
                    case Tool.Line:
                        var line = new Line
                        {
                            Stroke = strokeBrush,
                            StrokeThickness = thick,
                            X1 = pos.X,
                            Y1 = pos.Y,
                            X2 = pos.X,
                            Y2 = pos.Y
                        };
                        _preview = line;
                        DrawCanvas.Children.Add(line);
                        break;

                    case Tool.Rect:
                        var rect = new Rectangle
                        {
                            Stroke = strokeBrush,
                            StrokeThickness = thick,
                            Fill = Brushes.Transparent
                        };
                        _preview = rect;
                        DrawCanvas.Children.Add(rect);
                        Canvas.SetLeft(rect, pos.X);
                        Canvas.SetTop(rect, pos.Y);
                        break;

                    case Tool.Ellipse:
                        var ell = new Ellipse
                        {
                            Stroke = strokeBrush,
                            StrokeThickness = thick,
                            Fill = Brushes.Transparent
                        };
                        _preview = ell;
                        DrawCanvas.Children.Add(ell);
                        Canvas.SetLeft(ell, pos.X);
                        Canvas.SetTop(ell, pos.Y);
                        break;
                }
            }
            else
            {
                // stop
                if (_preview is Line ln)
                {
                    ln.X2 = pos.X; ln.Y2 = pos.Y;
                }
                else if (_preview is Rectangle r)
                {
                    UpdateRect(_start!.Value, pos, r);
                }
                else if (_preview is Ellipse el)
                {
                    UpdateCircle(_start!.Value, pos, el);
                }

                _start = null;
                _preview = null;
                _isDrawing = false;
            }
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging || _isResizing)
            {
                _isDragging = _isResizing = false;
                DrawCanvas.ReleaseMouseCapture();
                return;
            }
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(DrawCanvas);

            // resizing
            if (_isResizing && _selected != null)
            {
                if (_selected is Line l)
                {
                    l.X2 = pos.X; l.Y2 = pos.Y;
                }
                else if (_selected is Rectangle r)
                {
                    double left = Canvas.GetLeft(r);
                    double top = Canvas.GetTop(r);
                    double width = pos.X - left;
                    double height = pos.Y - top;
                    if (width > 0 && height > 0)
                    {
                        r.Width = width; r.Height = height;
                    }
                }
                else if (_selected is Ellipse el)
                {
                    double cx = Canvas.GetLeft(el) + el.Width / 2;
                    double cy = Canvas.GetTop(el) + el.Height / 2;
                    double radius = Dist(pos.X, pos.Y, cx, cy);
                    el.Width = el.Height = radius * 2;
                    Canvas.SetLeft(el, cx - radius);
                    Canvas.SetTop(el, cy - radius);
                }
                UpdateHandle();
                UpdateTextboxCoords();
                return;
            }

            // moving
            if (_isDragging && _selected != null)
            {
                double dx = pos.X - _dragStartCanvas.X;
                double dy = pos.Y - _dragStartCanvas.Y;

                if (_selected is Line l)
                {
                    l.X1 = _origX1 + dx; l.Y1 = _origY1 + dy;
                    l.X2 = _origX2 + dx; l.Y2 = _origY2 + dy;
                }
                else
                {
                    Canvas.SetLeft(_selected, _origLeft + dx);
                    Canvas.SetTop(_selected, _origTop + dy);
                }
                UpdateHandle();
                UpdateTextboxCoords();
                return;
            }

            // drawing preview
            if (!_isDrawing || _preview == null || !_start.HasValue) return;

            if (_preview is Line ln)
            {
                ln.X2 = pos.X; ln.Y2 = pos.Y;
            }
            else if (_preview is Rectangle r)
            {
                UpdateRect(_start.Value, pos, r);
            }
            else if (_preview is Ellipse el)
            {
                UpdateCircle(_start.Value, pos, el);
            }
        }

        // --- HELPERS ---
        private static void UpdateRect(Point a, Point b, Rectangle r)
        {
            double left = Math.Min(a.X, b.X), top = Math.Min(a.Y, b.Y);

            r.Width = Math.Abs(b.X - a.X);
            r.Height = Math.Abs(b.Y - a.Y);

            Canvas.SetLeft(r, left);
            Canvas.SetTop(r, top);
        }
        private static void UpdateCircle(Point c, Point e, Ellipse el)
        {
            double dx = e.X - c.X, dy = e.Y - c.Y, r = Math.Sqrt(dx * dx + dy * dy);

            el.Width = el.Height = 2 * r;

            Canvas.SetLeft(el, c.X - r);
            Canvas.SetTop(el, c.Y - r);
        }
        private static double Dist(double ax, double ay, double bx, double by)
        {
            double dx = ax - bx, dy = ay - by;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        private static bool TryDouble(TextBox tb, out double v)
        {
            return double.TryParse(tb.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
        }
        private void UpdateTextboxCoords()
        {
            if (_selected == null) return;

            if (_selected is Line ln)
            {
                TbX1.Text = ln.X1.ToString("0.##", CultureInfo.InvariantCulture);
                TbY1.Text = ln.Y1.ToString("0.##", CultureInfo.InvariantCulture);
                TbX2.Text = ln.X2.ToString("0.##", CultureInfo.InvariantCulture);
                TbY2.Text = ln.Y2.ToString("0.##", CultureInfo.InvariantCulture);
            }
            else if (_selected is Rectangle r)
            {
                double left = Canvas.GetLeft(r);
                double top = Canvas.GetTop(r);
                double right = left + r.Width;
                double bottom = top + r.Height;

                TbX1.Text = left.ToString("0.##", CultureInfo.InvariantCulture);
                TbY1.Text = top.ToString("0.##", CultureInfo.InvariantCulture);
                TbX2.Text = right.ToString("0.##", CultureInfo.InvariantCulture);
                TbY2.Text = bottom.ToString("0.##", CultureInfo.InvariantCulture);
            }
            else if (_selected is Ellipse el)
            {
                double cx = Canvas.GetLeft(el) + el.Width / 2;
                double cy = Canvas.GetTop(el) + el.Height / 2;
                double ex = cx + el.Width / 2;
                double ey = cy;

                TbX1.Text = cx.ToString("0.##", CultureInfo.InvariantCulture);
                TbY1.Text = cy.ToString("0.##", CultureInfo.InvariantCulture);
                TbX2.Text = ex.ToString("0.##", CultureInfo.InvariantCulture);
                TbY2.Text = ey.ToString("0.##", CultureInfo.InvariantCulture);
            }
        }
        private void ApplyParams_Click(object sender, RoutedEventArgs e)
        {
            var strokeBrush = Brushes.Black;
            double thick = 2;

            if (!TryDouble(TbX1, out double x1) || !TryDouble(TbY1, out double y1) ||
                !TryDouble(TbX2, out double x2) || !TryDouble(TbY2, out double y2))
                return;

            if (_selected != null)
            {
                if (_selected is Line ln)
                {
                    ln.X1 = x1; ln.Y1 = y1;
                    ln.X2 = x2; ln.Y2 = y2;
                }
                else if (_selected is Rectangle r)
                {
                    double left = Math.Min(x1, x2);
                    double top = Math.Min(y1, y2);
                    double w = Math.Abs(x2 - x1);
                    double h = Math.Abs(y2 - y1);
                    r.Width = w; r.Height = h;
                    Canvas.SetLeft(r, left);
                    Canvas.SetTop(r, top);
                }
                else if (_selected is Ellipse el)
                {
                    double dx = x2 - x1, dy = y2 - y1;
                    double radius = Math.Sqrt(dx * dx + dy * dy);
                    el.Width = el.Height = 2 * radius;
                    Canvas.SetLeft(el, x1 - radius);
                    Canvas.SetTop(el, y1 - radius);
                }

                UpdateHandle();
                return;
            }

            switch (_currentTool)
            {
                case Tool.Line:
                    {
                        var ln = new Line
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2,
                            Stroke = strokeBrush,
                            StrokeThickness = thick,
                            StrokeStartLineCap = PenLineCap.Flat,
                            StrokeEndLineCap = PenLineCap.Flat
                        };
                        DrawCanvas.Children.Add(ln);
                        break;
                    }

                case Tool.Rect:
                    {
                        double left = Math.Min(x1, x2);
                        double top = Math.Min(y1, y2);
                        double w = Math.Abs(x2 - x1);
                        double h = Math.Abs(y2 - y1);

                        var r = new Rectangle
                        {
                            Width = w,
                            Height = h,
                            Stroke = strokeBrush,
                            StrokeThickness = thick,
                            Fill = Brushes.Transparent
                        };
                        DrawCanvas.Children.Add(r);
                        Canvas.SetLeft(r, left);
                        Canvas.SetTop(r, top);
                        break;
                    }

                case Tool.Ellipse:
                    {
                        double dx = x2 - x1, dy = y2 - y1;
                        double radius = Math.Sqrt(dx * dx + dy * dy);

                        var el = new Ellipse
                        {
                            Width = 2 * radius,
                            Height = 2 * radius,
                            Stroke = strokeBrush,
                            StrokeThickness = thick,
                            Fill = Brushes.Transparent
                        };
                        DrawCanvas.Children.Add(el);
                        Canvas.SetLeft(el, x1 - radius);
                        Canvas.SetTop(el, y1 - radius);
                        break;
                    }
            }
        }

        // --- SELECTION ---
        private void SelectShape(Shape? s)
        {
            if (_selected != null) 
                _selected.StrokeDashArray = null;

            _selected = s;

            if (_selected != null)
                _selected.StrokeDashArray = new DoubleCollection { 4, 2 };

            UpdateHandle();
        }
        private void UpdateHandle()
        {
            if (_resizeHandle != null) DrawCanvas.Children.Remove(_resizeHandle);
            if (_selected == null) return;

            var dot = new Ellipse
            {
                Width = HANDLE_SIZE,
                Height = HANDLE_SIZE,
                Fill = Brushes.Red,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5,
                IsHitTestVisible = true
            };

            if (_selected is Line l)
            {
                Canvas.SetLeft(dot, l.X2 - HANDLE_SIZE / 2);
                Canvas.SetTop(dot, l.Y2 - HANDLE_SIZE / 2);
            }
            else if (_selected is Rectangle r)
            {
                double left = Canvas.GetLeft(r);
                double top = Canvas.GetTop(r);
                Canvas.SetLeft(dot, left + r.Width - HANDLE_SIZE / 2);
                Canvas.SetTop(dot, top + r.Height - HANDLE_SIZE / 2);
            }
            else if (_selected is Ellipse el)
            {
                double cx = Canvas.GetLeft(el) + el.Width / 2;
                double cy = Canvas.GetTop(el) + el.Height / 2;
                Canvas.SetLeft(dot, cx + el.Width / 2 - HANDLE_SIZE / 2);
                Canvas.SetTop(dot, cy - HANDLE_SIZE / 2);
            }

            _resizeHandle = dot;
            DrawCanvas.Children.Add(dot);
        }

        // --- CLEAR ---
        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            DrawCanvas.Children.Clear();
            _selected = null;
            _resizeHandle = null;
            _isDrawing = _isDragging = _isResizing = false;
        }

        // --- SAVE & LOAD ---
        private record ShapeData(string Type, double X1, double Y1, double X2, double Y2);

        private void SaveCanvas_Click(object sender, RoutedEventArgs e)
        {
            // gather shapes
            var list = new List<ShapeData>();

            foreach (var child in DrawCanvas.Children)
            {
                if (child is Line line)
                {
                    list.Add(new ShapeData("Line", line.X1, line.Y1, line.X2, line.Y2));
                }
                else if (child is Rectangle rect)
                {
                    double left = Canvas.GetLeft(rect);
                    double top = Canvas.GetTop(rect);
                    list.Add(new ShapeData("Rect", left, top, left + rect.Width, top + rect.Height));
                }
                else if (child is Ellipse ellipse)
                {
                    double cx = Canvas.GetLeft(ellipse) + ellipse.Width / 2;
                    double cy = Canvas.GetTop(ellipse) + ellipse.Height / 2;
                    double r = ellipse.Width / 2;
                    list.Add(new ShapeData("Ellipse", cx, cy, r, 0));
                }
            }

            // save
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = "shapes.json",
                Title = "Save drawing"
            };

            if (dialog.ShowDialog() == true)
            {
                string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dialog.FileName, json);
                MessageBox.Show($"Saved {list.Count} shapes to:\n{dialog.FileName}", "Saved");
            }
        }
        private void LoadCanvas_Click(object sender, RoutedEventArgs e)
        {
            // load file
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Load drawing"
            };

            if (dialog.ShowDialog() != true)
                return;

            var json = File.ReadAllText(dialog.FileName);
            var list = JsonSerializer.Deserialize<List<ShapeData>>(json);
            if (list == null)
            {
                MessageBox.Show("Invalid or empty file.", "Error");
                return;
            }

            // draw shapes
            DrawCanvas.Children.Clear();

            foreach (var s in list)
            {
                switch (s.Type)
                {
                    case "Line":
                        var l = new Line
                        {
                            X1 = s.X1,
                            Y1 = s.Y1,
                            X2 = s.X2,
                            Y2 = s.Y2,
                            Stroke = Brushes.Black,
                            StrokeThickness = 2
                        };
                        DrawCanvas.Children.Add(l);
                        break;

                    case "Rect":
                        var r = new Rectangle
                        {
                            Width = Math.Abs(s.X2 - s.X1),
                            Height = Math.Abs(s.Y2 - s.Y1),
                            Stroke = Brushes.Black,
                            StrokeThickness = 2,
                            Fill = Brushes.Transparent
                        };
                        Canvas.SetLeft(r, Math.Min(s.X1, s.X2));
                        Canvas.SetTop(r, Math.Min(s.Y1, s.Y2));
                        DrawCanvas.Children.Add(r);
                        break;

                    case "Ellipse":
                        var el = new Ellipse
                        {
                            Width = 2 * s.X2,
                            Height = 2 * s.X2,
                            Stroke = Brushes.Black,
                            StrokeThickness = 2,
                            Fill = Brushes.Transparent
                        };
                        Canvas.SetLeft(el, s.X1 - s.X2);
                        Canvas.SetTop(el, s.Y1 - s.X2);
                        DrawCanvas.Children.Add(el);
                        break;
                }
            }
        }
    }
}
