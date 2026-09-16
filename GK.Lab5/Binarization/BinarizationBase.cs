using System.Windows.Media.Imaging;

namespace GK.Lab5.Binarization
{
    // Base class for binarization methods
    public static class BinarizationBase
    {
        private const int BLACK = unchecked((int)0xFF000000);
        private const int WHITE = unchecked((int)0xFFFFFFFF);

        // Pixels <= threshold become black, > threshold become white
        public static void Binarize(WriteableBitmap bitmap, byte[] grayscaleBuffer, int threshold)
        {
            // Binarize the image based on the threshold
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int index = 0;

            bitmap.Lock();

            unsafe
            {
                int* pixelPtr = (int*)bitmap.BackBuffer;

                // Iterate through each pixel and apply the threshold
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (grayscaleBuffer[index] <= threshold) // Pixel becomes black
                        {
                            *pixelPtr = BLACK;
                        }
                        else // Pixel becomes white
                        {
                            *pixelPtr = WHITE;
                        }

                        index++;
                        pixelPtr++;
                    }
                }
            }

            // Mark the entire bitmap as dirty to update the display
            bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
            bitmap.Unlock();
        }
    }
}
