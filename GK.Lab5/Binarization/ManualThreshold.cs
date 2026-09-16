using System.Windows.Media.Imaging;
using GK.Lab5.Helpers;

namespace GK.Lab5.Binarization
{
    // Manual threshold selection
    public static class ManualThreshold
    {
        public static void Apply(WriteableBitmap bitmap, int threshold)
        {
            byte[] grayscaleBuffer = GrayscaleConverter.ToGrayscaleBuffer(bitmap);
            BinarizationBase.Binarize(bitmap, grayscaleBuffer, threshold);
        }
    }
}
