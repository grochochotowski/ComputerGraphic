using System;

namespace GK.Lab4.Helpers
{
    public static class ColorUtils
    {
        // Clamps a double value to the byte range [0, 255] and rounds it.
        public static byte Clamp(double value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)Math.Round(value);
        }
    }
}
