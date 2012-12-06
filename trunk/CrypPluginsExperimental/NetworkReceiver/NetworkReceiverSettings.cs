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
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
    
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
      
        #endregion
    }
}
