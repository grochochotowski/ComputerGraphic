using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GK.Lab5.Helpers;
using GK.Lab5.HistogramOperations;
using GK.Lab5.Binarization;

namespace GK.Lab5
{
    public partial class MainWindow : Window
    {
        // Transform and navigation
        private Point origin;           // Original offset before drag
        private Point start;            // Mouse position at start of drag
        private double scale;           // Current zoom scale
        private string? lastLoadedPath; // Path to last loaded file

        public MainWindow()
        {
            InitializeComponent();
            lastLoadedPath = null;
            
            // Set default selections
            StretchingRadioButton.IsChecked = true;
            ManualRadioButton.IsChecked = true;
            
            scale = 1.0;
            UpdateScaleTextBlock();
        }

        // Load image
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Load Image",
                InitialDirectory = lastLoadedPath == null
                    ? Directory.GetCurrentDirectory()
                    : Path.GetDirectoryName(lastLoadedPath) ?? Directory.GetCurrentDirectory(),
                Filter = "Image files|*.ppm;*.jpeg;*.jpg",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() != true) return;

            string path = dialog.FileName;
            lastLoadedPath = path;
            LoadFile(path);
            ResetTransform();
        }

        // Load last image
        private void LoadLast_Click(object sender, RoutedEventArgs e)
        {
            if (lastLoadedPath != null)
            {
                LoadFile(lastLoadedPath);
            }
        }

        // Load image from file
        private void LoadFile(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show($"File {path} does not exist.");
                return;
            }

            using FileStream fileStream = new FileStream(path, FileMode.Open,
                FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
            
            string extension = Path.GetExtension(path).ToLower();
            string errorMessage = $"Error loading file {path}.";

            if (extension == ".ppm")
            {
                WriteableBitmap? bitmap = ImageLoader.LoadPpmImage(path);
                if (bitmap == null)
                {
                    MessageBox.Show(errorMessage);
                }
                else
                {
                    SetImageSource(bitmap);
                }
            }
            else if (extension == ".jpg" || extension == ".jpeg")
            {
                try
                {
                    var decoder = new JpegBitmapDecoder(fileStream,
                        BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                    var frame = decoder.Frames[0];
                    int width = frame.PixelWidth;
                    int height = frame.PixelHeight;

                    var bitmap = new WriteableBitmap(width, height, 96, 96,
                        PixelFormats.Bgra32, null);

                    var rect = new Int32Rect(0, 0, width, height);
                    int stride = width * frame.Format.BitsPerPixel / 8;
                    int size = stride * height;
                    frame.CopyPixels(rect, bitmap.BackBuffer, size, stride);

                    bitmap.Lock();
                    bitmap.AddDirtyRect(rect);
                    bitmap.Unlock();

                    SetImageSource(bitmap);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{errorMessage} ({ex.Message})");
                }
            }
            else
            {
                MessageBox.Show("Unsupported format.");
            }
        }

        // Set image source and adjust size
        private void SetImageSource(WriteableBitmap bitmap)
        {
            int bitmapWidth = bitmap.PixelWidth;
            int bitmapHeight = bitmap.PixelHeight;

            double imageWidth, imageHeight;
            if (bitmapWidth > Image.MaxWidth)
            {
                imageWidth = Image.MaxWidth;
                imageHeight = (imageWidth * bitmapHeight) / bitmapWidth;

                if (imageHeight > Image.MaxHeight)
                {
                    imageHeight = Image.MaxHeight;
                    imageWidth = (imageHeight * bitmapWidth) / bitmapHeight;
                }
            }
            else
            {
                imageWidth = bitmapWidth;
                imageHeight = (imageWidth * bitmapHeight) / bitmapWidth;

                if (imageHeight > Image.MaxHeight)
                {
                    imageHeight = Image.MaxHeight;
                    imageWidth = (imageHeight * bitmapWidth) / bitmapHeight;
                }
            }

            Image.Width = imageWidth;
            Image.Height = imageHeight;
            Image.Source = bitmap;
        }

        // Handle zooming
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point position = e.GetPosition(Image);
            Matrix matrix = Image.RenderTransform.Value;

            if (e.Delta > 0)
            {
                // Zoom in
                matrix.ScaleAtPrepend(1.1, 1.1, position.X, position.Y);
                scale *= 1.1;
            }
            else
            {
                // Zoom out
                matrix.ScaleAtPrepend(1 / 1.1, 1 / 1.1, position.X, position.Y);
                scale /= 1.1;
            }

            UpdateScaleTextBlock();
            Image.RenderTransform = new MatrixTransform(matrix);
        }

        // Reset transform on middle mouse button click
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle) return;

            ResetTransform();
        }

        // Reset image transform
        private void ResetTransform()
        {
            Image.RenderTransform = Transform.Identity;
            scale = 1;
            UpdateScaleTextBlock();
        }

        // Handle start of dragging
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Image.IsMouseCaptured) return;

            Image.CaptureMouse();
            start = e.GetPosition(Border);
            Matrix matrix = Image.RenderTransform.Value;
            origin.X = matrix.OffsetX;
            origin.Y = matrix.OffsetY;
        }

        // Handle end of dragging
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image.ReleaseMouseCapture();
        }

        // Show pixel info and handle dragging
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            Point mouseRelativeToImage = e.GetPosition(Image);
            var imageSource = Image.Source as BitmapSource;

            if (imageSource != null)
            {
                int width = imageSource.PixelWidth;
                int height = imageSource.PixelHeight;

                int pixelX = (int)((mouseRelativeToImage.X * width) / Image.Width);
                int pixelY = (int)((mouseRelativeToImage.Y * height) / Image.Height);

                if (pixelX >= 0 && pixelX < width && pixelY >= 0 && pixelY < height)
                {
                    XTextBox.Text = pixelX.ToString();
                    YTextBox.Text = pixelY.ToString();

                    const int bytesPerPixel = 4;
                    byte[] bytes = new byte[bytesPerPixel];
                    imageSource.CopyPixels(new Int32Rect(pixelX, pixelY, 1, 1),
                        bytes, bytesPerPixel, 0);

                    BTextBox.Text = bytes[0].ToString();
                    GTextBox.Text = bytes[1].ToString();
                    RTextBox.Text = bytes[2].ToString();
                }
            }

            if (!Image.IsMouseCaptured) return;

            Point currentPosition = e.GetPosition(Border);
            Matrix matrix = Image.RenderTransform.Value;
            matrix.OffsetX = origin.X + (currentPosition.X - start.X);
            matrix.OffsetY = origin.Y + (currentPosition.Y - start.Y);
            Image.RenderTransform = new MatrixTransform(matrix);
        }

        // Update scale display
        private void UpdateScaleTextBlock()
        {
            ScaleTextBlock.Text = ((int)Math.Truncate(scale * 100)).ToString() + "%";
        }

        // Perform normalization
        private void PerformNormalization_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = Image.Source as WriteableBitmap;
            if (bitmap == null) return;

            if (StretchingRadioButton.IsChecked == true)
            {
                HistogramStretching.Apply(bitmap);
            }
            else if (EqualizationRadioButton.IsChecked == true)
            {
                HistogramEqualization.Apply(bitmap);
            }
        }

        // Handle binarization option changes
        private void Bin_Checked(object sender, RoutedEventArgs e)
        {
            ThresholdLabel.Visibility = Visibility.Collapsed;
            ThresholdTextBox.Visibility = Visibility.Collapsed;

            if (sender == ManualRadioButton)
            {
                ThresholdLabel.Visibility = Visibility.Visible;
                ThresholdLabel.Content = "Threshold";
                ThresholdTextBox.Visibility = Visibility.Visible;
                return;
            }

            if (sender == PercentBlackSelectionRadioButton)
            {
                ThresholdLabel.Visibility = Visibility.Visible;
                ThresholdLabel.Content = "Percent of Black Pixels";
                ThresholdTextBox.Visibility = Visibility.Visible;
                return;
            }
        }

        // Perform binarization
        private void PerformBinarization_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = Image.Source as WriteableBitmap;
            if (bitmap == null) return;

            if (MeanIterativeSelectionRadioButton.IsChecked == true)
            {
                MeanIterativeSelection.Apply(bitmap);
            }
            else if (EntropySelectionRadioButton.IsChecked == true)
            {
                EntropySelection.Apply(bitmap);
            }
            else if (MinimumErrorRadioButton.IsChecked == true)
            {
                MinimumError.Apply(bitmap);
            }
            else if (FuzzyMinimumErrorRadioButton.IsChecked == true)
            {
                FuzzyMinimumError.Apply(bitmap);
            }
            else if (ManualRadioButton.IsChecked == true)
            {
                if (!int.TryParse(ThresholdTextBox.Text, out int threshold))
                {
                    MessageBox.Show("Please enter a threshold value (integer).");
                    return;
                }
                ManualThreshold.Apply(bitmap, threshold);
            }
            else if (PercentBlackSelectionRadioButton.IsChecked == true)
            {
                if (!double.TryParse(ThresholdTextBox.Text, out double percent) 
                    || percent < 0 || percent > 1)
                {
                    MessageBox.Show("Please enter a percent value between 0 and 1.");
                    return;
                }
                PercentBlackSelection.Apply(bitmap, percent);
            }
        }
    }
}
