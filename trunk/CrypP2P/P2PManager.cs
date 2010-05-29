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
using System.Windows.Forms;
using Cryptool.P2P.Internal;
using Cryptool.P2P.Worker;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.PeerToPeer.Internal;

namespace Cryptool.P2P
{
    public sealed class P2PManager
    {
        #region Singleton

        public static readonly P2PManager Instance = new P2PManager();

        private P2PManager()
        {
            P2PBase = new P2PBase();
            P2PSettings = new P2PSettings();
            P2PBase.AllowLoggingToMonitor = P2PSettings.Log2Monitor;

            // Register events

            // to forward event from overlay/dht MessageReceived-Event from P2PBase
            P2PBase.OnP2PMessageReceived += p2pBase_OnP2PMessageReceived;

            // Register exit event to terminate P2P connection without loosing data
            // TODO check if this is correct, should be - but handler is not called (and application does not shut down), probably unrelated to this problem
            Application.ApplicationExit += HandleDisconnectByApplicationShutdown;
        }

        #endregion

        # region Constants

        public const string P2PDisconnectImageUri = "images/peer2peer-disconnect.png";
        public const string P2PConnectImageUri = "images/peer2peer-connect.png";

        # endregion

        #region Variables

        public P2PBase P2PBase { get; set; }
        public P2PSettings P2PSettings { get; set; }
        public bool IsP2PConnecting { get; internal set; }

        #endregion

        #region Events

        public static event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        public event P2PBase.P2PMessageReceived OnPeerMessageReceived;

        #endregion Events

        public bool P2PConnected()
        {
            return P2PBase.Started;
        }

        public string UserInfo()
        {
            if (!P2PConnected())
            {
                return null;
            }

            string userName;
            var userInfo = P2PBase.GetPeerID(out userName);
            return userInfo + " (" + userName + ")";
        }

        public void HandleConnectOnStartup()
        {
            if (P2PSettings.ConnectOnStartup && IsReadyToConnect())
            {
                GuiLogMessage("Connect on startup enabled. Establishing connection...", NotificationLevel.Info);
                new ConnectionWorker(P2PBase, P2PSettings).Start();
            }
        }

        private bool IsReadyToConnect()
        {
            if (String.IsNullOrEmpty(P2PSettings.PeerName))
            {
                GuiLogMessage("Peer-to-peer not fully configured: username missing.", NotificationLevel.Error);
                return false;
            }

            if (String.IsNullOrEmpty(P2PSettings.WorldName))
            {
                GuiLogMessage("Peer-to-peer not fully configured: world name missing.", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        public PeerId GetPeerId(out string userName)
        {
            return P2PBase.GetPeerID(out userName);
        }

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        private void p2pBase_OnP2PMessageReceived(PeerId sourceAddr, byte[] data)
        {
            if (OnPeerMessageReceived != null)
                OnPeerMessageReceived(sourceAddr, data);
        }

        #region Framework methods

        private void HandleDisconnectByApplicationShutdown(object sender, EventArgs e)
        {
            if (P2PConnected())
            {
                new ConnectionWorker(P2PBase, P2PSettings).Start();
            }
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }

        #endregion Framework methods

        #region DHT operations

        public static bool Store(string key, byte[] data)
        {
            if (!Instance.P2PConnected())
                throw new NotConnectedException();

            return Instance.P2PBase.SynchStore(key, data);
        }

        public static bool Store(string key, string data)
        {
            if (!Instance.P2PConnected())
                throw new NotConnectedException();
           
            return Instance.P2PBase.SynchStore(key, data);
        }

        public static byte[] Retrieve(string key)
        {
            if (!Instance.P2PConnected())
                throw new NotConnectedException();
            
            return Instance.P2PBase.SynchRetrieve(key);
        }

        public static bool Remove(string key)
        {
            if (!Instance.P2PConnected())
                throw new NotConnectedException();

            return Instance.P2PBase.SynchRemove(key);
        }

        #endregion DHT operations
    }
}