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

namespace Cryptool.TEA
{
    public class TEASettings : ISettings
    {
        #region ISettings Members

        private int action = 0; //0=encrypt, 1=decrypt
        private int padding = 0; //0="Zeros"=default, 1="None", 2="PKCS7"
        private int version = 0; //0="TEA"=default, 1="XTEA, 2=XXTEA"
        private int rounds = 64;

        [ContextMenu( "ActionCaption", "ActionTooltip",1, ContextMenuControlType.ComboBox, new int[] { 1, 2}, "ActionList1", "ActionList2")]
        [TaskPane( "ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get { return this.action; }
            set { this.action = (int)value; }
        }

        [ContextMenu("PaddingCaption", "PaddingTooltip", 3, ContextMenuControlType.ComboBox, null, "PaddingList1", "PaddingList2", "PaddingList3")]
        [TaskPane("PaddingTPCaption", "PaddingTPTooltip", "", 3, false, ControlType.ComboBox, new String[] { "PaddingList1", "PaddingList2", "PaddingList3" })]
        public int Padding
        {
            get { return this.padding; }
            set
            {
                if (((int)value) != padding)
                {
                    this.padding = (int)value;
                    OnPropertyChanged("Padding");                    
                }
            }
        }

        [ContextMenu("VersionCaption", "VersionTooltip", 4, ContextMenuControlType.ComboBox, null, "VersionList1", "VersionList3", "VersionList3")]
        [TaskPane("VersionCaption", "VersionTooltip", "", 4, false, ControlType.ComboBox, new String[] { "VersionTPList1", "VersionTPList2", "VersionTPList3" })]
        public int Version
        {
            get { return this.version; }
            set
            {
                if (((int)value) != version)
                {
                    this.version = (int)value;
                    OnPropertyChanged("Padding");                    
                }
            }
        }

        [TaskPane("RoundsCaption", "RoundsTooltip", "RoundsGroup", 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Rounds
        {
            get { return this.rounds; }
            set
            {
                if (((int)value) != rounds)
                {
                    this.rounds = value;
                    OnPropertyChanged("Rounds");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propName);
        }

        #endregion
    }
}
