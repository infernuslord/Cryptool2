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
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Generator;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using PacketDotNet;
using SharpPcap;
using SharpPcap.WinPcap;

namespace Cryptool.Plugins.NetworkCapture
{
    [Author("Matthäus Wander", "wander@cryptool.org", "University of Duisburg-Essen", "http://www.vs.uni-due.de")]
    [PluginInfo(true, "Network Capture", "Capture data from a network device (requires WinPcap library)", null, "CrypWin/images/default.png")]
    public class NetworkCapture : IGeneratorMisc
    {
        #region Private Variables

        private readonly NetworkCaptureSettings settings = new NetworkCaptureSettings();
        private WinPcapDeviceList devices;

        private CStreamWriter writer;

        #endregion

        #region Data Properties

        /// <summary>
        /// Throws OnPropertyChanged one time and then continously delivers a stream of data.
        /// Stream will close when plugin is stopped.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Captured stream", "Data stream that has been captured from selected network device", null)]
        public ICryptoolStream DataStream
        {
            get;
            private set;
        }

        /// <summary>
        /// Throws OnPropertyChanged every time a new packet arrives.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Captured packets", "Data packets that have been captured from selected network device", null)]
        public byte[] DataPacket
        {
            get;
            private set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (devices == null || devices.Count == 0)
            {
                GuiLogMessage("No device available. Is WinPcap installed?", NotificationLevel.Error);
                return;
            }

            ProgressChanged(0, 1);

            WinPcapDevice dev = devices[settings.Device];
            dev.OnPacketArrival += OnPacketArrival;

            writer = new CStreamWriter();

            dev.Open();
            dev.StartCapture();
            OnPropertyChanged("DataStream");

            ProgressChanged(0.5, 1);
        }

        void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            GuiLogMessage("Packet: " + e.ToString(), NotificationLevel.Debug);

            if (writer != null && !writer.IsClosed)
            {
                writer.Write(e.Packet.Data);
            }

            this.DataPacket = e.Packet.Data;
            OnPropertyChanged("DataPacket");
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            if (devices.Count > 0)
            {
                WinPcapDevice dev = devices[settings.Device];
                dev.StopCapture();
                dev.Close();
            }

            if (writer != null && !writer.IsClosed)
            {
                writer.Close();
            }

            ProgressChanged(1, 1);
        }

        public void Initialize()
        {
            try
            {

                this.devices = WinPcapDeviceList.Instance;
                foreach (WinPcapDevice dev in devices)
                {
                    this.settings.Collection.Add(dev.Description);
                }
            }
            catch (PcapException e)
            {
                GuiLogMessage("No device available. Is WinPcap installed?", NotificationLevel.Error);
            }
        }

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
