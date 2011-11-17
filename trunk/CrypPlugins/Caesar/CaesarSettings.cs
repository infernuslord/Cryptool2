/*                              
   Copyright 2009 Arno Wacker, Uni Duisburg-Essen

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
using System.ComponentModel;
using System.Windows;
using Cryptool.PluginBase;
using System.Windows.Controls;

namespace Cryptool.Caesar
{
    public class CaesarSettings : ISettings
    {
        #region Public Caesar specific interface
        
        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Caesar plugin
        /// </summary>
        public delegate void CaesarLogMessage(string msg, NotificationLevel loglevel);

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>
        public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };


        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event CaesarLogMessage LogMessage;

        public delegate void CaesarReExecute();

        public event CaesarReExecute ReExecute;

        /// <summary>
        /// Retrieves the current sihft value of Caesar (i.e. the key), or sets it
        /// </summary>
        [PropertySaveOrder(0)]
        public int ShiftKey
        {
            get { return shiftValue; }
            set
            {
                setKeyByValue(value);
            }
        }

        /// <summary>
        /// Retrieves the current setting whether the alphabet should be treated as case sensitive or not
        /// </summary>
        [PropertySaveOrder(1)]
        public bool CaseSensitiveAlphabet
        {
            get
            {
                if (caseSensitiveAlphabet == 0)
                {   return false;   }
                else
                {   return true;    }
            }
            set {} // readonly, because there are some problems if we omit the set part.
        }

        #endregion

        #region Private variables
        private int selectedAction = 0;
        private string upperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string lowerAlphabet = "abcdefghijklmnopqrstuvwxyz";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private char shiftChar = 'C';
        private int shiftValue = 2; 
        // private int shiftValue = 2;
        private UnknownSymbolHandlingMode unknownSymbolHandling = UnknownSymbolHandlingMode.Ignore;
        private int caseSensitiveAlphabet = 0; // 0 = case insensitve, 1 = case sensitive
        #endregion

        #region Private methods

        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if ((value[i] == value[j]) || (!CaseSensitiveAlphabet & (char.ToUpper(value[i]) == char.ToUpper(value[j]))))
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
        /// Set the new shiftValue and the new shiftCharacter to offset % alphabet.Length
        /// </summary>
        private void setKeyByValue(int offset)
        {
            // making sure the shift value lies within the alphabet range      
            offset = offset % alphabet.Length;

            // set the new shiftChar
            shiftChar = alphabet[offset];

            // set the new shiftValue
            shiftValue = offset;

            // Anounnce this to the settings pane
            OnPropertyChanged("ShiftValue");
            OnPropertyChanged("ShiftChar");

            // print some info in the log.
            LogMessage("Accepted new shift value " + offset + "! (Adjusted shift character to \'" + shiftChar + "\')", NotificationLevel.Info);
        }

        private void setKeyByCharacter(string value)
        {
            try
            {
                int offset;
                if (this.CaseSensitiveAlphabet)
                {
                    offset = alphabet.IndexOf(value[0]);
                }
                else
                {
                    offset = alphabet.ToUpper().IndexOf(char.ToUpper(value[0]));
                }
                
                if (offset >= 0)
                {
                    shiftValue = offset;
                    shiftChar = alphabet[shiftValue];
                    LogMessage("Accepted new shift character \'" + shiftChar + "\'! (Adjusted shift value to " + shiftValue + ")", NotificationLevel.Info);
                    OnPropertyChanged("ShiftValue");
                    OnPropertyChanged("ShiftChar");
                }
                else
                {
                    LogMessage("Bad input \"" + value + "\"! (Character not in alphabet!) Reverting to " + shiftChar.ToString() + "!", NotificationLevel.Error);
                }
            }
            catch (Exception e)
            {
                LogMessage("Bad input \"" + value + "\"! (" + e.Message + ") Reverting to " + shiftChar.ToString() + "!", NotificationLevel.Error);
            }
        } 

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(4)]
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionTPCaption", "ActionTPTooltip", null, 1, true, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    OnPropertyChanged("Action");

                    if (ReExecute != null) ReExecute();   
                }
            }
        }
        
        [PropertySaveOrder(5)]
        [TaskPane("ShiftValueCaption", "ShiftValueTooltip", null, 2, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]        
        public int ShiftValue
        {
            get { return shiftValue; }
            set
            {
                setKeyByValue(value);
                if (ReExecute != null) ReExecute();
            }
        }


        [PropertySaveOrder(6)]
        [TaskPaneAttribute("ShiftCharCaption", "ShiftCharTooltip", null, 3, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Za-z]$")]
        public string ShiftChar
        {
            get { return this.shiftChar.ToString(); }
            set
            {
                setKeyByCharacter(value);
                if (ReExecute != null) ReExecute();
            }
        }

        [PropertySaveOrder(7)]
        [ContextMenu("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 4, true, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get { return (int)this.unknownSymbolHandling; }
            set
            {
                if ((UnknownSymbolHandlingMode)value != unknownSymbolHandling)
                {
                    this.unknownSymbolHandling = (UnknownSymbolHandlingMode)value;
                    OnPropertyChanged("UnknownSymbolHandling");

                    if (ReExecute != null) ReExecute();   
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", Orientation.Vertical)]
        [PropertySaveOrder(9)]
        [TaskPane("AlphabetSymbolsCaption", "AlphabetSymbolsTooltip", null, 6, true, ControlType.TextBox, "")]
        public string AlphabetSymbols
        {
          get { return this.alphabet; }
          set
          {
            string a = removeEqualChars(value);
            if (a.Length == 0) // cannot accept empty alphabets
            {
              LogMessage("Ignoring empty alphabet from user! Using previous alphabet: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
            }
            else if (!alphabet.Equals(a))
            {
              this.alphabet = a;
              setKeyByValue(shiftValue); //re-evaluate if the shiftvalue is still within the range
              LogMessage("Accepted new alphabet from user: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
              OnPropertyChanged("AlphabetSymbols");

              if (ReExecute != null) ReExecute();
            }
          }
        }

        /// <summary>
        /// Visible setting how to deal with alphabet case. 0 = case insentive, 1 = case sensitive
        /// </summary>   
        //[SettingsFormat(1, "Normal")]
        [PropertySaveOrder(8)]
        [ContextMenu("AlphabetCaseCaption", "AlphabetCaseTooltip", 7, ContextMenuControlType.ComboBox, null, new string[] { "AlphabetCaseList1", "AlphabetCaseList2" })]
        [TaskPane("AlphabetCaseCaption", "AlphabetCaseTooltip", null, 7, true, ControlType.ComboBox, new string[] { "AlphabetCaseList1", "AlphabetCaseList2" })]
        public int AlphabetCase
        {
            get { return this.caseSensitiveAlphabet; }
            set
            {
                if (value == caseSensitiveAlphabet)
                    return;

                this.caseSensitiveAlphabet = value;
                if (value == 0)
                {
                    if (alphabet == (upperAlphabet + lowerAlphabet))
                    {
                        alphabet = upperAlphabet;
                        LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                        OnPropertyChanged("AlphabetSymbols");                        
                        // re-set also the key (shiftvalue/shiftChar to be in the range of the new alphabet
                        setKeyByValue(shiftValue);
                    }
                }
                else
                {
                    if (alphabet == upperAlphabet)
                    {
                        alphabet = upperAlphabet + lowerAlphabet;
                        LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                        OnPropertyChanged("AlphabetSymbols");                        
                    }
                }

                // remove equal characters from the current alphabet
                string a = alphabet;
                alphabet = removeEqualChars(alphabet);

                if (a != alphabet)
                {
                    OnPropertyChanged("AlphabetSymbols");
                    LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                }

                OnPropertyChanged("AlphabetCase");
                if (ReExecute != null) ReExecute();
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
