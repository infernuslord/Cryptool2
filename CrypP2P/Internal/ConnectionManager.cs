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
using System.Threading;
using System.Windows.Threading;
using Cryptool.P2P.Worker;
using Cryptool.PluginBase;
using PeersAtPlay.CertificateLibrary.Certificates;
using System.IO;

namespace Cryptool.P2P.Internal
{
    public class ConnectionManager
    {
        #region Delegates

        public delegate void P2PConnectionStateChangeEventHandler(object sender, bool newState);
        public delegate void P2PTryConnectingStateChangeEventHandler(object sender, bool newState);

        #endregion

        #region Events

        public event P2PConnectionStateChangeEventHandler OnP2PConnectionStateChangeOccurred;
        public event P2PTryConnectingStateChangeEventHandler OnP2PTryConnectingStateChangeOccurred;

        #endregion

        private readonly object connectLock = new object();
        private readonly P2PBase p2PBase;
        private DateTime lastConnectionAttempt;

        public ConnectionManager(P2PBase p2PBase)
        {
            this.p2PBase = p2PBase;
        }

        public bool Disconnected { get; private set; }

        private bool isConnecting;
        public bool IsConnecting { 
            get { return isConnecting; } 
            internal set
            {
                isConnecting = value;
                if (OnP2PTryConnectingStateChangeOccurred != null)
                {
                    OnP2PTryConnectingStateChangeOccurred(this, isConnecting);
                }
            }
        }
        
        public void Connect()
        {
            lock (connectLock)
            {
                Disconnected = false;
                lastConnectionAttempt = DateTime.Now;
                
                if (p2PBase.IsConnected || IsConnecting)
                {
                    P2PManager.GuiLogMessage("Cannot connect, already connected or connecting.",
                                             NotificationLevel.Warning);
                    return;
                }

                if (!IsReadyToConnect())
                {
                    P2PManager.GuiLogMessage("Cannot connect, configuration is broken.", NotificationLevel.Warning);
                    return;
                }

                IsConnecting = true;
            }

            P2PManager.GuiLogMessage("Dispatching connect request with ConnectionWorker.", NotificationLevel.Debug);
            new ConnectionWorker(p2PBase, this).Start();
        }

        public void Disconnect()
        {
            lock (connectLock)
            {
                Disconnected = true;
                if (!p2PBase.IsConnected || IsConnecting)
                {
                    P2PManager.GuiLogMessage("Cannot disconnect, no connection or connection attempt active.",
                                             NotificationLevel.Warning);
                    return;
                }

                IsConnecting = true;
            }

            P2PManager.GuiLogMessage("Dispatching disconnect request with ConnectionWorker.", NotificationLevel.Debug);
            new ConnectionWorker(p2PBase, this).Start();
        }

        public bool IsReadyToConnect()
        {
            if (String.IsNullOrEmpty(P2PSettings.Default.PeerName))
            {
                P2PManager.GuiLogMessage("Peer-to-peer not fully configured: username missing.", NotificationLevel.Info);
                return false;
            }

            string password = null;
            if (P2PSettings.Default.RememberPassword)
            {
                password = P2PBase.DecryptString(P2PSettings.Default.Password);
            }
            else
            {
                password = P2PBase.DecryptString(P2PBase.Password);
            }

            if (String.IsNullOrEmpty(password))
            {
                P2PManager.GuiLogMessage("Peer-to-peer not fully configured: password missing.",
                                         NotificationLevel.Info);
                return false;
            }

            if (String.IsNullOrEmpty(P2PSettings.Default.WorldName))
            {
                P2PManager.GuiLogMessage("Peer-to-peer not fully configured: world name missing.",
                                         NotificationLevel.Info);
                return false;
            }

            return true;
        }

        public void FireConnectionStatusChange()
        {
            if (OnP2PConnectionStateChangeOccurred != null)
            {
                OnP2PConnectionStateChangeOccurred(this, p2PBase.IsConnected);
            }
        }
    }
}