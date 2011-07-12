/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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

using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;

namespace Cryptool.Trivium
{
    public class TriviumSettings : ISettings
    {
        #region ISettings Members

        private bool hasChanges = false;

        private int keystreamLength = 32;
        [TaskPane("KeystreamLengthCaption", "KeystreamLengthTooltip", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int KeystreamLength
        {
            get { return this.keystreamLength; }
            set
            {
                this.keystreamLength = value;
                //OnPropertyChanged("KeystreamLength");
                HasChanges = true;
            }
        }

        private int initRounds = 1152;
        [TaskPane("InitRoundsCaption", "InitRoundsTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int InitRounds
        {
            get { return this.initRounds; }
            set
            {
                this.initRounds = value;
                //OnPropertyChanged("InitRounds");
                HasChanges = true;
            }
        }

        private bool useByteSwapping = false;
        [ContextMenu("UseByteSwappingCaption", "UseByteSwappingTooltip", 1, ContextMenuControlType.CheckBox, null, new string[] { "UseByteSwappingList1" })]
        [TaskPane("UseByteSwappingCaption", "UseByteSwappingTooltip", null, 2, false, ControlType.CheckBox, "", null)]
        public bool UseByteSwapping
        {
            get { return this.useByteSwapping; }
            set
            {
                this.useByteSwapping = (bool)value;
                //OnPropertyChanged("UseByteSwapping");
                HasChanges = true;
            }
        }

        private bool hexOutput = false;
        [ContextMenu("HexOutputCaption", "HexOutputTooltip", 2, ContextMenuControlType.CheckBox, null, new string[] { "HexOutputList1" })]
        [TaskPane("HexOutputCaption", "HexOutputTooltip", null, 3, false, ControlType.CheckBox, "", null)]
        public bool HexOutput
        {
            get { return this.hexOutput; }
            set
            {
                this.hexOutput = (bool)value;
                //OnPropertyChanged("HexOutput");
                HasChanges = true;
            }
        }

        private string inputKey = string.Empty;
        [TaskPane("InputKeySettingsCaption", "InputKeySettingsTooltip", null, 4, false, ControlType.TextBox, null)]
        public string InputKey
        {
            get { return inputKey; }
            set
            {
                this.inputKey = value;
                //OnPropertyChanged("InputKey");
                HasChanges = true;
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
