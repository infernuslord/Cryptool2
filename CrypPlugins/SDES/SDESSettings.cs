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
    /// <summary>
    /// Settings for the SDES plugin
    /// </summary>
    public class SDESSettings : ISettings
    {
        #region private

        private bool hasChanges = false;
        private int action = 0; //0=encrypt, 1=decrypt
        private int mode = 0; //0="ECB", 1="CBC"
      
        #endregion
        
        #region events

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        #endregion

        #region public

        /// <summary>
        /// Gets/Sets the action of this plugin. Do you want the input data to be encrypted or decrypted?
        /// 1 = Encrypt
        /// 2 = Decrypt
        /// </summary>
        [ContextMenu( "ActionCaption", "ActionTooltip",1, ContextMenuControlType.ComboBox, new int[] { 1, 2}, "Encrypt","Decrypt")]
        [TaskPane( "ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
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

        /// <summary>
        /// Sets the block cipher mode of this plugin
        /// 1 = ECB
        /// 2 = CBC
        /// </summary>
        [ContextMenu( "ModeCaption", "ModeTooltip",2,ContextMenuControlType.ComboBox,null, new String[] {"Electronic Code Book (ECB)","Cipher Block Chaining (CBC)" })]
        [TaskPane( "ModeTPCaption", "ModeTPTooltip", null, 2, false, ControlType.ComboBox, new String[] { "Electronic Bode Book (ECB)","Cipher Block Chaining (CBC)", })]
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

        /// <summary>
        /// Did the plugins properties change?
        /// </summary>
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region private/protecte

        /// <summary>
        /// A property changed
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Change the plugins icon (used for encryption/decryption icon)
        /// </summary>
        /// <param name="Icon">icon number</param>
        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }

        #endregion
    }

}//end namespace Cryptool.Plugins.Cryptography.Encryption
