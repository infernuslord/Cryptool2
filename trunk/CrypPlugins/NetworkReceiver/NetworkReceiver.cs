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

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System;
using System.Net.Sockets;
using System.Net;
using Cryptool.PluginBase.IO;
using NetworkReceiver;
using Timer = System.Timers.Timer;

namespace Cryptool.Plugins.NetworkReceiver
{
    [Author("Christopher Konze", "Christopher.Konze@cryptool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/")]
    [PluginInfo("NetworkReceiver.Properties.Resources", "PluginCaption", "PluginTooltip", "NetworkReceiver/userdoc.xml", new[] { "NetworkReceiver/Images/package.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class NetworkReceiver : ICrypComponent
    {
        private const int UpdateReceivingrate = 1;
        #region Private variables

        private readonly NetworkReceiverSettings settings;
        private readonly NetworkReceiverPresentation presentation;
        private Timer calculateSpeedrate = null;

        private IPEndPoint endPoint;

        private HashSet<string> uniqueSrcIps;
        private List<byte[]> lastPackages;
        private int receivedPackagesCount;
        private bool isRunning;
        private bool returnLastPackage = true;

        private DateTime startTime;
        private UdpClient udpSocked;
        private CStreamWriter streamWriter = new CStreamWriter();

        private Socket serverSocket;
        private Socket clientSocket;
        private List<Socket> clientSocketList;
        private bool shutdown;

        public int ReceivedDataSize
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set;
        }


        #endregion

        public NetworkReceiver()
        {
            presentation = new NetworkReceiverPresentation(this);
            settings = new NetworkReceiverSettings(this);
        }

        #region Helper Functions

        //Functions for async TCP Server

        private void AcceptCallback(IAsyncResult ar)
        {
            if (!shutdown)
            {
                try
                {
                    if (settings.NumberOfClients == 0)
                    {
                        clientSocket = serverSocket.EndAccept(ar);
                        clientSocketList.Add(clientSocket);
                        presentation.UpdatePresentationClientCount(clientSocketList.Count);
                        StateObject sO = new StateObject();
                        sO.workSocket = clientSocket;  // create new state for each client connection
                        clientSocket.BeginReceive(sO.DataToReceive, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), sO);
                        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                    }
                    else if(settings.NumberOfClients != 0)
                    {
                        if (clientSocketList.Count < settings.NumberOfClients)
                        {
                            clientSocket = serverSocket.EndAccept(ar);
                            clientSocketList.Add(clientSocket);
                            presentation.UpdatePresentationClientCount(clientSocketList.Count);
                            StateObject sO = new StateObject();
                            sO.workSocket = clientSocket;  // create new state for each client connection
                            clientSocket.BeginReceive(sO.DataToReceive, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), sO);
                            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                        }
                        else
                        {
                            GuiLogMessage("Client couldn't connect, the adjusted limit of "+settings.NumberOfClients+" is already reached!", NotificationLevel.Info);
                            return;
                        }
                    }
                    

                }
                catch (Exception e)
                {
                    GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }

        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!shutdown && IsTimeLeft() && IsPackageLimitNotReached())
            {
                try
                {
                    StateObject state = (StateObject)ar.AsyncState;
                    Socket current = state.workSocket;
                    string ipFrom = null;
                    int received = 0;
                    ipFrom = ((IPEndPoint)current.LocalEndPoint).Address.ToString();

                    received = current.EndReceive(ar); //stop receiving
                    byte[] bufferData = new byte[received];

                    if (received == 0)
                    {
                        current.Shutdown(SocketShutdown.Both);
                        current.Close();
                        clientSocketList.Remove(current);
                        presentation.UpdatePresentationClientCount(clientSocketList.Count);
                        return;
                    }
                    
                    Array.Copy(state.DataToReceive, bufferData, received); // copy received data into buffer

                    WriteDateToPresentation(bufferData, ipFrom); // write received data into presentation
                    current.BeginReceive(state.DataToReceive, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state); // start receiving

                }
                catch (Exception e)
                {
                    GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }

        }

        private void WriteDateToPresentation(byte[] data, string ipFrom)
        {
            if (data != null && ipFrom != null) // wont be "flase" anyway, but you never know
            {
                receivedPackagesCount++;
                // package recieved. fill local storage
                if (lastPackages.Count > NetworkReceiverPresentation.MaxStoredPackage)
                {
                    lastPackages.RemoveAt(lastPackages.Count - 1);
                }
                else
                {
                    lastPackages.Add(data);
                }

                uniqueSrcIps.Add(ipFrom);
                ReceivedDataSize += data.Length;


                // create Package
                var length = data.Length % 100;
                var packet = new PresentationPackage
                {
                    PackageSize = generateSizeString(data.Length) + "yte", // 42B + "yte"
                    IPFrom = ipFrom,                    
                    Payload = (settings.ByteAsciiSwitch
                         ? Encoding.ASCII.GetString(data, 0, length)
                         : BitConverter.ToString(data, 0, length))
                };

                // update Presentation
                if (settings.Protocol == 0)
                {
                    presentation.UpdatePresentation(packet, receivedPackagesCount, uniqueSrcIps.Count);
                }
                else if (settings.Protocol == 1)
                {
                    presentation.UpdatePresentation(packet, receivedPackagesCount, clientSocketList.Count);
                }

                //update output
                streamWriter = new CStreamWriter();
                PackageStream = streamWriter;
                streamWriter.Write(data);
                streamWriter.Close();
                OnPropertyChanged("PackageStream");
                if (returnLastPackage) //change single output if no item is selected
                {
                    SingleOutput = data;
                    OnPropertyChanged("SingleOutput");
                }
            }
        }


        /// <summary>
        /// the maximum left time in ms till we abroche the user's whised timeout time 
        /// </summary>
        /// <returns></returns>
        private int TimeTillTimelimit()
        {
            int timeTillTimeout = settings.Timeout - DateTime.Now.Subtract(startTime).Seconds;
            return (timeTillTimeout <= 0) ? 0 : timeTillTimeout * 1000;
        }



        /// <summary>
        /// decides on the basis of the given timeout, whether the component continues to wait for more packages
        /// remember timeout = 0 means: dont time out
        /// </summary>
        /// <returns></returns>
        private bool IsTimeLeft()
        {
            return (settings.Timeout > DateTime.Now.Subtract(startTime).Seconds || settings.Timeout == 0);
        }


        /// <summary>
        /// decides on basis of user's input whether the component is allowed to receiving another package
        /// remember PackageLimit  = 0 means: dont limit the amount of packages
        /// </summary>
        /// <returns></returns>
        private bool IsPackageLimitNotReached()
        {
            return (settings.PackageLimit < receivedPackagesCount || settings.PackageLimit == 0);
        }


        /// <summary>
        /// creates a size string corespornsing to the size of the given amount of bytes with a B or kB ending
        /// </summary>
        /// <returns></returns>
        private string generateSizeString(int size)
        {
            if (size < 1024)
            {
                return size + " B";
            }
            else
            {
                return Math.Round((double)(size / 1024.0), 2) + " kB";
            }
        }

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.OutputData, "StreamOutput", "StreamOutputTooltip")]
        public ICryptoolStream PackageStream
        {
            get;
            private set;
        }


