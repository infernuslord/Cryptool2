/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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
using System.Windows;
using System.Windows.Controls;

namespace Cryptool.Plugins.PlayfairAnalysisStatistic
{
    public class PlayfairAnalysisStatisticSettings : ISettings
    {
        #region Private Variables

        private string textCorpusFile;
        private int matrixSize = 0;
        private string alphabet;
        private char removeChar;
        private char replacementChar;
        private char separator;
        private char separatorReplacement;
        private bool corpusToUpper;
        private bool convertSpecialSigns;
        private bool replaceCharacter;
                        
        #endregion

        #region TaskPane Settings

        [PropertySaveOrder(0)]
        [TaskPane( "TextCorpusFileCaption", "TextCorpusFileTooltip", null, 1, false, ControlType.OpenFileDialog)]
        public string TextCorpusFile
        {
            get
            {
                return textCorpusFile;
            }
            set
            {                
                if (textCorpusFile != value)
                {
                    textCorpusFile = value;
                    OnPropertyChanged("TextCorpusFile");
                }
            }
        }

        [PropertySaveOrder(2)]
        [TaskPane( "MatrixSizeCaption", "MatrixSizeTooltip", null, 2, false, ControlType.RadioButton, new string[] { "5 x 5", "6 x 6" })]
        public int MatrixSize
        {
            get
            {
                return matrixSize;
            }
            set
            {
                if (matrixSize != value)
                {
                    if (value == 0)
                    {
                        Alphabet = SmallAlphabet;                        
                    }
                    else
                    {
                        Alphabet = LargeAlphabet;                       
                    }
                                        
                    matrixSize = value;
                    OnPropertyChanged("MatrixSize");
                }
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane( "AlphabetCaption", "AlphabetTooltip", null, 3, false, ControlType.TextBox, "")]
        public string Alphabet
        {
            get { return this.alphabet; }
            set
            {
                if (value != this.alphabet)
                {
                    this.alphabet = value;
                    OnPropertyChanged("Alphabet");
                }
            }
        }

        [PropertySaveOrder(6)]
        [TaskPane( "ReplaceCharacterCaption", "ReplaceCharacterTooltip", null, 4, false, ControlType.CheckBox, "")]
        public bool ReplaceCharacter
        {
            get { return this.replaceCharacter; }
            set
            {
                if (value == this.replaceCharacter)
                    return;

                this.replaceCharacter = value;
                OnPropertyChanged("ReplaceCharacter");
                if (value)
                {
                    if (TaskPaneAttributeChanged != null)
                    {
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RemoveChar", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ReplacementChar", Visibility.Visible)));
                    }
                }
                else
                {
                    if (TaskPaneAttributeChanged != null)
                    {
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RemoveChar", Visibility.Collapsed)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ReplacementChar", Visibility.Collapsed)));
                    }
                }
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane( "RemoveCharCaption", "RemoveCharTooltip", null, 5, false, ControlType.TextBox, "")]
        public char RemoveChar
        {
            get { return this.removeChar; }
            set
            {
                if (value != this.removeChar)
                {
                    this.removeChar = value;
                    OnPropertyChanged("RemoveChar");   
                }
            }
        }


        [PropertySaveOrder(5)]
        [TaskPane( "ReplacementCharCaption", "ReplacementCharTooltip", null, 6, false, ControlType.TextBox, "")]
        public char ReplacementChar
        {
            get { return this.replacementChar; }
            set
            {
                if (value != this.replacementChar)
                {
                    this.replacementChar = value;
                    OnPropertyChanged("ReplacementChar");   
                }
            }
        }

        [PropertySaveOrder(7)]
        [TaskPane( "SeparatorCaption", "SeparatorTooltip", null, 7, false, ControlType.TextBox, "")]
        public char Separator
        {
            get { return this.separator; }
            set
            {
                if (value != this.separator)
                {
                    this.separator = value;
                    OnPropertyChanged("Separator");                    
                }
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane( "SeparatorReplacementCaption", "SeparatorReplacementTooltip", null, 8, false, ControlType.TextBox, "")]
        public char SeparatorReplacement
        {
            get { return this.separatorReplacement; }
            set
            {
                if (value != this.separatorReplacement)
                {
                    this.separatorReplacement = value;
                    OnPropertyChanged("SeparatorReplacement");   
                }
            }
        }

        [PropertySaveOrder(9)]
        [TaskPane( "CorpusToUpperCaption", "CorpusToUpperTooltip", null, 9, false, ControlType.CheckBox, "")]
        public bool CorpusToUpper
        {
            get { return this.corpusToUpper; }
            set
            {
                if (value != this.corpusToUpper)
                {
                    this.corpusToUpper = value;
                    OnPropertyChanged("CorpusToUpper");
                }
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane( "ConvertSpecialSignsCaption", "ConvertSpecialSignsTooltip", null, 10, false, ControlType.CheckBox, "")]
        public bool ConvertSpecialSigns
        {
            get { return this.convertSpecialSigns; }
            set
            {
                if (value != this.convertSpecialSigns)
                {
                    this.convertSpecialSigns = value;
                    OnPropertyChanged("ConvertSpecialSigns");
                }
            }
        }


        #endregion

        #region public variables

        public string SmallAlphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        public string LargeAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
                
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
