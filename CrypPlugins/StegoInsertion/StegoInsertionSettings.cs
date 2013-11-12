/* HOWTO: Set year, author name and organization.
   Copyright 2011 CrypTool 2 Team

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
using System.Windows;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.Plugins.StegoInsertion
{
    public class StegoInsertionSettings : ISettings
    {
        #region Private Variables

        private int selectedAction = 0;
        private int maxMessageBytesPerCarrierUnit = 1;

        #endregion

        #region TaskPane Settings

        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, null, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, true, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get
            {
                return this.selectedAction;
            }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Action");
                }
            }
        }

        /// <summary>
        /// In a MIDI file each "Program Change" MIDI message is a carrier unit. In an IL file each method is a carrier unit.
        /// </summary>
        [TaskPane("MaxMessageBytesPerCarrierUnitCaption", "MaxMessageBytesPerCarrierUnitTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 8)]
        public int MaxMessageBytesPerCarrierUnit
        {
            get
            {
                return maxMessageBytesPerCarrierUnit;
            }
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                if (maxMessageBytesPerCarrierUnit != value)
                {
                    maxMessageBytesPerCarrierUnit = value;
                    OnPropertyChanged("MaxMessageBytesPerCarrierUnit");
                }
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            switch (Action)
            {
                case 0: // Encryption
                    settingChanged("MaxMessageBytesPerCarrierUnit", Visibility.Visible);
                    break;
                case 1: // Decryption
                    settingChanged("MaxMessageBytesPerCarrierUnit", Visibility.Collapsed);
                    break;
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

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
