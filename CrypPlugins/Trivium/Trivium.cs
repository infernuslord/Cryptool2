/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using Cryptool.PluginBase.Miscellaneous;
using System.Security.Cryptography;
// for IControl
using Cryptool.PluginBase.Control;
// reference to the CubeAttackController interface (own dll)
using Cryptool.CubeAttackController;

namespace Cryptool.Trivium
{
    [Author("Soeren Rinne, David Oruba & Daehyun Strobel", "soeren.rinne@cryptool.org", "Ruhr-Universitaet Bochum, Chair for Embedded Security (EmSec)", "http://www.trust.ruhr-uni-bochum.de/")]
    [PluginInfo("Trivium.Properties.Resources", "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Trivium/icon.png", "Trivium/Images/encrypt.png", "Trivium/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Trivium : ICrypComponent
    {
        #region IPlugin Members

        private TriviumSettings settings;
        private string inputString = null;
        private string outputString;
        private string inputKey;
        private string inputIV;
        private bool stop = false;

        #endregion

        #region Public Variables
        public List<uint> a = new List<uint>(new uint[93]);
        public List<uint> b = new List<uint>(new uint[84]);
        public List<uint> c = new List<uint>(new uint[111]);
        #endregion

        public Trivium()
        {
            this.settings = new TriviumSettings();
            //((TriviumSettings)(this.settings)).LogMessage += Trivium_LogMessage;
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (TriviumSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                this.inputString = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public string InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "InputIVCaption", "InputIVTooltip", true)]
        public string InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        public void Dispose()
        {
                stop = false;
                inputKey = null;
                outputString = null;
                inputString = null;
                }

        public int[] hextobin(char[] hex)
        {
            int i;
            int[] bin = new int[hex.Length * 4];

            for (i = 0; i < hex.Length; i++)
            {
                switch (hex[i])
                {
                    case '0':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '1':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case '2':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '3':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case '4':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '5':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case '6':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '7':
                        bin[i * 4] = 0;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case '8':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case '9':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'a':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'b':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'c':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'd':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'e':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'f':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'A':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'B':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 0;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'C':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'D':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 0;
                        bin[i * 4 + 3] = 1;
                        break;
                    case 'E':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 0;
                        break;
                    case 'F':
                        bin[i * 4] = 1;
                        bin[i * 4 + 1] = 1;
                        bin[i * 4 + 2] = 1;
                        bin[i * 4 + 3] = 1;
                        break;
                }
            }

            return bin;
        }

        public string bintohex(string bin)
        {
            int i;
            string hex = null;
            string temp;

            for (i = 0; i < bin.Length / 4; i++)
            {
                temp = null;
                temp += bin[i * 4];
                temp += bin[i * 4 + 1];
                temp += bin[i * 4 + 2];
                temp += bin[i * 4 + 3];

                switch (temp)
                {
                    case "0000":
                        hex += '0';
                        break;
                    case "0001":
                        hex += '1';
                        break;
                    case "0010":
                        hex += '2';
                        break;
                    case "0011":
                        hex += '3';
                        break;
                    case "0100":
                        hex += '4';
                        break;
                    case "0101":
                        hex += '5';
                        break;
                    case "0110":
                        hex += '6';
                        break;
                    case "0111":
                        hex += '7';
                        break;
                    case "1000":
                        hex += '8';
                        break;
                    case "1001":
                        hex += '9';
                        break;
                    case "1010":
                        hex += 'A';
                        break;
                    case "1011":
                        hex += 'B';
                        break;
                    case "1100":
                        hex += 'C';
                        break;
                    case "1101":
                        hex += 'D';
                        break;
                    case "1110":
                        hex += 'E';
                        break;
                    case "1111":
                        hex += 'F';
                        break;
                }
            }

            return hex;
        }

        // TODO: check if input is boolean or hex
        // returns -1 if no hex
        // returns 1 is hex
        private int checkInput(string input)
        {
            int returnValue = 1;
            return returnValue;
        }


        public void Execute()
        {
            process(1);
        }

        private void process(int padding)
        {
            //Encrypt/Decrypt String
            try
            {
                // a test vector; should have the following output:
                // FC9659CB953A37F...
                //string IV_string = "288ff65dc42b92f960c7";
                //string key_string = "0f62b5085bae0154a7fa";
                string input_string = inputString;

                // check input string
                if (checkInput(input_string) == -1)
                    return;

                string IV_string = inputIV;
                string key_string = inputKey;

                int[] IV = new int[IV_string.Length * 4];
                int[] key = new int[key_string.Length * 4];

                IV = hextobin(IV_string.ToCharArray());
                key = hextobin(key_string.ToCharArray());

                GuiLogMessage("length of IV: " + IV.Length, NotificationLevel.Info);
                GuiLogMessage("length of key: " + key.Length, NotificationLevel.Info);

                // test if padding to do
                int bitsToPad = (32 - input_string.Length % 32) % 32;
                GuiLogMessage("Bits to pad: " + bitsToPad, NotificationLevel.Info);
                // pad partial block with zeros
                if (bitsToPad != 0)
                {
                    for (int i = 0; i < bitsToPad; i++)
                    {
                        input_string += "0";
                    }
                }

                string keystream;

                int keyStreamLength = settings.KeystreamLength;
                if ((keyStreamLength % 32) != 0)
                {
                    GuiLogMessage("Keystream length must be a multiple of 32. " + keyStreamLength + " != 32", NotificationLevel.Error);
                    return;
                }

                // encryption/decryption
                DateTime startTime = DateTime.Now;

                GuiLogMessage("Starting encryption [Keysize=80 Bits]", NotificationLevel.Info);

                // init Trivium
                initTrivium(IV, key);

                // generate keystream with given length (TODO: padding if inputstring % 32 != 0)
                // ACHTUNG, mag keine grossen zahlen als inputs
                // EDIT 30.07.09: Jetzt mag er große Zahlen ;)
                if (settings.KeystreamLength <= 0)
                {
                    keystream = keystreamTrivium(input_string.Length);
                    //GuiLogMessage("DEBUG: inputString.Length + bitsToPad: " + (inputString.Length + bitsToPad), NotificationLevel.Info);
                }
                else
                {
                    keystream = keystreamTrivium(settings.KeystreamLength);
                }

                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;

                if (!settings.HexOutput)
                {
                    outputString = keystream;
                    GuiLogMessage("Keystream hex: " + bintohex(keystream), NotificationLevel.Info);
                }
                else
                {
                    outputString = bintohex(keystream);
                    GuiLogMessage("Keystream binary: " + keystream, NotificationLevel.Info);
                }
                OnPropertyChanged("OutputString");

                if (!stop)
                {
                    GuiLogMessage("Encryption complete in " + duration + "! (input length : " + input_string.Length + ", keystream/output length: " + keystream.Length + " bit)", NotificationLevel.Info);
                }

                if (stop)
                {
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                }
            }
            catch (Exception exception)
            {
                GuiLogMessage(exception.Message, NotificationLevel.Error);
            }
            finally
            {
                ProgressChanged(1, 1);
            }
        }

        public void initTrivium(int[] IV, int[] key)
        {
            int i;
            if (settings.UseByteSwapping)
            {
                int[] buffer = new int[8];
                // Byte-Swapping Key
                for (int l = 0; l < 10; l++)
                {
                    for (int k = 0; k < 8; k++)
                        buffer[k] = key[((l * 8) + 7) - k];
                    for (int k = 0; k < 8; k++)
                        key[(l * 8) + k] = buffer[k];
                }
                // Byte-Swapping IV
                for (int l = 0; l < 10; l++)
                {
                    for (int k = 0; k < 8; k++)
                        buffer[k] = IV[((l * 8) + 7) - k];
                    for (int k = 0; k < 8; k++)
                        IV[(l * 8) + k] = buffer[k];
                }
            }
            for (i = 0; i < 80; i++)
            {
                a[i] = (uint)key[i]; // hier key rein als binär
                b[i] = (uint)IV[i]; // hier IV rein als binär
                c[i] = 0;
            }
            while (i < 84)
            {
                a[i] = 0;
                b[i] = 0;
                c[i] = 0;
                i++;
            }
            while (i < 93)
            {
                a[i] = 0;
                c[i] = 0;
                i++;
            }
            while (i < 108)
            {
                c[i] = 0;
                i++;
            }
            while (i < 111)
            {
                c[i] = 1;
                i++;
            }
            int initRounds = settings.InitRounds;
            for (i = 0; i < initRounds; i++) // default 1152 = 4 * 288
            {
                a.Insert(0, c[65] ^ (c[108] & c[109]) ^ c[110] ^ a[68]);
                b.Insert(0, a[66] ^ (a[91] & a[92]) ^ a[93] ^ b[77]);
                c.Insert(0, b[69] ^ (b[82] & b[83]) ^ b[84] ^ c[86]);
                a.RemoveAt(93);
                b.RemoveAt(84);
                c.RemoveAt(111);
            }
        }

        public string keystreamTrivium(int nBits)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < nBits; i++)
            {
                builder.Append((int)(a[65] ^ a[92] ^ b[68] ^ b[83] ^ c[65] ^ c[110]));
                a.Insert(0, c[65] ^ (c[108] & c[109]) ^ c[110] ^ a[68] ^ (c[108] & c[109]) ^ a[68]);
                b.Insert(0, a[66] ^ (a[91] & a[92]) ^ a[93] ^ b[77] ^ (a[91] & a[92]) ^ b[77]);
                c.Insert(0, b[69] ^ (b[82] & b[83]) ^ b[84] ^ c[86] ^ (b[82] & b[83]) ^ c[86]);
                a.RemoveAt(93);
                b.RemoveAt(84);
                c.RemoveAt(111);
            }
            if (settings.UseByteSwapping)
            {
                int[] temp = new int[nBits];

                // Little-Endian für den Keystream
                for (int k = 0; k < nBits; k++)
                    temp[k] = builder[k];
                for (int l = 0; l < nBits / 32; l++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        builder[(l * 32) + k] = (char)temp[(l * 32) + 24 + k];
                        builder[(l * 32) + 8 + k] = (char)temp[(l * 32) + 16 + k];
                        builder[(l * 32) + 16 + k] = (char)temp[(l * 32) + 8 + k];
                        builder[(l * 32) + 24 + k] = (char)temp[(l * 32) + k];
                    }
                }
            }
            return builder.ToString();
        }

