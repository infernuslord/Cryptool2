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
using System.IO;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.Vigenere
{
    public class VigenereSettings : ISettings
    {
        #region Public Vigenere specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Vigenere plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void VigenereLogMessage(string msg, NotificationLevel logLevel);

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>
        public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };

        /// <summary>
        /// Fire if a new text has to be shown in the status bar
        /// </summary>
        public event VigenereLogMessage LogMessage;

        /// <summary>
        /// Retrieves the current shift values of Vigenere (i.e. the key), or sets it
        /// </summary>
        [PropertySaveOrder(0)]
        public int[] ShiftKey
        {
            get { return shiftValue; }
            set { setKeyByValue(value); }
        }

        /// <summary>
        /// Retrieves the current setting whether the alphabet should be treated as case sensitive or not
        /// </summary>
        [PropertySaveOrder(1)]
        public bool CaseSensitiveAlphabet 
        {
            get
            {
                if (caseSensitiveAlphabet == 0) return false;
                else return true;
            }
            set { } //readonly
        }

        /// <summary>
        /// Returns true if some settings have been changed. This value should be set externally to false. e.g
        /// when a project was saved.
        /// </summary>
        [PropertySaveOrder(2)]
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region Private variables
        private bool hasChanges;
        private int selectedAction = 0;
        private int selectedModus = 0;
        private string upperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string lowerAlphabet = "abcdefghijklmnopqrstuvwxyz";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private char[] shiftChar = { 'B', 'C', 'D' };
        private int[] shiftValue = { 1, 2, 3 };
        private UnknownSymbolHandlingMode unknowSymbolHandling = UnknownSymbolHandlingMode.Ignore;
        private int caseSensitiveAlphabet = 0; //0=case insensitive, 1 = case sensitive
        #endregion

        #region Private methods
        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if((value[i]) == value[j] || (!CaseSensitiveAlphabet & (char.ToUpper(value[i]) == char.ToUpper(value[j]))))
                    {
                        LogMessage("Removing duplicate letter: \'" + value[j] + "\' from alphabet!", NotificationLevel.Warning);
                        value = value.Remove(j,1);
                        j--;
                        length--;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Parse the offset string and set the shiftValue/ShiftChar accordingly
        /// </summary>
        /// <param name="offsetString"></param>
        private void setKeyByValue(string offsetString)
        {
            try
            {
                string[] offsetStr = offsetString.Split(',');
                int[] offset = new int[offsetStr.Length];
                for (int i = 0; i < offsetStr.Length; i++)
                {
                    offset[i] = int.Parse(offsetStr[i]);
                }
                setKeyByValue(offset);
            }
            catch (Exception e)
            {
                LogMessage("Bad input \"" + offsetString + "\"! (" + e.Message + ") Reverting to " + shiftValue.ToString() + "!", NotificationLevel.Error);
                OnPropertyChanged("ShiftValue");
            }
        }

        /// <summary>
        /// Set the new shiftValue and the new shiftCharacter to offset % alphabet.Length
        /// </summary>
        /// <param name="offset"></param>
        public void setKeyByValue(int[] offset)
        {
            hasChanges = true;

            //making sure the shift value lies within the alphabet range
            for (int i = 0; i < offset.Length; i++)
                offset[i] = offset[i] % alphabet.Length;

            //set the new shiftChar
            shiftChar = new char[offset.Length];
            for (int i = 0; i < offset.Length; i++)
                shiftChar[i] = alphabet[offset[i]];

            //set the new shiftValue
            shiftValue = new int[offset.Length];
            for (int i = 0; i < offset.Length; i++)
                shiftValue[i] = offset[i];

            //Anounnce this to the settings pane
            OnPropertyChanged("ShiftValue");
            OnPropertyChanged("ShiftChar");

            //print some info in the log.
            LogMessage("Accepted new shift value " + offset.ToString() + "! (Adjusted shift character to '" + shiftChar.ToString() + "\'", NotificationLevel.Info);
        }

        private void setKeyByCharacter(string value)
        {
            try
            {
                int[] offset = new int[value.Length];
                if (this.CaseSensitiveAlphabet)
                {
                    for (int i = 0; i < value.Length; i++)
                        offset[i] = alphabet.IndexOf(value[i]);
                }
                else
                {
                    for (int i = 0; i < value.Length; i++)
                        offset[i] = alphabet.ToUpper().IndexOf(char.ToUpper(value[i]));
                }
                for (int i = 0; i < offset.Length; i++)
                {
                    if (offset[i] >= 0)
                    {
                        hasChanges = true;
                        shiftValue = new int[offset.Length];
                        for (int j = 0; j < offset.Length; j++)
                            shiftValue[j] = offset[j];
                        shiftChar = new char[shiftValue.Length];
                        for (int j = 0; j < shiftValue.Length; j++)
                            shiftChar[j] = alphabet[shiftValue[j]];
                        LogMessage("Accepted new shift character \'" + shiftChar.ToString() + "\'! (Adjusted shift value to " + shiftValue.ToString() + ")", NotificationLevel.Info);
                        OnPropertyChanged("ShiftValue");
                        OnPropertyChanged("ShiftChar");
                        break;
                    }
                    else
                    {
                        LogMessage("Bad input \"" + value + "\"! (Character not in alphabet!) Reverting to " + shiftChar.ToString() + "!", NotificationLevel.Error);
                    }
                }
            }
            catch (Exception e)
            {
                LogMessage("Bad input \"" + value + "\"! (" + e.Message + ") Reverting to " + shiftChar.ToString() + "!", NotificationLevel.Error);
            }
        }
        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(3)]
        [ContextMenu("ModusCaption", "ModusTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Autokey", "Classic")]
        [TaskPane("ModusCaption", "ModusTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Autokey", "Classic" })]
        public int Modus
        {
            get { return this.selectedModus; }
            set
            {
                if (value != selectedModus) HasChanges = true;
                this.selectedModus = value;
                OnPropertyChanged("Modus");
            }
        }


        [PropertySaveOrder(4)]
        [ContextMenu("ActionCaption", "ActionTooltip", 2, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Encrypt", "Decrypt")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 2, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
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

        [PropertySaveOrder(5)]
        [TaskPane("ShiftValueTPCaption", "ShiftValueTPTooltip", null, 3, false, ControlType.TextBox, null)]
        public string ShiftValue
        {
            get
            {
                StringBuilder str = new StringBuilder(string.Empty);
                for (int i = 0; i < this.shiftValue.Length; i++)
                {
                    str.Append(this.shiftValue[i].ToString());
                    if (i != this.shiftValue.Length - 1)
                        str.Append(",");
                }
                return str.ToString();
            }
            set { setKeyByValue(value); }
        }

        [PropertySaveOrder(6)]
        [TaskPane("ShiftCharCaption", "ShiftCharTooltip", null, 4, false, ControlType.TextBox, null)]
        public string ShiftChar
        {
            get { return new String(this.shiftChar); }
            set { setKeyByCharacter(value); }
        }

        [PropertySaveOrder(7)]
        [ContextMenu("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", 5, ContextMenuControlType.ComboBox, null, "Ignore (leave unmodified)", "Remove", "Replace with \'?\'")]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 5, false, ControlType.ComboBox, new string[] { "Ignore (leave unmodified)", "Remove", "Replace with \'?\'" })]
        public int UnknownSymbolHandling
        {
            get { return (int)this.unknowSymbolHandling; }
            set
            {
                if ((UnknownSymbolHandlingMode)value != unknowSymbolHandling) hasChanges = true;
                this.unknowSymbolHandling = (UnknownSymbolHandlingMode)value;
                OnPropertyChanged("UnknownSymbolHandling");
            }
        }

        [PropertySaveOrder(8)]
        [ContextMenu("AlphabetCaseCaption", "AlphabetCaseTooltip", 8, ContextMenuControlType.ComboBox, null, "Case insensitive", "Case sensitive")]
        [TaskPane("AlphabetCaseCaption", "AlphabetCaseTooltip", null, 8, false, ControlType.ComboBox, new string[] { "Case insensitive", "Case sensitive" })]
        public int AlphabetCase
        {
            get { return this.caseSensitiveAlphabet; }
            set
            {
                if (value != caseSensitiveAlphabet) hasChanges = true;
                this.caseSensitiveAlphabet = value;
                if (value == 0)
                {
                    if (alphabet == (upperAlphabet + lowerAlphabet))
                    {
                        alphabet = upperAlphabet;
                        LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + "Symbols)",NotificationLevel.Info);
                        OnPropertyChanged("AlphabetSymbols");
                        //re-set also the key (shifvalue/shiftchar to be in the range of the new alphabet
                        setKeyByValue(shiftValue);
                    }
                }
                else
                {
                    if(alphabet == upperAlphabet)
                    {
                        alphabet = upperAlphabet + lowerAlphabet;
                        LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                        OnPropertyChanged("AlphabetSymbols");
                    }
                }

                //remove equal characters from the current alphabet
                string a = alphabet;
                alphabet = removeEqualChars(alphabet);

                if(a != alphabet)
                {
                    OnPropertyChanged("AlphabetSymbols");
                    LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)",NotificationLevel.Info);
                }
                OnPropertyChanged("CaseSensitiveAlphabet");
            }
        }

        [PropertySaveOrder(9)]
        [TaskPane("AlphabetSymbolsCaption", "AlphabetSymbolsTooltip", null, 7, false, ControlType.TextBox, null)]
        public string AlphabetSymbols
        {
            get { return this.alphabet; }
            set
            {
                string a = removeEqualChars(value);
                if (a.Length == 0) //cannot accept empty alphabets
                {
                    LogMessage("Ignoring empty alphabet from user! Using previous alphabet: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                }
                else if (!alphabet.Equals(a))
                {
                    hasChanges = true;
                    this.alphabet = a;
                    setKeyByValue(shiftValue); //re-evaluate if the shiftvalue is stillt within the range
                    LogMessage("Accepted new alphabet from user: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                    OnPropertyChanged("AlphabetSymbols");
                }
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
