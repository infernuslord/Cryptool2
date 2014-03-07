/*
   Copyright 2013 Christopher Konze, University of Kassel

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
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.NetworkReceiver
{
    public class NetworkReceiverSettings : ISettings
    {
        #region Private Variables

        private int port;
        private int timeout;
        private int packageLimit;
        private string deviceIp;
        private bool networkDevice = true;
        private readonly NetworkReceiver caller;
        private bool byteAsciiSwitch;
        private int protocol;
        private int numberOfClients;
        private int speedrateIntervall;

        public NetworkReceiverSettings(NetworkReceiver caller)
        {
            this.caller = caller;
            NetworkDevice = true;
        }

        #endregion

        #region TaskPane Settings
        
        [TaskPane("DeviceIpCaption", "DeviceIpCaptionTooltip", "NetworkConditions", 0, false, ControlType.TextBox)]
        public string DeviceIp
        {
            get { return deviceIp; }
            set
            {
                deviceIp = value;
                OnPropertyChanged("DeviceIp");
            }
        }
     
        [TaskPane("NetworkDeviceCaption", "NetworkDeviceCaptionTooltip", "NetworkConditions", 1, false, ControlType.CheckBox)]
        public bool NetworkDevice
        {
            get { return networkDevice; }
            set
            {
                if (value != networkDevice)
                {
                    if(!value)
                    {
                        var interfaces = getInterfaceIps();
                        if (interfaces.Contains(DeviceIp))
                        {
                            networkDevice = false; 
                        }
                        else
                        {
                            caller.GuiLogMessage("Interface IP not Available", NotificationLevel.Warning);
                            foreach (var @interface in interfaces)
                            {
                                caller.GuiLogMessage("interface: " + @interface, NotificationLevel.Info);
                            }
                            networkDevice = true;
                        }
                    }
                    else
                    {
                        networkDevice = true;
                    }
                    OnPropertyChanged("NetworkDevice");
                }
            }
        }

        [TaskPane("Port", "PortTooltip", "NetworkConditions", 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 65535)]
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

        [TaskPane("ByteAsciiSwitchCaption", "ByteAsciiSwitchCaptionTooltip", "PresentationSettings" , 3, false, ControlType.CheckBox)]
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
        
        [TaskPane("TimeLimit", "TimeLimitTooltip", "StopConditions", 4, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
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

        [TaskPane("PackageLimit", "PackageLimitTooltip", "StopConditions", 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
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


        [TaskPane("Protocol", "ProtocolTooltip", "NetworkConditions", 3, false, ControlType.ComboBox, new[] { "UDP", "TCP" })]
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

        [TaskPane("NumberOfClientsCaption", "NumberOfClientsTooltip", "TCPServerConditions", 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int NumberOfClients
        {
            get
            {
                return numberOfClients;
            }
            set
            {
                if (numberOfClients != value)
                {
                    numberOfClients = value;
                    OnPropertyChanged("NumberOfClients");
                }
            }
        }
        /// <summary>
        /// returns a list with all available networkinterfaces
        /// </summary>
        /// <returns></returns>
        private List<String> getInterfaceIps()
        {
            var interfaces = new List<string>();
                            
           foreach (var netInf in NetworkInterface.GetAllNetworkInterfaces())
           {
               var a = netInf.GetIPProperties().UnicastAddresses;
               foreach (var i in a)
               {
                   if (i.Address.AddressFamily == AddressFamily.InterNetwork)
                   {
                       interfaces.Add(i.Address.ToString());
                   }
               }
           }
           return interfaces;
        } 
            
        #endregion
    
        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
    
        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
      
        #endregion

        internal void UpdateTaskPaneVisibility()
        {

            if (TaskPaneAttributeChanged == null)
                return;



            switch (Protocol)
            {
                case 0:
                    TaskPaneAttribteContainer tba = new TaskPaneAttribteContainer("NumberOfClients", Visibility.Collapsed);
                    TaskPaneAttributeChangedEventArgs tbac = new TaskPaneAttributeChangedEventArgs(tba);
                    TaskPaneAttributeChanged(this, tbac);
                    break;

                case 1:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("NumberOfClients", Visibility.Visible)));  
                    break;
            }
        }
    }
}
