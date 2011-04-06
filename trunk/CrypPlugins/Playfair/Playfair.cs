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
using System.Collections;
using System.IO;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Playfair
{
    [Author("Sebastian Przybylski","sebastian@przybylski.org","Uni Siegen","http://www.uni-siegen.de")]
    [PluginInfo("Playfair.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", 
      "Playfair/Images/icon.png", "Playfair/Images/encrypt.png", "Playfair/Images/decrypt.png")]
    [EncryptionType(EncryptionType.Classic)]
    public class Playfair : IEncryption
    {
        #region Private variables

        private PlayfairSettings settings;
        private string inputString;
        private string outputString;
        private string preFormatedInputString;
        private int matrixSize;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public Playfair()
        {
            this.settings = new PlayfairSettings();
            ((PlayfairSettings)(this.settings)).LogMessage += Playfair_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.settings; }
            set { this.settings = (PlayfairSettings)value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputDataCaption", "OutputDataTooltip", "",false,false,QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", "",true,false,QuickWatchFormat.Text, null)]
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

        [PropertyInfo(Direction.OutputData, "PreFormatedInputStringCaption", "PreFormatedInputStringTooltip", null,false,false,QuickWatchFormat.Text,null)]
        public string PreFormatedInputString
        {
            get { return this.preFormatedInputString; }
            set
            {
                preFormatedInputString = value;
                OnPropertyChanged("PreFormatedInputString");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", "",false,false,QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }


        /// <summary>
        /// Playfair encryption
        /// </summary>
        public void Encrypt()
        {
            if (inputString != null)
            {
                StringBuilder output = new StringBuilder(string.Empty);
                //set selected matrix size
                if (settings.MatrixSize == 0) matrixSize = 5;
                else matrixSize = 6;

                //pre-format input text, if user activated this property
                if (settings.PreFormatText)
                    preFormatedInputString = preFormatText();
                else
                    preFormatedInputString = inputString;
                OnPropertyChanged("PreFormatedInputString");

                //begin the encryption
                for (int i = 0; i < preFormatedInputString.Length - 1; i++)
                {
                    int indexCh1 = settings.AlphabetMatrix.IndexOf(preFormatedInputString[i]);
                    i++;
                    int indexCh2 = settings.AlphabetMatrix.IndexOf(preFormatedInputString[i]);

                    //first, get new char index from cipher alphabet
                    int newIndexCh1;
                    int newIndexCh2;

                    int rowCh1 = indexCh1 / matrixSize;
                    int rowCh2 = indexCh2 / matrixSize;
                    int colCh1 = indexCh1 % matrixSize;
                    int colCh2 = indexCh2 % matrixSize;

                    if (rowCh1 == rowCh2)
                    {
                        newIndexCh1 = getRightNeighbour(indexCh1);
                        newIndexCh2 = getRightNeighbour(indexCh2);
                    }
                    else if (colCh1 == colCh2)
                    {
                        newIndexCh1 = getLowerNeighbour(indexCh1);
                        newIndexCh2 = getLowerNeighbour(indexCh2);
                    }
                    else
                    {
                        newIndexCh1 = getSubstitute(rowCh1, colCh2);
                        newIndexCh2 = getSubstitute(rowCh2, colCh1);
                    }
                    output.Append(settings.AlphabetMatrix[newIndexCh1]);
                    output.Append(settings.AlphabetMatrix[newIndexCh2]);
                }
                outputString = output.ToString();
                OnPropertyChanged("OutputString");
                OnPropertyChanged("OutputData");

            }
        }

        /// <summary>
        /// Playfair decryption
        /// </summary>
        public void Decrypt()
        {
            if (inputString != null)
            {
                StringBuilder output = new StringBuilder(string.Empty);
                //set selected matrix size
                if (settings.MatrixSize == 0) matrixSize = 5;
                else matrixSize = 6;

                // Decryption does not require preformat, otherwise the ciphertext format is wrong
                // We attempt to preformat nevertheless.

                //pre-format input text, if user activated this property
                if (settings.PreFormatText)
                    preFormatedInputString = preFormatText();
                else
                    preFormatedInputString = inputString;
                OnPropertyChanged("PreFormatedInputString");

                //begin the encryption
                for (int i = 0; i < preFormatedInputString.Length - 1; i++)
                {
                    int indexCh1 = settings.AlphabetMatrix.IndexOf(preFormatedInputString[i]);
                    i++;
                    int indexCh2 = settings.AlphabetMatrix.IndexOf(preFormatedInputString[i]);

                    //first, get new char index from cipher alphabet
                    int newIndexCh1;
                    int newIndexCh2;

                    int rowCh1 = indexCh1 / matrixSize;
                    int rowCh2 = indexCh2 / matrixSize;
                    int colCh1 = indexCh1 % matrixSize;
                    int colCh2 = indexCh2 % matrixSize;

                    if (rowCh1 == rowCh2)
                    {
                        newIndexCh1 = getLeftNeighbour(indexCh1);
                        newIndexCh2 = getLeftNeighbour(indexCh2);
                    }
                    else if (colCh1 == colCh2)
                    {
                        newIndexCh1 = getUpperNeighbour(indexCh1);
                        newIndexCh2 = getUpperNeighbour(indexCh2);
                    }
                    else
                    {
                        newIndexCh1 = getSubstitute(rowCh1, colCh2);
                        newIndexCh2 = getSubstitute(rowCh2, colCh1);
                    }
                    output.Append(settings.AlphabetMatrix[newIndexCh1]);
                    output.Append(settings.AlphabetMatrix[newIndexCh2]);
                }
                outputString = output.ToString();
                OnPropertyChanged("OutputString");
                OnPropertyChanged("OutputData");

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
        /// Fire, if status has to be shown in the progress bar
        /// </summary>
#pragma warning disable 67
				public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        

        /// <summary>
        /// Fire, if a message has to be shonw in the status bar
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

        public void PreExecution()
        {
            Dispose();
        }

        public void PostExecution()
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

        private int getRightNeighbour(int index)
        {
            if (index % matrixSize < matrixSize - 1) index++;
            else if (index % matrixSize == matrixSize - 1) index = index - matrixSize + 1;
            else index = -1;

            return index;
        }

        private int getLowerNeighbour(int index)
        {
            if (index + matrixSize < settings.AlphabetMatrix.Length) index = index + matrixSize;
            else index = (index + matrixSize) % settings.AlphabetMatrix.Length;

            return index;
        }

        private int getUpperNeighbour(int index)
        {
            if (index < matrixSize) index = settings.AlphabetMatrix.Length - (matrixSize - index);
            else index = index - matrixSize;

            return index;
        }

        private int getLeftNeighbour(int index)
        {
            if (index % matrixSize > 0) index--;
            else index = index + matrixSize - 1;

            return index;
        }

        private int getSubstitute(int row, int col)
        {
            return matrixSize * row + col;
        }

        private string preFormatText()
        {
            StringBuilder sb = new StringBuilder();

            //remove or replace nonalphabet char
            for (int i = 0; i < inputString.Length; i++)
            {
                char c = char.ToUpper(inputString[i]);

                if (settings.AlphabetMatrix.Contains(c))
                {
                    sb.Append(c);
                }
                else
                {
                    if (c == 'J') sb.Append("I");
                    if (c == 'Ä') sb.Append("AE");
                    if (c == 'Ö') sb.Append("OE");
                    if (c == 'Ü') sb.Append("UE");
                    if (c == 'ß') sb.Append("SS");
                }
            }

            //if separate char is enabled begin with separating
            if (settings.SeperatePairs)
            {
                for (int i = 0; i <= sb.Length-2; i+=2)
                {
                    if (sb[i] == sb[i + 1]) // same chars, insert X
                    {
                        if (sb[i] == settings.Separator) // avoid XX, use XY instead
                            sb.Insert(i+1, settings.SeparatorReplacement);
                        else
                            sb.Insert(i+1, settings.Separator);
                    }
                }
            }

            // does the input end with a single letter?
            if (sb.Length % 2 != 0)
            {
                if (sb[sb.Length-1] == settings.Separator) // avoid XX, use XY instead
                    sb.Append(settings.SeparatorReplacement);
                else
                    sb.Append(settings.Separator);
            }

            return sb.ToString();
        }

        private void Playfair_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

        public void Execute()
        {
            ProgressChanged(0, 1);

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

            ProgressChanged(1, 1);
        }

        public void Pause()
        {
        }

        #endregion

        #region Event Handling

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
