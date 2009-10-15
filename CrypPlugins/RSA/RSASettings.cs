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
using System.Collections.ObjectModel;

namespace Cryptool.Plugins.RSA
{
    /// <summary>
    /// Settings class for the RSA plugin
    /// </summary>
    class RSASettings : ISettings
    {
        #region private members

        private int action;
        private int coresUsed;
        private bool hasChanges = false;
        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();

        #endregion

        #region events

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public

        /// <summary>
        /// Constructs a new RSASettings
        /// detects the number of cores of the system and sets those to maximum number of cores
        /// which can be used
        /// </summary>
        public RSASettings()
        {
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i + 1).ToString());
            CoresUsed = Environment.ProcessorCount - 1;
        }
        
        /// <summary>
        /// Get the number of cores in a collection, used for the selection of cores
        /// </summary>
        public ObservableCollection<string> CoresAvailable
        {
            get { return coresAvailable; }
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                }
                OnPropertyChanged("CoresAvailable");
            }
        }

        /// <summary>
        /// Getter/Setter for the number of cores which should be used by RSA
        /// </summary>
        [TaskPane("CoresUsed", "Choose how many cores should be used", null, 1, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
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

        /// <summary>
        /// Getter/Setter for the action (encryption or decryption)
        /// </summary>
        [ContextMenu("Action", "Do you want the input data to be encrypted or decrypted?", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "Encryption", "Decryption")]
        [TaskPane("Action", "Do you want the input data to be encrypted or decrypted?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Encryption", "Decryption" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                action = value;
                OnPropertyChanged("Action");
            }
        }

        /// <summary>
        /// Did anything change on the settigns?
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

        #region private

        /// <summary>
        /// The property p has changes
        /// </summary>
        /// <param name="p">p</param>
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

    }//end RSASettings

}//end Cryptool.Plugins.RSA
