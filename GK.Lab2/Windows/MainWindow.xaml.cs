using GK.Lab2.Decoders;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GK.Lab2.Windows
{
    public partial class MainWindow : Window
    {
        // --- FIELDS ---
        private Point dragStart;
        private Point origin;
        private double scaleFactor = 1.0;
        private string initialDirectory;


        // --- CONSTRUCTOR ---
        public MainWindow()
        {
            InitializeComponent();
            initialDirectory = Directory.GetCurrentDirectory();
        }


        // --- OPEN FILE ---
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Open Image";
            dialog.InitialDirectory = initialDirectory;
            dialog.Filter = "Images (*.ppm;*.jpg;*.jpeg)|*.ppm;*.jpg;*.jpeg";

            if (dialog.ShowDialog() != true)
                return;

            string path = dialog.FileName;
            initialDirectory = Path.GetDirectoryName(path);

            using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BitmapSource bmp = null;

            try
            {
                string ext = Path.GetExtension(path).ToLower();

                // ppm or jpeg
                if (ext == ".ppm")
                    bmp = new PpmDecoder(fs).Decode();
                else
                    bmp = new JpegBitmapDecoder(fs,
                                                BitmapCreateOptions.PreservePixelFormat,
                                                BitmapCacheOption.OnLoad).Frames[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading file: " + ex.Message);
                return;
            }

            SetImage(bmp);
        }


        // --- SET IMAGE ---
        private void SetImage(BitmapSource bmp)
        {
            // size
            Image.Width = bmp.PixelWidth;
            Image.Height = bmp.PixelHeight;

            // assign
            Image.Source = bmp;

            // reset transform
            Image.RenderTransform = Transform.Identity;
            scaleFactor = 1.0;

            // ui
            PlaceholderText.Visibility = Visibility.Collapsed;
            ImageScroll.Visibility = Visibility.Visible;

            UpdateScaleText();
        }


        // --- SAVE AS JPEG ---
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (Image.Source == null)
            {
                MessageBox.Show("No image loaded.");
                return;
            }

            var win = new SaveWindow(initialDirectory, (BitmapSource)Image.Source);
            win.Owner = this;
            win.ShowDialog();

            initialDirectory = win.InitialDirectory;
        }


        // --- ZOOM (SCROLLVIEWER) ---
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Window_MouseWheel(sender, e);
            e.Handled = true;
        }


        // --- ZOOM (WHEEL) ---
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Image.Source == null) return;

            // factor
            double zoom = e.Delta > 0 ? 1.1 : 1 / 1.1;

            // pivot
            Point p = e.GetPosition(Image);

            // matrix
            Matrix m = Image.RenderTransform.Value;
            m.ScaleAtPrepend(zoom, zoom, p.X, p.Y);

            Image.RenderTransform = new MatrixTransform(m);

            scaleFactor *= zoom;
            UpdateScaleText();
        }


        // --- IMAGE MOVE ---
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            // not dragging = pixel inspector
            if (!Image.IsMouseCaptured)
            {
                UpdatePixelInfo(e);
                return;
            }

            // dragging
            Point currentPos = e.GetPosition(this);
            Matrix m = Image.RenderTransform.Value;

            m.OffsetX = origin.X + (currentPos.X - dragStart.X);
            m.OffsetY = origin.Y + (currentPos.Y - dragStart.Y);

            Image.RenderTransform = new MatrixTransform(m);
        }


        // --- PIXEL INFO ---
        private void UpdatePixelInfo(MouseEventArgs e)
        {
            if (Image.Source == null) return;

            BitmapSource src = (BitmapSource)Image.Source;
            Point imgPos = e.GetPosition(Image);

            int px = (int)imgPos.X;
            int py = (int)imgPos.Y;

            // bounds
            if (px >= 0 && px < src.PixelWidth &&
                py >= 0 && py < src.PixelHeight)
            {
                XTextBox.Text = px.ToString();
                YTextBox.Text = py.ToString();

                // pixel
                byte[] pxBytes = new byte[4];
                src.CopyPixels(new Int32Rect(px, py, 1, 1), pxBytes, 4, 0);

                BTextBox.Text = pxBytes[0].ToString();
                GTextBox.Text = pxBytes[1].ToString();
                RTextBox.Text = pxBytes[2].ToString();
            }
        }


        // --- PAN START ---
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Image.Source == null) return;

            Image.CaptureMouse();
            dragStart = e.GetPosition(this);

            // origin
            Matrix m = Image.RenderTransform.Value;
            origin = new Point(m.OffsetX, m.OffsetY);
        }


        // --- PAN STOP ---
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image.ReleaseMouseCapture();
        }


        // --- RESET VIEW ---
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle) return;

            Image.RenderTransform = Transform.Identity;
            scaleFactor = 1.0;
            UpdateScaleText();
        }


        // --- SCALE TEXT ---
        private void UpdateScaleText()
        {
            ScaleTextBlock.Text = $"{(int)(scaleFactor * 100)}%";
        }
    }
}
