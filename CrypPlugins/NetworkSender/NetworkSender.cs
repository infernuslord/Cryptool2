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
using System.IO;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Net.Sockets;
using System.Net;
using Cryptool.PluginBase.IO;


namespace Cryptool.Plugins.NetworkSender
{

    [Author("Mirko Sartorius", "mirkosartorius@web.de", "University of Kassel", "")]
    [PluginInfo("NetworkSender.Properties.Resources", "PluginCaption", "PluginToolTip", "NetworkSender/userdoc.xml", new[] { "NetworkSender/Images/package.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class NetworkInput : ICrypComponent
    {
        #region Private Variables
        private readonly NetworkSenderSettings settings;
        private readonly NetworkSenderPresentation presentation;
        private string addr;
        private IPEndPoint endPoint;
        private bool valid = false;
        private IPAddress ip;
        private bool isRunning;
        private CStreamReader streamReader;
        private Socket clientSocket;
        private int packageCount;
        private DateTime startTime;
        private TcpClient tcpClient = new TcpClient();
        private NetworkStream networkStream;
        private MemoryStream memoryStream;

        #endregion

        public NetworkInput()
        {
            presentation = new NetworkSenderPresentation(this);
            settings = new NetworkSenderSettings();
        }

        #region Helper Function

         private string generatePackageSizeString(byte[] data)
         {
             double size = data.Length;
             if (size < 1024)
             {
                 return size + " Byte";
             }
             else
             {
                 return Math.Round((double)(size / 1024.0), 2) + " kByte";
             }
         }

        //check ip adress to be valid
        public bool IsValidIP(string addr)
        {
            this.addr = addr;
            //boolean variable to hold the status
            //check to make sure an ip address was provided
            if (string.IsNullOrEmpty(addr))
            {
                //address wasnt provided so return false
                valid = false;
            }
            else
            {
                //use TryParse to see if this is a
                //valid ip address. TryParse returns a
                //boolean based on the validity of the
                //provided address, so assign that value
                //to our boolean variable
                valid = IPAddress.TryParse(addr, out ip);
            }
            //return the value
            return valid;
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Data to be send inside of a package over a network
        /// </summary>
        [PropertyInfo(Direction.InputData, "StreamInput", "StreamInputTooltip")]
        public ICryptoolStream PackageStream
        {
            get;
            set;
        }

        /// <summary>
        /// DestinationIp 
        /// </summary>
        [PropertyInfo(Direction.InputData, "IpInput", "IpInputTooltip")]
        public string DestinationIp_i
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            ProgressChanged(0, 1);

            if (settings.Protocol == 1)
            {
                
            //    var destinationIP = ("".Equals(DestinationIp_i)) ? settings.DeviceIP : DestinationIp_i;
                tcpClient.Connect(IPAddress.Parse(settings.DeviceIP), settings.Port);
            }

            DestinationIp_i = "";
            //init
            isRunning = true;
            startTime = DateTime.Now;
            packageCount = 0;

            //resets the presentation
            presentation.ClearList();
            presentation.RefreshMetaData(0);
            presentation.SetStaticMetaData(startTime.ToLongTimeString(), settings.Port);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {

            if (settings.Protocol == 0)
            {
                ProgressChanged(1, 100);

                //if DestinationIp_i is empty we use the ip in the settings.
                var destinationIP = ("".Equals(DestinationIp_i)) ? settings.DeviceIP : DestinationIp_i;

                if (IsValidIP(destinationIP))
                {
                    // Init
                    endPoint = new IPEndPoint(IPAddress.Parse(destinationIP), settings.Port);
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    using (streamReader = PackageStream.CreateReader())
                    {
                        var streamBuffer = new byte[65507]; //maximum payload size for udp
                        int bytesRead;
                        while ((bytesRead = streamReader.Read(streamBuffer)) > 0)
                        {
                            var packetData = new byte[bytesRead];
                            for (int i = 0; i < bytesRead; i++)
                            {
                                packetData[i] = streamBuffer[i]; // copy all read data to streamData 
                            }

                            presentation.RefreshMetaData(++packageCount);

                            //updates the presentation
                            presentation.AddPresentationPackage(new PresentationPackage
                            {

                                IPFrom = endPoint.Address.ToString(),
                                Payload = (settings.ByteAsciiSwitch ? Encoding.ASCII.GetString(packetData) : BitConverter.ToString(packetData)),
                                PackageSize = generatePackageSizeString(packetData)
                            });

                            //sends input data
                            clientSocket.SendTo(packetData, endPoint);
                        }
                    }
                }
                else
                {
                    GuiLogMessage("IP ungueltig!", NotificationLevel.Error);
                
                }



                // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
                ProgressChanged(1, 1);

            }
            else if (settings.Protocol == 1)
            {

      
            }

           
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            if (clientSocket != null && settings.Protocol == 0)
            {
                 clientSocket.Close();
            }
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            if (settings.Protocol == 1)
            {
                tcpClient.Close();
            }
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
