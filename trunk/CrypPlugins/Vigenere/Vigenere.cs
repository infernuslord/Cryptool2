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

namespace Cryptool.Vigenere
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo(false, "Vigenère", "Vigenère -- classic polyalphabetic substitution cipher", "Vigenere/DetailedDescription/Description.xaml",
      "Vigenere/Images/icon.png", "Vigenere/Images/encrypt.png", "Vigenere/Images/decrypt.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class Vigenere : IEncryption
    {
        #region Private variables

        private VigenereSettings settings;
        private CryptoolStream outputData;
        private string inputString;
        private string outputString;
        private char[] keyword;
        private enum VigenereMode { encrypt, decrypt };
        private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public Vigenere()
        {
            this.settings = new VigenereSettings();
            ((VigenereSettings)(this.settings)).LogMessage += Vigenere_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (VigenereSettings)value; }
        }

        [PropertyInfo(Direction.OutputData, "Stream output", "The string after processing with the Vigenère cipher is converted to a stream.Default encoding is used.", null, false, false, DisplayLevel.Beginner,QuickWatchFormat.Text, null)]
        public CryptoolStream OutputData
        {
            get
            {
                if (outputString != null)
                {
                    CryptoolStream cs = new CryptoolStream();
                    listCryptoolStreamsOut.Add(cs);
                    cs.OpenRead(this.GetPluginInfoAttribute().Caption, Encoding.Default.GetBytes(outputString.ToCharArray()));
                    return cs;
                }
                else
                {
                    return null;
                }
            }
            set { }
        }

        [PropertyInfo(Direction.InputData, "Text input", "Input a string to be processed by the Vigenère cipher", null, true, false, DisplayLevel.Beginner,QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.OutputData,"Text output", "The string after processing with the Vigenère cipher", null, false, false, DisplayLevel.Beginner,QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.InputData, "External alphabet input", "Input a string containing the alhabet which should be used by Vigenère. If no alphabet is provided on this input, the internal alphabet will be used.", null, false, false, DisplayLevel.Expert,QuickWatchFormat.Text, null)]
        public string InputAlphabet
        {
            get { return ((VigenereSettings)this.settings).AlphabetSymbols; }
            set
            {
                if (value != null && value != settings.AlphabetSymbols) 
                { 
                    ((VigenereSettings)this.settings).AlphabetSymbols = value;
                    OnPropertyChanged("InputAlphabet");
                }
            }
        }
        [PropertyInfo(Direction.InputData, "String", "Keyword as derived by the VigenereAnalyser", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string ShiftValue
        {
            get { return settings.ShiftChar; }
            set
            {
                if (value != settings.ShiftChar)
                {
                    settings.ShiftChar=value;
                    OnPropertyChanged("ShiftValue");

                }
            }
        }

        /// <summary>
        /// Vigenere encryption
        /// </summary>
        public void Encrypt()
        {
            ProcessVigenere(VigenereMode.encrypt);
        }

        /// <summary>
        /// Vigenere decryption
        /// </summary>
        public void Decrypt()
        {
            ProcessVigenere(VigenereMode.decrypt);
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
        /// Fire if progress bar status was changed
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        /// <summary>
        /// Fire if a new message has to be shown in the status bar
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

        /// <summary>
        /// Does the actual Vigenere processing, i.e. encryption or decryption
        /// </summary>
        /// <param name="mode"></param>
        private void ProcessVigenere(VigenereMode mode)
        {
            VigenereSettings cfg = (VigenereSettings)this.settings;
            StringBuilder output = new StringBuilder(String.Empty);
            string alphabet = cfg.AlphabetSymbols;

            if (!cfg.CaseSensitiveAlphabet)
            {
                alphabet = cfg.AlphabetSymbols.ToUpper();
            }
            if (inputString != null)
            {
                int shiftPos = 0;
                for (int i = 0; i < inputString.Length; i++)
                {
                    //get plaintext char which is currently processed
                    char currentChar = inputString[i];

                    //remember if it is upper case (ohterwise lowercase is assumed)
                    bool uppercase = char.IsUpper(currentChar);
                    
                    //get the position of the plaintext character in the alphabet
                    int ppos = 0;
                    if (cfg.CaseSensitiveAlphabet)
                    {
                        ppos = alphabet.IndexOf(currentChar);
                    }
                    else
                    {
                        ppos = alphabet.IndexOf(char.ToUpper(currentChar));
                    }

                    if (ppos >= 0)
                    {
                        //found the plaintext character in the alphabet, begin shifting
                        int cpos = 0;
                        switch (mode)
                        {
                            case VigenereMode.encrypt:
                                cpos = (ppos + cfg.ShiftKey[shiftPos]) % alphabet.Length;
                                break;
                            case VigenereMode.decrypt:
                                cpos = (ppos - cfg.ShiftKey[shiftPos] + alphabet.Length) % alphabet.Length;
                                break;
                        }

                        //inkrement shiftPos to map inputString whith all keys
                        //if shiftPos > ShiftKey.Length, begin again at the beginning
                        shiftPos++;
                        if (shiftPos >= cfg.ShiftKey.Length)
                            shiftPos = 0;

                        //we have the position of the ciphertext character, now we have to output it in the right case
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
                        //the plaintext character was not found in the alphabet, begin handling with unknown characters
                        switch ((VigenereSettings.UnknownSymbolHandlingMode)cfg.UnknownSymbolHandling)
                        {
                            case VigenereSettings.UnknownSymbolHandlingMode.Ignore:
                                output.Append(inputString[i]);
                                break;
                            case VigenereSettings.UnknownSymbolHandlingMode.Replace:
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
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="logeLevel"></param>
        private void Vigenere_LogMessage(string msg, NotificationLevel logLevel)
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
