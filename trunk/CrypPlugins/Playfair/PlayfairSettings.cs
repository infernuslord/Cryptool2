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

        #endregion

        #region Private variables

        private bool separatePairs = true;
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
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get { return this.selectedAction; }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    OnPropertyChanged("Action");                    
                }
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane( "KeyCaption", "KeyTooltip",null,2,false,ControlType.TextBox,null)]
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
                if (value != null && value.ToUpper() != key)
                {
                    this.key = value.ToUpper();
                    setKeyMatrix();
                    OnPropertyChanged("Key");
                    OnPropertyChanged("AlphabetMatrix");
                }
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane( "AlphabetMatrixCaption", "AlphabetMatrixTooltip", null, 3, false, ControlType.TextBox, "")]
        public string AlphabetMatrix
        {
            get { return this.alphabetMatrix; }
            set
            {
                if (value != this.alphabetMatrix)
                {
                    this.alphabetMatrix = value;
                    OnPropertyChanged("AlphabetMatrix");                    
                }
            }
        }

        [PropertySaveOrder(5)]
        [ContextMenu("PreFormatTextCaption", "PreFormatTextTooltip", 4, ContextMenuControlType.CheckBox, null, "PreFormatTextList1")]
        [TaskPane( "PreFormatTextCaption", "PreFormatTextTooltip",null,4,false,ControlType.CheckBox,"")]
        public bool PreFormatText
        {
            get { return this.preFormatText; }
            set
            {
                if (value != this.preFormatText)
                {
                    this.preFormatText = value;
                    OnPropertyChanged("PreFormatText");                    
                }
            }
        }

        [PropertySaveOrder(6)]
        [ContextMenu("IgnoreDuplicatesCaption", "IgnoreDuplicatesTooltip", 5, ContextMenuControlType.CheckBox, null, "IgnoreDuplicatesList1")]
        [TaskPane( "IgnoreDuplicatesCaption", "IgnoreDuplicatesTooltip",null,5,false,ControlType.CheckBox,"")]
        public bool IgnoreDuplicates
        {
            get { return this.ignoreDuplicates; }
            set
            {
                if (value != this.ignoreDuplicates)
                {
                    this.ignoreDuplicates = value;
                    OnPropertyChanged("IgnoreDuplicates");
                    setKeyMatrix();                    
                }
            }
        }

        [PropertySaveOrder(7)]
        [ContextMenu( "MatrixSizeCaption", "MatrixSizeTooltip",6,ContextMenuControlType.ComboBox,null,new string[]{"MatrixSizeList1", "MatrixSizeList2"})]
        [TaskPane( "MatrixSizeCaption", "MatrixSizeTooltip", null, 6,false, ControlType.ComboBox, "MatrixSizeList1", "MatrixSizeList2")]
        public int MatrixSize
        {
            get { return this.matrixSize; }
            set 
            {
                if (value != this.matrixSize)
                {
                    this.matrixSize = value;
                    setKeyMatrix();
                    OnPropertyChanged("MatrixSize");                    
                }
            }
        }

        [PropertySaveOrder(8)]
        [ContextMenu("SeparatePairsCaption", "SeparatePairsTooltip", 7, ContextMenuControlType.CheckBox, null, "SeparatePairsList1")]
        [TaskPane( "SeparatePairsTPCaption", "SeparatePairsTPTooltip", null, 7, false, ControlType.CheckBox, "")]
        public bool SeparatePairs
        {
            get { return this.separatePairs; }
            set
            {
                if (value != this.separatePairs)
                {
                    this.separatePairs = value;
                    OnPropertyChanged("SeparatePairs");                    
                }
            }
        }

        [PropertySaveOrder(9)]        
        [TaskPane( "SeparatorCaption", "SeparatorTooltip",null,8,false, ControlType.TextBox,"")]
        public char Separator
        {
            get { return char.ToUpper(this.separator); }
            set 
            {
                if (char.ToUpper(value) != this.separator)
                {
                    this.separator = char.ToUpper(value);
                    setSeparatorReplacement();
                    OnPropertyChanged("Separator");
                    OnPropertyChanged("SeparatorReplacement");                    
                }
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane( "SeparatorReplacementCaption", "SeparatorReplacementTooltip", null, 9, false, ControlType.TextBox, "")]
        public char SeparatorReplacement
        {
            get { return char.ToUpper(this.separatorReplacement);}
            set
            {
                if (char.ToUpper(value) != this.separatorReplacement)
                {
                    this.separatorReplacement = char.ToUpper(value);
                    setSeparator();
                    OnPropertyChanged("Separator");
                    OnPropertyChanged("SeparatorReplacement");                    
                }
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
