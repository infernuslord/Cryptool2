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
using PeersAtPlay.P2PStorage.DHT;
using PeersAtPlay.Util.Threading;

namespace Cryptool.P2P
{
    public sealed class P2PManager
    {
        #region Singleton

        public static readonly P2PManager Instance = new P2PManager();

        private P2PManager()
        {
            P2PBase = new P2PBase();

            // to forward event from overlay/dht MessageReceived-Event from P2PBase
            P2PBase.OnP2PMessageReceived += OnP2PMessageReceived;
        }

        #endregion

        #region Variables

        public P2PBase P2PBase { get; private set; }
        public bool IsP2PConnecting { get; internal set; }

        #endregion

        #region Events

        public static event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public delegate void P2PConnectionStateChangeEventHandler(object sender, bool newState);
        public static event P2PConnectionStateChangeEventHandler OnP2PConnectionStateChangeOccurred;

        public event P2PBase.P2PMessageReceived OnPeerMessageReceived;

        #endregion Events

        internal void FireConnectionStatusChange()
        {
            if (OnP2PConnectionStateChangeOccurred != null)
            {
                OnP2PConnectionStateChangeOccurred(this, IsP2PConnected());
            }
        }

        public bool IsP2PConnected()
        {
            return P2PBase.Started;
        }

        public string UserInfo()
        {
            if (!IsP2PConnected())
            {
                return null;
            }

            string userName;
            var userInfo = P2PBase.GetPeerId(out userName);
            return userInfo + " (" + userName + ")";
        }

        public void HandleConnectOnStartup()
        {
            if (P2PSettings.Default.ConnectOnStartup && IsReadyToConnect())
            {
                GuiLogMessage("Connect on startup enabled. Establishing connection...", NotificationLevel.Info);
                new ConnectionWorker(P2PBase).Start();
            }
        }

        private bool IsReadyToConnect()
        {
            if (String.IsNullOrEmpty(P2PSettings.Default.PeerName))
            {
                GuiLogMessage("Peer-to-peer not fully configured: username missing.", NotificationLevel.Error);
                return false;
            }

            if (String.IsNullOrEmpty(P2PSettings.Default.PeerName))
            {
                GuiLogMessage("Peer-to-peer not fully configured: world name missing.", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        public PeerId GetPeerId(out string userName)
        {
            return P2PBase.GetPeerId(out userName);
        }

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        private void OnP2PMessageReceived(PeerId sourceAddr, byte[] data)
        {
            if (OnPeerMessageReceived != null)
                OnPeerMessageReceived(sourceAddr, data);
        }

        #region Framework methods

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
        }

        #endregion Framework methods

        #region DHT operations (blocking)

        /// <summary>
        /// Stores the given data in the DHT. This method will block until a response has been received.
        /// 
        /// The underlying DHT is versionend. Store attempts will fail, if the latest version has not been retrieved before.
        /// </summary>
        /// <param name="key">key to write</param>
        /// <param name="data">data to write</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>true if the store attempt was successful, false otherwise</returns>
        public static bool Store(string key, byte[] data)
        {
            if (!Instance.IsP2PConnected())
                throw new NotConnectedException();

            return Instance.P2PBase.SynchStore(key, data);
        }

        /// <summary>
        /// Stores the given data in the DHT. This method will block until a response has been received.
        /// 
        /// The underlying DHT is versionend. Store attempts will fail, if the latest version has not been retrieved before.
        /// </summary>
        /// <param name="key">key to write</param>
        /// <param name="data">data to write</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>true if the store attempt was successful, false otherwise</returns>
        public static bool Store(string key, string data)
        {
            if (!Instance.IsP2PConnected())
                throw new NotConnectedException();
           
            return Instance.P2PBase.SynchStore(key, data);
        }

        /// <summary>
        /// Retrieves the latest version of a given in key from the DHT. This method will block until a response has been received.
        /// </summary>
        /// <param name="key">key to retrieve</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>byte array containing the data</returns>
        public static byte[] Retrieve(string key)
        {
            if (!Instance.IsP2PConnected())
                throw new NotConnectedException();
            
            return Instance.P2PBase.SynchRetrieve(key);
        }

        /// <summary>
        /// Removes a key and its data from the DHT. This method will block until a response has been received.
        /// 
        /// The underlying DHT is versionend. Remove attempts will fail, if the latest version has not been retrieved before.
        /// </summary>
        /// <param name="key">key to remove</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>bool determining wether the attempt was successful</returns>
        public static bool Remove(string key)
        {
            if (!Instance.IsP2PConnected())
                throw new NotConnectedException();

            return Instance.P2PBase.SynchRemove(key);
        }

        #endregion DHT operations (blocking)

        #region DHT operations (non-blocking)

        /// <summary>
        /// Stores the given data in the DHT.
        /// 
        /// The underlying DHT is versionend. Store attempts will fail, if the latest version has not been retrieved before.
        /// </summary>
        /// <param name="callback">Callback for asynchronous call</param>
        /// <param name="key">key to write</param>
        /// <param name="data">data to write</param>
        /// <param name="asyncState">Arbitrary data, which will be included in the callback parameter</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>Guid identifying the request</returns>
        public static Guid Store(AsyncCallback<StoreResult> callback, string key, byte[] data, object asyncState)
        {
            if (!Instance.IsP2PConnected())
                throw new NotConnectedException();

            return Instance.P2PBase.VersionedDht.Store(callback, key, data, asyncState);
        }

        /// <summary>
        /// Retrieves the latest version of a given in key from the DHT.
        /// </summary>
        /// <param name="callback">Callback for asynchronous call</param>
        /// <param name="key">key to retrieve</param>
        /// <param name="asyncState">Arbitrary data, which will be included in the callback parameter</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>Guid identifying the request</returns>
        public static Guid Retrieve(AsyncCallback<RetrieveResult> callback, string key, object asyncState)
        {
            if (!Instance.IsP2PConnected())
                throw new NotConnectedException();

            return Instance.P2PBase.Dht.Retrieve(callback, key, asyncState);
        }

        /// <summary>
        /// Removes a key and its data from the DHT.
        /// 
        /// The underlying DHT is versionend. Remove attempts will fail, if the latest version has not been retrieved before.
        /// </summary>
        /// <param name="callback">Callback for asynchronous call</param>
        /// <param name="key">key to remove</param>
        /// <param name="asyncState">Arbitrary data, which will be included in the callback parameter</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>Guid identifying the request</returns>
        public static Guid Remove(AsyncCallback<RemoveResult> callback, string key, object asyncState)
        {
            if (!Instance.IsP2PConnected())
                throw new NotConnectedException();

            return Instance.P2PBase.VersionedDht.Remove(callback, key, asyncState);
        }

        #endregion DHT operations (non-blocking)

        public void HandleShutdown()
        {
            if (IsP2PConnected())
            {
                new ConnectionWorker(P2PBase).Start();
            }
        }
    }

    
}