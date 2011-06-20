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

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    public class StegoLeastSignificantBitSettings : ISettings
    {
        #region Private Variables

        private int selectedAction = 0;
        private int outputFileFormat = 0;
        private bool hasChanges = false;
        private int bitCount = 1;
        //private int noisePercent = 0;
        private bool customizeRegions = false;

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
                if (value != selectedAction) HasChanges = true;
                this.selectedAction = value;
                OnPropertyChanged("Action");
            }
        }

        [TaskPane("BitCountSettingsCaption", "BitCountSettingsTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int BitCount
        {
            get
            {
                return bitCount;
            }
            set
            {
                if (bitCount != value)
                {
                    bitCount = value;
                    hasChanges = true;
                    OnPropertyChanged("BitCount");
                }
            }
        }

        /* TODO: Enable this property only if Action==Encrypt
         * [TaskPane("Noise", "Percentage of the carrier that will be covered with noise", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int NoisePercent
        {
            get
            {
                return noisePercent;
            }
            set
            {
                if (noisePercent != value)
                {
                    noisePercent = value;
                    hasChanges = true;
                }
            }
        }*/

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

        [ContextMenu("OutputFileFormatCaption", "OutputFileFormatTooltip", 1, ContextMenuControlType.ComboBox, null, ".bmp", ".png", ".tif")]
        [TaskPane("OutputFileFormatCaption", "OutputFileFormatTooltip", null, 1, true, ControlType.ComboBox, new string[] { ".bmp", ".png", ".tif" })]
        public int OutputFileFormat
        {
            get
            {
                return this.outputFileFormat;
            }
            set
            {
                if (value != outputFileFormat) HasChanges = true;
                this.outputFileFormat = value;
                OnPropertyChanged("Action");
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
