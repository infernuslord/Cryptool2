/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.ComponentModel;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.RSASystem
{
    class RSASystemSettings : ISettings
    {
        private bool hasChanges = false;
        private int action = 0; //0=encrypt, 1=decrypt

        [ContextMenu("Action", "Do you want the input data to be encrypted or decrypted?", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Encrypt", "Decrypt")]
        [TaskPane("Action", "Do you want the input data to be encrypted or decrypted?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (((int)value) != action) hasChanges = true;
                this.action = (int)value;
                OnPropertyChanged("Action");
            }
        }

        #region ISettings Members

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

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion
    }
    
    
}
