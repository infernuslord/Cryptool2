/*
   Copyright 2008 Dr. Arno Wacker, University of Duisburg-Essen

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

namespace Cryptool.Plugins.Convertor
{
    public class StringToStreamConverterSettings : ISettings
    {
        #region Public StringToStreamConverter specific properties

        public enum EncodingTypes { Default = 0, Base64Binary = 1, HexStringBinary = 2, OctalStringBinary = 3, Unicode = 4, UTF7 = 5, UTF8 = 6, UTF32 = 7, ASCII = 8, BigEndianUnicode = 9 };

        /// <summary>
        /// Retrieves the current used encoding, or sets it.
        /// </summary>
        public EncodingTypes Encoding
        { 
            get { return this.encoding; }
            set 
            {
                if (this.Encoding != value) hasChanges = true;
                this.encoding = value; 
                OnPropertyChanged("EncodingSetting"); 
            }
        }


        /// <summary>
        /// Returns true if some properties where changed.
        /// </summary>
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion


        #region Algorithm settings properties (visible in the Settings pane)

        /// <summary>
        /// Encoding property used in the Settings pane. 
        /// </summary>
        [ContextMenu( "EncodingSettingCaption", "EncodingSettingTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7", "EncodingSettingList8", "EncodingSettingList9", "EncodingSettingList10" })]
        [TaskPane( "EncodingSettingCaption", "EncodingSettingTooltip", "", 1, false, ControlType.ComboBox, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7", "EncodingSettingList8", "EncodingSettingList9", "EncodingSettingList10" })]
        public int EncodingSetting
        {
            get
            {
                return (int)this.encoding;
            }
            set
            {
                if (this.encoding != (EncodingTypes)value) hasChanges = true;
                this.encoding = (EncodingTypes)value;
                OnPropertyChanged("EncodingSetting");
            }
        }

        #endregion

        #region Private variables
        private EncodingTypes encoding = EncodingTypes.UTF8;
        private bool hasChanges = false;
        #endregion

        #region Private methods
        // nothing here
        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}
