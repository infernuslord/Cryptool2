/*
   Copyright 2010 Paul Lelgemann, University of Duisburg-Essen

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
using System.Text;
using System.ComponentModel;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Forms;
using Cryptool.P2P.Worker;
using DevComponents.WpfRibbon;
using Cryptool.P2P.Helper;
using Cryptool.P2P.Internal;

namespace Cryptool.P2P
{
    public sealed class P2PManager
    {
        #region Singleton
        static readonly P2PManager INSTANCE = new P2PManager();
        private P2PManager() { }

        public static P2PManager Instance
        {
            get
            {
                return INSTANCE;
            }
        }
        #endregion

        # region Constants
        public const string P2PDisconnectImageURI = "images/peer2peer-disconnect.png";
        public const string P2PConnectImageURI = "images/peer2peer-connect.png";
        # endregion

        #region Private variables
        private P2PBase P2PBase { get; set; }
        private P2PSettings P2PSettings { get; set; }
        private ButtonDropDown P2PButton { get; set; }
        #endregion

        #region Events
        public static event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        public event P2PBase.P2PMessageReceived OnPeerMessageReceived;
        #endregion Events

        /// <summary>
        /// Initialises variables and important environment settings 
        /// regarding certificates.
        /// </summary>
        /// <param name="p2pButton">Button that can change the connection state 
        /// and displays it by showing different images.</param>
        public void Initialize(ButtonDropDown p2pButton)
        {
            this.P2PButton = p2pButton;
            this.P2PBase = new P2PBase();
            this.P2PSettings = new P2PSettings();
            this.P2PBase.AllowLoggingToMonitor = this.P2PSettings.Log2Monitor;

            // Validate certificats
            if (!PAPCertificate.CheckAndInstallPAPCertificates())
            {
                GuiLogMessage("Certificates not validated, P2P might not be working!", NotificationLevel.Error);
                return;
            }

            // Register events

            // to forward event from overlay/dht MessageReceived-Event from P2PBase
            this.P2PBase.OnP2PMessageReceived += new P2PBase.P2PMessageReceived(p2pBase_OnP2PMessageReceived);

            // Register exit event to terminate P2P connection without loosing data
            // TODO check if this is correct, should be - but handler is not called (and application does not shut down), probably unrelated to this problem
            Application.ApplicationExit += new EventHandler(HandleDisconnectByApplicationShutdown);
        }

        /// <summary>
        /// Changes the current connection state to the P2P network. 
        /// If there is currently no connection, it will try to connect.
        /// If a connection is present, it will disconnect.
        /// The actual work will be done asynchronous.
        /// </summary>
        public void ToggleConnectionState()
        {
            new ConnectionWorker(P2PBase, P2PSettings, P2PButton).Start();
        }

        public bool P2PConnected()
        {
            return P2PBase.Started;
        }

        #region DHT operations
        // TODO add error handling, if P2P if not connected
        public static bool Store(string key, byte[] data)
        {
            return INSTANCE.P2PBase.SynchStore(key, data);
        }

        public static byte[] Retrieve(string key)
        {
            return INSTANCE.P2PBase.SynchRetrieve(key);
        }

        public static bool Remove(string key)
        {
            return INSTANCE.P2PBase.SynchRemove(key);
        }
        #endregion DHT operations

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        private void p2pBase_OnP2PMessageReceived(PeerId sourceAddr, byte[] data)
        {
            if (OnPeerMessageReceived != null)
                OnPeerMessageReceived(sourceAddr, data);
        }

        #region Framework methods
        void HandleDisconnectByApplicationShutdown(object sender, EventArgs e)
        {
            if (P2PConnected())
            {
                new ConnectionWorker(P2PBase, P2PSettings, P2PButton).Start();
            }
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }
        #endregion Framework methods
    }
}
