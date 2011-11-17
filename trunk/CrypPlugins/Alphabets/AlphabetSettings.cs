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
using System.ComponentModel;

namespace Cryptool.Alphabets
{
    public class AlphabetSettings : ISettings
    {
        public enum UpperLowerCase {UpperCase, LowerCase, Both};
        public enum YesNo {Yes, No};
        
        private string selectedAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string defaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private UpperLowerCase upperLowerCaseLetter = UpperLowerCase.UpperCase;
        private YesNo spaceSensitivity = YesNo.No;
        private YesNo numerals = YesNo.No;
        private YesNo punctuation = YesNo.No;

        public string DefaultAlphabet
        {
            get { return this.defaultAlphabet; }
            set
            {
                this.defaultAlphabet = value;
                Alphabet = value; //set new default alphabet
                OnPropertyChanged("DefaultAlphabet");
            }
        }
        
        public string Alphabet
        {
            get { return this.selectedAlphabet; }
            set 
            { 
                this.selectedAlphabet = value;
                OnPropertyChanged("Alphabet");
            }
        }

        [ContextMenu("UpperLowerCaseLettersCaption", "UpperLowerCaseLettersTooltip", 0, ContextMenuControlType.ComboBox, null, new string[] { "UpperLowerCaseLettersList1", "UpperLowerCaseLettersList2", "UpperLowerCaseLettersList3" })]
        [TaskPane("UpperLowerCaseLettersCaption", "UpperLowerCaseLettersTooltip", null, 2, false, ControlType.ComboBox, new string[] { "UpperLowerCaseLettersList1", "UpperLowerCaseLettersList2", "UpperLowerCaseLettersList3" })]
        public int UpperLowerCaseLetters
        {
            get { return (int)this.upperLowerCaseLetter; }
            set
            {
                this.upperLowerCaseLetter = (UpperLowerCase)value;
                setCaseSensitivity();
                OnPropertyChanged("UpperLowerCaseLetters");
            }
        }

        [ContextMenu("SpaceSensitivityCaption", "SpaceSensitivityTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "SpaceSensitivityList1", "SpaceSensitivityList2" })]
        [TaskPane("SpaceSensitivityCaption", "SpaceSensitivityTooltip", null, 3, false, ControlType.ComboBox, new string[] { "SpaceSensitivityList1", "SpaceSensitivityList2" })]
        public int SpaceSensitivity
        {
            get { return (int)this.spaceSensitivity; }
            set
            {
                this.spaceSensitivity = (YesNo)value;
                setSpaceSensitivity();
                OnPropertyChanged("SpaceSensitivity");
            }
        }

        [ContextMenu("NumeralsCaption", "NumeralsTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "NumeralsList1", "NumeralsList2" })]
        [TaskPane("NumeralsCaption", "NumeralsTooltip", null, 4, false, ControlType.ComboBox, new string[] { "NumeralsList1", "NumeralsList2" })]
        public int Numerals
        {
            get { return (int)this.numerals; }
            set
            {
                this.numerals = (YesNo)value;
                setNumerals();
                OnPropertyChanged("Numerals");
            }
        }

        [ContextMenu("PunctuationCaption", "PunctuationTooltip", 3, ContextMenuControlType.ComboBox, null, new string[] { "PunctuationList1", "PunctuationList2" })]
        [TaskPane("PunctuationCaption", "PunctuationTooltip", null, 5, false, ControlType.ComboBox, new string[] { "PunctuationList1", "PunctuationList2" })]
        public int Punctuation
        {
            get { return (int)this.punctuation; }
            set
            {
                this.punctuation = (YesNo)value;
                setPunctuation();
                OnPropertyChanged("Punctuation");
            }
        }

        private string openFilename;
        [TaskPane("OpenFilenameCaption", "OpenFilenameTooltip", null, 1, false, ControlType.OpenFileDialog, FileExtension = "Cryptool Alphabet (*.cta)|*.cta")]
        public string OpenFilename
        {
          get { return openFilename; }
          set
          {
            if (value != openFilename)
            {
              openFilename = value;
                OnPropertyChanged("OpenFilename");
            }
          }
        }

        public string TargetFilenameSuggestion { get; set; }
        private string targetFilename;
        [TaskPane("TargetFilenameCaption", "TargetFilenameTooltip", null, 1, false, ControlType.SaveFileDialog, FileExtension = "Cryptool Alphabet (*.cta)|*.cta")]
        public string TargetFilename
        {
          get { return targetFilename; }
          set
          {
            targetFilename = value;
            OnPropertyChanged("TargetFilename");
          }
        }

        private void setCaseSensitivity()
        {
            Alphabet = Alphabet.Trim(defaultAlphabet.ToLower().ToCharArray());
            Alphabet = Alphabet.Trim(defaultAlphabet.ToUpper().ToCharArray());
            switch (upperLowerCaseLetter)
            {
                case UpperLowerCase.UpperCase:
                    Alphabet = Alphabet + defaultAlphabet.ToUpper();
                    break;
                case UpperLowerCase.LowerCase:
                    Alphabet = Alphabet + defaultAlphabet.ToLower();
                    break;
                case UpperLowerCase.Both:
                    Alphabet = Alphabet + DefaultAlphabet.ToUpper() + DefaultAlphabet.ToLower();
                    break;
                default:
                    break;
            }
        }

        private void setSpaceSensitivity()
        {
            switch (spaceSensitivity)
            {
                case YesNo.Yes:
                    Alphabet = Alphabet + " ";
                    break;
                case YesNo.No:
                    Alphabet = Alphabet.Trim();
                    break;
                default:
                    break;
            }
        }

        private void setNumerals()
        {
            switch (numerals)
            {
                case YesNo.Yes:
                    Alphabet = Alphabet + "0123456789";
                    break;
                case YesNo.No:
                    char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                    Alphabet = Alphabet.Trim(digits);
                    break;
                default:
                    break;
            }
        }

        private void setPunctuation()
        {
            switch (punctuation)
            {
                case YesNo.Yes:
                    Alphabet = Alphabet + ".,:;!?()";
                    break;
                case YesNo.No:
                    char[] punct = { '.', ',', ':', ';', '!', '?', '(', ')' };
                    Alphabet = Alphabet.Trim(punct);
                    break;
                default:
                    break;
            }

        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
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
