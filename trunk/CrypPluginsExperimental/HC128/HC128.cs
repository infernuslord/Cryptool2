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
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.HC128
{
    [Author("Maxim Serebrianski", "ms_1990@gmx.de", "University of Mannheim", "http://www.uni-mannheim.de/1/startseite/index.html")]
    [PluginInfo("HC128.Properties.Resources", "PluginCaption", "PluginTooltip", "HC128/DetailedDescription/doc.xml", new[] { "HC128/Images/hc128.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class HC128 : ICrypComponent
    {
        #region Private Variables
        private HC128Settings settings;
        private string inputString;
        private string outputString;
        private string inputKey;
        private string inputIV;
        private bool stop = false;
        #endregion

        #region Public Variables
        public uint[] p = new uint[512];
        public uint[] q = new uint[512];
        public uint count = 0;
        public byte[] Out;
        public byte[] In;
        public string keyStream = "";
        public byte[] key, iv;
        public byte[] buffer = new byte[4];
        public int idx = 0;
        #endregion

        public HC128()
        {
            this.settings = new HC128Settings();
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (HC128Settings)value; }
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

        /* Main method for launching the cipher */
        public void Execute()
        {
            StringBuilder builder = new StringBuilder();
            In = stringToByteArray(inputString);
            Out = new byte[In.Length];
            try
            {
                DateTime startTime = DateTime.Now;
                GuiLogMessage("Starting encryption...", NotificationLevel.Info);

                /* Initialize the algorithm */
                init();

                /* Generate ciphertext bytes */
                generateOutBytes(In, 0, In.Length, Out, 0);
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

        /* Reset method */
        public void Dispose()
        {
            stop = false;
            inputKey = null;
            outputString = null;
            inputString = null;
            keyStream = null;
        }

        /* Convert the input string into a byte array
         * If the string length is not a multiple of 2 a '0' is added at the end */
        public byte[] stringToByteArray(string input)
        {
            if (input.Length % 2 == 1) input += '0';
            byte[] array = new byte[input.Length / 2];
            for (int i = 0, j = 0; i < input.Length; i += 2, j++) array[j] = Convert.ToByte(input.Substring(i, 2), 16);
            return array;
        }

        /* Convert the IV string into a byte array */
        public byte[] IVstringToByteArray(string input)
        {
            byte[] array = new byte[16];

            if (input.Length != 32)
            {
                stop = true;
                GuiLogMessage("IV length must be 16 byte (128 bit)!", NotificationLevel.Error);
            }
            else
            {
                for (int i = 0, j = 0; i < input.Length; i += 2, j++) array[j] = Convert.ToByte(input.Substring(i, 2), 16);
            }
            return array;
        }

        /* Convert the key string into a byte array */
        public byte[] KEYstringToByteArray(string input)
        {
            byte[] array = new byte[16];

            if (input.Length != 32)
            {
                stop = true;
                GuiLogMessage("Key length must be 16 byte (128 bit)!", NotificationLevel.Error);
            }
            else
            {
                for (int i = 0, j = 0; i < input.Length; i += 2, j++) array[j] = Convert.ToByte(input.Substring(i, 2), 16);
            }
            return array;
        }

        /* Main initialization method */
        public void init()
        {
            byte[] iv = IVstringToByteArray(inputIV);
            byte[] key = KEYstringToByteArray(inputKey);
            count = 0;

            uint[] w = new uint[1280];

            for (int i = 0; i < 16; i++)
            {
                w[i >> 2] |= (uint)(key[i] & 0xff) << (8 * (i & 0x3));
            }
            Array.Copy(w, 0, w, 4, 4);

            for (int i = 0; i < iv.Length && i < 16; i++)
            {
                w[(i >> 2) + 8] |= (uint)(iv[i] & 0xff) << (8 * (i & 0x3));
            }
            Array.Copy(w, 8, w, 12, 4);

            for (int i = 16; i < 1280; i++)
            {
                w[i] = (uint)(f2(w[i - 2]) + w[i - 7] + f1(w[i - 15]) + w[i - 16] + i);
            }

            Array.Copy(w, 256, p, 0, 512);
            Array.Copy(w, 768, q, 0, 512);

            for (int i = 0; i < 512; i++)
            {
                p[i] = Round();
            }
            for (int i = 0; i < 512; i++)
            {
                q[i] = Round();
            }

            count = 0;
        }

        /* Computes x % 1024 */
        public uint mod1024(uint x)
        {
            return x & 0x3FF;
        }

        /* Computes x % 512 */
        public uint mod512(uint x)
        {
            return x & 0x1FF;
        }

        /* (x - y) % 512 */
        public uint minus(uint x, uint y)
        {
            return mod512(x - y);
        }

        /* Left rotation function */
        public uint leftRotation(uint x, int bits)
        {
            return (x << bits) | (x >> -bits);
        }

        /* Right rotation function */
        public uint rightRotation(uint x, int bits)
        {
            return (x >> bits) | (x << -bits);
        }

        /* f1 function */
        public uint f1(uint x)
        {
            return rightRotation(x, 7) ^ rightRotation(x, 18) ^ (x >> 3);
        }

        /* f2 function */
        public uint f2(uint x)
        {
            return rightRotation(x, 17) ^ rightRotation(x, 19) ^ (x >> 10);
        }

        /* g1 function */
        public uint g1(uint x, uint y, uint z)
        {
            return (rightRotation(x, 10) ^ rightRotation(z, 23)) + rightRotation(y, 8);
        }

        /* g2 function */
        public uint g2(uint x, uint y, uint z)
        {
            return (leftRotation(x, 10) ^ leftRotation(z, 23)) + leftRotation(y, 8);
        }

        /* h1 function */
        public uint h1(uint x)
        {
            return q[x & 0xFF] + q[((x >> 16) & 0xFF) + 256];
        }

        /* h2 function */
        public uint h2(uint x)
        {
            return p[x & 0xFF] + p[((x >> 16) & 0xFF) + 256];
        }

        /* One cipher round */
        public uint Round()
        {
            uint j = mod512(count);
            uint result;
            if (count < 512)
            {
                p[j] += g1(p[minus(j, 3)], p[minus(j, 10)], p[minus(j, 511)]);
                result = h1(p[minus(j, 12)]) ^ p[j];
            }
            else
            {
                q[j] += g2(q[minus(j, 3)], q[minus(j, 10)], q[minus(j, 511)]);
                result = h2(q[minus(j, 12)]) ^ q[j];
            }
            count = mod1024(count + 1);
            return result;
        }

        /* Generate key stream */
        public byte generateKeyStream()
        {
            if (idx == 0)
            {
                uint step = Round();
                buffer[0] = (byte)(step & 0xFF);
                step >>= 8;
                buffer[1] = (byte)(step & 0xFF);
                step >>= 8;
                buffer[2] = (byte)(step & 0xFF);
                step >>= 8;
                buffer[3] = (byte)(step & 0xFF);
            }
            byte result = buffer[idx];
            idx = idx + 1 & 0x3;
            keyStream += String.Format("{0:X2}", result);
            return result;
        }

        /* Generate ciphertext */
        public void generateOutBytes(byte[] input, int inOffset, int length, byte[] output, int outOffset)
        {
            if ((inOffset + length) > input.Length)
            {
                GuiLogMessage("Input buffer too short!", NotificationLevel.Error);
            }

            if ((outOffset + length) > output.Length)
            {
                GuiLogMessage("Output buffer too short!", NotificationLevel.Error);
            }

            for (int i = 0; i < length; i++)
            {
                output[outOffset + i] = (byte)(input[inOffset + i] ^ generateKeyStream());
            }
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
