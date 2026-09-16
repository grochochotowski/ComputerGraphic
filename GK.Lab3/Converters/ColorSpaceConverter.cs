namespace GK.Lab3.Logic
{
    public static class ColorSpaceConverter
    {
        // RGB -> CMYK
        public static void RgbToCmyk(int r, int g, int b,
                                     out double c, out double m, out double y, out double k)
        {
            double R = r / 255.0;
            double G = g / 255.0;
            double B = b / 255.0;

            k = 1 - Math.Max(R, Math.Max(G, B));

            if (k >= 1.0)
            {
                c = 0; m = 0; y = 0;
                return;
            }

            double denom = (1 - k);

            c = (1 - R - k) / denom;
            m = (1 - G - k) / denom;
            y = (1 - B - k) / denom;
        }

        // CMYK -> RGB
        public static void CmykToRgb(double c, double m, double y, double k,
                                     out int r, out int g, out int b)
        {
            r = (int)(255 * (1 - c) * (1 - k));
            g = (int)(255 * (1 - m) * (1 - k));
            b = (int)(255 * (1 - y) * (1 - k));

            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);
        }
    }
}
