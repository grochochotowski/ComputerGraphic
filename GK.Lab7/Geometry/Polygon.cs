using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GK.Lab7.Geometry
{
    public class Polygon : ObservableObject
    {
        private ObservableCollection<Vertex> vertices = new ObservableCollection<Vertex>();
        public ObservableCollection<Vertex> Vertices
        {
            get => vertices;
            set { vertices = value; OnPropertyChanged(); }
        }

        // Constructor
        public Polygon()
        {
            Vertices = new ObservableCollection<Vertex>();
        }

        // Methods
        public Point[] CloneVertices()
        {
            var ret = new Point[Vertices.Count];
            int i = 0;
            foreach (var v in Vertices)
                ret[i++] = new Point(v.X, v.Y);
            return ret;
        }

        // Restores vertices from an array of Points
        public void RestoreVerticesFrom(Point[] vertices)
        {
            if (vertices.Length != Vertices.Count)
                throw new ArgumentException("vertices.Length != Vertices.Count");
            int i = 0;
            foreach (var v in Vertices)
            {
                var argV = vertices[i++];
                v.SetXYWithoutRedraw(argV.X, argV.Y);
            }
        }

        // Returns the index of the vertex intersecting with the given point, or -1 if none
        public int IntersectingVertexIndex(Point point)
        {
            for (int i = 0; i < Vertices.Count; ++i)
            {
                if (Vertices[i].Intersects(point))
                    return i;
            }
            return -1;
        }

        // Transform helper
        private void Transform(Matrix mat)
        {
            foreach (var v in Vertices)
            {
                var p = new Point(v.X, v.Y);
                p = mat.Transform(p);
                v.SetXYWithoutRedraw(p.X, p.Y);
            }
        }

        // Translate
        public void Translate(double vectorX, double vectorY)
        {
            var mat = Matrix.Identity;
            mat.Prepend(new Matrix(1, 0, 0, 1, vectorX, vectorY));
            Transform(mat);
        }

        // Rotate
        public void Rotate(double centerX, double centerY, double angle)
        {
            var mat = Matrix.Identity;
            mat.Prepend(new Matrix(1, 0, 0, 1, centerX, centerY));
            angle = -angle; // Negate for correct rotation direction
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            mat.Prepend(new Matrix(cos, -sin, sin, cos, 0, 0));
            mat.Prepend(new Matrix(1, 0, 0, 1, -centerX, -centerY));
            Transform(mat);
        }

        // Scale
        public void Scale(double centerX, double centerY, double factorX, double factorY)
        {
            var mat = Matrix.Identity;
            mat.Prepend(new Matrix(1, 0, 0, 1, centerX, centerY));
            mat.Prepend(new Matrix(factorX, 0, 0, factorY, 0, 0));
            mat.Prepend(new Matrix(1, 0, 0, 1, -centerX, -centerY));
            Transform(mat);
        }

        // Draw polygon on bitmap
        public void DrawOn(WriteableBitmap bmp, int color)
        {
            var cnt = Vertices.Count;
            if (cnt == 0) return;
            int iLimit = cnt - 1;
            for (int i = 0; i < iLimit; ++i)
            {
                var vi = Vertices[i];
                BresenhamLine(bmp, vi, Vertices[i + 1], color);
                vi.DrawRectangle(bmp, color);
            }
            Vertices[iLimit].DrawRectangle(bmp, color);
            if (cnt >= 3)  // Close polygon if at least 3 vertices
                BresenhamLine(bmp, Vertices[cnt - 1], Vertices[0], color);
        }

        // Bresenham's line algorithm
        public static void BresenhamLine(WriteableBitmap bitmap, Vertex start, Vertex end, int color)
        {
            // Bresenham's line algorithm
            int x1 = (int)start.X, y1 = (int)start.Y, x2 = (int)end.X, y2 = (int)end.Y;
            int x = x1, y = y1;
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            // Set up direction increments
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            // Determine which direction is the longest
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);

            // Swap if necessary
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            // Initialize the numerator
            int numerator = longest >> 1;
            int bmpW = bitmap.PixelWidth, bmpH = bitmap.PixelHeight;
            int bmpWdy1 = bmpW * dy1, bmpWdy2 = bmpW * dy2;

            // Draw pixels
            unsafe
            {
                int* pBackBuffer = (int*)bitmap.BackBuffer + x + bmpW * y;

                // Loop through the longest direction
                for (int i = 0; i <= longest; i++)
                {
                    if (x >= 0 && x < bmpW && y >= 0 && y < bmpH) // Check bounds
                    {
                        *pBackBuffer = color;
                        bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
                    }
                    numerator += shortest;
                    if (!(numerator < longest)) // Move in the shortest direction
                    {
                        numerator -= longest;
                        x += dx1;
                        y += dy1;
                        pBackBuffer += dx1 + bmpWdy1;
                    }
                    else // Move in the longest direction
                    {
                        x += dx2;
                        y += dy2;
                        pBackBuffer += dx2 + bmpWdy2;
                    }
                }
            }
        }
    }
}
