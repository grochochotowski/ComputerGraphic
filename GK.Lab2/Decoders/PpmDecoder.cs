using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GK.Lab2.Decoders
{
    public class PpmDecoder
    {
        // --- BUFFER ---
        private readonly byte[] buffer;
        private int index;


        // --- CONSTRUCTOR ---
        public PpmDecoder(FileStream stream)
        {
            int length = (int)stream.Length;
            if (length > (1 << 30))
                throw new OutOfMemoryException("PPM is larger than 1GB.");

            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            index = 0;
        }


        // --- BASIC READ ---
        private int ReadByte() => buffer[index++];
        private int PeekByte() => buffer[index];
        private void SkipByte() => index++;
        private bool EndOfStream => index == buffer.Length;


        // --- DECODE ---
        public WriteableBitmap Decode()
        {
            SkipWhitespace();
            if (ReadByte() != 'P') return null;

            int type = ReadByte();

            SkipWhitespace();
            if (!ReadNumber(out int width)) return null;
            if (!ReadNumber(out int height)) return null;
            if (!ReadNumber(out int maxVal)) return null;

            if (width <= 0 || height <= 0 || maxVal <= 0)
                return null;

            return type switch
            {
                '3' => DecodeAscii(width, height, maxVal),
                '6' => DecodeBinary(width, height, maxVal),
                _ => null
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
                int pixelIndex = 0;

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
                        p[pixelIndex++] = (a << 24) | (r << 16) | (g << 8) | b;
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
                int pixelIndex = 0;

                // is 16-bit
                bool is16bit = maxVal > 255;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int a = 255;

                        // rgb
                        int r = ReadBinary(maxVal, is16bit);
                        int g = ReadBinary(maxVal, is16bit);
                        int b = ReadBinary(maxVal, is16bit);

                        // ARGB
                        p[pixelIndex++] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }

                bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
                bmp.Unlock();
            }

            return bmp;
        }


        // --- READ BINARY ---
        private int ReadBinary(int maxVal, bool is16bit)
        {
            if (EndOfStream) return 0;

            // 8-bit
            if (!is16bit)
                return (ReadByte() * 255) / maxVal;

            // 16-bit
            int high = ReadByte();
            int low = ReadByte();
            int v16 = (high << 8) | low;

            return (v16 * 255) / maxVal;
        }


        // --- READ ASCII ---
        private int ReadScaled(int maxVal)
        {
            if (!ReadNumber(out int v)) return 0;
            return (v * 255) / maxVal;
        }


        // --- READ NUMBER ---
        private bool ReadNumber(out int result)
        {
            result = 0;
            int count = 0;

            while (!EndOfStream)
            {
                int c = PeekByte();

                // comment
                if (c == '#') { SkipComment(); continue; }

                // stop
                if (char.IsWhiteSpace((char)c)) break;

                // invalid
                if (c < '0' || c > '9') return false;

                // digit
                result = result * 10 + (c - '0');
                SkipByte();
                count++;
            }

            if (count == 0) return false;

            SkipWhitespace();
            return true;
        }


        // --- SKIP WS ---
        private void SkipWhitespace()
        {
            while (!EndOfStream)
            {
                int c = PeekByte();

                if (c == '#') { SkipComment(); continue; }
                if (!char.IsWhiteSpace((char)c)) break;

                SkipByte();
            }
        }


        // --- SKIP COMMENT ---
        private void SkipComment()
        {
            while (!EndOfStream && ReadByte() != '\n') ;
        }
    }
}
