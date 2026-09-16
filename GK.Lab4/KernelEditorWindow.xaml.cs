using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using GK.Lab4.Helpers;
using GK.Lab4.ImageEnhancement;

namespace GK.Lab4
{
    /// <summary>
    /// Interactive dialog window for creating and editing custom convolution kernels.
    /// Dynamically generates a grid of TextBox controls based on user-specified dimensions.
    /// </summary>
    public partial class KernelEditorWindow : Window
    {
        private readonly int _kernelHeight;
        private readonly int _kernelWidth;
        private readonly PixelData _pixelData;
        private TextBox[,] _textBoxes = null!;

        /// <summary>
        /// Initializes a new instance of the KernelEditorWindow.
        /// </summary>
        /// <param name="height">Height of the kernel (must be odd)</param>
        /// <param name="width">Width of the kernel (must be odd)</param>
        /// <param name="pixelData">Reference to the PixelData instance for applying the kernel</param>
        public KernelEditorWindow(int height, int width, PixelData pixelData)
        {
            InitializeComponent();

            _kernelHeight = height;
            _kernelWidth = width;
            _pixelData = pixelData;

            // Update the window title to show dimensions
            Title = $"Kernel Editor - {height}x{width}";

            // Create the dynamic grid of TextBoxes
            CreateKernelGrid();
        }

        /// <summary>
        /// Dynamically creates a grid of TextBox controls based on kernel dimensions.
        /// </summary>
        private void CreateKernelGrid()
        {
            // Clear any existing controls
            KernelGrid.Children.Clear();
            KernelGrid.RowDefinitions.Clear();
            KernelGrid.ColumnDefinitions.Clear();

            // Create row and column definitions
            for (int i = 0; i < _kernelHeight; i++)
            {
                KernelGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int i = 0; i < _kernelWidth; i++)
            {
                KernelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            // Create the TextBox array
            _textBoxes = new TextBox[_kernelHeight, _kernelWidth];

            // Populate the grid with TextBoxes
            for (int row = 0; row < _kernelHeight; row++)
            {
                for (int col = 0; col < _kernelWidth; col++)
                {
                    TextBox textBox = new TextBox
                    {
                        Width = 60,
                        Height = 30,
                        Margin = new Thickness(3),
                        Text = "0",
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        ToolTip = $"Kernel[{row},{col}]"
                    };

                    // Set the grid position
                    Grid.SetRow(textBox, row);
                    Grid.SetColumn(textBox, col);

                    // Add to the grid
                    KernelGrid.Children.Add(textBox);

                    // Store reference in the array
                    _textBoxes[row, col] = textBox;
                }
            }

            // Set center cell to 1 by default for convenience
            if (_kernelHeight > 0 && _kernelWidth > 0)
            {
                int centerRow = _kernelHeight / 2;
                int centerCol = _kernelWidth / 2;
                _textBoxes[centerRow, centerCol].Text = "1";
            }
        }

        /// <summary>
        /// Extracts the kernel values from the TextBox grid.
        /// </summary>
        /// <param name="kernel">Output parameter containing the parsed kernel</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if extraction successful, false otherwise</returns>
        private bool TryExtractKernel(out double[,]? kernel, out string errorMessage)
        {
            kernel = new double[_kernelHeight, _kernelWidth];
            errorMessage = string.Empty;

            for (int row = 0; row < _kernelHeight; row++)
            {
                for (int col = 0; col < _kernelWidth; col++)
                {
                    string text = _textBoxes[row, col].Text.Trim();

                    if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    {
                        errorMessage = $"Invalid value at position [{row},{col}]: '{text}'. Please enter a valid number.";
                        kernel = null;
                        return false;
                    }

                    kernel[row, col] = value;
                }
            }

            return true;
        }

        /// <summary>
        /// Handles the Cancel button click - closes the dialog without applying changes.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the Apply button click - validates and applies the kernel to the image.
        /// </summary>
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate and extract kernel values
            if (!TryExtractKernel(out double[,]? kernel, out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Invalid Kernel Values",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate normalization factor
            if (!double.TryParse(NormalizationFactorInput.Text, NumberStyles.Float,
                CultureInfo.InvariantCulture, out double normalizationFactor))
            {
                MessageBox.Show("Invalid normalization factor. Please enter a valid number.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Math.Abs(normalizationFactor) < 0.0001)
            {
                MessageBox.Show("Normalization factor cannot be zero or near-zero.",
                    "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if image is loaded
            if (!_pixelData.HasImage)
            {
                MessageBox.Show("No image loaded. Please load an image first.",
                    "No Image", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Apply the custom kernel
                int width = _pixelData.CurrentWb!.PixelWidth;
                int height = _pixelData.CurrentWb.PixelHeight;
                int stride = _pixelData.CurrentWb.BackBufferStride;

                byte[] resultPixels = CustomMaskConvolution.ApplyCustomMask(
                    _pixelData.Data!, width, height, stride, kernel!, normalizationFactor);

                _pixelData.UpdateImage(resultPixels);

                // Close the dialog with success
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying custom kernel: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
