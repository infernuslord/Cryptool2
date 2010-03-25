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

        [ContextMenu("Uppercase/Lowercase", "Use uppercase, lowercase letters", 0, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "Uppercase", "Lowercase", "Both" })]
        [TaskPane("Uppercase/Lowercase", "Use uppercase, lowercase letters", null, 2, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Uppercase", "Lowercase", "Both" })]
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

        [ContextMenu("Space", "Use space", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "Yes", "No" })]
        [TaskPane("Space", "Use space", null, 3, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Yes", "No" })]
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

        [ContextMenu("Numerals", "Use numerals", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "Yes", "No" })]
        [TaskPane("Numerals", "Use numerals", null, 4, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Yes", "No" })]
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

        [ContextMenu("Punctuation", "Use punctations", 3, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "Yes", "No" })]
        [TaskPane("Punctuation", "Use punctations", null, 5, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Yes", "No" })]
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
        [TaskPane("Filename", "Select a file with an alphabet.", null, 1, false, DisplayLevel.Beginner, ControlType.OpenFileDialog, FileExtension = "Cryptool Alphabet (*.cta)|*.cta")]
        public string OpenFilename
        {
          get { return openFilename; }
          set
          {
            if (value != openFilename)
            {
              openFilename = value;
              HasChanges = true;
              OnPropertyChanged("OpenFilename");
            }
          }
        }

        public string TargetFilenameSuggestion { get; set; }
        private string targetFilename;
        [TaskPane("Target FileName", "File to write alphabet into.", null, 1, false, DisplayLevel.Beginner, ControlType.SaveFileDialog, FileExtension = "Cryptool Alphabet (*.cta)|*.cta")]
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

        private bool hasChanges;
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
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