        [PropertyInfo(Direction.OutputData, "SingleOutput", "SingleOutputTooltip")]
        public byte[] SingleOutput
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

            if (settings.Protocol == 0)
            {
                endPoint = new IPEndPoint(!settings.NetworkDevice ? IPAddress.Parse(settings.DeviceIp) : IPAddress.Any, 0);
                udpSocked = new UdpClient(settings.Port);
            }
            else if (settings.Protocol == 1)
            {
                try
                {
                    shutdown = false;
             //       endPoint = new IPEndPoint(IPAddress.Any, settings.Port);
                    endPoint = new IPEndPoint(!settings.NetworkDevice ? IPAddress.Parse(settings.DeviceIp) : IPAddress.Any, settings.Port);
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    serverSocket.Bind(endPoint);
                    serverSocket.Listen(100); //maybe 0 backlog
                    clientSocketList = new List<Socket>();
                 //   serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                    //      allDone = new ManualResetEvent(false);

                }
                catch (Exception e)
                {
                  //  GuiLogMessage(e.Message, NotificationLevel.Error);
                    GuiLogMessage("verbindung fehlgeschlagen, Receiver", NotificationLevel.Error);
                }


            }


            //init / reset
            uniqueSrcIps = new HashSet<string>();
            isRunning = true;
            returnLastPackage = true;
            startTime = DateTime.Now;
            lastPackages = new List<byte[]>();
            receivedPackagesCount = 0;

