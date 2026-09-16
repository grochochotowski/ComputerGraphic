using System.Windows;
using System.Windows.Controls;
using GK.Lab4.Helpers;
using GK.Lab4.PointTransformations;
using GK.Lab4.ImageEnhancement;

namespace GK.Lab4
{
    public partial class MainWindow : Window
    {
        // Manages the current image state (WriteableBitmap and pixel data)
        private readonly PixelData _pixelData = new PixelData();

        public MainWindow()
        {
            InitializeComponent();
        }

        //--------------------------------------------------------------------------------
        // IMAGE LOADING
        //--------------------------------------------------------------------------------
        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = ImageLoader.LoadFromDialog();
            if (bitmap != null)
            {
                _pixelData.LoadImage(bitmap);
                DisplayImage.Source = _pixelData.CurrentWb;
            }
        }

        //--------------------------------------------------------------------------------
        // POINT TRANSFORMATIONS
        //--------------------------------------------------------------------------------
        private void ArithmeticOperation_Click(object sender, RoutedEventArgs e)
        {
            if (!_pixelData.HasImage) return;
            if (!double.TryParse(ValueInput.Text, out double value)) return;

            string? op = (sender as Button)?.Tag?.ToString();
            if (op == null) return;

            ArithmeticOperations.Apply(_pixelData.CurrentWb!, op, value);
            _pixelData.SyncPixelData();
        }

        // Brightness
        private void Brightness_Click(object sender, RoutedEventArgs e)
        {
            if (!_pixelData.HasImage) return;
            if (!double.TryParse(ValueInput.Text, out double level)) return;

            BrightnessAdjustment.Apply(_pixelData.CurrentWb!, level);
            _pixelData.SyncPixelData();
        }

        // Grayscale - Average Method
        private void GrayscaleAvg_Click(object sender, RoutedEventArgs e)
        {
            if (!_pixelData.HasImage) return;

            GrayscaleConversion.ApplyAverage(_pixelData.CurrentWb!);
            _pixelData.SyncPixelData();
        }

        // Grayscale - Weighted Method
        private void GrayscaleWeighted_Click(object sender, RoutedEventArgs e)
        {
            if (!_pixelData.HasImage) return;

            GrayscaleConversion.ApplyWeighted(_pixelData.CurrentWb!);
            _pixelData.SyncPixelData();
        }

        //--------------------------------------------------------------------------------
        // IMAGE ENHANCEMENT FILTERS (Convolution)
        //--------------------------------------------------------------------------------

        // Applies the selected filter to the current image.
        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            if (!_pixelData.HasImage) return;

            string? filterType = (sender as Button)?.Tag?.ToString();
            if (filterType == null) return;

            int width = _pixelData.CurrentWb!.PixelWidth;
            int height = _pixelData.CurrentWb.PixelHeight;
            int stride = _pixelData.CurrentWb.BackBufferStride;

            byte[]? resultPixels = null;

            switch (filterType)
            {
                case "Average":
                case "Gauss":
                case "Sharpen":
                    resultPixels = ConvolutionFilters.ApplyFilter(_pixelData.Data!, width, height, stride, filterType);
                    break;

                case "Median":
                    resultPixels = MedianFilter.Apply(_pixelData.Data!, width, height, stride);
                    break;

                case "Sobel":
                    resultPixels = SobelEdgeDetection.Apply(_pixelData.Data!, width, height, stride);
                    break;
            }

            if (resultPixels != null)
            {
                _pixelData.UpdateImage(resultPixels);
            }
        }

        //--------------------------------------------------------------------------------
        // CUSTOM EDITOR
        //--------------------------------------------------------------------------------

        // Opens the custom editor dialog.
        private void OpenKernelEditor_Click(object sender, RoutedEventArgs e)
        {
            if (!_pixelData.HasImage)
            {
                MessageBox.Show("Please load an image first.", "No Image Loaded",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Parse and validate height input
            if (!int.TryParse(KernelHeightInput.Text, out int height))
            {
                MessageBox.Show("Invalid kernel height. Please enter a valid integer.", "Invalid Input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Parse and validate width input
            if (!int.TryParse(KernelWidthInput.Text, out int width))
            {
                MessageBox.Show("Invalid kernel width. Please enter a valid integer.", "Invalid Input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate dimensions are odd numbers
            if (height % 2 == 0 || width % 2 == 0)
            {
                MessageBox.Show("Kernel dimensions must be odd numbers (3, 5, 7, 9, 11).",
                    "Invalid Dimensions", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate dimensions are in reasonable range
            if (height < 3 || height > 11 || width < 3 || width > 11)
            {
                MessageBox.Show("Kernel dimensions must be between 3 and 11 (odd numbers only).",
                    "Invalid Dimensions", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Open the kernel editor dialog
            try
            {
                KernelEditorWindow editorWindow = new KernelEditorWindow(height, width, _pixelData)
                {
                    Owner = this
                };

                // Show as modal dialog
                bool? result = editorWindow.ShowDialog();

                // If the user applied the kernel, the image has already been updated
                if (result == true)
                {
                    // Successfully applied kernel
                    MessageBox.Show("Custom kernel applied successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening kernel editor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
