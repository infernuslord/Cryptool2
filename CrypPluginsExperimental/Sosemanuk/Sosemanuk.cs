/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, 
   software distributed under the License is distributed on an 
   "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
   either express or implied. See the License for the specific 
   language governing permissions and limitations under the License.
*/
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System;

namespace Cryptool.Plugins.Sosemanuk
{
    [Author("Robin Nelle", "rnelle@mail.uni-mannheim.de",
        "Uni Mannheim - Lehrstuhl Prof. Dr. Armknecht",
        "http://ls.wim.uni-mannheim.de/")]
    [PluginInfo("Sosemanuk.Properties.Resources", "PluginCaption",
        "PluginTooltip", "Sosemanuk/userdoc.xml",
        new[] { "Sosemanuk/icon.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]

    public class Sosemanuk : ICrypComponent
    {
        #region Private Variables
        private readonly SosemanukSettings settings
            = new SosemanukSettings();

        // Subkeys for Serpent24: 100 32-bit words
        private int[] serpent24SubKeys = new int[100];

        //Internal cipher state
        private int[] lfsr = new int[10];
        private int fsmR1, fsmR2;

        //Input
        private string inputKey;
        private string inputIV;
        private string inputString;

        //Keystream
        string keyStream = "";

        //Plaintext
        byte[] plainText = new byte[0];

        //Output
        private string outputString;

        /*
         * mulAlpha[] is used to multiply a word by alpha; 
         *mulAlpha[x] is equal to x * alpha^4.
         *
         * divAlpha[] is used to divide a word by alpha; 
         * divAlpha[x] is equal to x / alpha.
         */
        private static readonly int[] mulAlpha = new int[256];
        private static readonly int[] divAlpha = new int[256];

        #endregion

        #region Data Properties
        [PropertyInfo(Direction.InputData, "InputStringCaption",
            "InputStringTooltip", false)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                this.inputString = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption",
            "InputKeyTooltip", true)]
        public string InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", 
            "InputIVTooltip", true)]
        public string InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", 
            "OutputStringTooltip", true)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        #endregion

        public ISettings Settings
        { get { return settings; } }

        public UserControl Presentation
        {
            get { return null; }
        }

        static Sosemanuk()
        {
            /*
             * We first build exponential and logarithm tables
             * relatively to beta in F_{2^8}. We set 
             * log(0x00) = 0xFF conventionaly, but this is 
             * actually not used in our computations.
             */
            int[] expb = new int[256];
            for (int i = 0, x = 0x01; i < 0xFF; i++)
            {
                expb[i] = x;
                x <<= 1;
                if (x > 0xFF)
                    x ^= 0x1A9;
            }
            expb[0xFF] = 0x00;
            int[] logb = new int[256];
            for (int i = 0; i < 0x100; i++)
                logb[expb[i]] = i;

            /*
             * We now compute mulAlpha[] and divAlpha[]. For all
             * x != 0, we work with invertible numbers, which are
             * as such powers of beta. Multiplication (in F_{2^8})
             * is then implemented as integer addition modulo 255,
             * over the exponents computed by the logb[] table.
             *
             * We have the following equations:
             * alpha^4 = beta^23 * alpha^3 + beta^245 * alpha^2
             *           + beta^48 * alpha + beta^239
             * 1/alpha = beta^16 * alpha^3 + beta^39 * alpha^2
             *           + beta^6 * alpha + beta^64
             */
            mulAlpha[0x00] = 0x00000000;
            divAlpha[0x00] = 0x00000000;
            for (int x = 1; x < 0x100; x++)
            {
                int ex = logb[x];
                mulAlpha[x] = (expb[(ex + 23) % 255] << 24)
                    | (expb[(ex + 245) % 255] << 16)
                    | (expb[(ex + 48) % 255] << 8)
                    | expb[(ex + 239) % 255];
                divAlpha[x] = (expb[(ex + 16) % 255] << 24)
                    | (expb[(ex + 39) % 255] << 16)
                    | (expb[(ex + 6) % 255] << 8)
                    | expb[(ex + 64) % 255];
            }
        }

        /**
	    * Decode a 32-bit value from a buffer (little-endian).
	    *
	    * @param buf   the input buffer
	    * @param off   the input offset
	    * @return  the decoded value
	    */
        private static int decode32le(byte[] buf, int off)
        {
            return (buf[off] & 0xFF)
                | ((buf[off + 1] & 0xFF) << 8)
                | ((buf[off + 2] & 0xFF) << 16)
                | ((buf[off + 3] & 0xFF) << 24);
        }

        /**
	    * Encode a 32-bit value into a buffer (little-endian).
	    *
	    * @param val   the value to encode
	    * @param buf   the output buffer
	    * @param off   the output offset
	    */
        private static void encode32le(int val, byte[] buf,int off)
        {
            buf[off] = (byte)val;
            buf[off + 1] = (byte)(val >> 8);
            buf[off + 2] = (byte)(val >> 16);
            buf[off + 3] = (byte)(val >> 24);
        }

        /**
        * Left-rotate a 32-bit value by some bit.
        *
        * @param val   the value to rotate
        * @param n     the rotation count (between 1 and 31)
           */
        private static int rotateLeft(int val, int n)
        {
            int changeBitRotateLeft = (val >> (32 - n));
            if (val < 0)
            {
                changeBitRotateLeft = (int)(((uint)val) >> 
                    (32 - n));
            }
            return (val << n) | changeBitRotateLeft;
        }

