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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.NetworkSender;

namespace Cryptool.Plugins.NetworkSender
{
    public class NetworkSenderSettings : ISettings
    {
        #region Private Variables

        private int port;
        private string deviceIP;
        private bool byteAsciiSwitch;
        private bool tryConnect = false;
        private int connectIntervall;
        private int protocol;

        #endregion


        #region TaskPane Settings
        [TaskPane("DeviceIpCaption", "DeviceIpCaptionTooltip", "NetworkConditions", 0, false, ControlType.TextBox)]
        public string DeviceIP
        {
            get { return deviceIP; }
            set
            {
                deviceIP = value;
                OnPropertyChanged("DeviceIp");
            }
        }
   
        [TaskPane("Port", "PortToolTip", "NetworkConditions", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 65535)]
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

        [TaskPane("ByteAsciiSwitchCaption", "ByteAsciiSwitchCaptionTooltip", "PresentationSettings", 3, false, ControlType.CheckBox)]
        public bool ByteAsciiSwitch
        {
            get { return byteAsciiSwitch; }
            set
            {
                if (value != byteAsciiSwitch)
                {
                    byteAsciiSwitch = value;
                    OnPropertyChanged("ByteAsciiSwitch");
                }
            }
        }

        [TaskPane("Protocol", "ProtocolToolTip", "NetworkConditions", 2, false, ControlType.ComboBox, new[] { "UDP", "TCP" })]
        public int Protocol
        {
            get
            {
                return protocol;
            }
            set
            {
                if (protocol != value)
                {
                    protocol = value;
                    OnPropertyChanged("Protocol");
                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("TryConnectCaption", "TryConnectCaptionToolTip", "TCPIPSettings", 4, false, ControlType.CheckBox)]
        public bool TryConnect
        {
            get { return tryConnect; }
            set
            {
                if (value != tryConnect)
                {
                    tryConnect = value;
                    OnPropertyChanged("TryConnect");
                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("ConnectIntervallCaption", "ConnectIntervallCaptionToolTip", "TCPIPSettings", 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 100, 10000)]
        public int ConnectIntervall
        {
            get
            {
                return connectIntervall;
            }
            set
            {
                if (connectIntervall != value)
                {
                    connectIntervall = value;
                    OnPropertyChanged("ConnectIntervall");
                }
            }
        }

        #endregion

        #region events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

        internal void UpdateTaskPaneVisibility()
        {
            
            if (TaskPaneAttributeChanged == null)
                return;

            

            switch (Protocol)
            {
                case 0:
                    TaskPaneAttribteContainer tba = new TaskPaneAttribteContainer("TryConnect", Visibility.Collapsed);
                    TaskPaneAttribteContainer tbb = new TaskPaneAttribteContainer("ConnectIntervall", Visibility.Collapsed);
                    TaskPaneAttributeChangedEventArgs tbac = new TaskPaneAttributeChangedEventArgs(tba);
                    TaskPaneAttributeChangedEventArgs tbbc = new TaskPaneAttributeChangedEventArgs(tbb);
                    TaskPaneAttributeChanged(this, tbac);
                    TaskPaneAttributeChanged(this, tbbc);
                    break;

                case 1:
                    
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("TryConnect", Visibility.Visible)));
                    if (tryConnect)
                    {
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ConnectIntervall", Visibility.Visible)));
                    }
                    else
                    {
                        TaskPaneAttribteContainer tbaa = new TaskPaneAttribteContainer("ConnectIntervall", Visibility.Collapsed);
                        TaskPaneAttributeChangedEventArgs tbbb = new TaskPaneAttributeChangedEventArgs(tbaa);
                        TaskPaneAttributeChanged(this, tbbb);
                    }
                  //  TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ConnectIntervall", Visibility.Visible)));
                    break;
            }
        }

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
