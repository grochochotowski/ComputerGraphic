using System;
using System.IO;

namespace GK.Lab2.Decoders
{
    public class BufferedBinaryReader : IDisposable
    {
        // --- FIELDS ---
        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int size;
            
        private int offset;
        private int buffered;

        // --- PROPS ---
        public bool EndOfStream => stream.Position == stream.Length && Available == 0;
        public int Available => Math.Max(0, buffered - offset);


        // --- CONSTRUCTOR ---
        public BufferedBinaryReader(Stream s, int bufferSize)
        {
            stream = s;
            size = bufferSize;

            buffer = new byte[size];
            offset = size;
        }


        // --- REFILL BUFFER ---
        private void Fill()
        {
            // unread = old bytes
            int unread = size - offset;

            // free space
            int toRead = size - unread;

            // reset ptr
            offset = 0;
            buffered = unread;

            // move old bytes
            if (unread > 0)
                Buffer.BlockCopy(buffer, toRead, buffer, 0, unread);

            // read new bytes
            while (toRead > 0)
            {
                int read = stream.Read(buffer, unread, toRead);
                if (read == 0) break;

                unread += read;
                buffered += read;
                toRead -= read;
            }
        }


        // --- PEEK ---
        public int Peek()
        {
            if (Available == 0)
            {
                Fill();
                if (Available == 0)
                    return -1;
            }

            return buffer[offset];
        }


        // --- READ ---
        public int ReadByte()
        {
            int b = Peek();
            offset++;
            return b;
        }


        // --- SKIP ---
        public void SkipByte() => offset++;


        // --- DISPOSE ---
        public void Dispose() => stream.Dispose();
    }
}