            // reset gui
            presentation.ClearPresentation();
            presentation.SetStaticMetaData(startTime.ToLongTimeString(), settings.Port.ToString(CultureInfo.InvariantCulture));

            //start speedrate calculator
            calculateSpeedrate = new System.Timers.Timer { Interval = UpdateReceivingrate * 1000 }; // seconds
            calculateSpeedrate.Elapsed += new ElapsedEventHandler(CalculateSpeedrateTick);
            calculateSpeedrate.Start();
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {

            while (IsTimeLeft() && IsPackageLimitNotReached() && isRunning)
            {

                try
                {
                    byte[] data = null;
                    string ipFrom = null;
                    if (settings.Protocol == 0) // UDP receiver
                    {
                        // wait for package
                        ProgressChanged(1, 1);
                        udpSocked.Client.ReceiveTimeout = TimeTillTimelimit();
                        data = udpSocked.Receive(ref endPoint);
                        ProgressChanged(0.5, 1);
                        ipFrom = endPoint.Address.ToString();

                        WriteDateToPresentation(data, ipFrom);

                    }
                    else if (settings.Protocol == 1 && !shutdown) // TCP receiver
                    {

                        serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                        Thread.Sleep(100);

                    }


                    ProgressChanged(0.5, 1);
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode != 10004 || isRunning) // if we stop during the socket waits,if we stop 
                    {                             //during the socket waits, we won't show an error message
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            if (settings.Protocol == 0)
            {
                udpSocked.Close();

            }

            if (settings.Protocol == 1)
            {
                shutdown = true;
            }



            if (settings.Protocol == 1 && serverSocket != null)
            {
                serverSocket.Close();
            }

            if (settings.Protocol == 1 && clientSocket != null && clientSocket.Connected)
            {
                foreach (Socket client in clientSocketList)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Disconnect(true);
                    client.Close();
                }
            }

            if (streamWriter != null)
            {
                streamWriter.Close();
            }

        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {


            calculateSpeedrate.Stop();
            isRunning = false;
            if (settings.Protocol == 0)
            {
                udpSocked.Close(); //we have to close it forcely, but we catch the error
            }

        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            presentation.ListView.MouseDoubleClick -= MouseDoubleClick;
            presentation.ListView.MouseDoubleClick += MouseDoubleClick;
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
            presentation.ListView.MouseDoubleClick -= MouseDoubleClick;
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

        private void MouseDoubleClick(object sender, EventArgs e)
        {
            if (-1 < presentation.ListView.SelectedIndex
                && presentation.ListView.SelectedIndex < NetworkReceiverPresentation.MaxStoredPackage)
            {
                returnLastPackage = false;
                SingleOutput = lastPackages[presentation.ListView.SelectedIndex];
                OnPropertyChanged("SingleOutput");
            }
            else
            {
                returnLastPackage = true;
            }
        }

        /// <summary>
        /// tickmethod for the CalculateSpeedrateTick timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateSpeedrateTick(object sender, EventArgs e)
        {
            var speedrate = ReceivedDataSize / UpdateReceivingrate;
            presentation.UpdateSpeedrate(generateSizeString(speedrate) + "/s"); // 42kb +"/s"
            ReceivedDataSize = 0;
        }

        #endregion
    }
}
