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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Documents;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Webcam;
using Cryptool.Plugins.Webcam.Libaries;

namespace Cryptool.Plugins.Webcam
{
    // HOWTO: rename class (click name, press F2)
    public class WebcamSettings : ISettings
    {
        #region Private Variables

        private int quality = 50;
        private ObservableCollection<string> device = new ObservableCollection<string>();
        private int capDevice;
        private int sendPicture = 1000;
        private int takePictureChoice;

        #endregion

        public WebcamSettings()
        {
            Device.Clear();
            for (int i = 0; i < CapDevice.DeviceMonikers.Length-1; i++)
            {
                Device.Add(FilterInfo.GetName(CapDevice.DeviceMonikers[i].MonikerString));
            }
            capDevice = CapDevice.DeviceMonikers.Length-1;
        }
        
        public ObservableCollection<string> Device
        {
            get { return device; }
            set
            {
                if (value != device)
                {
                    device = value;
                    OnPropertyChanged("Device");
                }
            }
        }


        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>

        [TaskPane("DeviceChoice", "DeviceChoiceToolTip", null, 0, false, ControlType.DynamicComboBox, new string[] { "Device" })]
        public int DeviceChoice
        {
            get
            {
                return capDevice;
            }
            set
            {
                if (capDevice != value)
                {
                    capDevice = value;
                    OnPropertyChanged("DeviceChoice");
                }
            }
        }

        [TaskPane("TakePictureChoice", "TakePictureChoiceToolTip", null, 0, false, ControlType.ComboBox, new string[] { "True/False", "True", "False" })]
        public int TakePictureChoice
        {
            get
            {
                return takePictureChoice;
            }
            set
            {
                if (takePictureChoice != value)
                {
                    takePictureChoice = value;
                    OnPropertyChanged("TakePictureChoice");
                }
            }
        }

        [TaskPane("PictureQuality", "PictureQualityToolTip", "DeviceSettings", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int PictureQuality
        {
            get
            {
                return quality;
            }
            set
            {
                if (quality != value)
                {
                    quality = value;
                    OnPropertyChanged("PictureQuality");
                }
            }
        }

        [TaskPane("SendPicture", "SendPictureToolTip", "DeviceSettings", 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 40, 10000)]
        public int SendPicture
        {
            get
            {
                return sendPicture;
            }
            set
            {
                if (sendPicture != value)
                {
                    sendPicture = value;
                    OnPropertyChanged("SendPicture");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
