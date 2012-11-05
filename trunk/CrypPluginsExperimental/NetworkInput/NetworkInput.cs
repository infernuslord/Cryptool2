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
using System.Globalization;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Net.Sockets;
using System.Net;
using Cryptool.PluginBase.IO;
using NetworkInput;
using NetworkInput.Model;


namespace Cryptool.Plugins.NetworkInput
{

    [Author("Mirko Sartorius", "mirkosartorius@web.de", "University of Kassel", "")]
    [PluginInfo("NetworkInput.Properties.Resources", "PluginCaption", "PluginToolTip", "NetworkInput/userdoc.xml", new[] { "NetworkInput/Images/1352124662_ark.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class NetworkInput : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly NetworkInputSettings settings;
        private readonly NetworkInputPresentation presentation = new NetworkInputPresentation();
        private string addr;
        private IPEndPoint endPoint;
        private bool valid = false;
        private IPAddress ip;
        private byte[] packetData;
        private bool isRunning;
        private CStreamReader streamReader;
        private Socket clientSocket;
        private int bytesRead;
        private DateTime startTime;

        #endregion

         public NetworkInput()
        {

            settings = new NetworkInputSettings(this);
        }

        #region Helper Function

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
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "StreamInput", "StreamInputTooltip")]
        public ICryptoolStream PackageStream
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

            //init
            isRunning = true;
            startTime = DateTime.Now;


            //resets the presentation
            presentation.ClearList();
            presentation.RefreshMetaData(0);
            presentation.SetStaticMetaData(startTime.ToLongTimeString());



        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(1, 100);

            if (IsValidIP(settings.DeviceIP))
            {
                // Init
                endPoint = new IPEndPoint(IPAddress.Parse(settings.DeviceIP), settings.Port);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                using (streamReader = PackageStream.CreateReader())
                {

                    packetData = new byte[1024];


                    while ((bytesRead = streamReader.Read(packetData)) > 0)
                    {
                        var input = new List<byte>();
                        //cuts the zeros of the packet data   
                        foreach (var zero in packetData)
                        {
                            if (zero != 0)
                            {
                                input.Add(zero);
                            }
                            else
                            {
                                break;
                            }
                        }

                        //updates the presentation
                        presentation.AddPresentationPackage(new PresentationPackage
                        {
                            IPFrom = endPoint.Address.ToString(),
                            Payload = BitConverter.ToString(input.ToArray())
                        });
                        presentation.RefreshMetaData(input.Count);

                        //sends input data
                        clientSocket.SendTo(input.ToArray(), endPoint);
                    }
                }
            }
            else
            {
                GuiLogMessage("Ungueltige IP!", NotificationLevel.Error);
            }



            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            clientSocket.Close();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
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

        private void GuiLogMessage(string message, NotificationLevel logLevel)
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
