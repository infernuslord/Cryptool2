/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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

namespace SHA
{
    public class SHASettings : ISettings
    {
        private bool hasChanges = false;
        public enum ShaFunction { SHA1, SHA256, SHA384, SHA512 };

        private ShaFunction selectedShaFunction = ShaFunction.SHA1;

        [ContextMenu( "SHAFunctionCaption", "SHAFunctionTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "SHA1", "SHA256", "SHA384", "SHA512" })]
        [TaskPane( "SHAFunctionCaption", "SHAFunctionTooltip", "", 1, false, ControlType.ComboBox, new string[] { "SHA1", "SHA256", "SHA384", "SHA512"})]
        public int SHAFunction
        {
            get { return (int)this.selectedShaFunction; }
            set
            {
                this.selectedShaFunction = (ShaFunction)value;
                OnPropertyChanged("SHAFunction");
            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public bool HasChanges
        {
          get { return hasChanges; }
          set { hasChanges = value; }
        }

        #endregion

    }
}
