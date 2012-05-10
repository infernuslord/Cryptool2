/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using Cryptool.PluginBase.Control;

namespace Cryptool.Plugins.Grain_v1
{
    [Author("Maxim Serebrianski", "ms_1990@gmx.de", "University of Mannheim", "http://www.uni-mannheim.de/1/startseite/index.html")]
    [PluginInfo("Grain_v1.Properties.Resources", "PluginCaption", "PluginTooltip", "Grain v1/DetailedDescription/doc.xml", new[] { "Grain v1/Images/grain.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Grain : ICrypComponent
    {
        #region IPlugin Members
        private GrainSettings settings;
        private string inputString;
        private string outputString;
        private string inputKey;
        private string inputIV;
        private bool stop = false;
        #endregion

        #region Public Variables
        public uint[] lfsr;
        public uint[] nfsr;
        public const int STATE_SIZE = 5;
        public byte[] workingKey;
        public byte[] workingIV;
        public byte[] outp;
        public byte[] Out;
        public byte[] In;
        public string keyStream = "";
        public uint output;
        public uint index = 2;
        #endregion

        public Grain()
        {
            this.settings = new GrainSettings();
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (GrainSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "InputStreamCaption", "InputStreamTooltip", true)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                this.inputString = value;
                OnPropertyChanged("InputString");
            }
        }

        [PropertyInfo(Direction.InputData, "KeyDataCaption", "KeyDataTooltip", true)]
        public string InputKey
        {
            get { return this.inputKey; }
            set
            {
                this.inputKey = value;
                OnPropertyChanged("InputKey");
            }
        }

        [PropertyInfo(Direction.InputData, "IVCaption", "IVTooltip", true)]
        public string InputIV
        {
            get { return this.inputIV; }
            set
            {
                this.inputIV = value;
                OnPropertyChanged("InputIV");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
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
            keyStream = null;
        }

        public void Execute()
        {
            StringBuilder builder = new StringBuilder();
            In = stringToByteArray(inputString);
            Out = new byte[In.Length];
            try
            {
                DateTime startTime = DateTime.Now;
                GuiLogMessage("Starting encryption...", NotificationLevel.Info);
                init();
                processBytes(In, 0, In.Length, Out, 0);
                foreach (byte b in Out) builder.Append(String.Format("{0:X2}", b));
                TimeSpan duration = DateTime.Now - startTime;
                OutputString = builder.ToString();
                if (stop) GuiLogMessage("Aborted!", NotificationLevel.Info);
                else
                {
                    GuiLogMessage("Encryption complete in " + duration + "!", NotificationLevel.Info);
                    GuiLogMessage("Key Stream: " + keyStream, NotificationLevel.Info);
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

        public byte[] stringToByteArray(string input)
        {
            if (input.Length % 2 == 1) input += '0';
            byte[] array = new byte[input.Length / 2];
            for (int i = 0, j = 0; i < input.Length; i += 2, j++) array[j] = Convert.ToByte(input.Substring(i, 2), 16);
            return array;
        }

        public byte[] IVstringToByteArray(string input)
        {
            byte[] array = new byte[8];

            if (input.Length != 16)
            {
                stop = true;
                GuiLogMessage("IV length must be 8 byte (64 bit)!", NotificationLevel.Error);
            }
            else
            {
                for (int i = 0, j = 0; i < input.Length; i += 2, j++) array[j] = Convert.ToByte(input.Substring(i, 2), 16); 
            }
            return array;
        }

        public byte[] KEYstringToByteArray(string input)
        {
            byte[] array = new byte[10];

            if (input.Length != 20)
            {
                stop = true;
                GuiLogMessage("Key length must be 10 byte (80 bit)!", NotificationLevel.Error);
            }
            else
            {
                for (int i = 0, j = 0; i < input.Length; i += 2, j++) array[j] = Convert.ToByte(input.Substring(i, 2), 16);
            }
            return array;
        }

        public void init()
        {
            byte[] iv = IVstringToByteArray(inputIV);
            byte[] key = KEYstringToByteArray(inputKey);
            workingIV = new byte[key.Length];
            workingKey = new byte[key.Length];
            lfsr = new uint[STATE_SIZE];
            nfsr = new uint[STATE_SIZE];
            outp = new byte[2];

            Array.Copy(iv, 0, workingIV, 0, iv.Length);
            Array.Copy(key, 0, workingKey, 0, key.Length);

            setKey(workingKey, workingIV);
            initGrain();
        }

        public void initGrain()
        {
            for (int i = 0; i < 10; i++)
            {
                output = getOutput();
                nfsr = shift(nfsr, getOutputNFSR() ^ lfsr[0] ^ output);
                lfsr = shift(lfsr, getOutputLFSR() ^ output);
            }
        }

        public uint getOutputNFSR()
        {
            uint b0 = nfsr[0];
            uint b9 = nfsr[0] >> 9 | nfsr[1] << 7;
            uint b14 = nfsr[0] >> 14 | nfsr[1] << 2;
            uint b15 = nfsr[0] >> 15 | nfsr[1] << 1;
            uint b21 = nfsr[1] >> 5 | nfsr[2] << 11;
            uint b28 = nfsr[1] >> 12 | nfsr[2] << 4;
            uint b33 = nfsr[2] >> 1 | nfsr[3] << 15;
            uint b37 = nfsr[2] >> 5 | nfsr[3] << 11;
            uint b45 = nfsr[2] >> 13 | nfsr[3] << 3;
            uint b52 = nfsr[3] >> 4 | nfsr[4] << 12;
            uint b60 = nfsr[3] >> 12 | nfsr[4] << 4;
            uint b62 = nfsr[3] >> 14 | nfsr[4] << 2;
            uint b63 = nfsr[3] >> 15 | nfsr[4] << 1;

            return (b62 ^ b60 ^ b52 ^ b45 ^ b37 ^ b33 ^ b28 ^ b21 ^ b14 ^ b9 ^ b0 ^ b63 & b60 ^ b37 & b33 
                ^ b15 & b9 ^ b60 & b52 & b45 ^ b33 & b28 & b21 ^ b63 & b45 & b28 & b9 ^ b60 & b52 & b37 & b33 
                ^ b63 & b60 & b21 & b15 ^ b63 & b60 & b52 & b45 & b37 ^ b33 & b28 & b21 & b15 & b9 
                ^ b52 & b45 & b37 & b33 & b28 & b21) & 0x0000FFFF;
        }

        public uint getOutputLFSR()
        {
            uint s0 = lfsr[0];
            uint s13 = lfsr[0] >> 13 | lfsr[1] << 3;
            uint s23 = lfsr[1] >> 7 | lfsr[2] << 9;
            uint s38 = lfsr[2] >> 6 | lfsr[3] << 10;
            uint s51 = lfsr[3] >> 3 | lfsr[4] << 13;
            uint s62 = lfsr[3] >> 14 | lfsr[4] << 2;

            return (s0 ^ s13 ^ s23 ^ s38 ^ s51 ^ s62) & 0x0000FFFF;
        }

        public uint getOutput()
        {
            uint b1 = nfsr[0] >> 1 | nfsr[1] << 15;
            uint b2 = nfsr[0] >> 2 | nfsr[1] << 14;
            uint b4 = nfsr[0] >> 4 | nfsr[1] << 12;
            uint b10 = nfsr[0] >> 10 | nfsr[1] << 6;
            uint b31 = nfsr[1] >> 15 | nfsr[2] << 1;
            uint b43 = nfsr[2] >> 11 | nfsr[3] << 5;
            uint b56 = nfsr[3] >> 8 | nfsr[4] << 8;
            uint b63 = nfsr[3] >> 15 | nfsr[4] << 1;
            uint s3 = lfsr[0] >> 3 | lfsr[1] << 13;
            uint s25 = lfsr[1] >> 9 | lfsr[2] << 7;
            uint s46 = lfsr[2] >> 14 | lfsr[3] << 2;
            uint s64 = lfsr[4];

            return (s25 ^ b63 ^ s3 & s64 ^ s46 & s64 ^ s64 & b63 ^ s3 & s25 & s46 ^ s3 & s46 & s64 ^ s3 & s46 & b63 
                ^ s25 & s46 & b63 ^ s46 & s64 & b63 ^ b1 ^ b2 ^ b4 ^ b10 ^ b31 ^ b43 ^ b56) & 0x0000FFFF;
        }

        public uint[] shift(uint[] array, uint val)
        {
            array[0] = array[1];
            array[1] = array[2];
            array[2] = array[3];
            array[3] = array[4];
            array[4] = val;

            return array;
        }

        public void setKey(byte[] keyBytes, byte[] ivBytes)
        {
            ivBytes[8] = (byte)0xFF;
            ivBytes[9] = (byte)0xFF;
            workingKey = keyBytes;
            workingIV = ivBytes;

            int j = 0;
            for (int i = 0; i < nfsr.Length; i++)
            {
                nfsr[i] = (uint)(workingKey[j + 1] << 8 | workingKey[j] & 0xFF) & 0x0000FFFF;
                lfsr[i] = (uint)(workingIV[j + 1] << 8 | workingIV[j] & 0xFF) & 0x0000FFFF;
                j += 2;
            }
        }

        public void processBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
        {
            if ((inOff + len) > input.Length)
            {
                GuiLogMessage("Input buffer too short!", NotificationLevel.Error);
            }

            if ((outOff + len) > output.Length)
            {
                GuiLogMessage("Output buffer too short!", NotificationLevel.Error);
            }

            for (int i = 0; i < len; i++)
            {
                output[outOff + i] = (byte)(input[inOff + i] ^ getKeyStream());
            }
        }

        public void oneRound()
        {
            output = getOutput();
            outp[0] = (byte)output;
            outp[1] = (byte)(output >> 8);

            nfsr = shift(nfsr, getOutputNFSR() ^ lfsr[0]);
            lfsr = shift(lfsr, getOutputLFSR());
        }

        public byte getKeyStream()
        {
            if (index > 1)
            {
                oneRound();
                index = 0;
            }
            byte b = outp[index++];
            keyStream += String.Format("{0:X2}", b);
            return b;
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
        }

        private void StatusChanged(int imageIndex)
        {
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
        }

        #endregion

    }
}
