using System;
using System.Windows.Media.Imaging;
using GK.Lab5.Helpers;

namespace GK.Lab5.Binarization
{
    // Entropy-based threshold selection (Kapur's method)
    public static class EntropySelection
    {
        public static int Apply(WriteableBitmap bitmap)
        {
            // Convert to grayscale and compute histogram
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            // Convert to grayscale
            byte[] grayscaleBuffer = GrayscaleConverter.ToGrayscaleBuffer(bitmap);
            int[] histogram = GrayscaleConverter.ComputeHistogram(grayscaleBuffer, width, height);

            // Normalize histogram
            double[] normalizedHistogram = new double[histogram.Length];
            int totalPixels = width * height;
            for (int i = 0; i < normalizedHistogram.Length; i++)
            {
                normalizedHistogram[i] = histogram[i] / (double)totalPixels;
            }

            // Entropy-based threshold selection
            int threshold = 127;
            double maxSum = double.MinValue;
            double cumulativeEntropy = 0;
            double cumulativeProbability = 0;

            // Total entropy of the image
            double totalEntropy = 0;
            for (int i = 0; i < 256; i++)
            {
                totalEntropy -= normalizedHistogram[i] * Log2(normalizedHistogram[i]);
            }

            // Maximum histogram value for low and high classes
            double maxLow = normalizedHistogram[0];

            for (int i = 0; i < 256; i++)
            {
                cumulativeProbability += normalizedHistogram[i];

                if (normalizedHistogram[i] > maxLow) // Update maxLow
                {
                    maxLow = normalizedHistogram[i];
                }

                double maxHigh = i < 255 ? normalizedHistogram[i + 1] : normalizedHistogram[i];
                for (int j = i + 2; j < 256; j++) // Update maxHigh
                {
                    if (normalizedHistogram[j] > maxHigh)
                    {
                        maxHigh = normalizedHistogram[j];
                    }
                }

                // Update cumulative entropy
                cumulativeEntropy -= normalizedHistogram[i] * Log2(normalizedHistogram[i]);

                // Compute entropy function
                double entropyFunction =
                    cumulativeEntropy * Log2(cumulativeProbability) / (totalEntropy * Log2(maxLow)) +
                    (1 - cumulativeEntropy / totalEntropy) * Log2(1 - cumulativeProbability) / Log2(maxHigh);

                // Update threshold if a new maximum is found
                if (entropyFunction > maxSum)
                {
                    maxSum = entropyFunction;
                    threshold = i;
                }
            }

            // Binarize the image using the selected threshold
            BinarizationBase.Binarize(bitmap, grayscaleBuffer, threshold);

            return threshold;
        }

        // Log base 2 with handling for zero
        private static double Log2(double value)
        {
            if (value == 0) return double.MinValue;
            return Math.Log(value, 2);
        }
    }
}
