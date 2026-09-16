using System.Windows.Media.Imaging;
using GK.Lab5.Helpers;

namespace GK.Lab5.Binarization
{
    // Mean iterative selection (Isodata)
    public static class MeanIterativeSelection
    {
        public static int Apply(WriteableBitmap bitmap)
        {
            // Compute histogram
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            // Convert to grayscale buffer
            byte[] grayscaleBuffer = GrayscaleConverter.ToGrayscaleBuffer(bitmap);
            int[] histogram = GrayscaleConverter.ComputeHistogram(grayscaleBuffer, width, height);

            // Iterative selection of threshold
            const int maxGrayValue = 255;
            byte threshold = 0;
            byte previousThreshold;
            int[] cumulativeHistogram = new int[histogram.Length];
            cumulativeHistogram[0] = histogram[0];
            for (int v = 1; v <= 255; v++)
            {
                cumulativeHistogram[v] = cumulativeHistogram[v - 1] + histogram[v];
            }

            // Iterate until convergence
            do
            {
                previousThreshold = threshold;

                int backgroundNumerator = 0;
                for (int i = 0; i <= previousThreshold; i++) // Background
                {
                    backgroundNumerator += i * histogram[i];
                }

                int foregroundNumerator = 0;
                for (int j = previousThreshold + 1; j <= maxGrayValue; j++) // Foreground
                {
                    foregroundNumerator += j * histogram[j];
                }

                // Compute means
                int backgroundDenominator = cumulativeHistogram[previousThreshold];
                int foregroundDenominator = cumulativeHistogram[maxGrayValue] - cumulativeHistogram[previousThreshold];

                double backgroundMean = backgroundNumerator / (double)backgroundDenominator;
                double foregroundMean = foregroundNumerator / (double)foregroundDenominator;

                threshold = (byte)((backgroundMean + foregroundMean) / 2.0);

            } while (threshold != previousThreshold);

            // Binarize image
            BinarizationBase.Binarize(bitmap, grayscaleBuffer, threshold);

            return threshold;
        }
    }
}
