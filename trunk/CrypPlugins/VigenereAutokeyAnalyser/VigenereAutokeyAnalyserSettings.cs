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

namespace Cryptool.Plugins.VigenereAutokeyAnalyser
{
    public class VigenereAutokeyAnalyserSettings : ISettings
    {
        #region Private Variables

        private int language = 0;                                   //Set the expected language (0: English ; 1: German ; 2: French ; 3: Spain)
        private int modus = 0;                                      //Set the modus (0: Autokey ; 1: Original Vigenere)
        private String alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";     //The standard configuration

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Choose the modus to work with
        /// </summary>
        [ContextMenu("ModusCaption", "ModusTooltip", 2, ContextMenuControlType.ComboBox, null, new String[] { "ModusList1", "ModusList2" })]
        [TaskPane("ModusCaption", "ModusTooltip", null, 2, false, ControlType.ComboBox, new String[] { "ModusList1", "ModusList2" })]
        public int Modus // Autokey or Repeatedkey
        {
            get { return this.modus; }
            set
            {
                if (((int)value) != modus)
                {
                    this.modus = (int)value;
                    OnPropertyChanged("Modus");                    
                }
            }
        }

        /// <summary>
        /// Choose the language frequency to work with
        /// </summary>
        [ContextMenu("LanguageCaption", "LanguageTooltip", 2, ContextMenuControlType.ComboBox, null, new String[] { "LanguageList1", "LanguageList2", "LanguageList3", "LanguageList4" })]
        [TaskPane("LanguageCaption", "LanguageTooltip", null, 2, false, ControlType.ComboBox, new String[] { "LanguageList1", "LanguageList2", "LanguageList3", "LanguageList4" })]
        public int Language // Expected letter frequencies
        {
            get { return this.language; }
            set
            {
                if (((int)value) != language)
                {
                    this.language = (int)value;
                    OnPropertyChanged("Language");                    
                }
            }
        }

        /// <summary>
        /// Choose the alphabet letters to work with
        /// </summary>
        [TaskPane("AlphabetSymbolsCaption", "AlphabetSymbolsTooltip", null, 7, false, ControlType.TextBox, null)]
        public string AlphabetSymbols
        {
            get { return this.alphabet; }
            set
            {
                string a = removeEqualChars(value);     //removes all char that are used twice in the alphabet

                if (a.Length > 0 && !alphabet.Equals(a)) //only if not empty
                {
                    this.alphabet = a;
                    OnPropertyChanged("AlphabetSymbols");
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Removes all the letters in the provided alphabet that are used twice
        /// </summary>
        private string removeEqualChars(string value)
        {
            string res = "";
            foreach (char c in value)
                if (res.IndexOf(c) < 0) res += c;
            return res;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

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
