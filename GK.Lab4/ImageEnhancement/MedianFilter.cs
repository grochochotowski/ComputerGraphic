using System.Collections.Generic;

namespace GK.Lab4.ImageEnhancement
{
    public static class MedianFilter
    {
        // Applies a median filter to the input pixel data.
        public static byte[] Apply(byte[] inputPixels, int width, int height, int stride)
        {
            int kRadius = 1;
            byte[] outputPixels = (byte[])inputPixels.Clone();

            for (int y = kRadius; y < height - kRadius; y++)
            {
                for (int x = kRadius; x < width - kRadius; x++)
                {
                    // Lists to hold the 9 neighbors for each channel
                    List<byte> neighboursR = new List<byte>();
                    List<byte> neighboursG = new List<byte>();
                    List<byte> neighboursB = new List<byte>();

                    // 1. Collect neighbors
                    for (int ky = -kRadius; ky <= kRadius; ky++)
                    {
                        for (int kx = -kRadius; kx <= kRadius; kx++)
                        {
                            int offset = (y + ky) * stride + (x + kx) * 4;
                            // BGRA order
                            neighboursB.Add(inputPixels[offset]);
                            neighboursG.Add(inputPixels[offset + 1]);
                            neighboursR.Add(inputPixels[offset + 2]);
                        }
                    }

                    // 2. Sort the collected values
                    neighboursR.Sort();
                    neighboursG.Sort();
                    neighboursB.Sort();

                    // 3. Select the median (the 5th element in a 9-element list)
                    byte medianR = neighboursR[4];
                    byte medianG = neighboursG[4];
                    byte medianB = neighboursB[4];

                    // 4. Write the median value to the output array
                    int outputOffset = y * stride + x * 4;
                    outputPixels[outputOffset] = medianB;
                    outputPixels[outputOffset + 1] = medianG;
                    outputPixels[outputOffset + 2] = medianR;
                }
            }

            return outputPixels;
        }
    }
}
