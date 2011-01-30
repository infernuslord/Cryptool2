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
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;

namespace Cryptool.Vernam
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(false, "Vernam", "Vernam -- substitution cipher / stream cipher which is build by XORing the plaintext with a (pseudo) random stream of data to generate the ciphertext [One-time-Pad]", "Vernam/DetailedDescription/Description.xaml",
      "Vernam/Images/icon.png", "Vernam/Images/encrypt.png", "Vernam/Images/decrypt.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class Vernam : IEncryption
    {
        #region Private variables

        private VernamSettings settings;
        private byte[] inputKey;
        private string inputString;
        private string outputString;
        private enum VernamMode { encrypt, decrypt };

        #endregion

        #region Public interface

        /// <summary>
        /// Contructor
        /// </summary>
        public Vernam()
        {
            this.settings = new VernamSettings();
            ((VernamSettings)(this.settings)).LogMessage += Vernam_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (VernamSettings)value; }
        }

        [PropertyInfo(Direction.InputData, "Text input", "Input a string to be processed by the Vernam cipher", null, true, false,QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.InputData, "Key", "Input key as byte array", null, true, false, QuickWatchFormat.Text, null)]
        public byte[] InputKey
        {
            get { return this.inputKey; }
            set
            {
                if (this.inputKey != value)
                {
                    this.inputKey = value;
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "Stream output", "The string after processing with the Caesar cipher is converted to a stream. Default encoding is used.", null, false, false, QuickWatchFormat.Text, null)]
        public ICryptoolStream OutputStream
        {
            get
            {
                if (outputString != null)
                {
                    return new CStreamWriter(Encoding.Default.GetBytes(outputString));
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        [PropertyInfo(Direction.OutputData, "Text output","The string after processing with the Vernam cipher", null, false, false,QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }


        /// <summary>
        /// Vernam encryption
        /// </summary>
        public void Encrypt()
        {
            ProcessVernam(VernamMode.encrypt);
        }

        /// <summary>
        /// Vernam decryption
        /// </summary>
        public void Decrypt()
        {
            ProcessVernam(VernamMode.decrypt);
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
        /// Fire if progress bar has to be updated
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        /// <summary>
        /// Fire, if new message has to be shown in the status bar
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
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

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion



        #region Private methods

        private void ProcessVernam(VernamMode mode)
        {
            if (inputString != null && inputKey != null)
            {
                StringBuilder output = new StringBuilder(string.Empty);
                inputKey = refillKey(inputKey, inputString.Length);

                char cpos = '\0';
                for (int i = 0; i < inputString.Length; i++)
                {
                    switch (mode)
                    {
                        case VernamMode.encrypt:
                            cpos = (char)(inputString[i] ^ inputKey[i]);
                            break;
                        case VernamMode.decrypt:
                            cpos = (char)(inputString[i] ^ inputKey[i]);
                            break;
                    }
                    output.Append(Convert.ToString(cpos));

                    //show the progress
                    if (OnPluginProgressChanged != null)
                    {
                        OnPluginProgressChanged(this, new PluginProgressEventArgs(i, inputString.Length - 1));
                    }
                }
                outputString = output.ToString();
                OnPropertyChanged("OutputString");
                OnPropertyChanged("OutputStream");
                
            }
        }

        private byte[] refillKey(byte[] key, int inputDataLength)
        {
          try
          {
            byte[] fullKey = new byte[inputDataLength];
            for (int i = 0; i < inputDataLength; i++)
              fullKey[i] = key[i % key.Length];
            return fullKey;
          }
          catch (Exception ex)
          {
            Vernam_LogMessage(ex.Message, NotificationLevel.Error);            
          }
          return null;
        }

        void Vernam_LogMessage(string msg, NotificationLevel logLevel)
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
            switch (settings.Action)
            {
                case 0:
                    Encrypt();
                    break;
                case 1:
                    Decrypt();
                    break;
                default:
                    break;
            }
        }

        public void Pause()
        {

        }

        #endregion
    }
}
