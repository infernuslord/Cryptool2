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

namespace Cryptool.Plugins.RSA
{
    class RSASettings : ISettings
    {
        #region private variables
        private int mode;
        private int coresUsed;
        private bool hasChanges = false;
        #endregion

        public RSASettings()
        {
            CoresUsed = 0; //start with only 1 CPU
        }

        #region taskpane
        [TaskPane("CoresUsed", "Choose how many cores are used", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8" })]
        public int CoresUsed
        {
            get { return this.coresUsed; }
            set
            {
                if (value != this.coresUsed)
                {
                    this.coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                    HasChanges = true;
                }
            }
        }
                
        [ContextMenu("Mode", "Select the RSA mode", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Encryption", "Decryption")]
        [TaskPane("Source", "Select the RSA mode", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Encryption", "Decryption" })]
        public int Mode
        {
            get { return this.mode; }
            set
            {
                mode = value;
                OnPropertyChanged("Mode");
            }
        }

        #endregion

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
