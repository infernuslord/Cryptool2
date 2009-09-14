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
using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;

namespace Cryptool.Plugins.Cryptography.Encryption
{
    public class SDESSettings : ISettings
    {
        private bool hasChanges = false;
        private int action = 0; //0=encrypt, 1=decrypt
        private int mode = 0; //0="ECB", 1="CBC"

        [ContextMenu("Action","Do you want the input data to be encrypted or decrypted?",1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2}, "Encrypt","Decrypt")]
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

        [ContextMenu("Chaining mode","Select the block cipher mode of operation.",2,DisplayLevel.Beginner,ContextMenuControlType.ComboBox,null, new String[] {"Electronic Code Book (ECB)","Cipher Block Chaining (CBC)" })]
        [TaskPane("Chaining Mode", "Select the block cipher mode of operation.", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "Electronic Bode Book (ECB)","Cipher Block Chaining (CBC)", })]
        public int Mode
        {
            get { return this.mode; }
            set
            {
                if (((int)value) != mode) hasChanges = true;
                this.mode = (int)value;
                OnPropertyChanged("Mode");
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }

    }
}
