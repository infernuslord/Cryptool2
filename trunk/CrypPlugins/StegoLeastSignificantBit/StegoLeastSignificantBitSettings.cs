/* 
   Copyright 2011 Corinna John

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
using System.Windows;

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    public class StegoLeastSignificantBitSettings : ISettings
    {
        #region Private Variables

        private int selectedAction = 0;
        private int outputFileFormat = 0;
        private bool customizeRegions = false;
        private bool showRegions = false;

        #endregion

        #region TaskPane Settings

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

        [TaskPane("CustomizeRegionsCaption", "CustomizeRegionsTooltip", null, 1, false, ControlType.CheckBox)]
        public bool CustomizeRegions
        {
            get
            {
                return customizeRegions;
            }
            set
            {
                if (customizeRegions != value)
                {
                    customizeRegions = value;
                    OnPropertyChanged("CustomizeRegions");
                }
            }
        }

        [TaskPane("ShowRegionsCaption", "ShowRegionsTooltip", null, 1, false, ControlType.CheckBox)]
        public bool ShowRegions
        {
            get
            {
                return showRegions;
            }
            set
            {
                if (showRegions != value)
                {
                    showRegions = value;
                    OnPropertyChanged("ShowRegions");
                }
            }
        }

        [TaskPane("OutputFileFormatCaption", "OutputFileFormatTooltip", null, 1, true, ControlType.ComboBox, new string[] { "OutputFileFormatList1", "OutputFileFormatList2", "OutputFileFormatList3" })]
        public int OutputFileFormat
        {
            get
            {
                return this.outputFileFormat;
            }
            set
            {
                if (value != outputFileFormat)
                {
                    this.outputFileFormat = value;
                    OnPropertyChanged("OutputFileFormat");   
                }
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            switch (Action)
            {
                case 0: // Encryption
                    settingChanged("OutputFileFormat", Visibility.Visible);
                    settingChanged("CustomizeRegions", Visibility.Visible);
                    settingChanged("ShowRegions", Visibility.Collapsed);
                    break;
                case 1: // Decryption
                    settingChanged("OutputFileFormat", Visibility.Collapsed);
                    settingChanged("CustomizeRegions", Visibility.Collapsed);
                    settingChanged("ShowRegions", Visibility.Visible);
                    break;
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

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
