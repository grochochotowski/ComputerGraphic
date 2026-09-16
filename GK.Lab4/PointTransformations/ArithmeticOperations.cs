using System;
using System.Windows;
using System.Windows.Media.Imaging;
using GK.Lab4.Helpers;

namespace GK.Lab4.PointTransformations
{
    public static class ArithmeticOperations
    {
        // Applies arithmetic operation (Add, Sub, Mul, Div).
        public static void Apply(WriteableBitmap wb, string operation, double value)
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

                            // Apply the specific transformation
                            (byte R_new, byte G_new, byte B_new) = ApplyOperation(R, G, B, operation, value);

                            // Write back the new values (A channel remains untouched)
                            pPixel[offset] = B_new;
                            pPixel[offset + 1] = G_new;
                            pPixel[offset + 2] = R_new;

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

        // Helper method to apply the arithmetic operation
        private static (byte R, byte G, byte B) ApplyOperation(byte R, byte G, byte B, string operation, double value)
        {
            double r_new = R, g_new = G, b_new = B;

            switch (operation)
            {
                case "Add":
                    r_new += value;
                    g_new += value;
                    b_new += value;
                    break;

                case "Sub":
                    r_new -= value;
                    g_new -= value;
                    b_new -= value;
                    break;

                case "Mul":
                    r_new *= value;
                    g_new *= value;
                    b_new *= value;
                    break;

                case "Div":
                    if (value != 0)
                    {
                        r_new /= value;
                        g_new /= value;
                        b_new /= value;
                    }
                    break;

                default:
                    return (R, G, B);
            }

            return (ColorUtils.Clamp(r_new), ColorUtils.Clamp(g_new), ColorUtils.Clamp(b_new));
        }
    }
}
