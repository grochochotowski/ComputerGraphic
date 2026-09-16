using System;
using System.Windows.Media.Imaging;
using GK.Lab5.Helpers;

namespace GK.Lab5.Binarization
{
    // Fuzzy minimum error threshold selection
    public static class FuzzyMinimumError
    {
        public static int Apply(WriteableBitmap bitmap)
        {
            // Convert to grayscale and compute histogram
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            // Get grayscale buffer
            byte[] grayscaleBuffer = GrayscaleConverter.ToGrayscaleBuffer(bitmap);
            int[] histogram = GrayscaleConverter.ComputeHistogram(grayscaleBuffer, width, height);

            // Normalize histogram
            double[] normalizedHistogram = new double[histogram.Length];
            int totalPixels = width * height;
            for (int i = 0; i < normalizedHistogram.Length; i++)
            {
                normalizedHistogram[i] = histogram[i] / (double)totalPixels;
            }

            // Fuzzy minimum error threshold selection
            int threshold = 127;
            double minimumEntropy = double.MaxValue;

            // Find max and min intensity values in histogram
            double max = 0;
            double min = 255;
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] > 0)
                {
                    if (i > max) max = i;
                    if (i < min) min = i;
                }
            }

            double C = max - min;

            // Iterate through all possible thresholds
            for (int t = 0; t < 255; t++)
            {
                double mu0 = 0;
                double backgroundProbability = 0;
                for (int i = 0; i <= t; i++) // Background class
                {
                    mu0 += i * normalizedHistogram[i];
                    backgroundProbability += normalizedHistogram[i];
                }
                mu0 /= backgroundProbability;

                double mu1 = 0;
                double foregroundProbability = 0;
                for (int i = t + 1; i < 256; i++) // Foreground class
                {
                    mu1 += i * normalizedHistogram[i];
                    foregroundProbability += normalizedHistogram[i];
                }
                mu1 /= foregroundProbability;

                double entropy = 0;

                for (int i = 0; i <= t; i++) // Background class
                {
                    entropy += Shannon(C / (C + Math.Abs(i - mu0))) * histogram[i];
                }

                for (int i = t + 1; i < 256; i++) // Foreground class
                {
                    entropy += Shannon(C / (C + Math.Abs(i - mu1))) * histogram[i];
                }

                entropy /= totalPixels;

                // Update minimum entropy and threshold
                if (entropy < minimumEntropy)
                {
                    threshold = t;
                    minimumEntropy = entropy;
                }
            }

            // Binarize image using the found threshold
            BinarizationBase.Binarize(bitmap, grayscaleBuffer, threshold);

            return threshold;
        }

        // Shannon entropy function
        private static double Shannon(double x)
        {
            return -x * Log(x) - (1 - x) * Log(1 - x);
        }

        // Natural logarithm
        private static double Log(double value)
        {
            if (value <= 0) return 0;
            return Math.Log(value);
        }
    }
}