        public void Encrypt()
        {
            //Encrypt Stream
            process(0);
        }

        public void Decrypt()
        {
            //Decrypt Stream
            process(1);
        }

        public void Initialize()
        {
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void Stop()
        {
            this.stop = true;
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            /*if (PropertyChanged != null)
            {
              PropertyChanged(this, new PropertyChangedEventArgs(name));
            }*/
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion

        #region IControl

        private IControlCubeAttack triviumSlave;
        [PropertyInfo(Direction.ControlSlave, "TriviumSlaveCaption", "TriviumSlaveTooltip")]
        public IControlCubeAttack TriviumSlave
        {
            get
            {
                if (triviumSlave == null)
                    triviumSlave = new CubeAttackControl(this);
                return triviumSlave;
            }
        }

        #endregion
    }

    #region TriviumControl : IControlCubeAttack

    public class CubeAttackControl : IControlCubeAttack
    {
        public event IControlStatusChangedEventHandler OnStatusChanged;
        private Trivium plugin;
        
        public CubeAttackControl(Trivium Plugin)
        {
            this.plugin = Plugin;
        }

        #region IControlEncryption Members

        public int GenerateBlackboxOutputBit(int[] IV, int[] key, int length)
        {
            if (key == null) // Online phase
                plugin.initTrivium(IV, plugin.hextobin(((TriviumSettings)plugin.Settings).InputKey.ToCharArray()));
            else // Preprocessing phase
                plugin.initTrivium(IV, key);
            return Int32.Parse(plugin.keystreamTrivium(length).Substring(plugin.keystreamTrivium(length).Length - 1, 1));
        }

        #endregion
    }

    #endregion

    enum TriviumImage
    {
        Default,
        Encode,
        Decode
    }
}