using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.Plugins.MD5Collider.Algorithm
{
    class IHVCalculator
    {
        private byte[] data;

        public IHVCalculator(byte[] data)
        {
            this.data = data;
        }

        public byte[] GetIHV()
        {
            int offset = 0;

            UInt32[] hv = new UInt32[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476 };
            while (data.Length > offset)
            {
                UInt32[] dataBlock = toLittleEndianIntegerArray(data, offset, 16);
                md5_compress(hv, dataBlock);
                offset += 64;
            }

            byte[] result = new byte[16];
            dumpLittleEndianIntegers(hv, result, 0);

            return result;
        }

        void md5_compress(UInt32[] ihv, UInt32[] block)
        {
            UInt32 a = ihv[0];
            UInt32 b = ihv[1];
            UInt32 c = ihv[2];
            UInt32 d = ihv[3];

            MD5_STEP(FF, ref a, b, c, d, block[0], 0xd76aa478, 7);
            MD5_STEP(FF, ref d, a, b, c, block[1], 0xe8c7b756, 12);
            MD5_STEP(FF, ref c, d, a, b, block[2], 0x242070db, 17);
            MD5_STEP(FF, ref b, c, d, a, block[3], 0xc1bdceee, 22);
            MD5_STEP(FF, ref a, b, c, d, block[4], 0xf57c0faf, 7);
            MD5_STEP(FF, ref d, a, b, c, block[5], 0x4787c62a, 12);
            MD5_STEP(FF, ref c, d, a, b, block[6], 0xa8304613, 17);
            MD5_STEP(FF, ref b, c, d, a, block[7], 0xfd469501, 22);
            MD5_STEP(FF, ref a, b, c, d, block[8], 0x698098d8, 7);
            MD5_STEP(FF, ref d, a, b, c, block[9], 0x8b44f7af, 12);
            MD5_STEP(FF, ref c, d, a, b, block[10], 0xffff5bb1, 17);
            MD5_STEP(FF, ref b, c, d, a, block[11], 0x895cd7be, 22);
            MD5_STEP(FF, ref a, b, c, d, block[12], 0x6b901122, 7);
            MD5_STEP(FF, ref d, a, b, c, block[13], 0xfd987193, 12);
            MD5_STEP(FF, ref c, d, a, b, block[14], 0xa679438e, 17);
            MD5_STEP(FF, ref b, c, d, a, block[15], 0x49b40821, 22);
            MD5_STEP(GG, ref a, b, c, d, block[1], 0xf61e2562, 5);
            MD5_STEP(GG, ref d, a, b, c, block[6], 0xc040b340, 9);
            MD5_STEP(GG, ref c, d, a, b, block[11], 0x265e5a51, 14);
            MD5_STEP(GG, ref b, c, d, a, block[0], 0xe9b6c7aa, 20);
            MD5_STEP(GG, ref a, b, c, d, block[5], 0xd62f105d, 5);
            MD5_STEP(GG, ref d, a, b, c, block[10], 0x02441453, 9);
            MD5_STEP(GG, ref c, d, a, b, block[15], 0xd8a1e681, 14);
            MD5_STEP(GG, ref b, c, d, a, block[4], 0xe7d3fbc8, 20);
            MD5_STEP(GG, ref a, b, c, d, block[9], 0x21e1cde6, 5);
            MD5_STEP(GG, ref d, a, b, c, block[14], 0xc33707d6, 9);
            MD5_STEP(GG, ref c, d, a, b, block[3], 0xf4d50d87, 14);
            MD5_STEP(GG, ref b, c, d, a, block[8], 0x455a14ed, 20);
            MD5_STEP(GG, ref a, b, c, d, block[13], 0xa9e3e905, 5);
            MD5_STEP(GG, ref d, a, b, c, block[2], 0xfcefa3f8, 9);
            MD5_STEP(GG, ref c, d, a, b, block[7], 0x676f02d9, 14);
            MD5_STEP(GG, ref b, c, d, a, block[12], 0x8d2a4c8a, 20);
            MD5_STEP(HH, ref a, b, c, d, block[5], 0xfffa3942, 4);
            MD5_STEP(HH, ref d, a, b, c, block[8], 0x8771f681, 11);
            MD5_STEP(HH, ref c, d, a, b, block[11], 0x6d9d6122, 16);
            MD5_STEP(HH, ref b, c, d, a, block[14], 0xfde5380c, 23);
            MD5_STEP(HH, ref a, b, c, d, block[1], 0xa4beea44, 4);
            MD5_STEP(HH, ref d, a, b, c, block[4], 0x4bdecfa9, 11);
            MD5_STEP(HH, ref c, d, a, b, block[7], 0xf6bb4b60, 16);
            MD5_STEP(HH, ref b, c, d, a, block[10], 0xbebfbc70, 23);
            MD5_STEP(HH, ref a, b, c, d, block[13], 0x289b7ec6, 4);
            MD5_STEP(HH, ref d, a, b, c, block[0], 0xeaa127fa, 11);
            MD5_STEP(HH, ref c, d, a, b, block[3], 0xd4ef3085, 16);
            MD5_STEP(HH, ref b, c, d, a, block[6], 0x04881d05, 23);
            MD5_STEP(HH, ref a, b, c, d, block[9], 0xd9d4d039, 4);
            MD5_STEP(HH, ref d, a, b, c, block[12], 0xe6db99e5, 11);
            MD5_STEP(HH, ref c, d, a, b, block[15], 0x1fa27cf8, 16);
            MD5_STEP(HH, ref b, c, d, a, block[2], 0xc4ac5665, 23);
            MD5_STEP(II, ref a, b, c, d, block[0], 0xf4292244, 6);
            MD5_STEP(II, ref d, a, b, c, block[7], 0x432aff97, 10);
            MD5_STEP(II, ref c, d, a, b, block[14], 0xab9423a7, 15);
            MD5_STEP(II, ref b, c, d, a, block[5], 0xfc93a039, 21);
            MD5_STEP(II, ref  a, b, c, d, block[12], 0x655b59c3, 6);
            MD5_STEP(II, ref  d, a, b, c, block[3], 0x8f0ccc92, 10);
            MD5_STEP(II, ref  c, d, a, b, block[10], 0xffeff47d, 15);
            MD5_STEP(II, ref  b, c, d, a, block[1], 0x85845dd1, 21);
            MD5_STEP(II, ref  a, b, c, d, block[8], 0x6fa87e4f, 6);
            MD5_STEP(II, ref  d, a, b, c, block[15], 0xfe2ce6e0, 10);
            MD5_STEP(II, ref c, d, a, b, block[6], 0xa3014314, 15);
            MD5_STEP(II, ref b, c, d, a, block[13], 0x4e0811a1, 21);
            MD5_STEP(II, ref a, b, c, d, block[4], 0xf7537e82, 6);
            MD5_STEP(II, ref d, a, b, c, block[11], 0xbd3af235, 10);
            MD5_STEP(II, ref c, d, a, b, block[2], 0x2ad7d2bb, 15);
            MD5_STEP(II, ref b, c, d, a, block[9], 0xeb86d391, 21);

            ihv[0] += a;
            ihv[1] += b;
            ihv[2] += c;
            ihv[3] += d;
        }

        delegate UInt32 RoundFunctionDelegate(UInt32 b, UInt32 c, UInt32 d);

        void MD5_STEP(RoundFunctionDelegate f, ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 m, UInt32 ac, Int32 rc)
        {
            a += f(b, c, d) + m + ac;
            a = (a << rc | a >> (32 - rc)) + b;
        }

        private void dumpLittleEndianIntegers(UInt32[] sourceArray, byte[] targetArray, int targetOffset)
        {
            for (int i = 0; i < sourceArray.Length; i++)
                dumpLittleEndianInteger(sourceArray[i], targetArray, targetOffset + i * 4);
        }

        private void dumpLittleEndianInteger(UInt32 integerValue, byte[] targetArray, int targetOffset)
        {
            byte[] result = BitConverter.GetBytes(integerValue);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(result);

            Array.Copy(result, 0, targetArray, targetOffset, 4);
        }

        private UInt32 toLittleEndianInteger(byte[] bytes, int offset)
        {
            byte[] bytesInProperOrder = new byte[4];
            Array.Copy(bytes, offset, bytesInProperOrder, 0, 4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytesInProperOrder);

            return BitConverter.ToUInt32(bytesInProperOrder, 0);
        }
        private UInt32 toLittleEndianInteger(byte[] bytes)
        {
            return toLittleEndianInteger(bytes, 0);
        }

        private UInt32[] toLittleEndianIntegerArray(byte[] bytes, int offset, int integerCount)
        {
            UInt32[] result = new UInt32[integerCount];
            for (int i = 0; i < result.Length; i++)
                result[i] = toLittleEndianInteger(bytes, offset + i * 4);

            return result;
        }

        UInt32 FF(UInt32 b, UInt32 c, UInt32 d)
        { return d ^ (b & (c ^ d)); }

        UInt32 GG(UInt32 b, UInt32 c, UInt32 d)
        { return c ^ (d & (b ^ c)); }

        UInt32 HH(UInt32 b, UInt32 c, UInt32 d)
        { return b ^ c ^ d; }

        UInt32 II(UInt32 b, UInt32 c, UInt32 d)
        { return c ^ (b | ~d); }

        UInt32 RL(UInt32 x, int n)
        { return (x << n) | (x >> (32 - n)); }

        UInt32 RR(UInt32 x, int n)
        { return (x >> n) | (x << (32 - n)); }
    }
}
