/*
   Copyright 2011 Matth�us Wander, University of Duisburg-Essen

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
using Cryptool.PluginBase.Cryptography;

using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;

namespace Cryptool.XOR
{
    [Author("Matth�us Wander", "wander@cryptool.org", "University of Duisburg-Essen", "http://www.vs.uni-due.de")]
    [PluginInfo("Cryptool.XOR.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL",
      "XOR/Images/icon.png", "XOR/Images/encrypt.png", "XOR/Images/decrypt.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class Xor : IEncryption
    {

        private ISettings settings;
        private byte[] inputData;
        private byte[] outputData;
        private byte[] key;

        /// <summary>
        /// Constructor
        /// </summary>
        public Xor()
        {
            this.settings = new XORSettings();
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = value; }
        }

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", "", true, false, QuickWatchFormat.Text, null)]
        public byte[] InputData
        {
            get { return this.inputData; }
            set
            {
                if (value != inputData)
                {
                    this.inputData = value;
                    OnPropertyChanged("InputData");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public byte[] Key
        {
            get { return this.key; }
            set
            {
                if (value != key)
                {
                    this.key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public byte[] OutputData
        {
            get { return outputData; }
            set { this.outputData = value; }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Fire, if progress bar has to be updated
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        /// <summary>
        /// Fire, if new message has to be shown in the status bar
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void Stop()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public void Execute()
        {
            // Don't process if input is empty
            if (inputData == null || inputData.Length == 0 || key == null || key.Length == 0)
                return;

            byte[] longKey = key;
            if (key.Length < inputData.Length) // repeat key if necessary
            {
                GuiLogMessage("Key is too short. Will be expanded to match input length", NotificationLevel.Warning);
                longKey = new byte[inputData.Length];

                int offset = 0;
                while(offset < longKey.Length)
                {
                    int readBytes = Math.Min(longKey.Length - offset, key.Length);

                    Array.Copy(key, 0, longKey, offset, readBytes);
                    offset += readBytes;
                }
            }

            outputData = new byte[inputData.Length]; // XOR now
            for (int i = 0; i < inputData.Length; i++)
            {
                outputData[i] = (byte)(inputData[i] ^ longKey[i]);

                //show the progress
                if (OnPluginProgressChanged != null)
                {
                    OnPluginProgressChanged(this, new PluginProgressEventArgs(i, inputData.Length - 1));
                }
            }

            OnPropertyChanged("OutputData");
        }

        public void Pause()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void GuiLogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }
    }
}
