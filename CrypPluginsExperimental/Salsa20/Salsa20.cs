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
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Cryptool.Plugins.Salsa20
{
    [Author("Maxim Serebrianski", "ms_1990@gmx.de", "University of Mannheim", "http://www.uni-mannheim.de/1/startseite/index.html")]
    [PluginInfo("Salsa20.Properties.Resources", "PluginCaption", "PluginTooltip", "Salsa20/DetailedDescription/doc.xml", new[] { "Salsa20/Images/salsa20.jpg" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Salsa20 : ICrypComponent
    {
        #region Private Variables
        private Salsa20Settings settings;
        private string inputString;
        private string outputString;
        private string inputKey;
        private string inputIV;
        private bool stop = false;
        private bool inputValid = false;
        #endregion

        #region Public Variables
        public static int stateSize = 16; // 16, 32 bit ints = 64 bytes
        public byte[] sigma, tau;
        public int index = 0;
        public uint[] engineState = new uint[stateSize]; // state
        public uint[] x = new uint[stateSize]; // internal buffer
        public byte[] keyStream = new byte[stateSize * 4];
        public byte[] workingKey; 
        public byte[] workingIV;
        public int cW0, cW1, cW2;
        public byte[] In, Out;
        public string KeyStream = "";
        #endregion

        public Salsa20()
        {
            this.settings = new Salsa20Settings();
        }

        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (Salsa20Settings)value; }
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
            sigma = Encoding.ASCII.GetBytes("expand 32-byte k");
            tau = Encoding.ASCII.GetBytes("expand 16-byte k");
            try
            {
                if (stop) GuiLogMessage("Aborted!", NotificationLevel.Info);
                else
                {
                    DateTime startTime = DateTime.Now;

                    /* Initialize the algorithm */
                    init();
                    if (stop)
                    {
                        GuiLogMessage("Aborted!", NotificationLevel.Info);
                    }
                    else
                    {
                        GuiLogMessage("Starting encryption...", NotificationLevel.Info);

                        /* Generate ciphertext bytes */
                        generateOutBytes(In, 0, In.Length, Out, 0);
                        foreach (byte b in Out) builder.Append(String.Format("{0:X2}", b));
                        TimeSpan duration = DateTime.Now - startTime;
                        OutputString = builder.ToString();

                        GuiLogMessage("Encryption complete in " + duration + "!", NotificationLevel.Info);
                        GuiLogMessage("Key Stream: " + KeyStream, NotificationLevel.Info);
                    }
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
            KeyStream = null;
        }

        /* Check string for hex characters only */
        public bool hexInput(string str)
        {
            Regex myRegex = new Regex("^[0-9A-Fa-f]*$");
            inputValid = myRegex.IsMatch(str);
            return inputValid;
        }

        /* Convert the input string into a byte array
         * If the string length is not a multiple of 2 a '0' is added at the end */
        public byte[] stringToByteArray(string input)
        {
            byte[] array = null;

            if (input.Length % 2 == 1) array = new byte[(input.Length / 2) + 1];
            else array = new byte[input.Length / 2];

            if (hexInput(input))
            {
                if (input.Length % 2 == 1)
                {
                    GuiLogMessage("Odd number of digits in the plaintext, adding 0 to last position.", NotificationLevel.Info);
                    input += '0';
                }
                for (int i = 0, j = 0; i < input.Length; i += 2, j++) array[j] = Convert.ToByte(input.Substring(i, 2), 16);
            }
            else
            {
                GuiLogMessage("Invalid character(s) in the plaintext detected. Only '0' to '9' and 'A' to 'F' are allowed!", NotificationLevel.Error);
                stop = true;
            }
            return array;
        }

        /* Convert the IV string into a byte array */
        public byte[] IVstringToByteArray(string input)
        {
            byte[] array = new byte[8];

            if (hexInput(input))
            {
                if (input.Length != 16)
                {
                    stop = true;
                    GuiLogMessage("IV length must be 8 byte (64 bit)!", NotificationLevel.Error);
                }
                else
                {
                    for (int i = 0, j = 0; i < input.Length; i += 2, j++) array[j] = Convert.ToByte(input.Substring(i, 2), 16);
                }
            }
            else
            {
                GuiLogMessage("Invalid character(s) in the IV detected. Only '0' to '9' and 'A' to 'F' are allowed!", NotificationLevel.Error);
                stop = true;
            }
            return array;
        }

        /* Convert the key string into a byte array */
        public byte[] KEYstringToByteArray(string input)
        {
            byte[] array = null;

            if (input.Length % 2 == 1) array = new byte[(input.Length / 2) + 1];
            else array = new byte[input.Length / 2];

            if (hexInput(input))
            {
                if (input.Length == 32 || input.Length == 64)
                {
                    for (int i = 0, j = 0; i < input.Length; i += 2, j++) array[j] = Convert.ToByte(input.Substring(i, 2), 16);
                }
                else
                {
                    stop = true;
                    GuiLogMessage("Key length must be 16 byte (128 bit) or 32 byte (256 bit)!", NotificationLevel.Error);
                }
            }
            else
            {
                GuiLogMessage("Invalid character(s) in the key detected. Only '0' to '9' and 'A' to 'F' are allowed!", NotificationLevel.Error);
                stop = true;
            }
            return array;
        }

        /* Main initialization method */
        public void init()
        {
            byte[] iv = IVstringToByteArray(inputIV);
            byte[] key = KEYstringToByteArray(inputKey);
            setKey(key, iv);
        }

        /* Keysetup */
        public void setKey(byte[] keyBytes, byte[] ivBytes)
        {
            workingKey = keyBytes;
            workingIV = ivBytes;

            index = 0;
            resetCounter();
            int offset = 0;
            byte[] constants;

            // Key
            engineState[1] = byteToIntLE(workingKey, 0);
            engineState[2] = byteToIntLE(workingKey, 4);
            engineState[3] = byteToIntLE(workingKey, 8);
            engineState[4] = byteToIntLE(workingKey, 12);

            if (workingKey.Length == 32)
            {
                constants = sigma;
                offset = 16;
            }
            else
            {
                constants = tau;
            }

            engineState[11] = byteToIntLE(workingKey, offset);
            engineState[12] = byteToIntLE(workingKey, offset + 4);
            engineState[13] = byteToIntLE(workingKey, offset + 8);
            engineState[14] = byteToIntLE(workingKey, offset + 12);
            engineState[0] = byteToIntLE(constants, 0);
            engineState[5] = byteToIntLE(constants, 4);
            engineState[10] = byteToIntLE(constants, 8);
            engineState[15] = byteToIntLE(constants, 12);

            // IV
            engineState[6] = byteToIntLE(workingIV, 0);
            engineState[7] = byteToIntLE(workingIV, 4);
            engineState[8] = engineState[9] = 0;
        }

        /* Left rotation */
        public uint rotateLeft(uint x, int y)
        {
            return (x << y) | (x >> -y);
        }

        /* Convert Integer to little endian byte array */
        public byte[] intToByteLE(uint x, byte[] output, int off)
        {
            output[off] = (byte)x;
            output[off + 1] = (byte)(x >> 8);
            output[off + 2] = (byte)(x >> 16);
            output[off + 3] = (byte)(x >> 24);
            return output;
        }

        /* Convert little endian byte array to Integer */
        public uint byteToIntLE(byte[] x, int offset)
        {
            return (uint)(((x[offset] & 255)) | ((x[offset + 1] & 255) << 8) | ((x[offset + 2] & 255) << 16) | (x[offset + 3] << 24));
        }

        /* Reset the counter */
        public void resetCounter()
        {
            cW0 = 0;
            cW1 = 0;
            cW2 = 0;
        }

        /* Check the limit of 2^70 bytes */
        public bool limitExceeded(int length)
        {
            if (cW0 >= 0)
            {
                cW0 += length;
            }
            else
            {
                cW0 += length;
                if (cW0 >= 0)
                {
                    cW1++;
                    if (cW1 == 0)
                    {
                        cW2++;
                        return (cW2 & 0x20) != 0;   // 2^(32 + 32 + 6)
                    }
                }

            }
            return false;
        }

        /* Generate key stream */
        public void generateKeyStream(uint[] input, byte[] output)
        {
            int offset = 0;

            Array.Copy(input, 0, x, 0, input.Length);

            for (int i = 0; i < 10; i++)
            {
                x[4] ^= rotateLeft((x[0] + x[12]), 7);
                x[8] ^= rotateLeft((x[4] + x[0]), 9);
                x[12] ^= rotateLeft((x[8] + x[4]), 13);
                x[0] ^= rotateLeft((x[12] + x[8]), 18);
                x[9] ^= rotateLeft((x[5] + x[1]), 7);
                x[13] ^= rotateLeft((x[9] + x[5]), 9);
                x[1] ^= rotateLeft((x[13] + x[9]), 13);
                x[5] ^= rotateLeft((x[1] + x[13]), 18);
                x[14] ^= rotateLeft((x[10] + x[6]), 7);
                x[2] ^= rotateLeft((x[14] + x[10]), 9);
                x[6] ^= rotateLeft((x[2] + x[14]), 13);
                x[10] ^= rotateLeft((x[6] + x[2]), 18);
                x[3] ^= rotateLeft((x[15] + x[11]), 7);
                x[7] ^= rotateLeft((x[3] + x[15]), 9);
                x[11] ^= rotateLeft((x[7] + x[3]), 13);
                x[15] ^= rotateLeft((x[11] + x[7]), 18);
                x[1] ^= rotateLeft((x[0] + x[3]), 7);
                x[2] ^= rotateLeft((x[1] + x[0]), 9);
                x[3] ^= rotateLeft((x[2] + x[1]), 13);
                x[0] ^= rotateLeft((x[3] + x[2]), 18);
                x[6] ^= rotateLeft((x[5] + x[4]), 7);
                x[7] ^= rotateLeft((x[6] + x[5]), 9);
                x[4] ^= rotateLeft((x[7] + x[6]), 13);
                x[5] ^= rotateLeft((x[4] + x[7]), 18);
                x[11] ^= rotateLeft((x[10] + x[9]), 7);
                x[8] ^= rotateLeft((x[11] + x[10]), 9);
                x[9] ^= rotateLeft((x[8] + x[11]), 13);
                x[10] ^= rotateLeft((x[9] + x[8]), 18);
                x[12] ^= rotateLeft((x[15] + x[14]), 7);
                x[13] ^= rotateLeft((x[12] + x[15]), 9);
                x[14] ^= rotateLeft((x[13] + x[12]), 13);
                x[15] ^= rotateLeft((x[14] + x[13]), 18);
            }
            for (int i = 0; i < stateSize; i++)
            {
                intToByteLE(x[i] + input[i], output, offset);
                offset += 4;
            }
            for (int i = stateSize; i < x.Length; i++)
            {
                intToByteLE(x[i], output, offset);
                offset += 4;
            }
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

            if (limitExceeded(length))
            {
                GuiLogMessage("2^70 byte limit per IV would be exceeded; Change IV", NotificationLevel.Error);
            }

            for (int i = 0; i < length; i++)
            {
                if (index == 0)
                {
                    generateKeyStream(engineState, keyStream);
                    engineState[8]++;
                    if (engineState[8] == 0)
                    {
                        engineState[9]++;
                    }
                }
                KeyStream += String.Format("{0:X2}", keyStream[index]);
                output[i + outOffset] = (byte)(keyStream[index] ^ input[i + inOffset]);
                index = (index + 1) & 63;
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

