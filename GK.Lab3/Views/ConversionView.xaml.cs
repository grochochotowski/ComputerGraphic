using GK.Lab3.Logic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GK.Lab3.Views
{
    public partial class ConversionView : UserControl
    {
        public event Action<Color>? ColorChanged;
        private bool _isUpdating = false;

        public ConversionView()
        {
            InitializeComponent();
            SliderK.Value = 0.5;
            RGB_SliderChanged(null, new RoutedPropertyChangedEventArgs<double>(0, 0)); 
        }


        // ================================================================
        // CHANGING HANDLERS
        // ================================================================

        // --- RGB Sliders ---
        private void RGB_SliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdating) return;

            int r = (int)SliderR.Value;
            int g = (int)SliderG.Value;
            int b = (int)SliderB.Value;

            _isUpdating = true;
            // Ensure Slider is the source of truth for TextBoxes
            BoxR.Text = r.ToString();
            BoxG.Text = g.ToString();
            BoxB.Text = b.ToString();

            // RGB -> CMYK
            ColorSpaceConverter.RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);

            // Update CMYK (Slider and TextBox)
            SliderC.Value = c; BoxC.Text = c.ToString("0.00");
            SliderM.Value = m; BoxM.Text = m.ToString("0.00");
            SliderY.Value = y; BoxY.Text = y.ToString("0.00");
            SliderK.Value = k; BoxK.Text = k.ToString("0.00");
            _isUpdating = false;

            UpdatePreviewColor(r, g, b);
        }

        // --- RGB TextBoxes ---
        private void RGB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            // Try to parse values, using 0 as default
            if (!int.TryParse(BoxR.Text, out int r)) r = 0;
            if (!int.TryParse(BoxG.Text, out int g)) g = 0;
            if (!int.TryParse(BoxB.Text, out int b)) b = 0;

            // Clamp to safe range [0, 255]
            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);

            _isUpdating = true;
            // Update Sliders and TextBoxes with clamped values
            SliderR.Value = r;
            SliderG.Value = g;
            SliderB.Value = b;

            // Enforce text update with clamped values
            BoxR.Text = r.ToString();
            BoxG.Text = g.ToString();
            BoxB.Text = b.ToString();
            BoxR.CaretIndex = BoxR.Text.Length;
            BoxG.CaretIndex = BoxG.Text.Length;
            BoxB.CaretIndex = BoxB.Text.Length;

            // RGB -> CMYK
            ColorSpaceConverter.RgbToCmyk(r, g, b, out double c, out double m, out double y, out double k);

            // Update CMYK (Slider and TextBox)
            SliderC.Value = c; BoxC.Text = c.ToString("0.00");
            SliderM.Value = m; BoxM.Text = m.ToString("0.00");
            SliderY.Value = y; BoxY.Text = y.ToString("0.00");
            SliderK.Value = k; BoxK.Text = k.ToString("0.00");
            _isUpdating = false;

            UpdatePreviewColor(r, g, b);
        }

        // --- CMYK Sliders ---
        private void CMYK_SliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdating) return;

            double c = SliderC.Value;
            double m = SliderM.Value;
            double y = SliderY.Value;
            double k = SliderK.Value;

            _isUpdating = true;
            // Ensure Slider is the source of truth for TextBoxes
            BoxC.Text = c.ToString("0.00");
            BoxM.Text = m.ToString("0.00");
            BoxY.Text = y.ToString("0.00");
            BoxK.Text = k.ToString("0.00");
            _isUpdating = false;

            // CMYK -> RGB
            ColorSpaceConverter.CmykToRgb(c, m, y, k, out int r, out int g, out int b);

            // Update RGB (Slider and TextBox)
            _isUpdating = true;
            SliderR.Value = r; BoxR.Text = r.ToString();
            SliderG.Value = g; BoxG.Text = g.ToString();
            SliderB.Value = b; BoxB.Text = b.ToString();
            _isUpdating = false;

            UpdatePreviewColor(r, g, b);
        }

        // --- CMYK TextBoxes ---
        private void CMYK_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;
        }

        private void CMYK_LostFocus(object sender, RoutedEventArgs e)
        {
            double tempC = 0, tempM = 0, tempY = 0, tempK = 0;

            // Try to parse values, using 0 as default
            if (!double.TryParse(BoxC.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tempC)) tempC = 0;
            if (!double.TryParse(BoxM.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tempM)) tempM = 0;
            if (!double.TryParse(BoxY.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tempY)) tempY = 0;
            if (!double.TryParse(BoxK.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out tempK)) tempK = 0;

            // Clamp to safe range [0, 1]
            double c = Math.Clamp(tempC, 0, 1);
            double m = Math.Clamp(tempM, 0, 1);
            double y = Math.Clamp(tempY, 0, 1);
            double k = Math.Clamp(tempK, 0, 1);

            _isUpdating = true;

            // Update Sliders and TextBoxes with clamped values
            BoxC.Text = c.ToString("0.00");
            BoxM.Text = m.ToString("0.00");
            BoxY.Text = y.ToString("0.00");
            BoxK.Text = k.ToString("0.00");

            SliderC.Value = c;
            SliderM.Value = m;
            SliderY.Value = y;
            SliderK.Value = k;

            // CMYK -> RGB
            ColorSpaceConverter.CmykToRgb(c, m, y, k, out int r, out int g, out int b);

            // Update RGB (Slider and TextBox)
            SliderR.Value = r; BoxR.Text = r.ToString();
            SliderG.Value = g; BoxG.Text = g.ToString();
            SliderB.Value = b; BoxB.Text = b.ToString();

            // Set carter at the end
            BoxC.CaretIndex = BoxC.Text.Length;
            BoxM.CaretIndex = BoxM.Text.Length;
            BoxY.CaretIndex = BoxY.Text.Length;
            BoxK.CaretIndex = BoxK.Text.Length;

            _isUpdating = false;

            UpdatePreviewColor(r, g, b);
        }


        // ================================================================
        // UPDATE COLOR PREVIEW
        // ================================================================
        private void UpdatePreviewColor(int r, int g, int b)
        {
            var color = Color.FromRgb((byte)r, (byte)g, (byte)b);
            ColorPreview.Background = new SolidColorBrush(color);
            ColorChanged?.Invoke(color);
        }
    }
}