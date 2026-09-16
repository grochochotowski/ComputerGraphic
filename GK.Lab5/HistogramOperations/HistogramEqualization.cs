using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GK.Lab5.HistogramOperations
{
    public static class HistogramEqualization
    {
        public static bool Apply(WriteableBitmap bitmap)
        {
            // Compute histograms for each channel
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            bitmap.Lock();

            unsafe
            {
                // Compute histograms
                int[] rCount = new int[256];
                int[] gCount = new int[256];
                int[] bCount = new int[256];

                // Count pixel values
                int* pixelPtr = (int*)bitmap.BackBuffer;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int argb = *pixelPtr;
                        rCount[(argb >> 16) & 255]++;
                        gCount[(argb >> 8) & 255]++;
                        bCount[argb & 255]++;
                        pixelPtr++;
                    }
                }

                // Compute cumulative distributions
                for (int v = 1; v <= 255; v++)
                {
                    rCount[v] = rCount[v - 1] + rCount[v];
                    gCount[v] = gCount[v - 1] + gCount[v];
                    bCount[v] = bCount[v - 1] + bCount[v];
                }

                // Find minimum non-zero values in cumulative distributions
                int[] rDist = rCount;
                int[] gDist = gCount;
                int[] bDist = bCount;
                int rDistMin = 0, gDistMin = 0, bDistMin = 0;

                for (int v = 0; v <= 255; v++) // Find first non-zero in rDist
                {
                    if (rDist[v] > 0) { rDistMin = rDist[v]; break; }
                }
                for (int v = 0; v <= 255; v++) // Find first non-zero in gDist
                {
                    if (gDist[v] > 0) { gDistMin = gDist[v]; break; }
                }
                for (int v = 0; v <= 255; v++) // Find first non-zero in bDist
                {
                    if (bDist[v] > 0) { bDistMin = bDist[v]; break; }
                }

                // Check for channels with all pixels having value 0
                const string errorMessage = "All pixels have value 0 for the ";
                if (rDistMin == 0) // No variation in red channel
                {
                    bitmap.Unlock();
                    MessageBox.Show(errorMessage + "red channel", "Cannot Equalize", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (gDistMin == 0) // No variation in green channel
                {
                    bitmap.Unlock();
                    MessageBox.Show(errorMessage + "green channel", "Cannot Equalize", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (bDistMin == 0) // No variation in blue channel
                {
                    bitmap.Unlock();
                    MessageBox.Show(errorMessage + "blue channel", "Cannot Equalize", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Normalize distributions to [0, 255]
                int totalPixels = width * height;
                int rDiv = totalPixels - rDistMin;
                int gDiv = totalPixels - gDistMin;
                int bDiv = totalPixels - bDistMin;

                // Calculate multipliers
                double rMult = 255.0 / rDiv;
                double gMult = 255.0 / gDiv;
                double bMult = 255.0 / bDiv;

                // Apply equalization
                for (int v = 0; v <= 255; v++)
                {
                    rDist[v] = (int)Math.Round((rDist[v] - rDistMin) * rMult);
                    gDist[v] = (int)Math.Round((gDist[v] - gDistMin) * gMult);
                    bDist[v] = (int)Math.Round((bDist[v] - bDistMin) * bMult);
                }

                // Reset pointer to the beginning
                pixelPtr = (int*)bitmap.BackBuffer;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int argb = *pixelPtr;

                        *pixelPtr = (255 << 24) |
                                   (rDist[(argb >> 16) & 255] << 16) |
                                   (gDist[(argb >> 8) & 255] << 8) |
                                   bDist[argb & 255];
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
