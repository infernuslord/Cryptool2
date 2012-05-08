/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Grain_v1
{
    // HOWTO: rename class (click name, press F2)
    public class GrainSettings : ISettings
    {
        #region ISettings Members

        private int keyLength = 80;
        [TaskPane("KeyLengthCaption", "KeyLengthTooltip", null, 0, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "80")]
        public int KeyLength
        {
            get { return this.keyLength; }
            set
            {
                this.keyLength = value;
                OnPropertyChanged("KeyLength");
            }
        }

        private int ivLength = 64;
        [TaskPane("IVLengthCaption", "IVLengthTooltip", null, 1, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "64")]
        public int IvLength
        {
            get { return this.ivLength; }
            set
            {
                this.ivLength = value;
                OnPropertyChanged("IvLength");
            }
        }      
       
        private bool binOutput = false;
        [TaskPane("BinaryOutputCaption", "BinaryOutputTooltip", null, 2, false, ControlType.CheckBox, "", null)]
        public bool BinOutput
        {
            get { return this.binOutput; }
            set
            {
                this.binOutput = (bool)value;
                OnPropertyChanged("BinOutput");
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
