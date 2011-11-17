/*
   Copyright 2010 CrypTool 2 Team

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
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;

namespace Cryptool.Plugins.PlayfairAnalysisStatistic
{    
    [Author("Christoph Hartmann", "chris-ha@freenet.de", "Johannes Gutenberg-Universität Mainz", "http://www.uni-mainz.de")]
    [PluginInfo("PlayfairAnalysisStatistic.Properties.Resources", true, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "CrypWin/images/default.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class PlayfairAnalysisStatistic : ICrypComponent
    {
        #region Private Variables
                
        private PlayfairAnalysisStatisticSettings settings = new PlayfairAnalysisStatisticSettings();        
        private byte[] unformattedTextByte;
        private string unformattedText;
        private string formattedText;
        private string sortedAlphabet;
        private double[,] logStat;
        private byte[] logStatByte;

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        
        /*
        [PropertyInfo(Direction.OutputData, "BigraphStatisticCaption", "BigraphStatisticTooltip", null)]
        public virtual double[] BigraphStatistic
        {
            get { return this.logStat2; }
            set
            {
                if (value != this.logStat2)
                {
                    this.logStat2 = value;
                    OnPropertyChanged("BigraphStatistic");
                }
            }
        }
        */

        ICryptoolStream csBigraphStatistic;
        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip")]
        public ICryptoolStream OutputStream
        {
            get
            {
                return csBigraphStatistic;
            }

            set
            {
                if (value != this.csBigraphStatistic)
                {
                    this.csBigraphStatistic = value;                    
                    OnPropertyChanged("OutputStream");
                }
            }
        }


        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        /// <summary>
        /// HOWTO: Enter the algorithm you'd like to implement in this method.
        /// </summary>
        public void Execute()
        {
            int matrixSize;
            
            switch (settings.MatrixSize)
            {
                case 0:
                    matrixSize = 5;                                      
                    break;
                case 1:
                    matrixSize = 6;                    
                    break;
                default:
                    matrixSize = 5;                    
                    break;
            }

            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            // HOWTO: After you have changed an output property, make sure you announce the name of the changed property to the CT2 core.
            
            //OnPropertyChanged("Difference");

            // HOWTO: You can pass error, warning, info or debug messages to the CT2 main window.

            GuiLogMessage("Text Corpus File: " + settings.TextCorpusFile, NotificationLevel.Info);


            if ((settings.Alphabet.Contains(settings.Separator)) && (settings.Alphabet.Contains(settings.SeparatorReplacement))
                && (settings.Alphabet.Contains(settings.ReplacementChar)))
            {

                if (!ContainsDuplicates(settings.Alphabet))
                {

                    if (settings.TextCorpusFile != null && settings.TextCorpusFile.EndsWith(".txt"))
                    {
                        if (((matrixSize == 5) && (settings.Alphabet.Length == 25))
                            || ((matrixSize == 6) && (settings.Alphabet.Length == 36)))
                        {
                            unformattedTextByte = System.IO.File.ReadAllBytes(settings.TextCorpusFile);
                            unformattedText = Encoding.Default.GetString(unformattedTextByte);

                            GuiLogMessage(settings.Alphabet, NotificationLevel.Info);

                            SortAlphabet(settings.Alphabet);
                            GuiLogMessage("sortedAlphabet: " + sortedAlphabet, NotificationLevel.Info);
                            GuiLogMessage("ToUpper: " + Convert.ToString(settings.CorpusToUpper), NotificationLevel.Info);

                            FormatText();

                            GuiLogMessage("formatted text: " + formattedText, NotificationLevel.Info);

                            CalcLogStat();

                            
                            // Write Alphabet Length, sortedAlphabet and logStat (double array) in logStatByte (byte array)
                            logStatByte = new byte[(8*logStat.Length) + 1 + sortedAlphabet.Length];
                            byte[] doubleValue = new byte[8];
                            int index = 0;

                            logStatByte[index] = (byte)sortedAlphabet.Length;
                            index++;

                            for (int i = 0; i < sortedAlphabet.Length; i++)
                            {
                                logStatByte[index] = Convert.ToByte(sortedAlphabet[i]);
                                index++;
                            }                            

                            for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
                            {
                                for (int j = 0; j < (int)Math.Pow(matrixSize, 2); j++)
                                {
                                    doubleValue = BitConverter.GetBytes(logStat[i, j]);
                                    for (int k = 0; k < 8; k++)
                                    {
                                        logStatByte[index] = doubleValue[k];
                                        index++;
                                    }
                                }
                            }


                            csBigraphStatistic = new CStreamWriter(logStatByte);
                            OnPropertyChanged("OutputStream");


                            GuiLogMessage("CalcLogStat completed: ", NotificationLevel.Info);                           

                        }

                        else
                        {
                            System.Windows.MessageBox.Show("Wrong Alphabet Length!\nAlphabet must contain " + Convert.ToString(Math.Pow(matrixSize, 2)) + " characters.");
                        }

                    }
                    else if (settings.TextCorpusFile == null)
                    {
                        System.Windows.MessageBox.Show("For calculating the bigraph statistic a text corpus file is needed!");
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Text Corpus File has to be a .txt file!");
                        settings.TextCorpusFile = null;
                    }

                }
                else
                {
                    System.Windows.MessageBox.Show("Alphabet contains duplicate characters!");
                }

            }

            else
            {
                System.Windows.MessageBox.Show("Alphabet must contain Separator, Separator Replacement and Replacement Character!");
            }
                        
            ProgressChanged(1, 1);            
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
            settings.MatrixSize = 0;
            settings.Alphabet = settings.SmallAlphabet;
            settings.RemoveChar = 'J';
            settings.ReplacementChar = 'I';
            settings.Separator = 'X';
            settings.SeparatorReplacement = 'Y';
            settings.CorpusToUpper = true;
            settings.ConvertSpecialSigns = true;
            settings.ReplaceCharacter = true;
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

        void SortAlphabet(string alphabet)
        {
            char[] alphabetArray;
            sortedAlphabet = "";
            
            alphabetArray = alphabet.ToCharArray();
            Array.Sort(alphabetArray);
            
            for (int i = 0; i < alphabet.Length; i++)
            {
                sortedAlphabet += alphabetArray[i];
            }                        
        }

        bool ContainsDuplicates(string alphabet)
        {
            bool returnVal = false;

            for (int i = 0; i < alphabet.Length - 1; i++)
            {
                for (int j = i + 1; j < alphabet.Length; j++)
                {
                    if (alphabet[i] == alphabet[j])
                        returnVal = true;
                }
            }
            return returnVal;
        }

        void FormatText()
        {            
            int length = unformattedText.Length;
            StringBuilder FormattedTextSB = new StringBuilder(unformattedText.Length);
            
            if (settings.CorpusToUpper)
            {
                formattedText = unformattedText.ToUpper();                
            }
            else
            {
                formattedText = unformattedText;
            }

            if (settings.ConvertSpecialSigns)
            {
                for (int i = 0; i < formattedText.Length; i++)
                {
                    if (formattedText[i] == 'Ä')
                        FormattedTextSB.Append("AE");
                    else if (formattedText[i] == 'Ö')
                        FormattedTextSB.Append("OE");
                    else if (formattedText[i] == 'Ü')
                        FormattedTextSB.Append("UE");
                    else if (formattedText[i] == 'ß')
                        if (settings.CorpusToUpper)
                            FormattedTextSB.Append("SS");
                        else
                            FormattedTextSB.Append("ss");
                    else if (formattedText[i] == 'ä')
                        FormattedTextSB.Append("ae");
                    else if (formattedText[i] == 'ö')
                        FormattedTextSB.Append("oe");
                    else if (formattedText[i] == 'ü')
                        FormattedTextSB.Append("ue");
                    else
                        FormattedTextSB.Append(formattedText[i]);                    
                }                
            }
            else
            {
                FormattedTextSB.Append(formattedText);
            }

            ProgressChanged(0.1, 1);

            if (settings.ReplaceCharacter)
            {            
                FormattedTextSB.Replace(settings.RemoveChar, settings.ReplacementChar);
            }

            formattedText = FormattedTextSB.ToString();
            FormattedTextSB = new StringBuilder(formattedText.Length);

            ProgressChanged(0.2, 1);
            
            for (int i = 0; i < formattedText.Length; i++)
            {
                if (sortedAlphabet.Contains(formattedText[i]))
                    FormattedTextSB.Append(formattedText[i]);
            }

            ProgressChanged(0.3, 1);
            

            for (int i = 0; i < FormattedTextSB.Length - 1; i += 2)
            {
                if (FormattedTextSB[i] == FormattedTextSB[i + 1])
                {
                    if (FormattedTextSB[i] == settings.Separator)
                        FormattedTextSB.Insert(i + 1, settings.SeparatorReplacement);
                    else
                        FormattedTextSB.Insert(i + 1, settings.Separator);                    
                }

                if ((i == (FormattedTextSB.Length / 2)) || (i == (FormattedTextSB.Length / 2) + 1) )
                    ProgressChanged(0.45, 1);
            }                                  

            ProgressChanged(0.6, 1);

            if ((FormattedTextSB.Length % 2) == 1)
            {
                if (FormattedTextSB[FormattedTextSB.Length - 1] == settings.Separator)
                    FormattedTextSB.Append(settings.SeparatorReplacement);
                else
                    FormattedTextSB.Append(settings.Separator);
            }

            formattedText = FormattedTextSB.ToString();
        }


        public void CalcLogStat()
        {
            int Pos1, Pos2;
            int sum = 0;
            int matrixSize;

            switch (settings.MatrixSize)
            {
                case 0:
                    matrixSize = 5;
                    break;

                case 1:
                    matrixSize = 6;                    
                    break;

                default:
                    matrixSize = 5;
                    break;
            }

            logStat = new double[(int)Math.Pow(matrixSize, 2), (int)Math.Pow(matrixSize, 2)];

            for (int i = 0; i < formattedText.Length - 1; i += 2)
            {
                Pos1 = sortedAlphabet.IndexOf(formattedText[i]);
                Pos2 = sortedAlphabet.IndexOf(formattedText[i + 1]);

                logStat[Pos1, Pos2]++;
                sum++;
            }                   

            ProgressChanged(0.8, 1);

            for (int i = 0; i < (int)Math.Pow(matrixSize, 2); i++)
            {
                for (int j = 0; j < (int)Math.Pow(matrixSize, 2); j++)
                {
                    if (logStat[i, j] > 0)
                    {
                        logStat[i, j] = Math.Log(logStat[i, j] / sum);
                    }
                    else
                        logStat[i, j] = -10;
                }
            }

        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }


        #endregion
    }
}
