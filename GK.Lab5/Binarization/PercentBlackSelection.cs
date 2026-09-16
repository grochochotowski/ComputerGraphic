using System.Windows.Media.Imaging;
using GK.Lab5.Helpers;

namespace GK.Lab5.Binarization
{
    // Percent black pixel selection
    public static class PercentBlackSelection
    {
        public static int Apply(WriteableBitmap bitmap, double percent)
        {
            // Validate percent value
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            // Convert to grayscale and compute histogram
            byte[] grayscaleBuffer = GrayscaleConverter.ToGrayscaleBuffer(bitmap);
            int[] histogram = GrayscaleConverter.ComputeHistogram(grayscaleBuffer, width, height);

            // Determine threshold based on desired percent of black pixels
            int totalPixels = width * height;
            int threshold;
            int accumulatedCount = 0;

            // Find the threshold where the accumulated pixel count reaches the desired percent
            for (threshold = 0; threshold <= 255; threshold++)
            {
                accumulatedCount += histogram[threshold];

                if ((double)accumulatedCount >= totalPixels * percent)
                {
                    break;
                }
            }

            // Binarize the image using the determined threshold
            BinarizationBase.Binarize(bitmap, grayscaleBuffer, threshold);

            return threshold;
        }
    }
}
