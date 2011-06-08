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
using Cryptool.PluginBase.Miscellaneous;
using System.Collections;
using System.IO;
using System.ComponentModel;

namespace Cryptool.ADFGVX
{
    [Author("Sebastian Przybylski","sebastian@przybylski.org","Uni-Siegen","http://www.uni-siegen.de")]
    [PluginInfo("Cryptool.ADFGVX.Properties.Resources", false,
        "PluginCaption", "PluginTooltip", "ADFGVX/DetailedDescription/doc.xml", "ADFGVX/Images/icon.png", "ADFGVX/Images/encrypt.png", "ADFGVX/Images/decrypt.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class ADFGVX : IEncryption
    {
        #region Private variables

        private ADFGVXSettings settings;
        private string inputString;
        private string outputString;
        
        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public ADFGVX()
        {
            this.settings = new ADFGVXSettings();
            ((ADFGVXSettings)(this.settings)).LogMessage += ADFGVX_LogMessage;
        }

        /// <summary>
        /// Get or set settings for the algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (ADFGVXSettings)value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public ICryptoolStream OutputData
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

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", "", true, false, QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.InputData, "InputAlphabetCaption", "InputAlphabetTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public string InputAlphabet
        {
            get { return ((ADFGVXSettings)this.settings).SubstitutionMatrix; }
            set
            {
                if (value != null && value != settings.SubstitutionMatrix)
                {
                    if (value.Length > settings.SubstitutionMatrix.Length)
                    {
                        value = value.Remove(settings.SubstitutionMatrix.Length);
                        ADFGVX_LogMessage("Input alphabet too long! Reduce alphabet to " + settings.SubstitutionMatrix.Length.ToString() + " characters!", NotificationLevel.Info);
                        ((ADFGVXSettings)this.settings).SubstitutionMatrix = value;
                    }
                    else if (value.Length < settings.SubstitutionMatrix.Length)
                    {
                        ADFGVX_LogMessage("Input alphabet too short! The alphabet must have at least " + settings.SubstitutionMatrix.Length.ToString() + " characters!", NotificationLevel.Info);
                        ((ADFGVXSettings)this.settings).SubstitutionMatrix = value;
                    }
                    else
                    {
                        ((ADFGVXSettings)this.settings).SubstitutionMatrix = value;
                    }
                    OnPropertyChanged("InputAlphabet");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "TranspositionPasswordCaption", "TranspositionPasswordTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public string TranspositionPassword
        {
            get { return settings.TranspositionPass; }
            set
            {
                if (value != settings.TranspositionPass)
                {
                    settings.TranspositionPass = value;
                }
            }
        }

        #endregion

        /// <summary>
        /// ADFGVX encryptor
        /// Attribute for action needed!
        /// </summary>
        public void Encrypt()
        {
            if (inputString != null)
            {
                string strInput;

                //Change input string to upper or lower case
                if (!settings.CaseSensitiveAlphabet)
                    strInput = InputString.ToUpper();
                else
                    strInput = InputString.ToLower();


                //remove or replace non alphabet char
                switch ((ADFGVXSettings.UnknownSymbolHandlingMode)settings.UnknownSymbolHandling)
                {
                    case ADFGVXSettings.UnknownSymbolHandlingMode.Remove:
                        strInput = removeNonAlphChar(strInput);
                        break;
                    case ADFGVXSettings.UnknownSymbolHandlingMode.Replace:
                        strInput = replaceNonAlphChar(strInput);
                        break;
                    default:
                        break;
                }



                string alphCipher = settings.SubstitutionMatrix;
                StringBuilder strOutput = new StringBuilder(string.Empty);

                //1. Step - Substitution
                for (int i = 0; i < strInput.Length; i++)
                {
                    for (int j = 0; j < alphCipher.Length; j++)
                    {
                        if (alphCipher[j] == strInput[i])
                        {
                            int line = j / 6;
                            int column = j % 6;
                            String pair = string.Empty;
                            switch (line)
                            {
                                case 0:
                                    pair = "A";
                                    break;
                                case 1:
                                    pair = "D";
                                    break;
                                case 2:
                                    pair = "F";
                                    break;
                                case 3:
                                    pair = "G";
                                    break;
                                case 4:
                                    pair = "V";
                                    break;
                                case 5:
                                    pair = "X";
                                    break;
                                default:
                                    break;
                            }
                            switch (column)
                            {
                                case 0:
                                    pair += "A";
                                    break;
                                case 1:
                                    pair += "D";
                                    break;
                                case 2:
                                    pair += "F";
                                    break;
                                case 3:
                                    pair += "G";
                                    break;
                                case 4:
                                    pair += "V";
                                    break;
                                case 5:
                                    pair += "X";
                                    break;
                                default:
                                    break;
                            }
                            strOutput.Append(pair[0]);
                            strOutput.Append(pair[1]);
                        }
                    }
                    //show the progress
                    if (OnPluginProgressChanged != null)
                    {
                        OnPluginProgressChanged(this, new PluginProgressEventArgs(i, strInput.Length - 1));
                    }
                }
                //2. Step - Transposition

                int[] newOrder = getOrder(settings.CleanTranspositionPass);
                StringBuilder strOutputData = new StringBuilder(string.Empty);

                for (int i = 0; i < newOrder.Length; i++)
                {
                    int tempOrder = newOrder[i];
                    for (int j = tempOrder; j < strOutput.Length; j=j+newOrder.Length)
                    {
                        strOutputData.Append(strOutput[j]);
                    }

                }
                OutputString = strOutputData.ToString();
                OnPropertyChanged("OutputString");
                OnPropertyChanged("OutputData");
            }
        }

        /// <summary>
        /// ADFGVX decryptor
        /// Attribute for action needed!
        /// </summary>
        public void Decrypt()
        {
            if (inputString != null)
            {
                string strInput;

                //Change input string to upper or lower case
                if (!settings.CaseSensitiveAlphabet)
                    strInput = inputString.ToUpper();
                else
                    strInput = inputString.ToLower();

                //check if input string consists only cipher chars
                if (!checkForRightCipherChar(strInput))
                {
                    ADFGVX_LogMessage("The cipher text does not consists only of cipher characters : \'A\',\'D\',\'F\',\'G\',\'V\',\'X\' !", NotificationLevel.Error);
                    return;
                }

                string alphCipher = settings.SubstitutionMatrix;
                StringBuilder strOutput = new StringBuilder(string.Empty);

                //1. Step Transposition
                int[] order = getOrder(settings.CleanTranspositionPass);
                char[] strOutputData = new char[strInput.Length];

                int count = 0;
                for (int i = 0; i < order.Length; i++)
                {
                    int tempOrder = order[i];
                    for (int j = tempOrder; j < strInput.Length;j=j+order.Length)
                    {
                        strOutputData[j] = strInput[count];
                        count++;
                    }
                }

                for (int i = 0; i < strOutputData.Length; i++)
                {
                    char[] pair = new char[2];
                    int line = 0;
                    int column = 0;
                    pair[0] = strOutputData[i];
                    i++;
                    pair[1] = strOutputData[i];

                    switch (pair[0])
                    {
                        case 'A':
                            line = 0;
                            break;
                        case 'D':
                            line = 1;
                            break;
                        case 'F':
                            line = 2;
                            break;
                        case 'G':
                            line = 3;
                            break;
                        case 'V':
                            line = 4;
                            break;
                        case 'X':
                            line = 5;
                            break;
                        default:
                            break;
                    }
                    switch (pair[1])
                    {
                        case 'A':
                            column = 0;
                            break;
                        case 'D':
                            column = 1;
                            break;
                        case 'F':
                            column = 2;
                            break;
                        case  'G':
                            column = 3;
                            break;
                        case 'V':
                            column = 4;
                            break;
                        case 'X':
                            column = 5;
                            break;
                        default:
                            break;
                    }
                    int ch = line * 6 + column;
                    strOutput.Append(alphCipher[ch]);

                    //show the progress
                    if (OnPluginProgressChanged != null)
                    {
                        OnPluginProgressChanged(this, new PluginProgressEventArgs(i, strOutputData.Length - 1));
                    }
                }
                OutputString = strOutput.ToString();
                OnPropertyChanged("OutputString");
                OnPropertyChanged("OutputData");
            }
        }

        /// <summary>
        /// Remove all ADFGVX char and check if any non cipher char exists,
        /// if so, do not decrypt
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool checkForRightCipherChar(string value)
        {
            char[] trimChars = {'A','D','F','G','V','X'};
            value = value.Trim(trimChars);

            if (value.Length > 0) return false;
            else return true;
        }

        /// <summary>
        /// Replace all non alphabet characters
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string replaceNonAlphChar(string value)
        {
            int length = value.Length;
            StringBuilder sb = new StringBuilder(string.Empty);

            for (int i = 0; i < length; i++)
            {
                string curChar = value[i].ToString();

                if (settings.SubstitutionMatrix.Contains(curChar))
                {
                    sb.Append(curChar);
                }
                else
                {
                    if (!settings.CaseSensitiveAlphabet)
                    {
                        if (value[i] == 'Ä')
                            sb.Append("AE");
                        else if (value[i] == 'Ö')
                            sb.Append("OE");
                        else if (value[i] == 'Ü')
                            sb.Append("UE");
                        else if (value[i] == 'ß')
                            sb.Append("SS");
                    }
                    else
                    {
                        if (value[i] == 'ä')
                            sb.Append("ae");
                        else if (value[i] == 'ö')
                            sb.Append("oe");
                        else if (value[i] == 'ü')
                            sb.Append("ue");
                        else if (value[i] == 'ß')
                            sb.Append("ss");
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Remove all non alphabet characters
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string removeNonAlphChar(string value)
        {
            int length = value.Length;
            for (int i = 0; i < length; i++)
            {
                if (!settings.SubstitutionMatrix.Contains(value[i]))
                {
                    value = value.Remove(i, 1);
                    i--;
                    length--;
                }
            }
            return value;
        }

        /// <summary>
        /// Get position of the transposiotion password characters
        /// </summary>
        /// <param name="transPass"></param>
        /// <returns></returns>
        private int[] getOrder(string transPass)
        {
            int[] key = new int[transPass.Length];
            for (int i = 0; i < transPass.Length; i++)
            {
                key[i] = settings.SubstitutionMatrix.IndexOf(transPass[i]);
            }

            int[] m_P = new int[key.Length];
            int[] m_IP = new int[key.Length];

            for (int idx1 = 0; idx1 < key.Length; idx1++)
            {
                m_P[idx1] = 0;
                for (int idx2 = 0; idx2 < key.Length; idx2++)
                {
                    if (idx1 != idx2)
                    {
                        if (key[idx1] > key[idx2])
                        {
                            m_P[idx1]++;
                        }
                        else if ((key[idx1] == key[idx2]) & (idx2 < idx1))
                        {
                            m_P[idx1]++;
                        }
                    }
                }
            }
            for (int idx = 0; idx < key.Length; idx++)
            {
                m_IP[m_P[idx]] = idx;
            }
            return m_IP;
        }

        #region IPlugin Members

        public void Dispose()
        {
            }

        public bool HasChanges
        {
            get { return settings.HasChanges; }
            set { settings.HasChanges = value; }
        }

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

        public void Initialize()
        {
            
        }

#pragma warning disable 67
				public event StatusChangedEventHandler OnPluginStatusChanged;
				public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
				public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

        public void Pause()
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

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        private void ADFGVX_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
        }
    }

   
}
