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

        public enum CaesarMode { Encrypt = 0, Decrypt = 1 };

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

        #endregion

        #region Private variables

        private CaesarMode selectedAction = CaesarMode.Encrypt;
        private string upperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string lowerAlphabet = "abcdefghijklmnopqrstuvwxyz";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private int shiftValue = 3;
        private string shiftString;
        private UnknownSymbolHandlingMode unknownSymbolHandling = UnknownSymbolHandlingMode.Ignore;
        private bool caseSensitiveSensitive = false;

        #endregion

        #region Private methods

        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if ((value[i] == value[j]) || (!CaseSensitive & (char.ToUpper(value[i]) == char.ToUpper(value[j]))))
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
        /// Set the new shiftValue and the new shiftString to offset % alphabet.Length
        /// </summary>
        private void setKeyByValue(int offset)
        {
            // making sure the shift value lies within the alphabet range      
            shiftValue = offset % alphabet.Length;
            shiftString = "A -> " + alphabet[shiftValue];

            // Anounnce this to the settings pane
            OnPropertyChanged("ShiftValue");
            OnPropertyChanged("ShiftString");

            // print some info in the log.
            LogMessage("Accepted new shift value: " + offset, NotificationLevel.Debug);
        }

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(4)]
        [TaskPane("ActionTPCaption", "ActionTPTooltip", null, 1, true, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public CaesarMode Action
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
        [TaskPane("ShiftStringCaption", "ShiftStringTooltip", null, 3, false, ControlType.TextBoxReadOnly)]
        public string ShiftString
        {
            get { return shiftString; }
        }

        [PropertySaveOrder(7)]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 4, true, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public UnknownSymbolHandlingMode UnknownSymbolHandling
        {
            get { return this.unknownSymbolHandling; }
            set
            {
                if (value != unknownSymbolHandling)
                {
                    this.unknownSymbolHandling = value;
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
        /// Visible setting how to deal with alphabet case. false = case insensitive, true = case sensitive
        /// </summary>   
        //[SettingsFormat(1, "Normal")]
        [PropertySaveOrder(8)]
        [TaskPane("AlphabetCaseCaption", "AlphabetCaseTooltip", null, 7, true, ControlType.CheckBox)]
        public bool CaseSensitive
        {
            get { return this.caseSensitiveSensitive; }
            set
            {
                if (value == caseSensitiveSensitive)
                    return;

                this.caseSensitiveSensitive = value;
                if (value)
                {
                    if (alphabet == upperAlphabet)
                    {
                        alphabet = upperAlphabet + lowerAlphabet;
                        LogMessage(
                            "Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length + " Symbols)",
                            NotificationLevel.Debug);
                        OnPropertyChanged("AlphabetSymbols");
                    }
                }
                else
                {
                    if (alphabet == (upperAlphabet + lowerAlphabet))
                    {
                        alphabet = upperAlphabet;
                        LogMessage(
                            "Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length + " Symbols)",
                            NotificationLevel.Debug);
                        OnPropertyChanged("AlphabetSymbols");
                        // re-set also the key (shiftvalue/shiftString to be in the range of the new alphabet
                        setKeyByValue(shiftValue);
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

                OnPropertyChanged("CaseSensitive");
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
