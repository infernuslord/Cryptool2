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

namespace Cryptool.Plugins.UDPReceiver
{
    public class UDPReceiverSettings : ISettings
    {
        #region Private Variables

        private int port = 0;
        private int timeout = 0;
        private int packageLimit = 0;

        #endregion

        #region TaskPane Settings


        [TaskPane("Port", "PortTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 65535)]
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                if (port != value)
                {
                    port = value;
                    OnPropertyChanged("Port");
                }
            }
        }

        [TaskPane("TimeLimit", "TimeLimitTooltip", "StopConditions", 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                if (timeout != value)
                {
                    timeout = value;
                    OnPropertyChanged("Timeout");
                }
            }
        }

        [TaskPane("PackageLimit", "PackageLimitTooltip", "StopConditions", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int PackageLimit
        {
            get
            {
                return packageLimit;
            }
            set
            {
                if (packageLimit != value)
                {
                    packageLimit = value;
                    OnPropertyChanged("PackageLimit");
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

        #endregion
    }
}