        /**
        * Set the private key. The key length must be between 1
        * and 32 bytes.
         *
         * @param key   the private key
         */
        public void setKey(byte[] key)
        {
            byte[] lkey;
            if (key.Length == 32)
            {
                lkey = key;
            }
            else
            {
                lkey = new byte[32];
                System.Array.Copy(key, 0, lkey, 0, key.Length);
                lkey[key.Length] = 0x01;
             
                for (int i1 = key.Length + 1; i1 <
                    lkey.Length; i1++)
                {
                    lkey[i1] = 0x00;
                }
            }

            int w0, w1, w2, w3, w4, w5, w6, w7;
            int r0, r1, r2, r3, r4, tt;
            int i = 0;

            w0 = decode32le(lkey, 0);
            w1 = decode32le(lkey, 4);
            w2 = decode32le(lkey, 8);
            w3 = decode32le(lkey, 12);
            w4 = decode32le(lkey, 16);
            w5 = decode32le(lkey, 20);
            w6 = decode32le(lkey, 24);
            w7 = decode32le(lkey, 28);
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (0));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (0+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (0+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (0+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r4 = r0;
            r0 |= r3;
            r3 ^= r1;
            r1 &= r4;
            r4 ^= r2;
            r2 ^= r3;
            r3 &= r0;
            r4 |= r1;
            r3 ^= r4;
            r0 ^= r1;
            r4 &= r0;
            r1 ^= r3;
            r4 ^= r2;
            r1 |= r0;
            r1 ^= r2;
            r0 ^= r3;
            r2 = r1;
            r1 |= r3;
            r1 ^= r0;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r4;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (4));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (4+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (4+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (4+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r4 = r0;
            r0 &= r2;
            r0 ^= r3;
            r2 ^= r1;
            r2 ^= r0;
            r3 |= r4;
            r3 ^= r1;
            r4 ^= r2;
            r1 = r3;
            r3 |= r4;
            r3 ^= r0;
            r0 &= r1;
            r4 ^= r0;
            r1 ^= r3;
            r1 ^= r4;
            r4 = ~r4;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (8));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (8+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (8+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (8+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r0 = ~r0;
            r2 = ~r2;
            r4 = r0;
            r0 &= r1;
            r2 ^= r0;
            r0 |= r3;
            r3 ^= r2;
            r1 ^= r0;
            r0 ^= r4;
            r4 |= r1;
            r1 ^= r3;
            r2 |= r0;
            r2 &= r4;
            r0 ^= r1;
            r1 &= r2;
            r1 ^= r0;
            r0 &= r2;
            r0 ^= r4;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r1;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (12));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (12+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (12+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (12+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r3 ^= r0;
            r4 = r1;
            r1 &= r3;
            r4 ^= r2;
            r1 ^= r0;
            r0 |= r3;
            r0 ^= r4;
            r4 ^= r3;
            r3 ^= r2;
            r2 |= r1;
            r2 ^= r4;
            r4 = ~r4;
            r4 |= r1;
            r1 ^= r3;
            r1 ^= r4;
            r3 |= r0;
            r1 ^= r3;
            r4 ^= r3;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r0;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (16));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (16+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (16+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (16+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r4 = r1;
            r1 |= r2;
            r1 ^= r3;
            r4 ^= r2;
            r2 ^= r1;
            r3 |= r4;
            r3 &= r0;
            r4 ^= r2;
            r3 ^= r1;
            r1 |= r4;
            r1 ^= r0;
            r0 |= r4;
            r0 ^= r2;
            r1 ^= r4;
            r2 ^= r1;
            r1 &= r0;
            r1 ^= r4;
            r2 = ~r2;
            r2 |= r0;
            r4 ^= r2;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r0;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (20));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (20+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (20+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (20+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r2 = ~r2;
            r4 = r3;
            r3 &= r0;
            r0 ^= r4;
            r3 ^= r2;
            r2 |= r4;
            r1 ^= r3;
            r2 ^= r0;
            r0 |= r1;
            r2 ^= r1;
            r4 ^= r0;
            r0 |= r3;
            r0 ^= r2;
            r4 ^= r3;
            r4 ^= r0;
            r3 = ~r3;
            r2 &= r4;
            r2 ^= r3;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r2;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (24));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (24+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (24+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (24+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r0 ^= r1;
            r1 ^= r3;
            r3 = ~r3;
            r4 = r1;
            r1 &= r0;
            r2 ^= r3;
            r1 ^= r2;
            r2 |= r4;
            r4 ^= r3;
            r3 &= r1;
            r3 ^= r0;
            r4 ^= r1;
            r4 ^= r2;
            r2 ^= r0;
            r0 &= r3;
            r2 = ~r2;
            r0 ^= r4;
            r4 |= r3;
            r2 ^= r4;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r2;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (28));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (28+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (28+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (28+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r1 ^= r3;
            r3 = ~r3;
            r2 ^= r3;
            r3 ^= r0;
            r4 = r1;
            r1 &= r3;
            r1 ^= r2;
            r4 ^= r3;
            r0 ^= r4;
            r2 &= r4;
            r2 ^= r0;
            r0 &= r1;
            r3 ^= r0;
            r4 |= r1;
            r4 ^= r0;
            r0 |= r3;
            r0 ^= r2;
            r2 &= r3;
            r0 = ~r0;
            r4 ^= r2;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r3;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (32));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (32+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (32+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (32+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r4 = r0;
            r0 |= r3;
            r3 ^= r1;
            r1 &= r4;
            r4 ^= r2;
            r2 ^= r3;
            r3 &= r0;
            r4 |= r1;
            r3 ^= r4;
            r0 ^= r1;
            r4 &= r0;
            r1 ^= r3;
            r4 ^= r2;
            r1 |= r0;
            r1 ^= r2;
            r0 ^= r3;
            r2 = r1;
            r1 |= r3;
            r1 ^= r0;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r4;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (36));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (36+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (36+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (36+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r4 = r0;
            r0 &= r2;
            r0 ^= r3;
            r2 ^= r1;
            r2 ^= r0;
            r3 |= r4;
            r3 ^= r1;
            r4 ^= r2;
            r1 = r3;
            r3 |= r4;
            r3 ^= r0;
            r0 &= r1;
            r4 ^= r0;
            r1 ^= r3;
            r1 ^= r4;
            r4 = ~r4;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (40));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (40+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (40+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (40+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r0 = ~r0;
            r2 = ~r2;
            r4 = r0;
            r0 &= r1;
            r2 ^= r0;
            r0 |= r3;
            r3 ^= r2;
            r1 ^= r0;
            r0 ^= r4;
            r4 |= r1;
            r1 ^= r3;
            r2 |= r0;
            r2 &= r4;
            r0 ^= r1;
            r1 &= r2;
            r1 ^= r0;
            r0 &= r2;
            r0 ^= r4;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r1;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (44));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (44+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (44+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (44+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r3 ^= r0;
            r4 = r1;
            r1 &= r3;
            r4 ^= r2;
            r1 ^= r0;
            r0 |= r3;
            r0 ^= r4;
            r4 ^= r3;
            r3 ^= r2;
            r2 |= r1;
            r2 ^= r4;
            r4 = ~r4;
            r4 |= r1;
            r1 ^= r3;
            r1 ^= r4;
            r3 |= r0;
            r1 ^= r3;
            r4 ^= r3;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r0;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (48));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (48+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (48+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (48+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r4 = r1;
            r1 |= r2;
            r1 ^= r3;
            r4 ^= r2;
            r2 ^= r1;
            r3 |= r4;
            r3 &= r0;
            r4 ^= r2;
            r3 ^= r1;
            r1 |= r4;
            r1 ^= r0;
            r0 |= r4;
            r0 ^= r2;
            r1 ^= r4;
            r2 ^= r1;
            r1 &= r0;
            r1 ^= r4;
            r2 = ~r2;
            r2 |= r0;
            r4 ^= r2;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r0;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (52));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (52+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (52+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (52+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r2 = ~r2;
            r4 = r3;
            r3 &= r0;
            r0 ^= r4;
            r3 ^= r2;
            r2 |= r4;
            r1 ^= r3;
            r2 ^= r0;
            r0 |= r1;
            r2 ^= r1;
            r4 ^= r0;
            r0 |= r3;
            r0 ^= r2;
            r4 ^= r3;
            r4 ^= r0;
            r3 = ~r3;
            r2 &= r4;
            r2 ^= r3;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r2;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (56));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (56+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (56+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (56+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r0 ^= r1;
            r1 ^= r3;
            r3 = ~r3;
            r4 = r1;
            r1 &= r0;
            r2 ^= r3;
            r1 ^= r2;
            r2 |= r4;
            r4 ^= r3;
            r3 &= r1;
            r3 ^= r0;
            r4 ^= r1;
            r4 ^= r2;
            r2 ^= r0;
            r0 &= r3;
            r2 = ~r2;
            r0 ^= r4;
            r4 |= r3;
            r2 ^= r4;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r2;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (60));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (60+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (60+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (60+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r1 ^= r3;
            r3 = ~r3;
            r2 ^= r3;
            r3 ^= r0;
            r4 = r1;
            r1 &= r3;
            r1 ^= r2;
            r4 ^= r3;
            r0 ^= r4;
            r2 &= r4;
            r2 ^= r0;
            r0 &= r1;
            r3 ^= r0;
            r4 |= r1;
            r4 ^= r0;
            r0 |= r3;
            r0 ^= r2;
            r2 &= r3;
            r0 = ~r0;
            r4 ^= r2;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r3;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (64));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (64+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (64+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (64+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r4 = r0;
            r0 |= r3;
            r3 ^= r1;
            r1 &= r4;
            r4 ^= r2;
            r2 ^= r3;
            r3 &= r0;
            r4 |= r1;
            r3 ^= r4;
            r0 ^= r1;
            r4 &= r0;
            r1 ^= r3;
            r4 ^= r2;
            r1 |= r0;
            r1 ^= r2;
            r0 ^= r3;
            r2 = r1;
            r1 |= r3;
            r1 ^= r0;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r4;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (68));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (68+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (68+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (68+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r4 = r0;
            r0 &= r2;
            r0 ^= r3;
            r2 ^= r1;
            r2 ^= r0;
            r3 |= r4;
            r3 ^= r1;
            r4 ^= r2;
            r1 = r3;
            r3 |= r4;
            r3 ^= r0;
            r0 &= r1;
            r4 ^= r0;
            r1 ^= r3;
            r1 ^= r4;
            r4 = ~r4;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (72));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (72+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (72+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (72+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r0 = ~r0;
            r2 = ~r2;
            r4 = r0;
            r0 &= r1;
            r2 ^= r0;
            r0 |= r3;
            r3 ^= r2;
            r1 ^= r0;
            r0 ^= r4;
            r4 |= r1;
            r1 ^= r3;
            r2 |= r0;
            r2 &= r4;
            r0 ^= r1;
            r1 &= r2;
            r1 ^= r0;
            r0 &= r2;
            r0 ^= r4;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r1;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (76));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (76+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (76+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (76+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r3 ^= r0;
            r4 = r1;
            r1 &= r3;
            r4 ^= r2;
            r1 ^= r0;
            r0 |= r3;
            r0 ^= r4;
            r4 ^= r3;
            r3 ^= r2;
            r2 |= r1;
            r2 ^= r4;
            r4 = ~r4;
            r4 |= r1;
            r1 ^= r3;
            r1 ^= r4;
            r3 |= r0;
            r1 ^= r3;
            r4 ^= r3;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r0;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (80));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (80+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (80+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (80+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r4 = r1;
            r1 |= r2;
            r1 ^= r3;
            r4 ^= r2;
            r2 ^= r1;
            r3 |= r4;
            r3 &= r0;
            r4 ^= r2;
            r3 ^= r1;
            r1 |= r4;
            r1 ^= r0;
            r0 |= r4;
            r0 ^= r2;
            r1 ^= r4;
            r2 ^= r1;
            r1 &= r0;
            r1 ^= r4;
            r2 = ~r2;
            r2 |= r0;
            r4 ^= r2;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r0;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (84));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (84+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (84+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (84+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r2 = ~r2;
            r4 = r3;
            r3 &= r0;
            r0 ^= r4;
            r3 ^= r2;
            r2 |= r4;
            r1 ^= r3;
            r2 ^= r0;
            r0 |= r1;
            r2 ^= r1;
            r4 ^= r0;
            r0 |= r3;
            r0 ^= r2;
            r4 ^= r3;
            r4 ^= r0;
            r3 = ~r3;
            r2 &= r4;
            r2 ^= r3;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r2;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (88));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (88+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (88+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (88+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r0 ^= r1;
            r1 ^= r3;
            r3 = ~r3;
            r4 = r1;
            r1 &= r0;
            r2 ^= r3;
            r1 ^= r2;
            r2 |= r4;
            r4 ^= r3;
            r3 &= r1;
            r3 ^= r0;
            r4 ^= r1;
            r4 ^= r2;
            r2 ^= r0;
            r0 &= r3;
            r2 = ~r2;
            r0 ^= r4;
            r4 |= r3;
            r2 ^= r4;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r2;
            tt = w4 ^ w7 ^ w1 ^ w3 ^ 
                (unchecked((int)0x9E3779B9) ^ (92));
            w4 = rotateLeft(tt, 11);
            tt = w5 ^ w0 ^ w2 ^ w4 ^ 
                (unchecked((int)0x9E3779B9) ^ (92+1));
            w5 = rotateLeft(tt, 11);
            tt = w6 ^ w1 ^ w3 ^ w5 ^ 
                (unchecked((int)0x9E3779B9) ^ (92+2));
            w6 = rotateLeft(tt, 11);
            tt = w7 ^ w2 ^ w4 ^ w6 ^ 
                (unchecked((int)0x9E3779B9) ^ (92+3));
            w7 = rotateLeft(tt, 11);
            r0 = w4;
            r1 = w5;
            r2 = w6;
            r3 = w7;
            r1 ^= r3;
            r3 = ~r3;
            r2 ^= r3;
            r3 ^= r0;
            r4 = r1;
            r1 &= r3;
            r1 ^= r2;
            r4 ^= r3;
            r0 ^= r4;
            r2 &= r4;
            r2 ^= r0;
            r0 &= r1;
            r3 ^= r0;
            r4 |= r1;
            r4 ^= r0;
            r0 |= r3;
            r0 ^= r2;
            r2 &= r3;
            r0 = ~r0;
            r4 ^= r2;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r4;
            serpent24SubKeys[i++] = r0;
            serpent24SubKeys[i++] = r3;
            tt = w0 ^ w3 ^ w5 ^ w7 ^ 
                (unchecked((int)0x9E3779B9) ^ (96));
            w0 = rotateLeft(tt, 11);
            tt = w1 ^ w4 ^ w6 ^ w0 ^ 
                (unchecked((int)0x9E3779B9) ^ (96+1));
            w1 = rotateLeft(tt, 11);
            tt = w2 ^ w5 ^ w7 ^ w1 ^ 
                (unchecked((int)0x9E3779B9) ^ (96+2));
            w2 = rotateLeft(tt, 11);
            tt = w3 ^ w6 ^ w0 ^ w2 ^ 
                (unchecked((int)0x9E3779B9) ^ (96+3));
            w3 = rotateLeft(tt, 11);
            r0 = w0;
            r1 = w1;
            r2 = w2;
            r3 = w3;
            r4 = r0;
            r0 |= r3;
            r3 ^= r1;
            r1 &= r4;
            r4 ^= r2;
            r2 ^= r3;
            r3 &= r0;
            r4 |= r1;
            r3 ^= r4;
            r0 ^= r1;
            r4 &= r0;
            r1 ^= r3;
            r4 ^= r2;
            r1 |= r0;
            r1 ^= r2;
            r0 ^= r3;
            r2 = r1;
            r1 |= r3;
            r1 ^= r0;
            serpent24SubKeys[i++] = r1;
            serpent24SubKeys[i++] = r2;
            serpent24SubKeys[i++] = r3;
            serpent24SubKeys[i++] = r4;
        }

        /**
         * Set the IV. 
         *
         * @param iv   the IV 
         */
        public void setIV(byte[] iv)
        {
            byte[] piv;
            if (iv.Length == 16)
            {
                piv = iv;
            }
            else
            {
                piv = new byte[16];
                System.Array.Copy(iv, 0, piv, 0, iv.Length);
                for (int i = iv.Length; i < piv.Length; i++)
                    piv[i] = 0x00;
            }

            int r0, r1, r2, r3, r4;

            r0 = decode32le(piv, 0);
            r1 = decode32le(piv, 4);
            r2 = decode32le(piv, 8);
            r3 = decode32le(piv, 12);

            r0 ^= serpent24SubKeys[0];
            r1 ^= serpent24SubKeys[0 + 1];
            r2 ^= serpent24SubKeys[0 + 2];
            r3 ^= serpent24SubKeys[0 + 3];
            r3 ^= r0;
            r4 = r1;
            r1 &= r3;
            r4 ^= r2;
            r1 ^= r0;
            r0 |= r3;
            r0 ^= r4;
            r4 ^= r3;
            r3 ^= r2;
            r2 |= r1;
            r2 ^= r4;
            r4 = ~r4;
            r4 |= r1;
            r1 ^= r3;
            r1 ^= r4;
            r3 |= r0;
            r1 ^= r3;
            r4 ^= r3;
            r1 = rotateLeft(r1, 13);
            r2 = rotateLeft(r2, 3);
            r4 = r4 ^ r1 ^ r2;
            r0 = r0 ^ r2 ^ (r1 << 3);
            r4 = rotateLeft(r4, 1);
            r0 = rotateLeft(r0, 7);
            r1 = r1 ^ r4 ^ r0;
            r2 = r2 ^ r0 ^ (r4 << 7);
            r1 = rotateLeft(r1, 5);
            r2 = rotateLeft(r2, 22);
            r1 ^= serpent24SubKeys[4];
            r4 ^= serpent24SubKeys[4 + 1];
            r2 ^= serpent24SubKeys[4 + 2];
            r0 ^= serpent24SubKeys[4 + 3];
            r1 = ~r1;
            r2 = ~r2;
            r3 = r1;
            r1 &= r4;
            r2 ^= r1;
            r1 |= r0;
            r0 ^= r2;
            r4 ^= r1;
            r1 ^= r3;
            r3 |= r4;
            r4 ^= r0;
            r2 |= r1;
            r2 &= r3;
            r1 ^= r4;
            r4 &= r2;
            r4 ^= r1;
            r1 &= r2;
            r1 ^= r3;
            r2 = rotateLeft(r2, 13);
            r0 = rotateLeft(r0, 3);
            r1 = r1 ^ r2 ^ r0;
            r4 = r4 ^ r0 ^ (r2 << 3);
            r1 = rotateLeft(r1, 1);
            r4 = rotateLeft(r4, 7);
            r2 = r2 ^ r1 ^ r4;
            r0 = r0 ^ r4 ^ (r1 << 7);
            r2 = rotateLeft(r2, 5);
            r0 = rotateLeft(r0, 22);
            r2 ^= serpent24SubKeys[8];
            r1 ^= serpent24SubKeys[8 + 1];
            r0 ^= serpent24SubKeys[8 + 2];
            r4 ^= serpent24SubKeys[8 + 3];
            r3 = r2;
            r2 &= r0;
            r2 ^= r4;
            r0 ^= r1;
            r0 ^= r2;
            r4 |= r3;
            r4 ^= r1;
            r3 ^= r0;
            r1 = r4;
            r4 |= r3;
            r4 ^= r2;
            r2 &= r1;
            r3 ^= r2;
            r1 ^= r4;
            r1 ^= r3;
            r3 = ~r3;
            r0 = rotateLeft(r0, 13);
            r1 = rotateLeft(r1, 3);
            r4 = r4 ^ r0 ^ r1;
            r3 = r3 ^ r1 ^ (r0 << 3);
            r4 = rotateLeft(r4, 1);
            r3 = rotateLeft(r3, 7);
            r0 = r0 ^ r4 ^ r3;
            r1 = r1 ^ r3 ^ (r4 << 7);
            r0 = rotateLeft(r0, 5);
            r1 = rotateLeft(r1, 22);
            r0 ^= serpent24SubKeys[12];
            r4 ^= serpent24SubKeys[12 + 1];
            r1 ^= serpent24SubKeys[12 + 2];
            r3 ^= serpent24SubKeys[12 + 3];
            r2 = r0;
            r0 |= r3;
            r3 ^= r4;
            r4 &= r2;
            r2 ^= r1;
            r1 ^= r3;
            r3 &= r0;
            r2 |= r4;
            r3 ^= r2;
            r0 ^= r4;
            r2 &= r0;
            r4 ^= r3;
            r2 ^= r1;
            r4 |= r0;
            r4 ^= r1;
            r0 ^= r3;
            r1 = r4;
            r4 |= r3;
            r4 ^= r0;
            r4 = rotateLeft(r4, 13);
            r3 = rotateLeft(r3, 3);
            r1 = r1 ^ r4 ^ r3;
            r2 = r2 ^ r3 ^ (r4 << 3);
            r1 = rotateLeft(r1, 1);
            r2 = rotateLeft(r2, 7);
            r4 = r4 ^ r1 ^ r2;
            r3 = r3 ^ r2 ^ (r1 << 7);
            r4 = rotateLeft(r4, 5);
            r3 = rotateLeft(r3, 22);
            r4 ^= serpent24SubKeys[16];
            r1 ^= serpent24SubKeys[16 + 1];
            r3 ^= serpent24SubKeys[16 + 2];
            r2 ^= serpent24SubKeys[16 + 3];
            r1 ^= r2;
            r2 = ~r2;
            r3 ^= r2;
            r2 ^= r4;
            r0 = r1;
            r1 &= r2;
            r1 ^= r3;
            r0 ^= r2;
            r4 ^= r0;
            r3 &= r0;
            r3 ^= r4;
            r4 &= r1;
            r2 ^= r4;
            r0 |= r1;
            r0 ^= r4;
            r4 |= r2;
            r4 ^= r3;
            r3 &= r2;
            r4 = ~r4;
            r0 ^= r3;
            r1 = rotateLeft(r1, 13);
            r4 = rotateLeft(r4, 3);
            r0 = r0 ^ r1 ^ r4;
            r2 = r2 ^ r4 ^ (r1 << 3);
            r0 = rotateLeft(r0, 1);
            r2 = rotateLeft(r2, 7);
            r1 = r1 ^ r0 ^ r2;
            r4 = r4 ^ r2 ^ (r0 << 7);
            r1 = rotateLeft(r1, 5);
            r4 = rotateLeft(r4, 22);
            r1 ^= serpent24SubKeys[20];
            r0 ^= serpent24SubKeys[20 + 1];
            r4 ^= serpent24SubKeys[20 + 2];
            r2 ^= serpent24SubKeys[20 + 3];
            r1 ^= r0;
            r0 ^= r2;
            r2 = ~r2;
            r3 = r0;
            r0 &= r1;
            r4 ^= r2;
            r0 ^= r4;
            r4 |= r3;
            r3 ^= r2;
            r2 &= r0;
            r2 ^= r1;
            r3 ^= r0;
            r3 ^= r4;
            r4 ^= r1;
            r1 &= r2;
            r4 = ~r4;
            r1 ^= r3;
            r3 |= r2;
            r4 ^= r3;
            r0 = rotateLeft(r0, 13);
            r1 = rotateLeft(r1, 3);
            r2 = r2 ^ r0 ^ r1;
            r4 = r4 ^ r1 ^ (r0 << 3);
            r2 = rotateLeft(r2, 1);
            r4 = rotateLeft(r4, 7);
            r0 = r0 ^ r2 ^ r4;
            r1 = r1 ^ r4 ^ (r2 << 7);
            r0 = rotateLeft(r0, 5);
            r1 = rotateLeft(r1, 22);
            r0 ^= serpent24SubKeys[24];
            r2 ^= serpent24SubKeys[24 + 1];
            r1 ^= serpent24SubKeys[24 + 2];
            r4 ^= serpent24SubKeys[24 + 3];
            r1 = ~r1;
            r3 = r4;
            r4 &= r0;
            r0 ^= r3;
            r4 ^= r1;
            r1 |= r3;
            r2 ^= r4;
            r1 ^= r0;
            r0 |= r2;
            r1 ^= r2;
            r3 ^= r0;
            r0 |= r4;
            r0 ^= r1;
            r3 ^= r4;
            r3 ^= r0;
            r4 = ~r4;
            r1 &= r3;
            r1 ^= r4;
            r0 = rotateLeft(r0, 13);
            r3 = rotateLeft(r3, 3);
            r2 = r2 ^ r0 ^ r3;
            r1 = r1 ^ r3 ^ (r0 << 3);
            r2 = rotateLeft(r2, 1);
            r1 = rotateLeft(r1, 7);
            r0 = r0 ^ r2 ^ r1;
            r3 = r3 ^ r1 ^ (r2 << 7);
            r0 = rotateLeft(r0, 5);
            r3 = rotateLeft(r3, 22);
            r0 ^= serpent24SubKeys[28];
            r2 ^= serpent24SubKeys[28 + 1];
            r3 ^= serpent24SubKeys[28 + 2];
            r1 ^= serpent24SubKeys[28 + 3];
            r4 = r2;
            r2 |= r3;
            r2 ^= r1;
            r4 ^= r3;
            r3 ^= r2;
            r1 |= r4;
            r1 &= r0;
            r4 ^= r3;
            r1 ^= r2;
            r2 |= r4;
            r2 ^= r0;
            r0 |= r4;
            r0 ^= r3;
            r2 ^= r4;
            r3 ^= r2;
            r2 &= r0;
            r2 ^= r4;
            r3 = ~r3;
            r3 |= r0;
            r4 ^= r3;
            r4 = rotateLeft(r4, 13);
            r2 = rotateLeft(r2, 3);
            r1 = r1 ^ r4 ^ r2;
            r0 = r0 ^ r2 ^ (r4 << 3);
            r1 = rotateLeft(r1, 1);
            r0 = rotateLeft(r0, 7);
            r4 = r4 ^ r1 ^ r0;
            r2 = r2 ^ r0 ^ (r1 << 7);
            r4 = rotateLeft(r4, 5);
            r2 = rotateLeft(r2, 22);
            r4 ^= serpent24SubKeys[32];
            r1 ^= serpent24SubKeys[32 + 1];
            r2 ^= serpent24SubKeys[32 + 2];
            r0 ^= serpent24SubKeys[32 + 3];
            r0 ^= r4;
            r3 = r1;
            r1 &= r0;
            r3 ^= r2;
            r1 ^= r4;
            r4 |= r0;
            r4 ^= r3;
            r3 ^= r0;
            r0 ^= r2;
            r2 |= r1;
            r2 ^= r3;
            r3 = ~r3;
            r3 |= r1;
            r1 ^= r0;
            r1 ^= r3;
            r0 |= r4;
            r1 ^= r0;
            r3 ^= r0;
            r1 = rotateLeft(r1, 13);
            r2 = rotateLeft(r2, 3);
            r3 = r3 ^ r1 ^ r2;
            r4 = r4 ^ r2 ^ (r1 << 3);
            r3 = rotateLeft(r3, 1);
            r4 = rotateLeft(r4, 7);
            r1 = r1 ^ r3 ^ r4;
            r2 = r2 ^ r4 ^ (r3 << 7);
            r1 = rotateLeft(r1, 5);
            r2 = rotateLeft(r2, 22);
            r1 ^= serpent24SubKeys[36];
            r3 ^= serpent24SubKeys[36 + 1];
            r2 ^= serpent24SubKeys[36 + 2];
            r4 ^= serpent24SubKeys[36 + 3];
            r1 = ~r1;
            r2 = ~r2;
            r0 = r1;
            r1 &= r3;
            r2 ^= r1;
            r1 |= r4;
            r4 ^= r2;
            r3 ^= r1;
            r1 ^= r0;
            r0 |= r3;
            r3 ^= r4;
            r2 |= r1;
            r2 &= r0;
            r1 ^= r3;
            r3 &= r2;
            r3 ^= r1;
            r1 &= r2;
            r1 ^= r0;
            r2 = rotateLeft(r2, 13);
            r4 = rotateLeft(r4, 3);
            r1 = r1 ^ r2 ^ r4;
            r3 = r3 ^ r4 ^ (r2 << 3);
            r1 = rotateLeft(r1, 1);
            r3 = rotateLeft(r3, 7);
            r2 = r2 ^ r1 ^ r3;
            r4 = r4 ^ r3 ^ (r1 << 7);
            r2 = rotateLeft(r2, 5);
            r4 = rotateLeft(r4, 22);
            r2 ^= serpent24SubKeys[40];
            r1 ^= serpent24SubKeys[40 + 1];
            r4 ^= serpent24SubKeys[40 + 2];
            r3 ^= serpent24SubKeys[40 + 3];
            r0 = r2;
            r2 &= r4;
            r2 ^= r3;
            r4 ^= r1;
            r4 ^= r2;
            r3 |= r0;
            r3 ^= r1;
            r0 ^= r4;
            r1 = r3;
            r3 |= r0;
            r3 ^= r2;
            r2 &= r1;
            r0 ^= r2;
            r1 ^= r3;
            r1 ^= r0;
            r0 = ~r0;
            r4 = rotateLeft(r4, 13);
            r1 = rotateLeft(r1, 3);
            r3 = r3 ^ r4 ^ r1;
            r0 = r0 ^ r1 ^ (r4 << 3);
            r3 = rotateLeft(r3, 1);
            r0 = rotateLeft(r0, 7);
            r4 = r4 ^ r3 ^ r0;
            r1 = r1 ^ r0 ^ (r3 << 7);
            r4 = rotateLeft(r4, 5);
            r1 = rotateLeft(r1, 22);
            r4 ^= serpent24SubKeys[44];
            r3 ^= serpent24SubKeys[44 + 1];
            r1 ^= serpent24SubKeys[44 + 2];
            r0 ^= serpent24SubKeys[44 + 3];
            r2 = r4;
            r4 |= r0;
            r0 ^= r3;
            r3 &= r2;
            r2 ^= r1;
            r1 ^= r0;
            r0 &= r4;
            r2 |= r3;
            r0 ^= r2;
            r4 ^= r3;
            r2 &= r4;
            r3 ^= r0;
            r2 ^= r1;
            r3 |= r4;
            r3 ^= r1;
            r4 ^= r0;
            r1 = r3;
            r3 |= r0;
            r3 ^= r4;
            r3 = rotateLeft(r3, 13);
            r0 = rotateLeft(r0, 3);
            r1 = r1 ^ r3 ^ r0;
            r2 = r2 ^ r0 ^ (r3 << 3);
            r1 = rotateLeft(r1, 1);
            r2 = rotateLeft(r2, 7);
            r3 = r3 ^ r1 ^ r2;
            r0 = r0 ^ r2 ^ (r1 << 7);
            r3 = rotateLeft(r3, 5);
            r0 = rotateLeft(r0, 22);
            lfsr[9] = r3;
            lfsr[8] = r1;
            lfsr[7] = r0;
            lfsr[6] = r2;
            r3 ^= serpent24SubKeys[48];
            r1 ^= serpent24SubKeys[48 + 1];
            r0 ^= serpent24SubKeys[48 + 2];
            r2 ^= serpent24SubKeys[48 + 3];
            r1 ^= r2;
            r2 = ~r2;
            r0 ^= r2;
            r2 ^= r3;
            r4 = r1;
            r1 &= r2;
            r1 ^= r0;
            r4 ^= r2;
            r3 ^= r4;
            r0 &= r4;
            r0 ^= r3;
            r3 &= r1;
            r2 ^= r3;
            r4 |= r1;
            r4 ^= r3;
            r3 |= r2;
            r3 ^= r0;
            r0 &= r2;
            r3 = ~r3;
            r4 ^= r0;
            r1 = rotateLeft(r1, 13);
            r3 = rotateLeft(r3, 3);
            r4 = r4 ^ r1 ^ r3;
            r2 = r2 ^ r3 ^ (r1 << 3);
            r4 = rotateLeft(r4, 1);
            r2 = rotateLeft(r2, 7);
            r1 = r1 ^ r4 ^ r2;
            r3 = r3 ^ r2 ^ (r4 << 7);
            r1 = rotateLeft(r1, 5);
            r3 = rotateLeft(r3, 22);
            r1 ^= serpent24SubKeys[52];
            r4 ^= serpent24SubKeys[52 + 1];
            r3 ^= serpent24SubKeys[52 + 2];
            r2 ^= serpent24SubKeys[52 + 3];
            r1 ^= r4;
            r4 ^= r2;
            r2 = ~r2;
            r0 = r4;
            r4 &= r1;
            r3 ^= r2;
            r4 ^= r3;
            r3 |= r0;
            r0 ^= r2;
            r2 &= r4;
            r2 ^= r1;
            r0 ^= r4;
            r0 ^= r3;
            r3 ^= r1;
            r1 &= r2;
            r3 = ~r3;
            r1 ^= r0;
            r0 |= r2;
            r3 ^= r0;
            r4 = rotateLeft(r4, 13);
            r1 = rotateLeft(r1, 3);
            r2 = r2 ^ r4 ^ r1;
            r3 = r3 ^ r1 ^ (r4 << 3);
            r2 = rotateLeft(r2, 1);
            r3 = rotateLeft(r3, 7);
            r4 = r4 ^ r2 ^ r3;
            r1 = r1 ^ r3 ^ (r2 << 7);
            r4 = rotateLeft(r4, 5);
            r1 = rotateLeft(r1, 22);
            r4 ^= serpent24SubKeys[56];
            r2 ^= serpent24SubKeys[56 + 1];
            r1 ^= serpent24SubKeys[56 + 2];
            r3 ^= serpent24SubKeys[56 + 3];
            r1 = ~r1;
            r0 = r3;
            r3 &= r4;
            r4 ^= r0;
            r3 ^= r1;
            r1 |= r0;
            r2 ^= r3;
            r1 ^= r4;
            r4 |= r2;
            r1 ^= r2;
            r0 ^= r4;
            r4 |= r3;
            r4 ^= r1;
            r0 ^= r3;
            r0 ^= r4;
            r3 = ~r3;
            r1 &= r0;
            r1 ^= r3;
            r4 = rotateLeft(r4, 13);
            r0 = rotateLeft(r0, 3);
            r2 = r2 ^ r4 ^ r0;
            r1 = r1 ^ r0 ^ (r4 << 3);
            r2 = rotateLeft(r2, 1);
            r1 = rotateLeft(r1, 7);
            r4 = r4 ^ r2 ^ r1;
            r0 = r0 ^ r1 ^ (r2 << 7);
            r4 = rotateLeft(r4, 5);
            r0 = rotateLeft(r0, 22);
            r4 ^= serpent24SubKeys[60];
            r2 ^= serpent24SubKeys[60 + 1];
            r0 ^= serpent24SubKeys[60 + 2];
            r1 ^= serpent24SubKeys[60 + 3];
            r3 = r2;
            r2 |= r0;
            r2 ^= r1;
            r3 ^= r0;
            r0 ^= r2;
            r1 |= r3;
            r1 &= r4;
            r3 ^= r0;
            r1 ^= r2;
            r2 |= r3;
            r2 ^= r4;
            r4 |= r3;
            r4 ^= r0;
            r2 ^= r3;
            r0 ^= r2;
            r2 &= r4;
            r2 ^= r3;
            r0 = ~r0;
            r0 |= r4;
            r3 ^= r0;
            r3 = rotateLeft(r3, 13);
            r2 = rotateLeft(r2, 3);
            r1 = r1 ^ r3 ^ r2;
            r4 = r4 ^ r2 ^ (r3 << 3);
            r1 = rotateLeft(r1, 1);
            r4 = rotateLeft(r4, 7);
            r3 = r3 ^ r1 ^ r4;
            r2 = r2 ^ r4 ^ (r1 << 7);
            r3 = rotateLeft(r3, 5);
            r2 = rotateLeft(r2, 22);
            r3 ^= serpent24SubKeys[64];
            r1 ^= serpent24SubKeys[64 + 1];
            r2 ^= serpent24SubKeys[64 + 2];
            r4 ^= serpent24SubKeys[64 + 3];
            r4 ^= r3;
            r0 = r1;
            r1 &= r4;
            r0 ^= r2;
            r1 ^= r3;
            r3 |= r4;
            r3 ^= r0;
            r0 ^= r4;
            r4 ^= r2;
            r2 |= r1;
            r2 ^= r0;
            r0 = ~r0;
            r0 |= r1;
            r1 ^= r4;
            r1 ^= r0;
            r4 |= r3;
            r1 ^= r4;
            r0 ^= r4;
            r1 = rotateLeft(r1, 13);
            r2 = rotateLeft(r2, 3);
            r0 = r0 ^ r1 ^ r2;
            r3 = r3 ^ r2 ^ (r1 << 3);
            r0 = rotateLeft(r0, 1);
            r3 = rotateLeft(r3, 7);
            r1 = r1 ^ r0 ^ r3;
            r2 = r2 ^ r3 ^ (r0 << 7);
            r1 = rotateLeft(r1, 5);
            r2 = rotateLeft(r2, 22);
            r1 ^= serpent24SubKeys[68];
            r0 ^= serpent24SubKeys[68 + 1];
            r2 ^= serpent24SubKeys[68 + 2];
            r3 ^= serpent24SubKeys[68 + 3];
            r1 = ~r1;
            r2 = ~r2;
            r4 = r1;
            r1 &= r0;
            r2 ^= r1;
            r1 |= r3;
            r3 ^= r2;
            r0 ^= r1;
            r1 ^= r4;
            r4 |= r0;
            r0 ^= r3;
            r2 |= r1;
            r2 &= r4;
            r1 ^= r0;
            r0 &= r2;
            r0 ^= r1;
            r1 &= r2;
            r1 ^= r4;
            r2 = rotateLeft(r2, 13);
            r3 = rotateLeft(r3, 3);
            r1 = r1 ^ r2 ^ r3;
            r0 = r0 ^ r3 ^ (r2 << 3);
            r1 = rotateLeft(r1, 1);
            r0 = rotateLeft(r0, 7);
            r2 = r2 ^ r1 ^ r0;
            r3 = r3 ^ r0 ^ (r1 << 7);
            r2 = rotateLeft(r2, 5);
            r3 = rotateLeft(r3, 22);
            fsmR1 = r2;
            lfsr[4] = r1;
            fsmR2 = r3;
            lfsr[5] = r0;
            r2 ^= serpent24SubKeys[72];
            r1 ^= serpent24SubKeys[72 + 1];
            r3 ^= serpent24SubKeys[72 + 2];
            r0 ^= serpent24SubKeys[72 + 3];
            r4 = r2;
            r2 &= r3;
            r2 ^= r0;
            r3 ^= r1;
            r3 ^= r2;
            r0 |= r4;
            r0 ^= r1;
            r4 ^= r3;
            r1 = r0;
            r0 |= r4;
            r0 ^= r2;
            r2 &= r1;
            r4 ^= r2;
            r1 ^= r0;
            r1 ^= r4;
            r4 = ~r4;
            r3 = rotateLeft(r3, 13);
            r1 = rotateLeft(r1, 3);
            r0 = r0 ^ r3 ^ r1;
            r4 = r4 ^ r1 ^ (r3 << 3);
            r0 = rotateLeft(r0, 1);
            r4 = rotateLeft(r4, 7);
            r3 = r3 ^ r0 ^ r4;
            r1 = r1 ^ r4 ^ (r0 << 7);
            r3 = rotateLeft(r3, 5);
            r1 = rotateLeft(r1, 22);
            r3 ^= serpent24SubKeys[76];
            r0 ^= serpent24SubKeys[76 + 1];
            r1 ^= serpent24SubKeys[76 + 2];
            r4 ^= serpent24SubKeys[76 + 3];
            r2 = r3;
            r3 |= r4;
            r4 ^= r0;
            r0 &= r2;
            r2 ^= r1;
            r1 ^= r4;
            r4 &= r3;
            r2 |= r0;
            r4 ^= r2;
            r3 ^= r0;
            r2 &= r3;
            r0 ^= r4;
            r2 ^= r1;
            r0 |= r3;
            r0 ^= r1;
            r3 ^= r4;
            r1 = r0;
            r0 |= r4;
            r0 ^= r3;
            r0 = rotateLeft(r0, 13);
            r4 = rotateLeft(r4, 3);
            r1 = r1 ^ r0 ^ r4;
            r2 = r2 ^ r4 ^ (r0 << 3);
            r1 = rotateLeft(r1, 1);
            r2 = rotateLeft(r2, 7);
            r0 = r0 ^ r1 ^ r2;
            r4 = r4 ^ r2 ^ (r1 << 7);
            r0 = rotateLeft(r0, 5);
            r4 = rotateLeft(r4, 22);
            r0 ^= serpent24SubKeys[80];
            r1 ^= serpent24SubKeys[80 + 1];
            r4 ^= serpent24SubKeys[80 + 2];
            r2 ^= serpent24SubKeys[80 + 3];
            r1 ^= r2;
            r2 = ~r2;
            r4 ^= r2;
            r2 ^= r0;
            r3 = r1;
            r1 &= r2;
            r1 ^= r4;
            r3 ^= r2;
            r0 ^= r3;
            r4 &= r3;
            r4 ^= r0;
            r0 &= r1;
            r2 ^= r0;
            r3 |= r1;
            r3 ^= r0;
            r0 |= r2;
            r0 ^= r4;
            r4 &= r2;
            r0 = ~r0;
            r3 ^= r4;
            r1 = rotateLeft(r1, 13);
            r0 = rotateLeft(r0, 3);
            r3 = r3 ^ r1 ^ r0;
            r2 = r2 ^ r0 ^ (r1 << 3);
            r3 = rotateLeft(r3, 1);
            r2 = rotateLeft(r2, 7);
            r1 = r1 ^ r3 ^ r2;
            r0 = r0 ^ r2 ^ (r3 << 7);
            r1 = rotateLeft(r1, 5);
            r0 = rotateLeft(r0, 22);
            r1 ^= serpent24SubKeys[84];
            r3 ^= serpent24SubKeys[84 + 1];
            r0 ^= serpent24SubKeys[84 + 2];
            r2 ^= serpent24SubKeys[84 + 3];
            r1 ^= r3;
            r3 ^= r2;
            r2 = ~r2;
            r4 = r3;
            r3 &= r1;
            r0 ^= r2;
            r3 ^= r0;
            r0 |= r4;
            r4 ^= r2;
            r2 &= r3;
            r2 ^= r1;
            r4 ^= r3;
            r4 ^= r0;
            r0 ^= r1;
            r1 &= r2;
            r0 = ~r0;
            r1 ^= r4;
            r4 |= r2;
            r0 ^= r4;
            r3 = rotateLeft(r3, 13);
            r1 = rotateLeft(r1, 3);
            r2 = r2 ^ r3 ^ r1;
            r0 = r0 ^ r1 ^ (r3 << 3);
            r2 = rotateLeft(r2, 1);
            r0 = rotateLeft(r0, 7);
            r3 = r3 ^ r2 ^ r0;
            r1 = r1 ^ r0 ^ (r2 << 7);
            r3 = rotateLeft(r3, 5);
            r1 = rotateLeft(r1, 22);
            r3 ^= serpent24SubKeys[88];
            r2 ^= serpent24SubKeys[88 + 1];
            r1 ^= serpent24SubKeys[88 + 2];
            r0 ^= serpent24SubKeys[88 + 3];
            r1 = ~r1;
            r4 = r0;
            r0 &= r3;
            r3 ^= r4;
            r0 ^= r1;
            r1 |= r4;
            r2 ^= r0;
            r1 ^= r3;
            r3 |= r2;
            r1 ^= r2;
            r4 ^= r3;
            r3 |= r0;
            r3 ^= r1;
            r4 ^= r0;
            r4 ^= r3;
            r0 = ~r0;
            r1 &= r4;
            r1 ^= r0;
            r3 = rotateLeft(r3, 13);
            r4 = rotateLeft(r4, 3);
            r2 = r2 ^ r3 ^ r4;
            r1 = r1 ^ r4 ^ (r3 << 3);
            r2 = rotateLeft(r2, 1);
            r1 = rotateLeft(r1, 7);
            r3 = r3 ^ r2 ^ r1;
            r4 = r4 ^ r1 ^ (r2 << 7);
            r3 = rotateLeft(r3, 5);
            r4 = rotateLeft(r4, 22);
            r3 ^= serpent24SubKeys[92];
            r2 ^= serpent24SubKeys[92 + 1];
            r4 ^= serpent24SubKeys[92 + 2];
            r1 ^= serpent24SubKeys[92 + 3];
            r0 = r2;
            r2 |= r4;
            r2 ^= r1;
            r0 ^= r4;
            r4 ^= r2;
            r1 |= r0;
            r1 &= r3;
            r0 ^= r4;
            r1 ^= r2;
            r2 |= r0;
            r2 ^= r3;
            r3 |= r0;
            r3 ^= r4;
            r2 ^= r0;
            r4 ^= r2;
            r2 &= r3;
            r2 ^= r0;
            r4 = ~r4;
            r4 |= r3;
            r0 ^= r4;
            r0 = rotateLeft(r0, 13);
            r2 = rotateLeft(r2, 3);
            r1 = r1 ^ r0 ^ r2;
            r3 = r3 ^ r2 ^ (r0 << 3);
            r1 = rotateLeft(r1, 1);
            r3 = rotateLeft(r3, 7);
            r0 = r0 ^ r1 ^ r3;
            r2 = r2 ^ r3 ^ (r1 << 7);
            r0 = rotateLeft(r0, 5);
            r2 = rotateLeft(r2, 22);
            r0 ^= serpent24SubKeys[96];
            r1 ^= serpent24SubKeys[96 + 1];
            r2 ^= serpent24SubKeys[96 + 2];
            r3 ^= serpent24SubKeys[96 + 3];
            lfsr[3] = r0;
            lfsr[2] = r1;
            lfsr[1] = r2;
            lfsr[0] = r3;
        }

        /**
        * FSM update.
        */
        private void updateFSM()
        {
            int oldR1 = fsmR1;
            fsmR1 = fsmR2 + (lfsr[1] ^ 
                ((fsmR1 & 0x01) != 0 ? lfsr[8] : 0));
            fsmR2 = rotateLeft(oldR1 * 0x54655307, 7);
        }

        /**
	     * LFSR update. The "dropped" value (s_t) is returned.
	     *
	     * @return  s_t
	     */
        private int updateLFSR()
        {
            int v1 = lfsr[9];

            int changeBitLFSR = (lfsr[3] >> 8);
            if (lfsr[3] < 0)
            {
                changeBitLFSR = (int)(((uint)lfsr[3]) >> 8);
            }
            int v2 = changeBitLFSR ^ divAlpha[lfsr[3] & 0xFF];

            int changeBitMulAlpha = (lfsr[0] >> 24);
            if (lfsr[0] < 0)
            {
                changeBitMulAlpha = (int)(((uint)lfsr[0]) >> 24);
            }
            int v3 = (lfsr[0] << 8) ^ mulAlpha[changeBitMulAlpha];
            int dropped = lfsr[0];

            for (int i = 0; i < 9; i++)
                lfsr[i] = lfsr[i + 1];
            lfsr[9] = v1 ^ v2 ^ v3;
            return dropped;
        }

        /**
        * Intermediate value computation. Note: this method is 
        * called before the LFSR update, and hence uses lfsr[9].
        *
        * @return  f_t
        */
        private int computeIntermediate()
        {
            return (lfsr[9] + fsmR1) ^ fsmR2;
        }

        /**
         * Produce 16 bytes of output stream into the provided 
         * buffer.
         *
         * @param buf   the output buffer
         * @param off   the output offset
         */
        private void makeStreamBlock(byte[] buf, int off)
        {
            updateFSM();
            int f0 = computeIntermediate();
            int s0 = updateLFSR();

            updateFSM();
            int f1 = computeIntermediate();
            int s1 = updateLFSR();

            updateFSM();
            int f2 = computeIntermediate();
            int s2 = updateLFSR();

            updateFSM();
            int f3 = computeIntermediate();
            int s3 = updateLFSR();

            /*
            * Apply the third S-box (number 2) on (f3, f2, f1, f0).
            */
            int f4 = f0;
            f0 &= f2;
            f0 ^= f3;
            f2 ^= f1;
            f2 ^= f0;
            f3 |= f4;
            f3 ^= f1;
            f4 ^= f2;
            f1 = f3;
            f3 |= f4;
            f3 ^= f0;
            f0 &= f1;
            f4 ^= f0;
            f1 ^= f3;
            f1 ^= f4;
            f4 = ~f4;

            /*
             * S-box result is in (f2, f3, f1, f4).
             */
            encode32le(f2 ^ s0, buf, off);
            encode32le(f3 ^ s1, buf, off + 4);
            encode32le(f1 ^ s2, buf, off + 8);
            encode32le(f4 ^ s3, buf, off + 12);
        }

        /*
         * Internal buffer for partial blocks. "streamPtr" points 
         * to the first stream byte which has been computed but 
         * not output.
         */
        private static readonly int BUFFERLEN = 16;
        private readonly byte[] streamBuf = new byte[BUFFERLEN];
        private int streamPtr = BUFFERLEN;

        /**
         * Produce the required number of stream bytes.
         *
         * @param buf   the destination buffer
         * @param off   the destination offset
         * @param len   the required stream length (in bytes)
         */
        public void makeStream(byte[] buf, int off, int len)
        {
            if (streamPtr < BUFFERLEN)
            {
                int blen = BUFFERLEN - streamPtr;
                if (blen > len)
                    blen = len;
                Array.Copy(streamBuf, streamPtr, buf, off, blen);
                streamPtr += blen;
                off += blen;
                len -= blen;
            }
            while (len > 0)
            {
                if (len >= BUFFERLEN)
                {
                    makeStreamBlock(buf, off);
                    off += BUFFERLEN;
                    len -= BUFFERLEN;
                }
                else
                {
                    makeStreamBlock(streamBuf, 0);
                    Array.Copy(streamBuf, 0, buf, off, len);
                    streamPtr = len;
                    len = 0;
                }
            }
        }

        /**
         * Transform String to Byte Array 
         */
        private byte[] HexStringToByteArray(string hexString, 
            string inputType)
        {
            string[] hexValuesSplit = hexString.Split(' ');

            if (inputType.Equals("key"))
            {
                if (hexValuesSplit.Length < 8 | 
                    hexValuesSplit.Length > 32)
                {
                    GuiLogMessage("Invalid key length (" + 
                        hexValuesSplit.Length * 8 +  " bits). It "+
                        "must be between 64 and 256 bits long. In "
                        +"hexadecimal representation: xx yy zz .."
                        , NotificationLevel.Error);
                    return null;
                }
            }

            if (inputType.Equals("iv"))
            {
                if (hexValuesSplit.Length < 4 
                    | hexValuesSplit.Length > 16)
                {
                    GuiLogMessage("Invalid iv length (" + 
                        hexValuesSplit.Length * 8 + " bits). It "+
                        "must be between 32 and 128 bits long. In"+
                        " hexadecimal representation: xx yy zz .."
                        , NotificationLevel.Error);
                    return null;
                }
            }

            byte[] hexArray = new byte[hexValuesSplit.Length];
            for (int i = 0; i < hexValuesSplit.Length; i++)
            {
                hexArray[i] = Byte.Parse(hexValuesSplit[i],
                    System.Globalization.NumberStyles.HexNumber);
            }
            return hexArray;
        }

        public void generateOutput(byte[] tmp)
        {
            keyStream = "";
            char[] hexnum = {
		        '0', '1', '2', '3', '4', '5', '6', '7',
		        '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
	        };

            for (int j = 0; j < tmp.Length; j++)
            {
                int v = tmp[j] & 0xFF;
                keyStream += ("" + hexnum[v >> 4])
                    + hexnum[v & 0x0F] + " ";
            }
            this.OutputString = keyStream;
        }

        public void PreExecution()
        {
            Dispose();
        }

        public void Execute()
        {
            ProgressChanged(0, 6);

            //Convert Input IV String into Byte Array
            byte[] IV = HexStringToByteArray(InputIV, "iv");
            ProgressChanged(1, 6);

            //Convert Input Key String into Byte Array
            byte[] key = HexStringToByteArray(InputKey, "key");
            ProgressChanged(2, 6);

            setKey(key);
            ProgressChanged(3, 6);

            setIV(IV);
            ProgressChanged(4, 6);

            //generation of the key stream
            byte[] tmp = new byte[512];
            makeStream(tmp, 0, tmp.Length);
            ProgressChanged(5, 6);

            try
            {
                //encoding of the plaintext
                plainText = HexStringToByteArray(InputString, 
                    "plain");

                for (byte i = 0; i < plainText.Length; i++)
                {
                    plainText[i] = (byte)(((tmp[i] + plainText[i])) 
                        % 256);
                }
                generateOutput(plainText);
            }
            catch (Exception exception)
            {
                GuiLogMessage("No valid input of plaintext", 
                    NotificationLevel.Info);
                GuiLogMessage("Generating 512 bit of keystream",
                    NotificationLevel.Info);
                generateOutput(tmp);
            }
            ProgressChanged(6, 6);
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            inputKey = null;
            outputString = null;
            OutputString = null;
            inputString = null;
            keyStream = "";
        }

        public void Stop()
        {
        }

        #region Event Handling

        public event StatusChangedEventHandler 
            OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler 
            OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler 
            OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, 
            NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, 
                this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, 
                new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, 
                this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}