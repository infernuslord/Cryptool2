/*                              
   Copyright 2009 Fabian Enkler

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
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.CaesarAnalysisHelper
{
    public enum Language
    {
        German,
        English,
        French,
        Spanish
    }

    class CaesarAnalysisHelperSettings : ISettings
    {
        internal char FrequentChar = 'e';

        private Language Lang = Language.German;

        [ContextMenu("Language", "The Language", 0, ContextMenuControlType.ComboBox, null, new[] { "German", "English", "French", "Spanish" })]
        [TaskPane("Language", "The text's language.", null, 0, false, ControlType.ComboBox, new[] { "German", "English", "French", "Spanish" })]
        public int TextLanguage
        {
            get
            {
                return (int)Lang;
            }
            set
            {
                try
                {
                    Lang = (Language)value;
                    switch (Lang)
                    {
                        case Language.German:
                        case Language.English:
                        case Language.French:
                        case Language.Spanish:
                            FrequentChar = 'e';
                            break;
                        default:
                            break;
                    }
                    OnPropertyChanged("TextLanguage");
                }
                catch (Exception)
                {
                }
            }
        }

        private bool hasChanges;
        public bool HasChanges
        {
            get { return hasChanges; }
            set
            {
                if (value != hasChanges)
                {
                    hasChanges = value;
                    OnPropertyChanged("HasChanges");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            if (name.ToLower() != "haschanges")
                HasChanges = true;
        }
    }
}