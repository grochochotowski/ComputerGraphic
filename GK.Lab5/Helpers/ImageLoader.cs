using System;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GK.Lab5.Helpers
{
    // PPM image loader supporting P3 (ASCII) and P6 (binary) formats
    public static class ImageLoader
    {
        public static WriteableBitmap? LoadPpmImage(string filePath)
        {
            try
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using BinaryReader reader = new BinaryReader(fileStream);

                string magicNumber = ReadLine(reader);
                if (magicNumber != "P3" && magicNumber != "P6")
                {
                    return null;
                }

                string line = ReadLine(reader);
                while (line.StartsWith("#"))
                {
                    line = ReadLine(reader);
                }

                string[] dimensions = line.Split(' ');
                int width = int.Parse(dimensions[0]);
                int height = int.Parse(dimensions[1]);

                int maxColorValue = int.Parse(ReadLine(reader));

                WriteableBitmap bitmap = new WriteableBitmap(
                    width,
                    height,
                    96,
                    96,
                    PixelFormats.Bgr24,
                    null
                );

                bitmap.Lock();

                unsafe
                {
                    byte* pBuffer = (byte*)bitmap.BackBuffer;
                    int stride = bitmap.BackBufferStride;

                    if (magicNumber == "P6")
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte r = reader.ReadByte();
                                byte g = reader.ReadByte();
                                byte b = reader.ReadByte();

                                int offset = y * stride + x * 3;

                                pBuffer[offset] = b;
                                pBuffer[offset + 1] = g;
                                pBuffer[offset + 2] = r;
                            }
                        }
                    }
                    else
                    {
                        string remainingData = Encoding.ASCII.GetString(reader.ReadBytes((int)(fileStream.Length - fileStream.Position)));
                        string[] values = remainingData.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        int valueIndex = 0;

                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte r = byte.Parse(values[valueIndex++]);
                                byte g = byte.Parse(values[valueIndex++]);
                                byte b = byte.Parse(values[valueIndex++]);

                                int offset = y * stride + x * 3;

                                pBuffer[offset] = b;
                                pBuffer[offset + 1] = g;
                                pBuffer[offset + 2] = r;
                            }
                        }
                    }
                }

                bitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, width, height));
                bitmap.Unlock();
                bitmap.Freeze();

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static string ReadLine(BinaryReader reader)
        {
            StringBuilder line = new StringBuilder();
            char c;

            while ((c = reader.ReadChar()) != '\n')
            {
                if (c != '\r')
                {
                    line.Append(c);
                }
            }

            return line.ToString().Trim();
        }
    }
}
