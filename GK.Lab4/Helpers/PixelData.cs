using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GK.Lab4.Helpers
{
    public class PixelData
    {
        public WriteableBitmap? CurrentWb { get; private set; }
        public byte[]? Data { get; private set; }

        // Loads an image into the WriteableBitmap and initializes the pixel data array.
        public void LoadImage(BitmapImage bitmap)
        {
            CurrentWb = new WriteableBitmap(bitmap);

            int stride = CurrentWb.BackBufferStride;
            Data = new byte[CurrentWb.PixelHeight * stride];

            // Copy pixels from the WriteableBitmap to the byte array
            CurrentWb.CopyPixels(Data, stride, 0);
        }

        // Updates the WriteableBitmap with new pixel data after processing.
        public void UpdateImage(byte[] newPixels)
        {
            if (CurrentWb == null) return;

            try
            {
                CurrentWb.Lock();

                // Copy the processed pixel data from the array back into the WriteableBitmap's back buffer
                Marshal.Copy(newPixels, 0, CurrentWb.BackBuffer, newPixels.Length);

                // Indicate that the entire image needs to be redrawn
                CurrentWb.AddDirtyRect(new Int32Rect(0, 0, CurrentWb.PixelWidth, CurrentWb.PixelHeight));
            }
            finally
            {
                CurrentWb.Unlock();
                // The newly processed pixels become the source for the next operation
                Data = newPixels;
            }
        }

        // Synchronizes the WriteableBitmap with the current pixel data array.
        public void SyncPixelData()
        {
            if (CurrentWb == null || Data == null) return;

            int stride = CurrentWb.BackBufferStride;
            CurrentWb.CopyPixels(Data, stride, 0);
        }

        // If there is a loaded image.
        public bool HasImage => CurrentWb != null && Data != null;
    }
}
