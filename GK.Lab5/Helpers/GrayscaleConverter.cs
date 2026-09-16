using System;
using System.Windows.Media.Imaging;

namespace GK.Lab5.Helpers
{
    public static class GrayscaleConverter
    {
        // Uses V (Value) from HSV = max(R, G, B)
        public static byte[] ToGrayscaleBuffer(WriteableBitmap bitmap)
        {
            // Ensure the bitmap is in BGRA32 format
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            byte[] buffer = new byte[width * height];
            int index = 0;

            // Lock the bitmap's back buffer for reading
            unsafe
            {
                int* pixelPtr = (int*)bitmap.BackBuffer;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int argb = *pixelPtr;

                        byte r = (byte)((argb >> 16) & 255);
                        byte g = (byte)((argb >> 8) & 255);
                        byte b = (byte)(argb & 255);

                        byte max = r > g ? r : g;
                        if (b > max) max = b;

                        buffer[index++] = max;
                        pixelPtr++;
                    }
                }
            }

            return buffer;
        }

        // Computes histogram of grayscale values
        public static int[] ComputeHistogram(byte[] grayscaleBuffer, int width, int height)
        {
            int[] histogram = new int[256];
            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    histogram[grayscaleBuffer[index++]]++;
                }
            }

            return histogram;
        }
    }
}
