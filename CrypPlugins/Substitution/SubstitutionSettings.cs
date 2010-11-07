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

namespace Cryptool.Substitution
{
    public class SubstitutionSettings : ISettings
    {
        #region Public Caesar specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Substitution plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void SubstitutionLogMessage(string msg, NotificationLevel logLevel);

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>
        public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };

        /// <summary>
        /// An enumaration fot the different key variante modes of refilling the cipher alphabet
        /// </summary>
        public enum KeyVariantMode { RestCharAscending = 0, RestCharDescending = 1, FixKeyAtbash = 2 };
        
        /// <summary>
        /// Fire if a new message has to be shown in the status bar
        /// </summary>
        public event SubstitutionLogMessage LogMessage;

        /// <summary>
        /// Retrieves the current settings whether the alphabet shoudl be treated as case sensitive or not
        /// </summary>
        [PropertySaveOrder(0)]
        public bool CaseSensitiveAlphabet
        {
            get
            {
                if (caseSensitiveAlphabet == 0) return false;
                else return true;
            }
            set { } //read only
        }

        /// <summary>
        /// Return true if some settings habe been changed. This value should be set externally to false e.g.
        /// when a project was saved.
        /// </summary>
        [PropertySaveOrder(1)]
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region Private variables

        private bool hasChanges;
        private int selectedAction = 0;
        private KeyVariantMode keyVariant = KeyVariantMode.RestCharAscending;
        private string upperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string lowerAlphabet = "abcdefghijklmnopqrstuvwxyz";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string cipherAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string keyValue;
        private UnknownSymbolHandlingMode unknowSymbolHandling = UnknownSymbolHandlingMode.Ignore;
        private int caseSensitiveAlphabet = 0; //0=case insensitive, 1 = case sensitive
        private Substitution substitution;

        #endregion

        public SubstitutionSettings(Substitution substitution)
        {
            this.substitution = substitution;
        }

