using System;
using System.Linq;
using GK.Lab4.Helpers;

namespace GK.Lab4.ImageEnhancement
{
    public static class CustomMaskConvolution
    {
        // Applies a custom convolution mask to the input pixel data.
        public static byte[] ApplyCustomMask(byte[] inputPixels, int width, int height, int stride,
            double[,] kernel, double normalizationFactor)
        {
            int kSize = kernel.GetLength(0);
            int kRadius = kSize / 2;

            // Validate kernel is square
            if (kernel.GetLength(0) != kernel.GetLength(1))
            {
                throw new ArgumentException("Kernel must be square (NxN)");
            }

            // Validate kernel size is odd
            if (kSize % 2 == 0)
            {
                throw new ArgumentException("Kernel size must be odd (3, 5, 7, etc.)");
            }

            // Validate normalization factor
            if (Math.Abs(normalizationFactor) < 0.0001)
            {
                throw new ArgumentException("Normalization factor cannot be zero or near-zero");
            }

            byte[] outputPixels = (byte[])inputPixels.Clone();

            // Boundary Handling: Loop over pixels excluding the border
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

        // Parses a kernel string into a 2D double array.
        public static double[,]? ParseKernelString(string kernelString)
        {
            try
            {
                // Split by semicolons to get rows
                string[] rows = kernelString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (rows.Length == 0)
                {
                    return null;
                }

                // Parse the first row to determine column count
                double[][] jaggedArray = new double[rows.Length][];
                for (int i = 0; i < rows.Length; i++)
                {
                    string[] values = rows[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                    jaggedArray[i] = values.Select(v => double.Parse(v.Trim())).ToArray();
                }

                // Validate dimensions
                int rowCount = jaggedArray.Length;
                int colCount = jaggedArray[0].Length;

                // Check if square
                if (rowCount != colCount)
                {
                    return null;
                }

                // Check if all rows have the same column count
                if (jaggedArray.Any(row => row.Length != colCount))
                {
                    return null;
                }

                // Check if size is odd
                if (rowCount % 2 == 0)
                {
                    return null;
                }

                // Convert to 2D array
                double[,] kernel = new double[rowCount, colCount];
                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < colCount; j++)
                    {
                        kernel[i, j] = jaggedArray[i][j];
                    }
                }

                return kernel;
            }
            catch
            {
                return null;
            }
        }

        // Calculates the sum of all elements in the kernel.
        public static double CalculateKernelSum(double[,] kernel)
        {
            double sum = 0.0;
            for (int i = 0; i < kernel.GetLength(0); i++)
            {
                for (int j = 0; j < kernel.GetLength(1); j++)
                {
                    sum += kernel[i, j];
                }
            }
            return sum;
        }

        // Validates the kernel string format.
        public static bool ValidateKernelString(string kernelString, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(kernelString))
            {
                errorMessage = "Kernel string cannot be empty";
                return false;
            }

            var kernel = ParseKernelString(kernelString);
            if (kernel == null)
            {
                errorMessage = "Invalid kernel format. Must be square, odd-sized (3x3, 5x5, 7x7, etc.), and use format: '1,0,-1;0,0,0;-1,0,1'";
                return false;
            }

            return true;
        }
    }
}
