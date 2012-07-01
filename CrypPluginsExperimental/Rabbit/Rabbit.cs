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

namespace Cryptool.Plugins.Rabbit
{
    [Author("Robin Nelle", "rnelle@mail.uni-mannheim.de",
         "Uni Mannheim - Lehrstuhl Prof. Dr. Armknecht",
         "http://ls.wim.uni-mannheim.de/")]
    [PluginInfo("Rabbit.Resources", "PluginCaption", 
        "PluginTooltip", "Rabbit/userdoc.xml", 
        new[] { "Rabbit/icon.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Rabbit5 : ICrypComponent
    {
        #region Private Variables
        private int[] x = new int[8];
        private int[] c = new int[8];
        private int carry;
        private byte[] ciphertext;
        private string inputString;
        private string inputKey;
        private string inputIV;
        private string outputString;
        private RabbitSettings settings = new RabbitSettings();

        #endregion

        #region Data Properties
        [PropertyInfo(Direction.InputData, "InputStringCaption",
            "InputStringTooltip", true)]
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

        #region IPlugin Members

        public void initRabbit()
        {
            for (int i = 0; i < 8; i++)
            {
                x[i] = 0;
                c[i] = 0;
            }
            carry = 0;

            ciphertext = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                ciphertext[i] = 0;
            }
        }



        private byte[] HexStringToByteArray(string hexString)
        {
            string[] hexValuesSplit = hexString.Split(' ');
            byte[] hexArray = new byte[hexValuesSplit.Length];
            for (int i = 0; i < hexValuesSplit.Length; i++)
            {
                hexArray[i] = Byte.Parse(hexValuesSplit[i],
                    System.Globalization.NumberStyles.HexNumber);
            }
            return hexArray;
        }

        public void Execute()
        {
            initRabbit();

            byte[] key = HexStringToByteArray(inputKey);
            // Stop when wrong Input key length
            if ((key.Length == 16) == false)
            {
                GuiLogMessage("Wrong input Key Length " + key.Length 
                    + ". Key Length must be 16 Bytes", 
                    NotificationLevel.Error);
                return;
            }

            byte[] message = HexStringToByteArray(inputString);
            // Stop when wrong Input message length
            if ((message.Length == 16) == false)
            {
                GuiLogMessage("Wrong message Length " + 
                    message.Length + ". Message length"+
                    " must be 16 Bytes", NotificationLevel.Error);
                return;
            }

            byte[] mixedmessage = new byte[message.Length];
            byte[] mixedKey = new byte[key.Length];
            int k = 0;
            for (int i = 0; i < 4; i++)
            {

                for (int j = 1; j <= 4; j++)
                {
                    mixedKey[k] = key[((4 - j) + (4 * i))];
                    mixedmessage[k] = message[((4 - j) + (4 * i))];
                    k++;
                }
            }
        #endregion

            keySetup(mixedKey);
            //check for valid input
            if (inputIV.Length > 0)
            {
                byte[] iv = HexStringToByteArray(inputIV);

                //check for right iv length
                if ((iv.Length == 8) == false)
                {
                    GuiLogMessage("Wrong input iv length " + 
                        iv.Length + ". IV length must be 8 Bytes", 
                        NotificationLevel.Error);
                    return;
                }
                else
                {
                    byte[] mixedIV = new byte[iv.Length];
                    int m = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 1; j <= 4; j++)
                        {
                            mixedIV[m] = iv[((4 - j) + (4 * i))];
                            m++;
                        }
                    }

                    //start iv setup
                    iv_setup(mixedIV);
                }
            }

            //start encoding
            cipher(mixedmessage, ciphertext, 16);

            outputString = "";
            for (int i = 0; i < 16; i++)
            {
                outputString += string.Format("{0:x}", ciphertext[i])
                    + " ";
            }
            OutputString = outputString;
        }

        private int rshift(int x, int y)
        {
            int shift = x >> y;

            if (x < 0)
            {
                shift = (int)(((uint)x) >> y);
            }
            return shift;
        }

        // Square a 32-bit unsigned integer to obtain the 64-bit 
        //result 
        //and return the upper 32 bits XOR the lower 32 bits 
        private int g_func(int x)
        {
            // Temporary variables
            int a, b, h, l;

            // Construct high and low argument for squaring
            a = x & 0xffff;
            b = rshift(x, 16);

            // Calculate high and low result of squaring
            h = (rshift((rshift((a * a), 17) + (a * b)), 15)) 
                + (b * b);
            l = x * x;

            // Return high XOR low
            return h ^ l;
        }

        //Compare the int values x and y
        private int compare(int x, int y)
        {
            long a = x;
            long b = y;
            a &= 0x00000000ffffffffL;
            b &= 0x00000000ffffffffL;

            return (a < b) ? 1 : 0;
        }

        // Left rotation of a 32-bit
        private int rotL(int x, int y)
        {
            return (x << y) | rshift(x, (32 - y));
        }

        // Calculate the next internal state
        private void next_state()
        {
            // Temporary variables
            int[] g = new int[8];
            int[] c_old = new int[8];

            // Save old counter values
            for (int i = 0; i < 8; i++)
            {
                c_old[i] = c[i];
            }

            //Calculate new counter values
            c[0] += 0x4d34d34d + carry;
            c[1] += (unchecked((int)0xd34d34d3)) + 
                compare(c[0], c_old[0]);
            c[2] += (unchecked((int)0x34d34d34)) + 
                compare(c[1], c_old[1]);
            c[3] += (unchecked((int)0x4d34d34d)) + 
                compare(c[2], c_old[2]);
            c[4] += (unchecked((int)0xd34d34d3)) + 
                compare(c[3], c_old[3]);
            c[5] += (unchecked((int)0x34d34d34)) + 
                compare(c[4], c_old[4]);
            c[6] += (unchecked((int)0x4d34d34d)) + 
                compare(c[5], c_old[5]);
            c[7] += (unchecked((int)0xd34d34d3)) + 
                compare(c[6], c_old[6]);
            carry = compare(c[7], c_old[7]);

            // Calculate the g-functions
            for (int i = 0; i < 8; i++)
            {
                g[i] = g_func(x[i] + c[i]);
            }

            // Calculate new state values
            x[0] = g[0] + rotL(g[7], 16) + rotL(g[6], 16);
            x[1] = g[1] + rotL(g[0], 8) + g[7];
            x[2] = g[2] + rotL(g[1], 16) + rotL(g[0], 16);
            x[3] = g[3] + rotL(g[2], 8) + g[1];
            x[4] = g[4] + rotL(g[3], 16) + rotL(g[2], 16);
            x[5] = g[5] + rotL(g[4], 8) + g[3];
            x[6] = g[6] + rotL(g[5], 16) + rotL(g[4], 16);
            x[7] = g[7] + rotL(g[6], 8) + g[5];
        }

        //transform byte[] to int
        public static int os2ip(byte[] a, int i)
        {
            int x0 = a[i + 3] & (unchecked((int)0x000000ff));
            int x1 = a[i + 2] << 8 & (unchecked((int)0x0000ff00));
            int x2 = a[i + 1] << 16 & (unchecked((int)0x00ff0000));
            int x3 = a[i] << 24 & (unchecked((int)0xff000000));

            return x0 | x1 | x2 | x3;
        }

        //transform int to byte[]
        public byte[] i2osp(int x)
        {
            byte[] s = new byte[4];

            s[0] = (byte)(x & 0x000000ff);
            s[1] = (byte)rshift((x & 0x0000ff00), 8);
            s[2] = (byte)rshift((x & 0x00ff0000), 16);
            s[3] = (byte)rshift((int)(x & 0xff000000), 24);
            return s;
        }


        // Initialize the cipher instance as a function of the
        // key (p_key)
        public void keySetup(byte[] p_key)
        {
            //Temporary variables
            int k0, k1, k2, k3, i;

            // Generate four subkeys

            k0 = os2ip(p_key, 0);
            k1 = os2ip(p_key, 4);
            k2 = os2ip(p_key, 8);
            k3 = os2ip(p_key, 12);

            // Generate initial state variables
            x[0] = k0;
            x[2] = k1;
            x[4] = k2;
            x[6] = k3;
            x[1] = (k3 << 16) | rshift(k2, 16);
            x[3] = (k0 << 16) | rshift(k3, 16);
            x[5] = (k1 << 16) | rshift(k0, 16);
            x[7] = (k2 << 16) | rshift(k1, 16);

            // Generate initial counter values
            c[0] = rotL(k2, 16);
            c[2] = rotL(k3, 16);
            c[4] = rotL(k0, 16);
            c[6] = rotL(k1, 16);
            c[1] = (k0 & (unchecked((int)0xffff0000))) | 
                (k1 & (unchecked((int)0x0000ffff)));
            c[3] = (k1 & (unchecked((int)0xffff0000))) | 
                (k2 & (unchecked((int)0x0000ffff)));
            c[5] = (k2 & (unchecked((int)0xffff0000))) | 
                (k3 & (unchecked((int)0x0000ffff)));
            c[7] = (k3 & (unchecked((int)0xffff0000))) | 
                (k0 & (unchecked((int)0x0000ffff)));

            // Clear carry bit 
            carry = 0;

            // Iterate the system four times
            for (i = 0; i < 4; i++)
            {
                next_state();
            }

            // Modify the counters
            for (i = 0; i < 8; i++)
            {
                c[(i + 4) & 0x7] ^= x[i];
            }
        }

        #region iv setup
        // Initialize the cipher instance as a function of the
        // IV (p_iv) 
        void iv_setup(byte[] p_iv)
        {
            // Temporary variables 
            int i0, i1, i2, i3, i;

            // Generate four subvectors
            i0 = os2ip(p_iv, 0);
            i2 = os2ip(p_iv, 4);
            i1 = (i0 >> 16) | (int)(i2 & 0xFFFF0000);
            i3 = (i2 << 16) | (int)(i0 & 0x0000FFFF);

            // Modify counter values 
            c[0] = c[0] ^ i0;
            c[1] = c[1] ^ i1;
            c[2] = c[2] ^ i2;
            c[3] = c[3] ^ i3;
            c[4] = c[4] ^ i0;
            c[5] = c[5] ^ i1;
            c[6] = c[6] ^ i2;
            c[7] = c[7] ^ i3;

            //Iterate the system four times 
            for (i = 0; i < 4; i++)
                next_state();
        }
        #endregion

        // Encrypt or decrypt data 
        public void cipher(byte[] p_src, byte[] p_dest, 
            long data_size)
        {

            // Temporary variables
            int i, j, m;
            int[] k = new int[4];
            byte[] t = new byte[4];

            for (i = 0; i < data_size; i += 16)
            {
                // Iterate the system
                next_state();

                k[0] = os2ip(p_src, i * 16 + 0) ^ x[0] ^ 
                    rshift(x[5], 16) ^ (x[3] << 16);
                k[1] = os2ip(p_src, i * 16 + 4) ^ x[2] ^ 
                    rshift(x[7], 16) ^ (x[5] << 16);
                k[2] = os2ip(p_src, i * 16 + 8) ^ x[4] ^ 
                    rshift(x[1], 16) ^ (x[7] << 16);
                k[3] = os2ip(p_src, i * 16 + 12) ^ x[6] ^ 
                    rshift(x[3], 16) ^ (x[1] << 16);

                //Encrypt 16 bytes of data
                for (j = 0; j < 4; j++)
                {
                    t = i2osp(k[j]);

                    for (m = 0; m < 4; m++)
                    {
                        p_dest[i * 16 + j * 4 + (m)] = t[m];
                    }
                }
            }
        }



        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler 
            OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler 
            OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;


        private void GuiLogMessage(string message, NotificationLevel 
            logLevel)
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

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }
}