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

namespace Cryptool.Playfair
{
    public class PlayfairSettings : ISettings
    {
        #region Public Playfair specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Playfair plugin
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void PlayfairLogMessage(string msg, NotificationLevel logLevel);

        /// <summary>
        /// Fire, if a new message has to be shown in the status bar
        /// </summary>
        public event PlayfairLogMessage LogMessage;

        [PropertySaveOrder(1)]
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region Private variables

        private bool hasChanges;
        private bool seperatePairs = true;
        private int selectedAction = 0;
        private bool preFormatText = true;
        private bool ignoreDuplicates = true;
        private int matrixSize = 0; //0=5x5, 1=6x6
        private string smallAlphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        private string largeAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private string alphabetMatrix = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        private string alphPool;
        private string key;
        private char separator = 'X';
        private char separatorReplacement = 'Y';

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(2)]
        [ContextMenu("Action","Select the Algorithm action",1,DisplayLevel.Beginner,ContextMenuControlType.ComboBox, new int[] {1,2}, "Encrypt","Decrypt")]
        [TaskPane("Action","Select the Algorithm action",null,1,false,DisplayLevel.Beginner,ControlType.ComboBox, new string[] {"Encrypt","Decrypt"})]
        public int Action
        {
            get { return this.selectedAction; }
            set
            {
                if (value != selectedAction) HasChanges = true;
                this.selectedAction = value;
                OnPropertyChanged("Action");
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane("Key value (multiple letters)","Enter one or multiple key values",null,2,false,DisplayLevel.Beginner,ControlType.TextBox,null)]
        public string Key
        {
            get 
            {
              if (key != null)
                return key.ToUpper();
              else
                return null;
            }
            set
            {
                if (value != null)
                {
                    if (value.ToUpper() != key) HasChanges = true;
                    this.key = value.ToUpper();
                    setKeyMatrix();
                    OnPropertyChanged("Key");
                    OnPropertyChanged("AlphabetMatrix");
                }
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane("Alphabet", "This is the used alphabet cipher to encrypt/decrypt.", null, 3, false, DisplayLevel.Expert, ControlType.TextBox, "")]
        public string AlphabetMatrix
        {
            get { return this.alphabetMatrix; }
            set
            {
                if (value != this.alphabetMatrix) HasChanges = true;
                this.alphabetMatrix = value;
                OnPropertyChanged("AlphabetMatrix");
            }
        }

        [PropertySaveOrder(5)]
        [ContextMenu("Pre-Format-Text","This is used to determine whether the text should be confinded to the alphabet used.",4,DisplayLevel.Beginner,ContextMenuControlType.CheckBox,null,"Pre-format text")]
        [TaskPane("Pre-Format-Text","This is used to determine whether the text should be confinded to the alphabet used.",null,4,false,DisplayLevel.Expert,ControlType.CheckBox,"")]
        public bool PreFormatText
        {
            get { return this.preFormatText; }
            set
            {
                if (value != this.preFormatText) HasChanges = true;
                this.preFormatText = value;
                OnPropertyChanged("PreFormatText");
            }
        }

        [PropertySaveOrder(6)]
        [ContextMenu("Ignore duplicates","Ignore duplicates within the key phrase",5,DisplayLevel.Beginner,ContextMenuControlType.CheckBox,null,"Ignore duplicates within the key phrase")]
        [TaskPane("Ignore duplicates","Ignore duplicates within the key phrase",null,5,false,DisplayLevel.Expert,ControlType.CheckBox,"")]
        public bool IgnoreDuplicates
        {
            get { return this.ignoreDuplicates; }
            set
            {
                if (value != this.ignoreDuplicates) HasChanges = true;
                this.ignoreDuplicates = value;
                OnPropertyChanged("IgnoreDuplicates");
                setKeyMatrix();
            }
        }

        [PropertySaveOrder(7)]
        [ContextMenu("Matrix size","Select whether the Playfair should be run with a 5x5 or 6x6 matrix",6,DisplayLevel.Expert,ContextMenuControlType.ComboBox,null,new string[]{"5 x 5","6 x 6"})]
        [TaskPane("Matrix size", "Select whether the Playfair should be run with a 5x5 or 6x6 matrix", null, 6,false, DisplayLevel.Expert, ControlType.ComboBox, "5 x 5","6 x 6")]
        public int MatrixSize
        {
            get { return this.matrixSize; }
            set 
            {
                if (value != this.matrixSize) HasChanges = true;
                this.matrixSize = value;
                setKeyMatrix();
                OnPropertyChanged("MatrixSize");
            }
        }

        [PropertySaveOrder(8)]
        [ContextMenu("Separate pairs", "Seperate pairs of identical letters", 7, DisplayLevel.Expert, ContextMenuControlType.CheckBox, null, "Separate pairs of identical letters")]
        [TaskPane("Separate pairs", "Separate pairs of identical letters", null, 7, false, DisplayLevel.Expert, ControlType.CheckBox, "")]
        public bool SeperatePairs
        {
            get { return this.seperatePairs; }
            set
            {
                if (value != this.seperatePairs) HasChanges = true;
                this.seperatePairs = value;
                OnPropertyChanged("SeperatePairs");
            }
        }

        [PropertySaveOrder(9)]        
        [TaskPane("Separator","Enter the character to separate pairs of identical letters",null,8,false,DisplayLevel.Expert, ControlType.TextBox,"")]
        public char Separator
        {
            get { return char.ToUpper(this.separator); }
            set 
            {
                if (char.ToUpper(value) != this.separator) HasChanges = true;
                this.separator = char.ToUpper(value);
                setSeparatorReplacement();
                OnPropertyChanged("Separator");
                OnPropertyChanged("SeparatorReplacement");
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("Separator replacement", "Enter the character to separate double separators.\nE.g. a double XX will be separate by insertion of Y", null, 9, false, DisplayLevel.Expert, ControlType.TextBox, "")]
        public char SeparatorReplacement
        {
            get { return char.ToUpper(this.separatorReplacement);}
            set
            {
                if (char.ToUpper(value) != this.separatorReplacement) HasChanges = true;
                this.separatorReplacement = char.ToUpper(value);
                setSeparator();
                OnPropertyChanged("Separator");
                OnPropertyChanged("SeparatorReplacement");
            }
        }




        #endregion

        #region Private Members

        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if (value[i] == value[j])
                    {
                        value = value.Remove(j, 1);
                        j--;
                        length--;
                    }
                }
            }
            return value;
        }

