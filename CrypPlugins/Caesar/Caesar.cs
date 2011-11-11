/*                              
   Copyright 2009 Arno Wacker, Uni Duisburg-Essen

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

using System.Collections.Generic;
using System.Text;
using Cryptool.PluginBase;

using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Caesar
{
    [Author("Dr. Arno Wacker", "arno.wacker@cryptool.org", "Uni Duisburg-Essen", "http://www.vs.uni-duisburg-essen.de")]
    [PluginInfo("Cryptool.Caesar.Properties.Resources", "PluginCaption", "PluginTooltip", "Caesar/DetailedDescription/doc.xml", "Caesar/Images/Caesar.png", "Caesar/Images/encrypt.png", "Caesar/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Caesar : ICrypComponent
    {
        #region Private elements

        private CaesarSettings settings;
        private bool isPlayMode = false;

        private enum CaesarMode { encrypt, decrypt };

        #endregion
        
        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public Caesar()
        {
            this.settings = new CaesarSettings();
            this.settings.LogMessage += Caesar_LogMessage;
            this.settings.ReExecute += Caesar_ReExecute;
        }
  
        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
        }


        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", false, QuickWatchFormat.Text, null)]
        public ICryptoolStream OutputData
        {
            get
            {
                if (OutputString != null)
                {
                    return new CStreamWriter(Encoding.UTF8.GetBytes(OutputString));
                }

                return null;
            }
        }

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true, QuickWatchFormat.Text, null)]
        public string InputString
        {
            get;
            set; 
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "InputAlphabetCaption", "InputAlphabetTooltip", false, QuickWatchFormat.Text, null)]
        public string InputAlphabet
        {
            get { return this.settings.AlphabetSymbols; }
            set 
            {
              if (value != null && value != settings.AlphabetSymbols) 
              { 
                this.settings.AlphabetSymbols = value;
                OnPropertyChanged("InputAlphabet");
              } 
            }
        }

        [PropertyInfo(Direction.InputData, "ShiftKeyCaption", "ShiftKeyTooltip", false, QuickWatchFormat.Text, null)]
        public int ShiftKey
        {
          get { return settings.ShiftKey; }
          set 
          { 
              settings.ShiftKey = value;
          }
        }


        /// <summary>
        /// Caesar encryption
        /// </summary>
        public void Encrypt()
        {
            ProcessCaesar(CaesarMode.encrypt);
        }

        /// <summary>
        /// Caesar decryption
        /// </summary>
        public void Decrypt()
        {
            ProcessCaesar(CaesarMode.decrypt);
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
        /// Fires events to indicate progress bar changes.
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fires events to indicate log messages.
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        /// <summary>
        /// Algorithm visualization (if any)
        /// </summary>
        public UserControl Presentation
        {
          get { return null; }
        }

        public void Stop()
        {

        }

        public void PostExecution()
        {
            isPlayMode = false;
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
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Does the actual Caesar processing, i.e. encryption or decryption
        /// </summary>
        private void ProcessCaesar(CaesarMode mode)
        {
            CaesarSettings cfg = (CaesarSettings)this.settings;
            StringBuilder output = new StringBuilder("");
            string alphabet = cfg.AlphabetSymbols;

            // If we are working in case-insensitive mode, we will use only
            // capital letters, hence we must transform the whole alphabet
            // to uppercase.
            if (!cfg.CaseSensitiveAlphabet)
            {
                alphabet = cfg.AlphabetSymbols.ToUpper(); ;
            }
            

            if (!string.IsNullOrEmpty(InputString))
            {
                for (int i = 0; i < InputString.Length; i++)
                {
                    // Get the plaintext char currently being processed.
                    char currentchar = InputString[i];

                    // Store whether it is upper case (otherwise lowercase is assumed).
                    bool uppercase = char.IsUpper(currentchar);

                    // Get the position of the plaintext character in the alphabet.
                    int ppos = 0;
                    if (cfg.CaseSensitiveAlphabet)
                    {
                        ppos = alphabet.IndexOf(currentchar);
                    }
                    else
                    {
                        ppos = alphabet.IndexOf(char.ToUpper(currentchar));
                    }
 
                    if (ppos >= 0)
                    {
                        // We found the plaintext character in the alphabet,
                        // hence we will commence shifting.
                        int cpos = 0; ;
                        switch (mode)
                        {
                            case CaesarMode.encrypt:
                                cpos = (ppos + cfg.ShiftKey) % alphabet.Length;
                                break;
                            case CaesarMode.decrypt:
                                cpos = (ppos - cfg.ShiftKey + alphabet.Length) % alphabet.Length;
                                break;
                        }

                        // We have the position of the ciphertext character,
                        // hence just output it in the correct case.
                        if (cfg.CaseSensitiveAlphabet)
                        {
                            output.Append(alphabet[cpos]);
                        }
                        else
                        {
                            if (uppercase)
                            {
                                output.Append(char.ToUpper(alphabet[cpos]));
                            }
                            else
                            {
                                output.Append(char.ToLower(alphabet[cpos]));
                            }
                        }
                        
                    }
                    else
                    {
                        // The plaintext character was not found in the alphabet,
                        // hence proceed with handling unknown characters.
                        switch ((CaesarSettings.UnknownSymbolHandlingMode)cfg.UnknownSymbolHandling)
                        {
                            case CaesarSettings.UnknownSymbolHandlingMode.Ignore:
                                output.Append(InputString[i]);
                                break;
                            case CaesarSettings.UnknownSymbolHandlingMode.Replace:
                                output.Append('?');
                                break;
                        }

                    }

                    // Show the progress.
                    ProgressChanged(i, InputString.Length - 1);

                }
                OutputString = output.ToString();
                OnPropertyChanged("OutputString");
                OnPropertyChanged("OutputData");
            }
        }


        /// <summary>
        /// Handles log messages from the settings class
        /// </summary>
        private void Caesar_LogMessage(string msg, NotificationLevel loglevel)
        {
            GuiLogMessage(msg, loglevel);
        }

        /// <summary>
        /// Handles re-execution events from settings class
        /// </summary>
        private void Caesar_ReExecute()
        {
            if (isPlayMode)
            {
                Execute();   
            }
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
			public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

		public void Execute()
        {
            isPlayMode = true;

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

        #endregion

    }
}
