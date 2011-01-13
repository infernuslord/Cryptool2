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
using System.ComponentModel;
using Cryptool.PluginBase;

namespace Nihilist
{
    class NihilistSettings : ISettings
    {
        private enum Actions
        {
            Encrypt,
            Decrypt
        }

        private Actions action = Actions.Encrypt;
        [ContextMenu("Action", "Select the Algorithm action", 0, ContextMenuControlType.ComboBox, null, new[] { "Encrypt", "Decrypt" })]
        [TaskPane("Action", "Select the Algorithm action", null, 0, false, ControlType.ComboBox, new[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get { return (int)action; }
            set
            {
                try
                {
                    action = (Actions)value;
                    OnPropertyChanged("Action");
                }
                catch (Exception)
                {
                    action = Actions.Encrypt;
                }
            }
        }

        private string keyWord = string.Empty;
        [TaskPane("Keyword", "This is the key used to en/decrypt.", null, 0, false, ControlType.TextBox)]
        public string KeyWord
        {
            get { return keyWord; }
            set
            {
                keyWord = value;
                OnPropertyChanged("KeyWord");
            }
        }

        private string secondKeyWord = string.Empty;
        [TaskPane("Second Keyword", "This is the second key used to en/decrypt.", null, 0, false, ControlType.TextBox)]
        public string SecondKeyWord
        {
            get { return secondKeyWord; }
            set
            {
                secondKeyWord = value;
                OnPropertyChanged("SecondKeyWord");
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
