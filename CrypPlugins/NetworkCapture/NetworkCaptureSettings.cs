/*
   Copyright 2011 Matthäus Wander, University of Duisburg-Essen

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
using System.Collections.ObjectModel;

namespace Cryptool.Plugins.NetworkCapture
{
    public class NetworkCaptureSettings : ISettings
    {
        #region Private Variables

        private bool hasChanges = false;

        private ObservableCollection<string> collection = new ObservableCollection<string>();
        private int currentDevice;

        #endregion

        #region TaskPane Settings

        [TaskPane( "DeviceCaption", "DeviceTooltip", "", 0, false, ControlType.DynamicComboBox, new string[] { "Collection" })]
        public int Device
        {
            get { return currentDevice; }
            set
            {
                if (value != currentDevice)
                {
                    HasChanges = true;
                    this.currentDevice = value;
                    OnPropertyChanged("Dictionary");
                }
            }
        }

        // CrypWin requires this to be a collection of strings
        [DontSave]
        public ObservableCollection<string> Collection
        {
            get { return collection; }
            set
            {
                collection = value;
                OnPropertyChanged("Collection");
            }
        }

        #endregion

        #region ISettings Members

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