        #region Private methods

        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if (value[i] == value[j] || (!CaseSensitiveAlphabet & (char.ToUpper(value[i]) == char.ToUpper(value[j]))))
                    {
                        LogMessage("Removing duplicate letter: \'" + value[j] + "\' from alphabet!", NotificationLevel.Warning);
                        value = value.Remove(j, 1);
                        j--;
                        length--;
                    }
                }
            }
            return value;
        }

        private void setCipherAlphabet(string value)
        {
            foreach (char c in value)
            {
                if (!alphabet.Contains(c))
                    substitution.GuiLogMessage("Key contains characters that are not part of the alphabet!", NotificationLevel.Error);
            }

            try
            {
                string a = null;
                bool found;
                switch (keyVariant)
                {
                    case KeyVariantMode.RestCharAscending:
                        a = value;
                        for (int i = 0; i < alphabet.Length; i++)
                        {
                            found = false;
                            for (int j = 0; j < a.Length; j++)
                            {
                                if (alphabet[i] == a[j])
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                a += alphabet[i];
                        }
                        break;
                    case KeyVariantMode.RestCharDescending:
                        a = value;
                        for (int i = alphabet.Length - 1; i >= 0; i--)
                        {
                            found = false;
                            for (int j = 0; j < a.Length; j++)
                            {
                                if (alphabet[i] == a[j])
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                a += alphabet[i];
                        }
                        break;
                    case KeyVariantMode.FixKeyAtbash:
                        a = string.Empty;
                        for (int i = alphabet.Length - 1; i >= 0; i--)
                            a += alphabet[i];
                        break;
                }
                CipherAlphabet = removeEqualChars(a);
                OnPropertyChanged("CipherAlphabet");
            }
            catch (Exception e)
            {
                LogMessage("Bad input \"" + value + "\"! (" + e.Message + ") Reverting to " + alphabet + "!", NotificationLevel.Error);
                OnPropertyChanged("CipherAlphabet");
            }
        }

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(2)]
        [ContextMenu("Action","Select the Algorithm action", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] {1,2}, "Encrypt","Decrypt")]
        [TaskPane("Action", "Select the Algorithm action", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] {"Encrypt","Decrypt"})]
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
        [TaskPane("Key value (multiple letter)", "Enter one or multiple letters as the key.", null, 2, false, DisplayLevel.Experienced, ControlType.TextBox,"",null)]
        public string KeyValue
        {
            get { return this.keyValue; }
            set 
            {
                this.keyValue = value;
                setCipherAlphabet(keyValue);
                OnPropertyChanged("KeyValue");
                OnPropertyChanged("AlphabetSymbols");
            }
            
        }

        [PropertySaveOrder(4)]
        [ContextMenu("Unknown symbol handling","What should be done with ecountered characters at the input which are not in the alphabet?",3,DisplayLevel.Expert,ContextMenuControlType.ComboBox, null, new string[] {"Ignore (leave unmodified)", "Remove","Replace with \'?\'"})]
        [TaskPane("Unknown symbol handling", "What should be done with ecountered characters at the input which are not in the alphabet?", null, 3, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] {"Ignore (leave unmodified)", "Remove","Replace with \'?\'"})]
        public int UnknownSymbolHandling
        {
            get { return (int)this.unknowSymbolHandling; }
            set
            {
                if ((UnknownSymbolHandlingMode)value != unknowSymbolHandling) HasChanges = true;
                this.unknowSymbolHandling = (UnknownSymbolHandlingMode)value;
                OnPropertyChanged("UnknownSymbolHandling");
            }
        }

        [PropertySaveOrder(5)]
        [ContextMenu("Alphabet case sensitivity","Should upper and lower case be treated differently? (Should a == A)",4,DisplayLevel.Expert, ContextMenuControlType.ComboBox,null, new string[] {"Case insensitive","Case sensitive"})]
        [TaskPane("Alphabet case sensitivity","Should upper and lower case be treated differently? (Should a == A)", null, 4, false, DisplayLevel.Expert, ControlType.ComboBox, new string[] {"Case insensitive","Case sensitive"})]
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
                        LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + "Symbols)", NotificationLevel.Info);
                        OnPropertyChanged("AlphabetSymbols");
                        setCipherAlphabet(keyValue);
                    }
                }
                else
                {
                    if (alphabet == upperAlphabet)
                    {
                        alphabet = upperAlphabet + lowerAlphabet;
                        LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                        OnPropertyChanged("AlphabetSymbols");
                        setCipherAlphabet(keyValue);
                    }
                }

                //remove equal characters from the current alphabet
                string a = alphabet;
                alphabet = removeEqualChars(alphabet);

                if (a != alphabet)
                {
                    OnPropertyChanged("AlphabetSymbols");
                    LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                }
                OnPropertyChanged("AlphabetCase");
            }
        }

        [PropertySaveOrder(6)]
        [ContextMenu("Key variant","Select the key variant for the cipher alphabet",5,DisplayLevel.Expert,ContextMenuControlType.ComboBox,null,new string[] {"Remaining characters are filled in ascending order","Remaining characters are filld in descending order","Atbash (the encryption is using a fixed key)"})]
        [TaskPane("Key variant","Select the key variant for the cipher alphabet", null, 5, false, DisplayLevel.Expert, ControlType.ComboBox,new string[] {"Remaining characters are filled in ascending order","Remaining characters are in descending order","Atbash (the encryption is using a fixed key)"})]
        public int KeyVariant
        {
            get { return (int)this.keyVariant; }
            set
            {
                if ((KeyVariantMode)value != keyVariant) HasChanges = true;
                this.keyVariant = (KeyVariantMode)value;
                setCipherAlphabet(keyValue);
                OnPropertyChanged("KeyVariant");
            }
        }

        [PropertySaveOrder(7)]
        [TaskPane("Alphabet","This is the used alphabet.", null, 5, false, DisplayLevel.Expert,ControlType.TextBox,"")]
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
                    HasChanges = true;
                    this.alphabet = a;
                    setCipherAlphabet(keyValue); //re-evaluate if the key value is still within the range
                    LogMessage("Accepted new alphabet from user: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                    OnPropertyChanged("AlphabetSymbols");
                }
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane("Cipher alphabet", "This is the used cipher alphabet.", null, 6, false, DisplayLevel.Expert, ControlType.TextBox, "")]
        public string CipherAlphabet
        {
            get { return this.cipherAlphabet; }
            set 
            {
                this.cipherAlphabet = value;
                OnPropertyChanged("CipherAlphabet");
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
