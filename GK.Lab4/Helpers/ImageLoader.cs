using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace GK.Lab4.Helpers
{
    public static class ImageLoader
    {
        // Opens a file dialog to load an image and returns it as a BitmapImage.
        public static BitmapImage? LoadFromDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    return new BitmapImage(new Uri(openFileDialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            return null;
        }
    }
}
