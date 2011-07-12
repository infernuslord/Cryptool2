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
using Cryptool.PluginBase.IO;
using System.Collections;
using System.IO;
using System.ComponentModel;
using System.Windows.Controls;

namespace Cryptool.Substitution
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Substitution.Properties.Resources", false,"PluginCaption", "PluginTooltip", "PluginDescriptionURL", 
      "Substitution/Images/icon.png", "Substitution/Images/encrypt.png", "Substitution/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Substitution : ICrypComponent
    {
        #region Private variables

        private SubstitutionSettings settings;
        private string inputString;
        private string outputString;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public Substitution()
        {
            this.settings = new SubstitutionSettings(this);
            ((SubstitutionSettings)(this.settings)).LogMessage += Substitution_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (SubstitutionSettings)value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", null, true, false, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", null, true, false, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", null, true, false,QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputAlphabetCaption", "InputAlphabetTooltip",  null, false, false, QuickWatchFormat.Text, null)]
        public string InputAlphabet
        {
            get { return ((SubstitutionSettings)this.settings).AlphabetSymbols; }
            set
            {
                if (value != null && value != settings.AlphabetSymbols)
                {
                    ((SubstitutionSettings)this.settings).AlphabetSymbols = value;
                    OnPropertyChanged("InputAlphabet");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "KeyValueCaption", "KeyValueTooltip", null, false, false, QuickWatchFormat.Text, null)]
        public string KeyValue
        {
            get { return settings.KeyValue; }
            set
            {
                if (value != settings.KeyValue)
                {
                    settings.KeyValue = value;
                }
            }
        }

        /// <summary>
        /// Substitution encryption
        /// </summary>
        public void Encrypt()
        {
            if(inputString != null)
            {
                SubstitutionSettings cfg = (SubstitutionSettings)this.settings;
                StringBuilder output = new StringBuilder(string.Empty);

                string alphabet = cfg.AlphabetSymbols;

                //in case we don't want consider case in the alphabet, we use only capital letters, hence transform
                //the whole alphabet to uppercase
                if(!cfg.CaseSensitiveAlphabet)
                {
                    alphabet = cfg.AlphabetSymbols.ToUpper();
                }

                for (int i = 0; i < inputString.Length; i++)
                {
                    //get plaintext char which is currently processed
                    char currentchar = inputString[i];

                    //remember if it is upper (otherwise lowercase is assumed)
                    bool uppercase = char.IsUpper(currentchar);

                    //get the position of the plaintext in the alphabet
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
                        //we found the plaintext character in the alphabet
                        if (cfg.CaseSensitiveAlphabet)
                        {
                            output.Append(cfg.CipherAlphabet[ppos]);
                        }
                        else
                        {
                            if (uppercase)
                            {
                                output.Append(char.ToUpper(cfg.CipherAlphabet[ppos]));
                            }
                            else
                            {
                                output.Append(char.ToLower(cfg.CipherAlphabet[ppos]));
                            }
                        }
                    }
                    else
                    {
                        //the plaintext character was not found in the alphabet, hence proceed whith handling unknown characters
                        switch ((SubstitutionSettings.UnknownSymbolHandlingMode)cfg.UnknownSymbolHandling)
                        {
                            case SubstitutionSettings.UnknownSymbolHandlingMode.Ignore:
                                output.Append(inputString[i]);
                                break;
                            case SubstitutionSettings.UnknownSymbolHandlingMode.Remove:
                                break;
                            case SubstitutionSettings.UnknownSymbolHandlingMode.Replace:
                                output.Append('?');
                                break;
                            default:
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
        /// Substitution decryption
        /// </summary>
        public void Decrypt()
        {
            if (inputString != null)
            {
                SubstitutionSettings cfg = (SubstitutionSettings)this.settings;
                StringBuilder output = new StringBuilder(string.Empty);

                string alphabet = cfg.AlphabetSymbols;

                //in case we do not want consider case in the alphabet, we use only capital letter, hence transform
                //the whole alphabet to uppercase
                if (!cfg.CaseSensitiveAlphabet)
                {
                    alphabet = cfg.AlphabetSymbols.ToUpper();
                }

                for (int i = 0; i < inputString.Length; i++)
                {
                    //get plaintext char which is currently processed
                    char currentchar = inputString[i];

                    //remember if it is upper case (otherwise lowercase is assumed)
                    bool uppercase = char.IsUpper(currentchar);

                    //get the position of the cipher text character in the alphabet
                    int ppos = 0;
                    if (cfg.CaseSensitiveAlphabet)
                    {
                        ppos = cfg.CipherAlphabet.IndexOf(currentchar);
                    }
                    else
                    {
                        ppos = cfg.CipherAlphabet.IndexOf(char.ToUpper(currentchar));
                    }

                    if (ppos >= 0)
                    {
                        //we found the cipher text character in the alphabet
                        if (cfg.CaseSensitiveAlphabet)
                        {
                            output.Append(alphabet[ppos]);
                        }
                        else
                        {
                            //find the right plain text char and append it to the output
                            if (uppercase)
                            {
                                output.Append(char.ToUpper(alphabet[ppos]));
                            }
                            else
                            {
                                output.Append(char.ToLower(alphabet[ppos]));
                            }
                        }
                    }
                    else
                    {
                        //the ciphertext character was not found in the alphabet, hence proceed with handling unknown characters
                        switch ((SubstitutionSettings.UnknownSymbolHandlingMode)cfg.UnknownSymbolHandling)
                        {
                            case SubstitutionSettings.UnknownSymbolHandlingMode.Ignore:
                                output.Append(inputString[i]);
                                break;
                            case SubstitutionSettings.UnknownSymbolHandlingMode.Remove:
                                break;
                            case SubstitutionSettings.UnknownSymbolHandlingMode.Replace:
                                output.Append('?');
                                break;
                            default:
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

        #endregion

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, loglevel));
        }

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
        /// Fire if progress bar status was changed
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        /// <summary>
        /// Fire if a new message has to be shown in the status bar
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

        private void Substitution_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public void Execute()
        {
            foreach (char c in settings.KeyValue)
            {
                if (!settings.AlphabetSymbols.Contains(c))
                {
                    GuiLogMessage("Key contains characters that are not part of the alphabet!", NotificationLevel.Error);
                    return;
                }
            }

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
        ///// <summary>
        ///// Substitution encryption
        ///// </summary>
        ///// <param name="inputData">The input data to encrypt</param>
        ///// <param name="key">Key which the encryption uses</param>
        ///// <param name="alphabet">Alphabet which the encryption uses</param>
        ///// <returns>The encrypted data as an int array</returns>
        //public Stream Encrypt(IEncryptionAlgorithmSettings settings)
        //{
        //    AlphabetConverter alphConv = new AlphabetConverter();
        //    int[] inputData = alphConv.StreamToIntArray(((SubstitutionSettings)settings).InputData);
        //    int[] outputData = new int[inputData.Length];
        //    int[] alphCipher = getAlphCipher(removeDuplicateChars(((SubstitutionSettings)settings).Key), ((SubstitutionSettings)settings).Alphabet);

        //    for (int i = 0; i < inputData.Length; i++)
        //    {
        //        for (int j = 0; j < ((SubstitutionSettings)settings).Alphabet.Length; j++)
        //        {
        //            if (((SubstitutionSettings)settings).Alphabet[j] == inputData[i])
        //            {
        //                outputData[i] = alphCipher[j];
        //                break;
        //            }
        //        }
        //    }
        //    return alphConv.intArrayToStream(outputData);
        //}

        ///// <summary>
        ///// Substitution decryption
        ///// </summary>
        ///// <param name="inputData">The input data to decrypt</param>
        ///// <param name="key">Key which the encryption uses</param>
        ///// <param name="alphabet">Alphabet which the encryption uses</param>
        ///// <returns>The decrypted data as an int array</returns>
        //public Stream Decrypt(IEncryptionAlgorithmSettings settings)
        //{
        //    AlphabetConverter alphConv = new AlphabetConverter();
        //    int[] inputData = alphConv.StreamToIntArray(((SubstitutionSettings)settings).InputData);
        //    int[] outputData = new int[inputData.Length];
        //    int[] alphCipher = getAlphCipher(removeDuplicateChars(((SubstitutionSettings)settings).Key), ((SubstitutionSettings)settings).Alphabet);

        //    for (int i = 0; i < inputData.Length; i++)
        //    {
        //        for (int j = 0; j < ((SubstitutionSettings)settings).Alphabet.Length; j++)
        //        {
        //            if (alphCipher[j] == inputData[i])
        //            {
        //                outputData[i] = ((SubstitutionSettings)settings).Alphabet[j];
        //                break;
        //            }
        //        }
        //    }
        //    return alphConv.intArrayToStream(outputData);
        //}

        ///// <summary>
        ///// Build alphabet cipher
        ///// </summary>
        ///// <param name="key">used key</param>
        ///// <param name="alphabet">The plain alphabet</param>
        ///// <returns>Alphabet for en-/decryption as an int array</returns>
        //private int[] getAlphCipher(int[] key, int[] alphabet)
        //{
        //    int count = 0;
        //    bool found;
        //    int[] alphCipher = new int[alphabet.Length];

        //    for (int i = 0; i < key.Length; i++)
        //    {
        //        alphCipher[i] = key[i];
        //    }

        //    for (int i = 0; i < alphabet.Length; i++)
        //    {
        //        found = false;
        //        for (int j = 0; j < key.Length && !found; j++)
        //        {
        //            if (alphabet[i] == key[j])
        //            {
        //                found = true;
        //                count++;
        //            }
        //        }
        //        if (!found)
        //        {
        //            alphCipher[i + key.Length - count] = alphabet[i];
        //        }
        //    }

        //    return alphCipher;
        //}

        ///// <summary>
        ///// Remove duplicate characters from the key array
        ///// </summary>
        ///// <param name="key">The key array</param>
        ///// <returns>The key array without duplicate characters</returns>
        //private int[] removeDuplicateChars(int[] key)
        //{
        //    bool found;
        //    int[] newKey;
        //    ArrayList alKey = new ArrayList();
            
        //    for (int i = 0; i < key.Length; i++)
        //    {
        //        found = false;
        //        for (int j = 0; j < alKey.Count; j++)
        //        {
        //            if (key[i] == (int)alKey[j])
        //            {
        //                found = true;
        //                break;
        //            }
        //        }
        //        if (!found)
        //        {
        //            alKey.Add(key[i]);
        //        }
        //    }

        //    newKey = new int[alKey.Count];
        //    for (int i = 0; i < alKey.Count; i++)
        //    {
        //        newKey[i] = (int)alKey[i];
        //    }

        //    return newKey;
        //}

    }
}
