using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace GK.Lab3.Graphics
{
    public static class RgbCube3D
    {
        private const int TextureSize = 256;

        private static Color VertexColor(Point3D p)
        {
            byte r = (byte)(p.X * 255);
            byte g = (byte)(p.Y * 255);
            byte b = (byte)(p.Z * 255);
            return Color.FromRgb(r, g, b);
        }

        private static ImageBrush CreateColorGradientBrush(int fixedComponent, double fixedValue)
        {
            var bmp = new WriteableBitmap(TextureSize, TextureSize, 96, 96, PixelFormats.Bgr32, null);
            int stride = bmp.PixelWidth * 4;
            byte[] pixels = new byte[stride * bmp.PixelHeight];
            byte fixedByte = (byte)(fixedValue * 255);

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    // Color values based on U and V
                    byte valU = (byte)(x * 255 / (TextureSize - 1));
                    byte valV = (byte)(y * 255 / (TextureSize - 1));

                    byte R = 0, G = 0, B = 0;

                    // Map U and V to RGB based on which component is fixed
                    switch (fixedComponent)
                    {
                        case 2:
                            R = valU;
                            G = valV;
                            B = fixedByte;
                            break;
                        case 1: 
                            R = valU;
                            G = fixedByte;
                            B = valV;
                            break;
                        case 0: 
                            R = fixedByte;
                            G = valU;
                            B = valV;
                            break;
                    }

                    int index = (y * stride) + (x * 4);

                    pixels[index + 0] = B;
                    pixels[index + 1] = G;
                    pixels[index + 2] = R;
                    pixels[index + 3] = 255;
                }
            }

            bmp.WritePixels(new Int32Rect(0, 0, TextureSize, TextureSize), pixels, stride, 0);

            // Return as ImageBrush
            return new ImageBrush(bmp)
            {
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.None,
            };
        }

        public static Model3DGroup CreateCube()
        {
            var group = new Model3DGroup();

            // Vertices of the cube
            Point3D[] v =
            {
                new Point3D(0,0,0), // 0: 000 (Black)
                new Point3D(1,0,0), // 1: 100 (Red)
                new Point3D(1,1,0), // 2: 110 (Yellow)
                new Point3D(0,1,0), // 3: 010 (Green)
                new Point3D(0,0,1), // 4: 001 (Blue)
                new Point3D(1,0,1), // 5: 101 (Magenta)
                new Point3D(1,1,1), // 6: 111 (White)
                new Point3D(0,1,1)  // 7: 011 (Cyan)
            };

            // Data for each face: indices, fixed component and its value
            var faceData = new[]
            {
                new { Indices = new[]{0,1,2, 0,2,3}, FixedComponent=2, FixedValue=0.0 },
                new { Indices = new[]{4,5,6, 4,6,7}, FixedComponent=2, FixedValue=1.0 },
                new { Indices = new[]{0,1,5, 0,5,4}, FixedComponent=1, FixedValue=0.0 },
                new { Indices = new[]{3,2,6, 3,6,7}, FixedComponent=1, FixedValue=1.0 },
                new { Indices = new[]{0,3,7, 0,7,4}, FixedComponent=0, FixedValue=0.0 },
                new { Indices = new[]{1,2,6, 1,6,5}, FixedComponent=0, FixedValue=1.0 },
            };

            foreach (var data in faceData)
            {
                MeshGeometry3D mesh = new MeshGeometry3D();

                // Add positions and texture coordinates
                for (int i = 0; i < data.Indices.Length; i++)
                {
                    var p = v[data.Indices[i]];
                    mesh.Positions.Add(p);

                    // U,V mapping based on fixed component of the face
                    if (data.FixedComponent == 2)
                    {
                        mesh.TextureCoordinates.Add(new Point(p.X, p.Y));
                    }
                    else if (data.FixedComponent == 1)
                    {
                        mesh.TextureCoordinates.Add(new Point(p.X, p.Z));
                    }
                    else
                    {
                        mesh.TextureCoordinates.Add(new Point(p.Y, p.Z));
                    }
                }

                // Define triangle indices
                mesh.TriangleIndices = new Int32Collection { 0, 1, 2, 3, 4, 5 };

                // Create brush for this face
                var brush = CreateColorGradientBrush(data.FixedComponent, data.FixedValue);
                var material = new DiffuseMaterial(brush);

                group.Children.Add(new GeometryModel3D(mesh, material)
                {
                    BackMaterial = material
                });
            }

            return group;
        }

        // Creates a 2D slice of the RGB cube at a given Z value
        public static GeometryModel3D CreateSlice(double sliceZ)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Vertices of the square slice
            Point3D p0 = new Point3D(0, 0, sliceZ);
            Point3D p1 = new Point3D(1, 0, sliceZ);
            Point3D p2 = new Point3D(1, 1, sliceZ);
            Point3D p3 = new Point3D(0, 1, sliceZ);

            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);

            // Indexes for two triangles
            mesh.TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 };

            // Texture coordinates
            mesh.TextureCoordinates.Add(new Point(0, 0)); // p0
            mesh.TextureCoordinates.Add(new Point(1, 0)); // p1
            mesh.TextureCoordinates.Add(new Point(1, 1)); // p2
            mesh.TextureCoordinates.Add(new Point(0, 1)); // p3

            // Create brush for the slice
            var brush = CreateColorGradientBrush(2, sliceZ);

            var material = new DiffuseMaterial(brush);
            return new GeometryModel3D(mesh, material)
            {
                BackMaterial = material
            };
        }
    }
}