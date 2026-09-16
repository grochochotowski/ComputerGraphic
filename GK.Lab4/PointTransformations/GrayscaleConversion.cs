using System;
using System.Windows;
using System.Windows.Media.Imaging;
using GK.Lab4.Helpers;

namespace GK.Lab4.PointTransformations
{
    public static class GrayscaleConversion
    {
        // Average method  - weights all color channels equally.
        public static void ApplyAverage(WriteableBitmap wb)
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

                            // Apply Average Method
                            int gray = (R + G + B) / 3;
                            byte l = (byte)gray;

                            // Set R, G, B to the same Luminosity value (A channel remains untouched)
                            pPixel[offset] = l;
                            pPixel[offset + 1] = l;
                            pPixel[offset + 2] = l;

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

        // Weighted method - using weighted luminosity method (Rec. 601 standard)
        public static void ApplyWeighted(WriteableBitmap wb)
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

                            // Apply Weighted Method
                            double gray = 0.299 * R + 0.587 * G + 0.114 * B;
                            byte l = ColorUtils.Clamp(gray);

                            // Set R, G, B to the same Luminosity value (A channel remains untouched)
                            pPixel[offset] = l;
                            pPixel[offset + 1] = l;
                            pPixel[offset + 2] = l;

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