        private string getKey()
        {
            string tempKey = string.Empty;

            for (int i = 0; i < key.Length; i++)
            {
                char cPos = key[i];
                if (tempKey.Contains(cPos))
                {
                    cPos = getNextChar(cPos);
                }
                tempKey += cPos.ToString();
            }
            return tempKey;
        }

        private char getNextChar(char value)
        {
            for (int i = 0; i < alphPool.Length; i++)
            {
                if (alphPool.IndexOf(alphPool[i]) > alphPool.IndexOf(value) && !key.Contains(alphPool[i]))
                {
                    value = alphPool[i];
                    alphPool = alphPool.Remove(i, 1);
                    break;
                }
            }
            return value;
        }

        private void setKeyMatrix()
        {
            string tempKey = key;

            string tempAlph = string.Empty;

            switch (matrixSize)
            {
                case 0:
                    tempAlph = smallAlphabet;
                    break;
                case 1:
                    tempAlph = largeAlphabet;
                    break;
                default:
                    break;
            }

            if (!ignoreDuplicates)
            {
                alphPool = tempAlph;
                tempKey = getKey();
            }

            AlphabetMatrix = removeEqualChars(tempKey + tempAlph);
        }

        private void setSeparator()
        {
            if (this.separator == this.separatorReplacement)
            {
                int separatorReplacementPos = AlphabetMatrix.IndexOf(this.separatorReplacement);
                int separatorPos = (separatorReplacementPos - 1 + AlphabetMatrix.Length) % AlphabetMatrix.Length;
                this.separator = AlphabetMatrix[separatorPos];
            }
        }

        private void setSeparatorReplacement()
        {
            if (this.separator == this.separatorReplacement)
            {
                int separatorPos = AlphabetMatrix.IndexOf(this.separator);
                int separatorReplacementPos = (separatorPos + 1) % AlphabetMatrix.Length;
                this.separatorReplacement = AlphabetMatrix[separatorReplacementPos];
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

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
