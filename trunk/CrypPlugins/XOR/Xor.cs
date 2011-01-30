/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(false, "XOR", "XOR -- substitution cipher which is build by simple exclusive disjunction (XOR) operations", "XOR/DetailedDescription/Description.xaml",
      "XOR/Images/icon.png", "XOR/Images/encrypt.png", "XOR/Images/decrypt.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class Xor : IEncryption
    {

        #region Private variables

        private XORSettings settings;
        private string inputString;
        private string outputString;
        private string key;

        #endregion
        
        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public Xor()
        {
            this.settings = new XORSettings();
            ((XORSettings)(this.settings)).LogMessage += Xor_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (XORSettings)value; }
        }

        [PropertyInfo(Direction.OutputData, "Stream output", "The string after processing with the Xor cipher is converted to a stream. Default encoding is used.", "", false, false, QuickWatchFormat.Text, null)]
        public ICryptoolStream OutputData
        {
            get
            {
                if (outputString == null)
                {
                    return null;
                }

                return new CStreamWriter(Encoding.Default.GetBytes(outputString));
            }
            set { }
        }

        [PropertyInfo(Direction.InputData, "Text input", "Input a string to be processed by the Xor cipher", "", true, false, QuickWatchFormat.Text, null)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                if (value != inputString)
                {
                    this.inputString = value;
                    OnPropertyChanged("InputString");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "Key input", "Input a key string", "", false, false, QuickWatchFormat.Text, null)]
        public string Key
        {
            get { return this.key; }
            set
            {
                if (value != this.key)
                {
                    this.key = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "Text output", "The string after processing with the Xor cipher", "", false, false, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        #endregion

        #region IPlugin members

        public void Initialize()
        {
        }

        public void Dispose()
        {
            }

        public bool HasChanges
        {
            get { return settings.HasChanges; }
            set { settings.HasChanges = value; }
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
            Dispose();
        }

        public void PreExecution()
        {
            Dispose();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Does the actual Xor processing
        /// </summary>
        private void ProcessXor()
        {
            if (inputString != null)
            {
                StringBuilder output = new StringBuilder(string.Empty);

                string longKey = key;
                //resize key string to same length as input
                if (inputString.Length > key.Length)
                {
                    int keyPos = 0;
                    for (int i = key.Length; i < inputString.Length; i++)
                    {
                        longKey += key[keyPos];
                        keyPos++;
                        if (keyPos == key.Length)
                            keyPos = 0;
                    }
                }

                char cpos = '\0';
                for (int i = 0; i < inputString.Length; i++)
                {
                    cpos = (char)(inputString[i] ^ longKey[i]);
                    output.Append(Convert.ToString(cpos));

                    //show the progress
                    if (OnPluginProgressChanged != null)
                    {
                        OnPluginProgressChanged(this, new PluginProgressEventArgs(i, inputString.Length - 1));
                    }
                }

                outputString = output.ToString();
                OnPropertyChanged("OutputString");
                OnPropertyChanged("OutputData");
            }
        }

        private void Xor_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
				public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public void Execute()
        {
            ProcessXor();
        }

        public void Pause()
        {
        }

        #endregion
    }
}
