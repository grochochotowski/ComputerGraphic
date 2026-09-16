using GK.Lab3.Graphics;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace GK.Lab3.Views
{
    public partial class RgbCubeView : UserControl
    {
        private Model3DGroup _scene = new Model3DGroup();
        private GeometryModel3D? _sliceModel;

        private Point _lastMousePos;
        private bool _isRotating = false;

        private AxisAngleRotation3D _rotationX = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0);
        private AxisAngleRotation3D _rotationY = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);

        public RgbCubeView()
        {
            InitializeComponent();
            BuildScene();
            SliceSlider_ValueChanged(SliceSlider, new RoutedPropertyChangedEventArgs<double>(0.0, SliceSlider.Value));
        }

        // ============================
        // BUILD SCENE
        // ============================
        private void BuildScene()
        {
            // 3D transforms setup
            Transform3DGroup transform = new Transform3DGroup();

            transform.Children.Add(new TranslateTransform3D(-0.5, -0.5, -0.5));
            transform.Children.Add(new RotateTransform3D(_rotationX));
            transform.Children.Add(new RotateTransform3D(_rotationY));
            transform.Children.Add(new TranslateTransform3D(0.5, 0.5, 0.5));
            transform.Children.Add(new ScaleTransform3D(1.2, 1.2, 1.2));

            _scene.Transform = transform;

            // Add the RGB cube model
            var cube = RgbCube3D.CreateCube();
            _scene.Children.Add(cube);

            // Add to the Viewport
            CubeModel.Content = _scene;
        }

        // ============================
        // SLICE HANDLING
        // ============================
        private void SliceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliceZ = SliceSlider.Value;
            if (_sliceModel != null)
                _scene.Children.Remove(_sliceModel);

            _sliceModel = RgbCube3D.CreateSlice(sliceZ);
            _scene.Children.Add(_sliceModel);

            Update2DSlice(sliceZ);
        }


        // ============================
        // MOUSE ROTATION
        // ============================
        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isRotating = true;
            _lastMousePos = e.GetPosition(this);
            Mouse.Capture(Viewport);
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRotating) return;

            Point current = e.GetPosition(this);
            double dx = current.X - _lastMousePos.X;
            double dy = current.Y - _lastMousePos.Y;
            _lastMousePos = current;

            _rotationY.Angle += dx * 0.5;
            _rotationX.Angle -= dy * 0.5;
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isRotating = false;
            Mouse.Capture(null);
        }

        // ============================
        // 2D SLICE PREVIEW
        // ============================
        private void Update2DSlice(double z)
        {
            int size = 256;
            WriteableBitmap bmp = new WriteableBitmap(size, size, 96, 96,
                                                     PixelFormats.Bgr32, null);

            int stride = bmp.PixelWidth * 4;
            byte[] pixels = new byte[stride * bmp.PixelHeight];
            byte fixedB = (byte)(z * 255);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    byte R = (byte)(x * 255 / (size - 1));
                    byte G = (byte)(y * 255 / (size - 1));
                    byte B = fixedB;

                    int index = (y * stride) + (x * 4);

                    pixels[index + 0] = B;
                    pixels[index + 1] = G;
                    pixels[index + 2] = R;
                    pixels[index + 3] = 255;
                }
            }

            bmp.WritePixels(new Int32Rect(0, 0, size, size), pixels, stride, 0);
            SlicePreview.Source = bmp;
        }
    }
}