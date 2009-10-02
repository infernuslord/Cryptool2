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
using Cryptool.PluginBase.Cryptography;

using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Caesar
{
    [Author("Dr. Arno Wacker", "arno.wacker@cryptool.org", "Uni Duisburg-Essen", "http://www.vs.uni-duisburg-essen.de")]
    [PluginInfo("Cryptool.Caesar.Resources.res", false, "pluginName", "pluginToolTip", "Caesar/DetailedDescription/Description.xaml",
      "Caesar/Images/Caesar.png", "Caesar/Images/encrypt.png", "Caesar/Images/decrypt.png")] 
    [EncryptionType(EncryptionType.Classic)]
    public class Caesar : IEncryption
    {
        #region Private variables

        private CaesarSettings settings;
        // private CryptoolStream outputData;
        private string inputString;
        private string outputString;
        private enum CaesarMode { encrypt, decrypt };
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
        private bool isPlayMode = false;
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
            get { return (ISettings)this.settings; }
            set { this.settings = (CaesarSettings)value; }
        }


        [PropertyInfo(Direction.OutputData, "propStreamOutputToolTip", "propStreamOutputDescription", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public CryptoolStream OutputData
        {
            get
            {
                if (outputString != null)
                {                    
                    CryptoolStream cs = new CryptoolStream();
                    listCryptoolStreamsOut.Add(cs);
                    cs.OpenRead(Encoding.Default.GetBytes(outputString.ToCharArray()));
                    return cs;
                }
                else
                {
                    return null;
                }
            }
            set { }
        }


        [PropertyInfo(Direction.InputData, "Text input", "Input a string to be processed by the Caesar cipher", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.OutputData, "Text output", "The string after processing with the Caesar cipher", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }


        [PropertyInfo(Direction.InputData, "External alphabet input", "Input a string containing the alphabet which should be used by Caesar.\nIf no alphabet is provided on this input, the internal alphabet will be used.", "", false, false, DisplayLevel.Expert, QuickWatchFormat.Text, null)]
        public string InputAlphabet
        {
            get { return ((CaesarSettings)this.settings).AlphabetSymbols; }
            set 
            {
              if (value != null && value != settings.AlphabetSymbols) 
              { 
                ((CaesarSettings)this.settings).AlphabetSymbols = value;
                OnPropertyChanged("InputAlphabet");
              } 
            }
        }

        [PropertyInfo(Direction.InputData, "Shift value (integer)", "Same setting as Shift value in Settings-Pane but as dynamic input.", "", false, false, DisplayLevel.Expert, QuickWatchFormat.Text, null)]
        public int ShiftKey
        {
          get { return settings.ShiftKey; }
          set 
          { 
            if (value != settings.ShiftKey)
            {
              settings.ShiftKey = value;
              // Execute();
            }
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
          foreach (CryptoolStream stream in listCryptoolStreamsOut)
          {
            stream.Close();
          }
          listCryptoolStreamsOut.Clear();
        }

        public bool HasChanges
        {
          get { return settings.HasChanges; }
          set { settings.HasChanges = value; }
        }

        /// <summary>
        /// Feuern, wenn sich sich eine Änderung des Fortschrittsbalkens ergibt 
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        /// <summary>
        /// Hier kommt das Darstellungs Control hin, Jungs!
        /// </summary>
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

            // in case we want don't consider case in the alphabet, we use only capital letters, hence transform 
            // the whole alphabet to uppercase
            if (!cfg.CaseSensitiveAlphabet)
            {
                alphabet = cfg.AlphabetSymbols.ToUpper(); ;
            }
            

            if (inputString != null)
            {
                for (int i = 0; i < inputString.Length; i++)
                {
                    // get plaintext char which is currently processed
                    char currentchar = inputString[i];

                    // remember if it is upper case (otherwise lowercase is assumed)
                    bool uppercase = char.IsUpper(currentchar);

                    // get the position of the plaintext character in the alphabet
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
                        // we found the plaintext character in the alphabet, hence we do the shifting
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

                        // we have the position of the ciphertext character, hence just output it in the correct case
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
                        // the plaintext character was not found in the alphabet, hence proceed with handling unknown characters
                        switch ((CaesarSettings.UnknownSymbolHandlingMode)cfg.UnknownSymbolHandling)
                        {
                            case CaesarSettings.UnknownSymbolHandlingMode.Ignore:
                                output.Append(inputString[i]);
                                break;
                            case CaesarSettings.UnknownSymbolHandlingMode.Replace:
                                output.Append('?');
                                break;
                        }

                    }

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


        /// <summary>
        /// Handles log messages from the settings class
        /// </summary>
        private void Caesar_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
            //if (OnGuiLogNotificationOccured != null)
            //{
            //    OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, loglevel));
            //}
        }

        /// <summary>
        /// Handles re-execution events from settings class
        /// </summary>
        private void Caesar_ReExecute()
        {
            if (isPlayMode)
                Execute();
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
                    Caesar_LogMessage("encrypting", NotificationLevel.Debug);
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
