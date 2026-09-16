using System;
using System.Windows.Media.Imaging;
using GK.Lab5.Helpers;

namespace GK.Lab5.Binarization
{
    // Minimum error threshold selection (Kittler-Illingworth)
    public static class MinimumError
    {
        public static int Apply(WriteableBitmap bitmap)
        {
            // Compute histogram
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            // Convert to grayscale
            byte[] grayscaleBuffer = GrayscaleConverter.ToGrayscaleBuffer(bitmap);
            int[] histogram = GrayscaleConverter.ComputeHistogram(grayscaleBuffer, width, height);

            // Find optimal threshold
            int threshold = 127;
            double minimumValue = double.MaxValue;

            // Pre-compute totals
            double P1 = 0;
            double P2 = 0;
            double Pi1 = 0;
            double Pi2 = 0;
            for (int i = 0; i < 256; i++)
            {
                int v = histogram[i];
                P2 += v;
                v *= i;
                Pi2 += v;
            }

            // Iterate through all possible thresholds
            for (int i = 0; i < 256; i++)
            {
                // Update class totals
                int v = histogram[i];

                P1 += v;
                P2 -= v;

                v *= i;
                Pi1 += v;
                Pi2 -= v;

                // Compute class variances
                double u1 = P1 > 0 ? Pi1 / P1 : 0;
                double u2 = P2 > 0 ? Pi2 / P2 : 0;
                double s1 = 0;
                if (P1 > 0)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        double deviation = j - u1;
                        s1 += deviation * deviation * histogram[j];
                    }
                    s1 /= P1;
                }

                // Second class variance
                double s2 = 0;
                if (P2 > 0)
                {
                    for (int j = i + 1; j < 256; j++)
                    {
                        double deviation = j - u2;
                        s2 += deviation * deviation * histogram[j];
                    }
                    s2 /= P2;
                }

                // Compute criterion function
                double J = 1 + 2 * P1 * (Log(s1) - Log(P1) + P2 * (Log(s2) - Log(P2)));

                if (J < minimumValue)
                {
                    threshold = i;
                    minimumValue = J;
                }
            }

            // 
            BinarizationBase.Binarize(bitmap, grayscaleBuffer, threshold);

            return threshold;
        }

        // Safe logarithm
        private static double Log(double value)
        {
            if (value <= 0) return 0;
            return Math.Log(value);
        }
    }
}
