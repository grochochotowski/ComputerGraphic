using System;
using System.Windows;
using System.Windows.Media.Imaging;
using GK.Lab4.Helpers;

namespace GK.Lab4.PointTransformations
{
    public static class BrightnessAdjustment
    {
        // Adjusts the brightness of the image by adding the specified level to each color channel.
        public static void Apply(WriteableBitmap wb, double level)
        {
            if (wb == null) return;

            int width = wb.PixelWidth;
            int height = wb.PixelHeight;
            int stride = wb.BackBufferStride;

            try
            {
                wb.Lock();
                IntPtr pBackBuffer = wb.BackBuffer;

                unsafe
                {
                    byte* pPixel = (byte*)pBackBuffer.ToPointer();

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int offset = y * stride + x * 4;

                            byte B = pPixel[offset];
                            byte G = pPixel[offset + 1];
                            byte R = pPixel[offset + 2];

                            // Apply brightness adjustment
                            double r_new = R + level;
                            double g_new = G + level;
                            double b_new = B + level;

                            // Clamp the values to [0, 255]
                            pPixel[offset] = ColorUtils.Clamp(b_new);
                            pPixel[offset + 1] = ColorUtils.Clamp(g_new);
                            pPixel[offset + 2] = ColorUtils.Clamp(r_new);

                            wb.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                        }
                    }
                }
            }
            finally
            {
                wb.Unlock();
            }
        }
    }
}
