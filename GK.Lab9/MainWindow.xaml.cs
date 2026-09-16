using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GK.Lab9
{
    public partial class MainWindow : Window
    {
        private WriteableBitmap? originalImage;
        private readonly DispatcherTimer debounceTimer;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize debounce timer
            debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            debounceTimer.Tick += DebounceTimer_Tick;

            // Set initial swatch colors
            Loaded += (s, e) => UpdateAllSwatches();
        }

        // --- Slider Event Handlers ---
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (originalImage == null) return;

            UpdateAllSwatches();
            
            // Debounce the live preview update
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        // --- Swatch Updates ---
        private void UpdateAllSwatches()
        {
            if (HueMinSlider == null) return; // Guard clause for startup

            // The main hue for saturation and value gradients, using HueMin for consistency
            double mainHue = HueMinSlider.Value;

            // --- Hue Sliders ---
            var hueGradient = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 0.5) };
            for (int i = 0; i <= 360; i += 60) // Add stops for a full spectrum
            {
                hueGradient.GradientStops.Add(new GradientStop(HsvToRgb(i, 100, 100), i / 360.0));
            }
            HueMinSlider.Background = hueGradient;
            HueMaxSlider.Background = hueGradient;

            // --- Saturation Sliders ---
            var satGradient = new LinearGradientBrush(
                HsvToRgb(mainHue, 0, 100), // Desaturated (grayish) version of the hue
                HsvToRgb(mainHue, 100, 100), // Fully saturated version of the hue
                new Point(0, 0.5), new Point(1, 0.5)
            );
            SaturationMinSlider.Background = satGradient;
            SaturationMaxSlider.Background = satGradient;

            // --- Value Sliders ---
            var valGradient = new LinearGradientBrush(
                HsvToRgb(mainHue, 100, 0), // Dark version of the hue (black)
                HsvToRgb(mainHue, 100, 100), // Bright version of the hue
                new Point(0, 0.5), new Point(1, 0.5)
            );
            ValueMinSlider.Background = valGradient;
            ValueMaxSlider.Background = valGradient;
        }

        // --- Image Loading and Analysis ---
        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*",
                InitialDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\img"))
            };

            if (openFileDialog.ShowDialog() != true) return;
            
            try
            {
                var bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
                originalImage = new WriteableBitmap(new FormatConvertedBitmap(bitmapImage, PixelFormats.Bgra32, null, 0));
                
                // Display the original image initially
                DisplayImage.Source = originalImage;
                
                // Perform initial analysis
                PerformAnalysis(isLivePreview: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                originalImage = null;
                DisplayImage.Source = null;
                ResultTextBlock.Text = "Failed to load image.";
            }
        }

        // --- Analysis Logic ---
        private void DebounceTimer_Tick(object? sender, EventArgs e)
        {
            debounceTimer.Stop();
            PerformAnalysis(isLivePreview: true);
        }

        // --- Analyze Button Click ---
        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            PerformAnalysis(isLivePreview: false);
        }

        // --- Core Analysis Method ---
        private void PerformAnalysis(bool isLivePreview)
        {
            if (originalImage == null)
            {
                if (!isLivePreview) ResultTextBlock.Text = "Please load an image first.";
                return;
            }
            
            int width = originalImage.PixelWidth;
            int height = originalImage.PixelHeight;
            long totalPixels = (long)width * height;
            long matchCount = 0;
            
            var previewBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            
            originalImage.Lock();
            previewBitmap.Lock();

            try
            {
                unsafe
                {
                    byte* pSrc = (byte*)originalImage.BackBuffer;
                    byte* pDst = (byte*)previewBitmap.BackBuffer;
                    int stride = originalImage.BackBufferStride;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int pixelOffset = y * stride + x * 4;
                            byte b = pSrc[pixelOffset];
                            byte g = pSrc[pixelOffset + 1];
                            byte r = pSrc[pixelOffset + 2];

                            RgbToHsv(r, g, b, out double h, out double s, out double v);

                            if (IsHsvMatch(h, s, v))
                            {
                                matchCount++;
                                // Highlight matching pixels in green
                                pDst[pixelOffset] = 0;   // B
                                pDst[pixelOffset + 1] = 255; // G
                                pDst[pixelOffset + 2] = 0;   // R
                                pDst[pixelOffset + 3] = 255; // A
                            }
                            else
                            {
                                // Show non-matching pixels as grayscale
                                byte gray = (byte)(r * 0.3 + g * 0.59 + b * 0.11);
                                pDst[pixelOffset] = gray;
                                pDst[pixelOffset + 1] = gray;
                                pDst[pixelOffset + 2] = gray;
                                pDst[pixelOffset + 3] = 255;
                            }
                        }
                    }
                }
            }
            finally
            {
                originalImage.Unlock();
                previewBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                previewBitmap.Unlock();
            }
            
            DisplayImage.Source = previewBitmap;

            if (!isLivePreview)
            {
                double percentage = totalPixels > 0 ? (double)matchCount / totalPixels * 100.0 : 0.0;
                ResultTextBlock.Text = $"Selected color pixels: {percentage:F2}%";
            }
        }

        // --- Color Matching Logic ---
        private bool IsHsvMatch(double h, double s, double v)
        {
            double hueMin = HueMinSlider.Value;
            double hueMax = HueMaxSlider.Value;
            bool hueMatch;
            if (hueMin <= hueMax)
            {
                hueMatch = (h >= hueMin && h <= hueMax);
            }
            else
            {
                hueMatch = (h >= hueMin || h <= hueMax);
            }
            
            bool saturationMatch = (s >= SaturationMinSlider.Value && s <= SaturationMaxSlider.Value);
            bool valueMatch = (v >= ValueMinSlider.Value && v <= ValueMaxSlider.Value);

            return hueMatch && saturationMatch && valueMatch;
        }

        // --- Color Conversion Methods ---
        private static void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v)
        {
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;

            double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
            double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
            
            v = max;
            double delta = max - min;

            if (delta == 0) { h = 0; s = 0; }
            else
            {
                s = delta / max;
                if (max == rNorm) h = (gNorm - bNorm) / delta;
                else if (max == gNorm) h = (bNorm - rNorm) / delta + 2;
                else h = (rNorm - gNorm) / delta + 4;
                h *= 60;
                if (h < 0) h += 360;
            }
            s *= 100;
            v *= 100;
        }

        // HSV to RGB conversion
        private static Color HsvToRgb(double h, double s, double v)
        {
            double r = 0, g = 0, b = 0;
            double hNorm = h / 360.0;
            double sNorm = s / 100.0;
            double vNorm = v / 100.0;

            if (sNorm == 0) { r = vNorm; g = vNorm; b = vNorm; }
            else
            {
                int i = (int)Math.Floor(hNorm * 6);
                double f = hNorm * 6 - i;
                double p = vNorm * (1 - sNorm);
                double q = vNorm * (1 - f * sNorm);
                double t = vNorm * (1 - (1 - f) * sNorm);

                switch (i % 6)
                {
                    case 0: r = vNorm; g = t; b = p; break;
                    case 1: r = q; g = vNorm; b = p; break;
                    case 2: r = p; g = vNorm; b = t; break;
                    case 3: r = p; g = q; b = vNorm; break;
                    case 4: r = t; g = p; b = vNorm; break;
                    case 5: r = vNorm; g = p; b = q; break;
                }
            }
            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }
    }
}