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

namespace Cryptool.Plugins.M_138
{
    public class M_138Settings : ISettings
    {
        #region Private Variables

        private int _encryptDecrypt = 0;
        private int _separatorStr = 0;
        private int _separatorOff = 0;

        #endregion

        #region TaskPane Settings

        [TaskPane("EncryptDeryptCap", "EncryptDeryptDes", null, 0, false, ControlType.ComboBox, new string[] { "EncryptSelection", "DecryptSelection" })]
        public int ModificationType
        {
            get
            {
                return _encryptDecrypt;
            }
            set
            {
                if (_encryptDecrypt != value)
                {
                    _encryptDecrypt = value;
                }
            }
        }

        [TaskPane("SeperatorStripCap", "SeperatorStripDes", null, 0, false, ControlType.ComboBox, new string[] {  ",", ".", "/" })]
        public int SeperatorStripChar
        {
            get
            {
                return _separatorStr;
            }
            set
            {
                if (_separatorStr != value)
                {
                    _separatorStr = value;
                }
            }
        }

        [TaskPane("SeperatorOffCap", "SeperatorOffDes", null, 0, false, ControlType.ComboBox, new string[] { "/", ",", "." })]
        public int SeperatorOffChar
        {
            get
            {
                return _separatorOff;
            }
            set
            {
                if (_separatorOff != value)
                {
                    _separatorOff = value;
                }
            }
        }


        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public void Initialize()
        {

        }

        #endregion
    }
}
