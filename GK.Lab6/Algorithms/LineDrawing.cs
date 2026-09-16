using System.Windows;
using System.Windows.Media.Imaging;

namespace GK.Lab6.Algorithms
{
    public static class LineDrawing
    {
        public static unsafe void BresenhamLine(WriteableBitmap bitmap, Point start, Point end, int color)
        {
            // Convert to integer coordinates
            int x1 = (int)start.X, y1 = (int)start.Y, x2 = (int)end.X, y2 = (int)end.Y;

            // Current position starts at the beginning
            int x = x1, y = y1;

            // Calculate the width and height of the line
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            // If horizontal direction
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            // If vertical direction
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            // Assume we move horizontally
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            // Identify the major and minor axis
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);

            // Swap if we are moving more vertically than horizontally
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);

                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            // Initialize the error term
            int numerator = longest >> 1;

            // Cache bitmap dimensions
            int bmpW = bitmap.PixelWidth;

            // Offsets for row movement
            int bmpWdy1 = bmpW * dy1;
            int bmpWdy2 = bmpW * dy2;

            unsafe
            {
                // Pointer to the starting pixel
                int* pBackBuffer = (int*)bitmap.BackBuffer + x + bmpW * y;

                // Iterate over the major axis
                for (int i = 0; i <= longest; i++)
                {
                    // Draw the pixel at current position
                    *pBackBuffer = color;
                    bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));

                    // Accumulate the error term
                    numerator += shortest;

                    // If error has accumulated enough, move on both axes
                    if (!(numerator < longest))
                    {
                        numerator -= longest;
                        x += dx1;
                        y += dy1;
                        pBackBuffer += dx1 + bmpWdy1;
                    }
                    else
                    {
                        // Otherwise, only move on the major axis
                        x += dx2;
                        y += dy2;
                        pBackBuffer += dx2 + bmpWdy2;
                    }
                }
            }
        }
    }
}
