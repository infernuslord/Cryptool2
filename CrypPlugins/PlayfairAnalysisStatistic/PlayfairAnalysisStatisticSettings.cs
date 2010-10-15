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
        private bool hasChanges = false;
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

        #region Constructor

        public PlayfairAnalysisStatisticSettings()
        {

        }

        #endregion

        #region TaskPane Settings

        [PropertySaveOrder(0)]
        [TaskPane("Text Corpus", "txt file used to create the bigraph statistic", null, 1, false, DisplayLevel.Beginner, ControlType.OpenFileDialog)]
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
                    hasChanges = true;
                }
            }
        }

        [PropertySaveOrder(2)]
        [TaskPane("Matrix Size", "Size of Matrix used for fomatting", null, 2, false, DisplayLevel.Beginner, ControlType.RadioButton, new string[] { "5 x 5", "6 x 6" })]
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
                    hasChanges = true;
                }
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane("Alphabet", "Alphabet used for formatting", null, 3, false, DisplayLevel.Expert, ControlType.TextBox, "")]
        public string Alphabet
        {
            get { return this.alphabet; }
            set
            {
                if (value != this.alphabet) HasChanges = true;
                this.alphabet = value;
                OnPropertyChanged("Alphabet");
            }
        }

        [PropertySaveOrder(6)]
        [TaskPane("Replace Character", "Replace a character by another", null, 4, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
        public bool ReplaceCharacter
        {
            get { return this.replaceCharacter; }
            set
            {
                if (value != this.replaceCharacter) HasChanges = true;
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
        [TaskPane("Replace", "Character to be replaced", null, 5, false, DisplayLevel.Expert, ControlType.TextBox, "")]
        public char RemoveChar
        {
            get { return this.removeChar; }
            set
            {
                if (value != this.removeChar) HasChanges = true;
                this.removeChar = value;
                OnPropertyChanged("RemoveChar");
            }
        }


        [PropertySaveOrder(5)]
        [TaskPane("by", "Replacement character", null, 6, false, DisplayLevel.Expert, ControlType.TextBox, "")]
        public char ReplacementChar
        {
            get { return this.replacementChar; }
            set
            {
                if (value != this.replacementChar) HasChanges = true;
                this.replacementChar = value;
                OnPropertyChanged("ReplacementChar");
            }
        }

        [PropertySaveOrder(7)]
        [TaskPane("Separator", "Enter the character to separate pairs of identical letters", null, 7, false, DisplayLevel.Expert, ControlType.TextBox, "")]
        public char Separator
        {
            get { return this.separator; }
            set
            {
                if (value != this.separator) HasChanges = true;
                this.separator = value;
                OnPropertyChanged("Separator");
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane("Separator Replacement", "Enter the character to separate double separators", null, 8, false, DisplayLevel.Expert, ControlType.TextBox, "")]
        public char SeparatorReplacement
        {
            get { return this.separatorReplacement; }
            set
            {
                if (value != this.separatorReplacement) HasChanges = true;
                this.separatorReplacement = value;
                OnPropertyChanged("SeparatorReplacement");
            }
        }

        [PropertySaveOrder(9)]
        [TaskPane("Text Corpus to upper", "Convert text corpus to upper case", null, 9, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
        public bool CorpusToUpper
        {
            get { return this.corpusToUpper; }
            set
            {
                if (value != this.corpusToUpper) HasChanges = true;
                this.corpusToUpper = value;
                OnPropertyChanged("CorpusToUpper");
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("Convert mutated vowels and sharp s", "Convert mutated vowels and sharp s", null, 10, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
        public bool ConvertSpecialSigns
        {
            get { return this.convertSpecialSigns; }
            set
            {
                if (value != this.convertSpecialSigns) HasChanges = true;
                this.convertSpecialSigns = value;
                OnPropertyChanged("ConvertSpecialSigns");
            }
        }


        #endregion

        #region public variables

        public string SmallAlphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        public string LargeAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        #endregion



        #region ISettings Members

        /// <summary>
        /// HOWTO: This flags indicates whether some setting has been changed since the last save.
        /// If a property was changed, this becomes true, hence CrypTool will ask automatically if you want to save your changes.
        /// </summary>
        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

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
