using System;
using GK.Lab4.Helpers;

namespace GK.Lab4.ImageEnhancement
{
    public static class ConvolutionFilters
    {
        // Applies a convolution filter to the input pixel data.
        public static byte[] Convolve(byte[] inputPixels, int width, int height, int stride, double[,] kernel, double normalizationFactor)
        {
            int kSize = kernel.GetLength(0);
            int kRadius = kSize / 2;

            byte[] outputPixels = (byte[])inputPixels.Clone(); // Start with a copy for boundaries

            // Boundary Handling - loop over pixels excluding the radius
            for (int y = kRadius; y < height - kRadius; y++)
            {
                for (int x = kRadius; x < width - kRadius; x++)
                {
                    double sumB = 0.0;
                    double sumG = 0.0;
                    double sumR = 0.0;

                    // Loop over the kernel neighborhood
                    for (int ky = -kRadius; ky <= kRadius; ky++)
                    {
                        for (int kx = -kRadius; kx <= kRadius; kx++)
                        {
                            int pixelX = x + kx;
                            int pixelY = y + ky;
                            int offset = pixelY * stride + pixelX * 4;

                            double kernelValue = kernel[ky + kRadius, kx + kRadius];

                            // Weighted sum calculation for each channel
                            sumB += inputPixels[offset] * kernelValue;
                            sumG += inputPixels[offset + 1] * kernelValue;
                            sumR += inputPixels[offset + 2] * kernelValue;
                        }
                    }

                    // Normalization and Clamping (Saturation)
                    int outputOffset = y * stride + x * 4;
                    outputPixels[outputOffset] = ColorUtils.Clamp(sumB / normalizationFactor);
                    outputPixels[outputOffset + 1] = ColorUtils.Clamp(sumG / normalizationFactor);
                    outputPixels[outputOffset + 2] = ColorUtils.Clamp(sumR / normalizationFactor);
                }
            }

            return outputPixels;
        }

        // Applies the specified filter type to the input pixel data.
        public static byte[]? ApplyFilter(byte[] inputPixels, int width, int height, int stride, string filterType)
        {
            double[,] kernel;
            double normalizationFactor;

            switch (filterType)
            {
                case "Average": // Smoothing Filter
                    kernel = new double[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
                    normalizationFactor = 9.0;
                    break;

                case "Sharpen": // High-pass Sharpening Filter
                    kernel = new double[,] { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };
                    normalizationFactor = 1.0;
                    break;

                case "Gauss": // Gaussian Blur
                    kernel = new double[,] { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } };
                    normalizationFactor = 16.0;
                    break;

                default:
                    return null;
            }

            return Convolve(inputPixels, width, height, stride, kernel, normalizationFactor);
        }
    }
}
