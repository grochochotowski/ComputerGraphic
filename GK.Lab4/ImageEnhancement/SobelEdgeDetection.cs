using System;
using GK.Lab4.Helpers;

namespace GK.Lab4.ImageEnhancement
{
    public static class SobelEdgeDetection
    {
        // Applies the Sobel edge detection filter to the input pixel data.
        public static byte[] Apply(byte[] inputPixels, int width, int height, int stride)
        {
            // Sobel Kernels
            double[,] kernelX = new double[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } }; // Vertical edge detection
            double[,] kernelY = new double[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } }; // Horizontal edge detection

            int kRadius = 1;

            // Arrays to hold the temporary, signed gradient values for R, G, B
            int size = width * height;
            int[] GxR = new int[size];
            int[] GyR = new int[size];
            int[] GxG = new int[size];
            int[] GyG = new int[size];
            int[] GxB = new int[size];
            int[] GyB = new int[size];

            byte[] outputPixels = (byte[])inputPixels.Clone();

            // 1. Calculate Gx and Gy (convolution results)
            for (int y = kRadius; y < height - kRadius; y++)
            {
                for (int x = kRadius; x < width - kRadius; x++)
                {
                    double sumX_R = 0.0, sumY_R = 0.0;
                    double sumX_G = 0.0, sumY_G = 0.0;
                    double sumX_B = 0.0, sumY_B = 0.0;

                    for (int ky = -kRadius; ky <= kRadius; ky++)
                    {
                        for (int kx = -kRadius; kx <= kRadius; kx++)
                        {
                            int offset = (y + ky) * stride + (x + kx) * 4;
                            double pixelValueB = inputPixels[offset];
                            double pixelValueG = inputPixels[offset + 1];
                            double pixelValueR = inputPixels[offset + 2];

                            double kernelValueX = kernelX[ky + kRadius, kx + kRadius];
                            double kernelValueY = kernelY[ky + kRadius, kx + kRadius];

                            sumX_R += pixelValueR * kernelValueX;
                            sumY_R += pixelValueR * kernelValueY;

                            sumX_G += pixelValueG * kernelValueX;
                            sumY_G += pixelValueG * kernelValueY;

                            sumX_B += pixelValueB * kernelValueX;
                            sumY_B += pixelValueB * kernelValueY;
                        }
                    }

                    int index = y * width + x;
                    // Store signed gradient values (no clamping yet)
                    GxR[index] = (int)sumX_R; GyR[index] = (int)sumY_R;
                    GxG[index] = (int)sumX_G; GyG[index] = (int)sumY_G;
                    GxB[index] = (int)sumX_B; GyB[index] = (int)sumY_B;
                }
            }

            // 2. Combine and Finalize Gradient Magnitude: G = |Gx| + |Gy|
            for (int y = kRadius; y < height - kRadius; y++)
            {
                for (int x = kRadius; x < width - kRadius; x++)
                {
                    int index = y * width + x;
                    int outputOffset = y * stride + x * 4;

                    // Calculate magnitude using L1 norm: |Gx| + |Gy|
                    double magR = Math.Abs(GxR[index]) + Math.Abs(GyR[index]);
                    double magG = Math.Abs(GxG[index]) + Math.Abs(GyG[index]);
                    double magB = Math.Abs(GxB[index]) + Math.Abs(GyB[index]);

                    // Clamp and write result (BGRA order)
                    outputPixels[outputOffset] = ColorUtils.Clamp(magB);
                    outputPixels[outputOffset + 1] = ColorUtils.Clamp(magG);
                    outputPixels[outputOffset + 2] = ColorUtils.Clamp(magR);
                }
            }

            return outputPixels;
        }
    }
}
