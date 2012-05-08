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
// for IControl
using Cryptool.PluginBase.Control;

namespace Cryptool.Plugins.Grain_v1
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Maxim Serebrianski", "ms_1990@gmx.de", "University of Mannheim", "http://www.uni-mannheim.de/1/startseite/index.html")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Grain_v1.Properties.Resources", "PluginCaption", "PluginTooltip", "Grain v1/DetailedDescription/doc.xml", new[] { "Grain v1/Images/grain.jpg" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
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
        public int[] NFSR = new int[80];
        public int[] LFSR = new int[80];
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
        }

        public int[] hextobin(string hex)
        {
            int[] binArray = new int[hex.Length * 4];
            StringBuilder bin = new StringBuilder();

            hex = hex.ToLower();
            for (int i = 0; i < hex.Length; i++)
            {
                if (hex[i].Equals('0')) bin.Append("0000");
                if (hex[i].Equals('1')) bin.Append("0001");
                if (hex[i].Equals('2')) bin.Append("0010");
                if (hex[i].Equals('3')) bin.Append("0011");
                if (hex[i].Equals('4')) bin.Append("0100");
                if (hex[i].Equals('5')) bin.Append("0101");
                if (hex[i].Equals('6')) bin.Append("0110");
                if (hex[i].Equals('7')) bin.Append("0111");
                if (hex[i].Equals('8')) bin.Append("1000");
                if (hex[i].Equals('9')) bin.Append("1001");
                if (hex[i].Equals('a')) bin.Append("1010");
                if (hex[i].Equals('b')) bin.Append("1011");
                if (hex[i].Equals('c')) bin.Append("1100");
                if (hex[i].Equals('d')) bin.Append("1101");
                if (hex[i].Equals('e')) bin.Append("1110");
                if (hex[i].Equals('f')) bin.Append("1111");
            }
            string temp = bin.ToString();
            for (int i = 0; i < temp.Length; i++) binArray[i] = Convert.ToInt32(temp.Substring(i,1));
            return binArray;
        }

        public string bintohex(string bin)
        {
            string hex = "";

            for (int i = 0; i < bin.Length; i += 4)
            {
                if (bin.Substring(i, 4) == "0000") hex += "0";
                if (bin.Substring(i, 4) == "0001") hex += "1";
                if (bin.Substring(i, 4) == "0010") hex += "2";
                if (bin.Substring(i, 4) == "0011") hex += "3";
                if (bin.Substring(i, 4) == "0100") hex += "4";
                if (bin.Substring(i, 4) == "0101") hex += "5";
                if (bin.Substring(i, 4) == "0110") hex += "6";
                if (bin.Substring(i, 4) == "0111") hex += "7";
                if (bin.Substring(i, 4) == "1000") hex += "8";
                if (bin.Substring(i, 4) == "1001") hex += "9";
                if (bin.Substring(i, 4) == "1010") hex += "A";
                if (bin.Substring(i, 4) == "1011") hex += "B";
                if (bin.Substring(i, 4) == "1100") hex += "C";
                if (bin.Substring(i, 4) == "1101") hex += "D";
                if (bin.Substring(i, 4) == "1110") hex += "E";
                if (bin.Substring(i, 4) == "1111") hex += "F";
            }
            return hex;
        }

        public void Execute()
        {
            try
            {
                int[] IV = hextobin( inputIV );
                int[] key = hextobin( inputKey );
                
                if (IV.Length != 64)
                {
                    GuiLogMessage("Invalid initialization vector length (" + IV.Length + " bits). It must be 64 Bits long!", NotificationLevel.Error);
                    return;
                }

                if (key.Length != 80)
                {
                    GuiLogMessage("Invalid key length (" + key.Length + " bits). It must be 80 bits long!", NotificationLevel.Error);
                    return;
                }

                GuiLogMessage("Length of IV: " + IV.Length, NotificationLevel.Info);
                GuiLogMessage("Length of key: " + key.Length, NotificationLevel.Info);

                int bitsToGenerate = inputString.Length * 4;

                // generate keystream
                DateTime startTime = DateTime.Now;

                GuiLogMessage("Starting encryption...", NotificationLevel.Info);

                initGrain(IV, key);
                string outputstream = output(bitsToGenerate);

                TimeSpan duration = DateTime.Now - startTime;

                OutputString = settings.BinOutput ? outputstream : bintohex(outputstream);
                
                if (stop)
                    GuiLogMessage("Aborted!", NotificationLevel.Info);
                else
                    GuiLogMessage("Encryption complete in " + duration + "! (input length : " + inputString.Length * 4 + " bits, keystream/output length: " + outputstream.Length + " bits)", NotificationLevel.Info);

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

        public void initGrain(int[] IV, int[] key)
        {
            int i;
            
            for (i = 0; i < 80; i++) NFSR[i] = key[i];  // initialize the NFSR with 80 key bits
            for (i = 0; i < 64; i++) LFSR[i] = IV[i];   // initialize the LFSR with 64 IV bits
            for (i = 64; i < 80; i++) LFSR[i] = 1;      // fill remaining LFSR slots with '1'

            for (i = 0; i < 160; i++) keyInit();
        }

        private void keyInit()
        {
            //g(x) next bit for NFSR
            int g_x = (LFSR[0] + NFSR[62] + NFSR[60] + NFSR[52] + NFSR[45] + NFSR[37] + NFSR[33] + NFSR[28] + NFSR[21] + NFSR[14] + NFSR[9] + NFSR[0] + (NFSR[63] * NFSR[60]) + (NFSR[37] * NFSR[33]) + (NFSR[15] * NFSR[9]) + (NFSR[60] * NFSR[52] * NFSR[45]) + (NFSR[33] * NFSR[28] * NFSR[21]) + (NFSR[63] * NFSR[45] * NFSR[28] * NFSR[9]) + (NFSR[60] * NFSR[52] * NFSR[37] * NFSR[33]) + (NFSR[63] * NFSR[60] * NFSR[21] * NFSR[15]) + (NFSR[63] * NFSR[60] * NFSR[52] * NFSR[45] * NFSR[37]) + (NFSR[33] * NFSR[28] * NFSR[21] * NFSR[15] * NFSR[9]) + (NFSR[52] * NFSR[45] * NFSR[37] * NFSR[33] * NFSR[28] * NFSR[21])) % 2;
            //f(x) next bit for LFSR
            int f_x = (LFSR[62] + LFSR[51] + LFSR[38] + LFSR[23] + LFSR[13] + LFSR[0]) % 2;

            //h(x)
            int inFunction = (LFSR[25] + NFSR[63] + (LFSR[3] * LFSR[64]) + (LFSR[46] * LFSR[64]) + (LFSR[64] * NFSR[63]) + (LFSR[3] * LFSR[25] * LFSR[46]) + (LFSR[3] * LFSR[46] * LFSR[64]) + (LFSR[3] * LFSR[46] * NFSR[63]) + (LFSR[25] * LFSR[46] * NFSR[63]) + (LFSR[46] * LFSR[64] * NFSR[63])) % 2;
            //z_i
            int outFunction = (NFSR[1] + NFSR[2] + NFSR[4] + NFSR[10] + NFSR[31] + NFSR[43] + NFSR[56] + inFunction) % 2;

            //f_x(80)
            int lfsrIn = (f_x + outFunction) % 2;
            //g_x(80)
            int nfsrIn = (g_x + LFSR[0] + outFunction) % 2;

            //shifting the registers to the left
            for (int i = 0; i < 79; i++)
            {
                LFSR[i] = LFSR[i + 1];
                NFSR[i] = NFSR[i + 1];
            }
            LFSR[79] = lfsrIn;
            NFSR[79] = nfsrIn;
        }

        private int Round()
        {
            //g(x) next bit for NFSR
            int g_x = (LFSR[0] + NFSR[62] + NFSR[60] + NFSR[52] + NFSR[45] + NFSR[37] + NFSR[33] + NFSR[28] + NFSR[21] + NFSR[14] + NFSR[9] + NFSR[0] + (NFSR[63] * NFSR[60]) + (NFSR[37] * NFSR[33]) + (NFSR[15] * NFSR[9]) + (NFSR[60] * NFSR[52] * NFSR[45]) + (NFSR[33] * NFSR[28] * NFSR[21]) + (NFSR[63] * NFSR[45] * NFSR[28] * NFSR[9]) + (NFSR[60] * NFSR[52] * NFSR[37] * NFSR[33]) + (NFSR[63] * NFSR[60] * NFSR[21] * NFSR[15]) + (NFSR[63] * NFSR[60] * NFSR[52] * NFSR[45] * NFSR[37]) + (NFSR[33] * NFSR[28] * NFSR[21] * NFSR[15] * NFSR[9]) + (NFSR[52] * NFSR[45] * NFSR[37] * NFSR[33] * NFSR[28] * NFSR[21])) % 2;
            //f(x) next bit for LFSR
            int f_x = (LFSR[62] + LFSR[51] + LFSR[38] + LFSR[23] + LFSR[13] + LFSR[0]) % 2;

            //h(x)
            int inFunction = (LFSR[25] + NFSR[63] + (LFSR[3] * LFSR[64]) + (LFSR[46] * LFSR[64]) + (LFSR[64] * NFSR[63]) + (LFSR[3] * LFSR[25] * LFSR[46]) + (LFSR[3] * LFSR[46] * LFSR[64]) + (LFSR[3] * LFSR[46] * NFSR[63]) + (LFSR[25] * LFSR[46] * NFSR[63]) + (LFSR[46] * LFSR[64] * NFSR[63])) % 2;
            //z_i
            int outFunction = (NFSR[1] + NFSR[2] + NFSR[4] + NFSR[10] + NFSR[31] + NFSR[43] + NFSR[56] + inFunction) % 2;

            //f_x(80)
            int lfsrIn = f_x;
            //g_x(80)
            int nfsrIn = (g_x + LFSR[0]) % 2;

            //shifting the registers to the left
            for (int i = 0; i < 79; i++)
            {
                LFSR[i] = LFSR[i + 1];
                NFSR[i] = NFSR[i + 1];
            }
            LFSR[79] = lfsrIn;
            NFSR[79] = nfsrIn;

            return outFunction;
        }

        public string keystreamGrain()
        {
            StringBuilder builder = new StringBuilder(inputString.Length * 4);

            for (int i = 0; i < inputString.Length * 4; i++)
            {
                builder.Append(Round());
                if (stop) break;
            }
            string keyStream = bintohex(builder.ToString());
            GuiLogMessage("Key Stream: " + keyStream, NotificationLevel.Info);
            return builder.ToString();
        }

        public string output(int nBits)
        {
            StringBuilder builder = new StringBuilder(nBits);
            int[] input = new int[nBits];
            int[] result = new int[nBits];
            input = hextobin(InputString);
            string keyStream = keystreamGrain();
            
            
            char[] temp = keyStream.ToCharArray();
            for (int i = 0; i < temp.Length; i++) result[i] = (int)temp[i];
            
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = (result[i] + input[i]) % 2;
            }
            foreach (int n in result)
            {
                builder.Append(n);
            }
            return builder.ToString();
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
