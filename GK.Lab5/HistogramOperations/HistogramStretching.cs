using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GK.Lab5.HistogramOperations
{
    public static class HistogramStretching
    {
        public static bool Apply(WriteableBitmap bitmap)
        {
            // Get min and max for each channel
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            bitmap.Lock();

            unsafe
            {
                // Initialize min and max with the first pixel
                int* pixelPtr = (int*)bitmap.BackBuffer;

                // Find min and max values for each channel
                byte rMin = (byte)(*pixelPtr >> 16);
                byte gMin = (byte)(*pixelPtr >> 8);
                byte bMin = (byte)(*pixelPtr);
                byte rMax = rMin, gMax = gMin, bMax = bMin;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte r = (byte)(*pixelPtr >> 16);
                        byte g = (byte)(*pixelPtr >> 8);
                        byte b = (byte)(*pixelPtr);

                        if (r < rMin) rMin = r;
                        else if (r > rMax) rMax = r;

                        if (g < gMin) gMin = g;
                        else if (g > gMax) gMax = g;

                        if (b < bMin) bMin = b;
                        else if (b > bMax) bMax = b;

                        pixelPtr++;
                    }
                }

                // Reset pointer to the beginning
                pixelPtr = (int*)bitmap.BackBuffer;

                // Calculate scaling factors
                int rDiv = rMax - rMin;
                int gDiv = gMax - gMin;
                int bDiv = bMax - bMin;

                // Check for channels with no variation
                const string errorMessage = " channel has the same value in all pixels.";
                if (rDiv == 0) // No variation in red channel
                {
                    bitmap.Unlock();
                    MessageBox.Show("Red" + errorMessage, "Cannot Stretch", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (gDiv == 0) // No variation in green channel
                {
                    bitmap.Unlock();
                    MessageBox.Show("Green" + errorMessage, "Cannot Stretch", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (bDiv == 0) // No variation in blue channel
                {
                    bitmap.Unlock();
                    MessageBox.Show("Blue" + errorMessage, "Cannot Stretch", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Apply histogram stretching
                double rMult = 255.0 / rDiv;
                double gMult = 255.0 / gDiv;
                double bMult = 255.0 / bDiv;

                // Stretch each pixel
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        double r = (byte)(*pixelPtr >> 16);
                        double g = (byte)(*pixelPtr >> 8);
                        double b = (byte)(*pixelPtr);

                        r = (r - rMin) * rMult;
                        g = (g - gMin) * gMult;
                        b = (b - bMin) * bMult;
                        *pixelPtr = (255 << 24) | ((int)r << 16) | ((int)g << 8) | (int)b;
                        pixelPtr++;
                    }
                }
            }

            // Finalize
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
            bitmap.Unlock();

            return true;
        }
    }
}
