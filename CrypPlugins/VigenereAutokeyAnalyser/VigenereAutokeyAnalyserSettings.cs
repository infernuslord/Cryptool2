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

        private bool hasChanges = false;
        private int language = 0;
        private String alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #endregion

        #region TaskPane Settings

        [ContextMenu("Expected Language", "Select the language you expect the plaintext to be", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new String[] { "English", "German", "French", "Spanish" })]
        [TaskPane("Expected Language", "Select the language you expect the plaintext to be", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "English", "German", "French", "Spanish" })]
        public int Language // Expected Letter Frequencies
        {
            get { return this.language; }
            set
            {
                if (((int)value) != language) hasChanges = true;
                this.language = (int)value;
                OnPropertyChanged("Language");
            }
        }

        [TaskPane("Alphabet", "This is the used alphabet.", null, 7, false, DisplayLevel.Expert, ControlType.TextBox, null)]
        public string AlphabetSymbols
        {
            get { return this.alphabet; }
            set
            {
                string a = removeEqualChars(value);
                if (a.Length == 0) //cannot accept empty alphabets and will just use the standard;
                {
                    
                }
                else if (!alphabet.Equals(a))
                {
                    hasChanges = true;
                    this.alphabet = a;
                    OnPropertyChanged("AlphabetSymbols");
                }
            }
        }

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

        #region Private methods
        
        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if ((value[i]) == (value[j]))
                    {                        
                        value = value.Remove(j, 1);
                        j--;
                        length--;
                    }
                }
            }
            return value;
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
