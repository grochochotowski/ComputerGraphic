using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using GK.Lab6.Models;

namespace GK.Lab6.Algorithms
{
    public static class BezierCurve
    {
        // Draws the Bézier curve by connecting calculated points with lines to avoid gaps.
        public static unsafe void DrawWithLines(WriteableBitmap bmp, ICollection<ControlPoint> points,
            double deltaT, int color)
        {
            int wid = bmp.PixelWidth;

            if (points.Count < 2) return;

            Point prevDrawPt;

            unsafe
            {
                int* p = (int*)bmp.BackBuffer;
                double t = 0.0;

                // Calculate and draw the first point at t=0
                Point curDrawPt = CalculatePoint(points.ToArray(), t);
                int x = (int)curDrawPt.X, y = (int)curDrawPt.Y;

                if (x >= 0 && x < wid && y >= 0 && y < bmp.PixelHeight)
                {
                    *(p + x + wid * y) = color;
                    bmp.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                }

                // Iterate through remaining parameter values
                for (t = deltaT; t <= 1.0; t += deltaT)
                {
                    prevDrawPt = curDrawPt;
                    curDrawPt = CalculatePoint(points.ToArray(), t);

                    // Draw line segment
                    LineDrawing.BresenhamLine(bmp, prevDrawPt, curDrawPt, color);
                }

                // Ensure the last point is drawn
                curDrawPt = CalculatePoint(points.ToArray(), 1.0);
                LineDrawing.BresenhamLine(bmp, prevDrawPt, curDrawPt, color);
            }
        }

        // Calculates the Bézier curve point at parameter t using formula.
        private static Point CalculatePoint(ControlPoint[] points, double t)
        {
            double x = 0;
            double y = 0;
            int n = points.Length - 1;

            for (int i = 0; i <= n; i++)
            {
                double bernstein_polynomial = BinomialCoefficient(n, i) * System.Math.Pow(t, i) * System.Math.Pow(1 - t, n - i);
                x += bernstein_polynomial * points[i].X;
                y += bernstein_polynomial * points[i].Y;
            }

            return new Point(x, y);
        }

        private static long BinomialCoefficient(int n, int k)
        {
            if (k < 0 || k > n)
            {
                return 0;
            }
            if (k == 0 || k == n)
            {
                return 1;
            }
            if (k > n / 2)
            {
                k = n - k;
            }

            long res = 1;
            for (int i = 1; i <= k; i++)
            {
                res = res * (n - i + 1) / i;
            }
            return res;
        }
    }
}
