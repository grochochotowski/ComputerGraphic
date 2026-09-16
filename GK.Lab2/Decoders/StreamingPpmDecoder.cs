using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GK.Lab2.Decoders
{
    public class StreamingPpmDecoder
    {
        // --- READER ---
        private readonly BufferedBinaryReader reader;


        // --- CONSTRUCTOR ---
        public StreamingPpmDecoder(FileStream stream)
        {
            // buffer (150 MB)
            reader = new BufferedBinaryReader(stream, 150 * 1024 * 1024);
        }


        // --- DECODE ---
        public WriteableBitmap Decode()
        {
            SkipWhitespace();

            if (reader.ReadByte() != 'P')
                throw new Exception("Invalid PPM header.");

            int type = reader.ReadByte();

            SkipWhitespace();
            if (!ReadNumber(out int width)) throw new Exception("Width error");
            if (!ReadNumber(out int height)) throw new Exception("Height error");
            if (!ReadNumber(out int maxVal)) throw new Exception("maxVal error");

            if (width <= 0 || height <= 0)
                throw new Exception("Invalid image size.");

            return type switch
            {
                '3' => DecodeAscii(width, height, maxVal),
                '6' => DecodeBinary(width, height, maxVal),
                _ => throw new Exception("Unsupported PPM type.")
            };
        }


        // --- P3 ASCII ---
        private WriteableBitmap DecodeAscii(int width, int height, int maxVal)
        {
            var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            unsafe
            {
                bmp.Lock();
                int* p = (int*)bmp.BackBuffer;
                int index = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int a = 255;

                        // rgb
                        int r = ReadScaled(maxVal);
                        int g = ReadScaled(maxVal);
                        int b = ReadScaled(maxVal);

                        // ARGB
                        p[index++] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }

                bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
                bmp.Unlock();
            }

            return bmp;
        }


        // --- P6 BINARY ---
        private WriteableBitmap DecodeBinary(int width, int height, int maxVal)
        {
            var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            unsafe
            {
                bmp.Lock();
                int* p = (int*)bmp.BackBuffer;
                int index = 0;

                bool is16bit = maxVal > 255;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int a = 255;

                        // rgb
                        int r = ReadBinary(is16bit, maxVal);
                        int g = ReadBinary(is16bit, maxVal);
                        int b = ReadBinary(is16bit, maxVal);

                        // ARGB
                        p[index++] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }

                bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
                bmp.Unlock();
            }

            return bmp;
        }


        // --- READ BINARY PIXEL ---
        private int ReadBinary(bool is16bit, int maxVal)
        {
            if (is16bit)
            {
                int hi = reader.ReadByte();
                int lo = reader.ReadByte();

                int val16 = (hi << 8) | lo;
                return val16 * 255 / maxVal;
            }

            return (reader.ReadByte() * 255) / maxVal;
        }


        // --- READ ASCII PIXEL ---
        private int ReadScaled(int maxVal)
        {
            if (!ReadNumber(out int v)) return 0;
            return (v * 255) / maxVal;
        }


        // --- READ NUMBER ---
        private bool ReadNumber(out int value)
        {
            value = 0;
            int digits = 0;

            while (!reader.EndOfStream)
            {
                int c = reader.Peek();

                if (c == '#') { SkipComment(); continue; }
                if (char.IsWhiteSpace((char)c)) break;
                if (c < '0' || c > '9') return false;

                value = value * 10 + (c - '0');
                reader.SkipByte();
                digits++;
            }

            if (digits == 0) return false;

            SkipWhitespace();
            return true;
        }


        // --- SKIP WS ---
        private void SkipWhitespace()
        {
            while (!reader.EndOfStream)
            {
                int c = reader.Peek();

                if (c == '#') { SkipComment(); continue; }
                if (!char.IsWhiteSpace((char)c)) return;

                reader.SkipByte();
            }
        }


        // --- SKIP COMMENT ---
        private void SkipComment()
        {
            while (!reader.EndOfStream)
            {
                int c = reader.ReadByte();
                if (c == '\n') return;
            }
        }
    }
}
