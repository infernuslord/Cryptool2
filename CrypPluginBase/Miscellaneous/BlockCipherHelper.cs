using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;

namespace Cryptool.PluginBase.Miscellaneous
{
    public static class BlockCipherHelper
    {
        public enum PaddingType { None, Zeros, PKCS7, ANSIX923, ISO10126, OneZeros };

        public static byte[] StreamToByteArray(ICryptoolStream stream)
        {
            byte[] buf = new byte[stream.Length];

            CStreamReader reader = stream.CreateReader();
            reader.WaitEof();
            reader.Seek(0, System.IO.SeekOrigin.Begin);
            reader.ReadFully(buf);
            reader.Close();

            return buf;
        }

        public static ICryptoolStream AppendPadding(ICryptoolStream input, PaddingType paddingtype, int blocksize)
        {
            return new CStreamWriter(AppendPadding(StreamToByteArray(input), paddingtype, blocksize));
        }

        public static byte[] AppendPadding(byte[] input, PaddingType paddingtype, int blocksize)
        {
            long l = blocksize - (input.Length % blocksize);

            if (paddingtype == PaddingType.None)
            {
                if (l % blocksize != 0) throw new Exception("Input must be a multiple of blocksize ("+blocksize+" bytes) if no padding is used.");
                return input;
            }
            else if (paddingtype == PaddingType.Zeros)
            {
                l %= blocksize; // add no zeros if message length is multiple of blocksize
            }

            byte[] buf = new byte[input.Length + l];
            Array.Copy(input, buf, input.Length);

            switch (paddingtype)
            {
                case PaddingType.Zeros:
                    for (int i = 0; i < l; i++)
                        buf[input.Length + i] = 0x00;
                    break;

                case PaddingType.OneZeros:
                    buf[input.Length] = 0x01;
                    for (int i = 1; i < l; i++)
                        buf[input.Length + i] = 0x00;
                    break;

                case PaddingType.PKCS7:
                    for (int i = 0; i < l; i++)
                        buf[input.Length + i] = (byte)l;
                    break;

                case PaddingType.ISO10126:
                    Random random = new Random();
                    for (int i = 0; i < l; i++)
                        buf[input.Length + i] = (byte)random.Next(256);
                    buf[buf.Length - 1] = (byte)l;
                    break;

                case PaddingType.ANSIX923:
                    for (int i = 0; i < l; i++)
                        buf[input.Length + i] = 0;
                    buf[buf.Length - 1] = (byte)l;
                    break;
            }

            return buf;
        }

        public static int StripPadding(byte[] input, int bytesRead, PaddingType paddingtype, int blocksize)
        {
            //if (bytesRead != input.Length) throw new Exception("Unexpected size of padding");
            if (bytesRead % blocksize != 0) throw new Exception("Unexpected blocksize ("+(bytesRead % blocksize)+" bytes) in padding ("+blocksize+" bytes expected)");

            if (paddingtype == PaddingType.Zeros)   // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 00 00 00 00 |
            {
                for (bytesRead--; bytesRead > 0; bytesRead--)
                    if (input[bytesRead] != 0) break;
                bytesRead++;
                if (bytesRead == 0) throw new Exception("Error in Zeros padding");
            }

            if (paddingtype == PaddingType.OneZeros)    // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 01 00 00 00 |
            {
                for (bytesRead--; bytesRead > 0; bytesRead--)
                    if (input[bytesRead] != 0) break;
                if (bytesRead < 0 || input[bytesRead] != 0x01) throw new Exception("Unexpected byte in 1-0 padding");
            }

            if (paddingtype == PaddingType.PKCS7)   // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 04 04 04 04 |
            {
                int l = input[input.Length - 1];
                if (l>blocksize) throw new Exception("Unexpected byte in PKCS7 padding");
                for (int i = 1; i <= l; i++)
                    if (input[input.Length-i] != l) throw new Exception("Unexpected byte in PKCS7 padding");
                bytesRead -= l;
            }

            if (paddingtype == PaddingType.ISO10126)    // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 81 A6 23 04 |
            {
                int l = input[input.Length - 1];
                if (l > blocksize) throw new Exception("Unexpected byte in ISO10126 padding");
                bytesRead -= l;
            }

            if (paddingtype == PaddingType.ANSIX923)    // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 00 00 00 04 |
            {
                int l = input[input.Length - 1];
                if (l > blocksize) throw new Exception("Unexpected byte in ANSIX923 padding");
                for (int i = 2; i <= l; i++)
                    if (input[input.Length - i] != 0) throw new Exception("Unexpected byte in ANSIX923 padding");
                bytesRead -= l;
            }

            return bytesRead;
        }

        public static byte[] StripPadding(byte[] input, PaddingType paddingtype, int blocksize)
        {
            int validBytes = StripPadding(input, input.Length, paddingtype, blocksize);
            byte[] buf = new byte[validBytes];
            Array.Copy(input, buf, validBytes);
            return buf;
        }

        public static ICryptoolStream StripPadding(ICryptoolStream input, PaddingType paddingtype, int blocksize)
        {
            return new CStreamWriter(StripPadding(StreamToByteArray(input), paddingtype, blocksize));
        }
                            
    }
}
