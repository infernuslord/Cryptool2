#define _DEBUG_

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.Windows;

namespace Cryptool.Plugins.Keccak
{
    public static class KeccakHashFunction
    {

        public static byte[] Hash(byte[] input, int outputLength, int rate, int capacity, ref KeccakPres pres)
        {
            #if _DEBUG_
            Console.WriteLine("#Keccak: running Keccak with the following parameters:");
            Console.WriteLine(
                "#Keccak: output length\t{0} bits\n" +
                "#Keccak: state size\t\t{1} bits\n" +
                "#Keccak: bit rate\t\t{2} bits\n" +
                "#Keccak: capacity\t\t{3} bits\n\n"
                , outputLength, rate + capacity, rate, capacity);
            #endif

            /* map each bit of the input to a byte */
            byte[] inputInBits = ByteArrayToBitArray(input);

            /* create sponge instance */
            Sponge sponge = new Sponge(rate, capacity, ref pres);            

            /* absorb input */
            sponge.Absorb(inputInBits);

            /* squeeze sponge to obtain output */
            Debug.Assert(outputLength % 8 == 0);
            byte[] outputInBits = sponge.Squeeze(outputLength);

            /* reverse 'bit to byte' mapping */
            byte[] output = BitArrayToByteArray(outputInBits);

            #if _DEBUG_
            Console.WriteLine("#Keccak: successfully hashed {0} input bits to {1} output bits!", inputInBits.Length, outputInBits.Length);
            Console.WriteLine("#Keccak: all work is done!");
            #endif
           

            return output;
        }


        #region helper methods

        public static byte[] SubArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string ByteArrayToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string ByteArrayToIntString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length);
            foreach (byte b in bytes)
            {
                hex.Append((int)b);
            }
            return hex.ToString();
        }

        public static void PrintByteArray(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2} ", b);
            }
            Console.WriteLine(hex.ToString());
            Console.WriteLine(" - " + bytes.Length + " bytes");
        }

        public static void PrintBits(byte[] bytes)
        {
            string hexStr = ByteArrayToIntString(bytes);
            Console.WriteLine(hexStr + " - " + hexStr.Length + " bits");
        }

        public static void PrintBits(byte[] bytes, int laneSize)
        {
            /* only print bit state if lane size is small enough*/
            if (laneSize >= 16)
            {
                return;
            }

            string hexStr = "";
            StringBuilder hex = new StringBuilder(bytes.Length);
            int j = 0;

            foreach (byte b in bytes)
            {
                if (j % laneSize == 0)
                {
                    hex.AppendFormat("{0:00}: ", j / laneSize);
                }

                hex.Append((int)b);

                j++;
                if (j % laneSize == 0)
                {
                    hex.Append("\n");
                }
            }

            hexStr = hex.ToString();
            hex.Clear();
            Console.WriteLine(hexStr); // + " - " + (hexStr.Length - (j / value)) + " bits");
        }

        public static void PrintBytes(byte[] bytes, int laneSize)
        {
            /* only print byte state if lane size is large enough*/
            if (laneSize < 16 && laneSize % 8 != 0)
            {
                return;
            }

            StringBuilder binaryBytes = new StringBuilder(bytes.Length);
            StringBuilder bitString = new StringBuilder(8);
            char[] bitChars = new char[8];

            for (int i = 0; i < bytes.Length; i += 8)
            {
                if (i % laneSize == 0)
                {
                    binaryBytes.AppendFormat("\n{0:00}: ", i / laneSize);
                }

                for (int j = 0; j < 8; j++)
                {
                    bitString.Append((int)bytes[i + j]);
                }
                for (int j = 0; j < 8; j++)
                {
                    bitChars[j] = bitString.ToString().ElementAt(8 - 1 - j);
                }

                binaryBytes.AppendFormat("{0:X2} ", Convert.ToByte(new string(bitChars), 2));
                bitString.Clear();

            }
            Console.WriteLine(binaryBytes.ToString());
        }

        public static string GetByteArrayAsString(byte[] bytes, int laneSize)
        {          
            /* get bit state if lane size is small */
            if (laneSize < 16 && laneSize % 8 != 0)
            {
                string hexStr = "";
                StringBuilder hex = new StringBuilder(bytes.Length);
                int j = 0;

                foreach (byte b in bytes)
                {
                    //if (j % laneSize == 0)
                    //{
                    //    hex.AppendFormat("{0:00}: ", j / laneSize);
                    //}

                    hex.Append((int)b);

                    j++;
                    if (j % laneSize == 0)
                    {
                        hex.Append("\n");
                    }
                }

                hexStr = hex.ToString();
                hex.Clear();

                return hexStr;
            }
            /* get byte presentation of state otherwise (lane size at least 2 bytes) */
            else
            {
                StringBuilder binaryBytes = new StringBuilder(bytes.Length);
                StringBuilder bitString = new StringBuilder(8);
                char[] bitChars = new char[8];

                for (int i = 0; i < bytes.Length; i += 8)
                {
                    /* append line break at the end of a lane */
                    if (i != 0 && i % laneSize == 0)
                    {
                        binaryBytes.Append("\n");
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        bitString.Append((int)bytes[i + j]);
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        bitChars[j] = bitString.ToString().ElementAt(8 - 1 - j);
                    }

                    binaryBytes.AppendFormat("{0:X2} ", Convert.ToByte(new string(bitChars), 2));
                                       
                    bitString.Clear();
                }

                return binaryBytes.ToString();
            }
        }

        /**
         * Converts a byte array to an another byte array. The returned byte array contains the bit representation of the input byte array
         * where each byte represents a bit of the input byte array
         * */
        public static byte[] ByteArrayToBitArray(byte[] bytes)
        {
            List<byte> bitsInBytes = new List<byte>(bytes.Length * 8);
            string bitString;
            char[] bitChars = new char[8];

            foreach (byte b in bytes)
            {
                /* convert each byte into a bit-string */
                bitString = Convert.ToString(b, 2).PadLeft(8, '0');

                /* swap every bit to get the right order of bits in memory */
                for (int i = 0; i < 8; i++)
                {
                    bitChars[i] = bitString.ElementAt(8 - 1 - i);
                }

                foreach (char c in bitChars)
                {
                    if (c == '0')
                    {
                        bitsInBytes.Add(0x00);
                    }
                    else if (c == '1')
                    {
                        bitsInBytes.Add(0x01);
                    }
                }
            }

            return bitsInBytes.ToArray();
        }

        public static byte[] BitArrayToByteArray(byte[] bitsInBytes)
        {
            string c;
            char[] bitChars = new char[8];
            StringBuilder bitString = new StringBuilder(8);

            Debug.Assert(bitsInBytes.Length % 8 == 0);
            byte[] bytes = new byte[bitsInBytes.Length / 8];

            for (int i = 0; i < bytes.Length; i++)
            {
                bitString.Clear();
                for (int j = 0; j < 8; j++)
                {
                    c = bitsInBytes[i * 8 + j] == 0x01 ? "1" : "0";
                    bitString.Append(c);
                }

                /* swap back every bit to get the right order of bits in a byte */
                for (int k = 0; k < 8; k++)
                {
                    bitChars[k] = bitString.ToString().ElementAt(8 - 1 - k);
                }

                bytes[i] = Convert.ToByte(new string(bitChars), 2);
            }

            return bytes;
        }

        #endregion

    }
}
