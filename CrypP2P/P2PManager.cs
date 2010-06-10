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
using Cryptool.P2P.Helper;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
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
            ConnectionManager = new ConnectionManager(P2PBase);

            SettingsHelper.ValidateSettings();
        }

        #endregion

        #region Variables

        public static ConnectionManager ConnectionManager { get; private set; }
        public static P2PBase P2PBase { get; private set; }
        public static bool IsAutoconnectConsoleOptionSet { get; set; }

        #endregion

        #region Events

        public static event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        #endregion Events

        #region CrypWin helper methods

        public static void HandleConnectOnStartup()
        {
            var isAutoconnectConfiguredOrRequested = P2PSettings.Default.ConnectOnStartup || IsAutoconnectConsoleOptionSet;
            var isReadyToConnect = ConnectionManager.IsReadyToConnect();

            if (isReadyToConnect && isAutoconnectConfiguredOrRequested)
            {
                GuiLogMessage("Connect on startup enabled. Establishing connection...", NotificationLevel.Info);
                ConnectionManager.Connect();
            }
        }

        public static void HandleDisconnectOnShutdown()
        {
            Disconnect();
        }

        #endregion

        #region Framework methods

        public static void GuiLogMessage(string message, NotificationLevel logLevel)
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
            if (!IsConnected)
                throw new NotConnectedException();

            return P2PBase.SynchStore(key, data);
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
            if (!IsConnected)
                throw new NotConnectedException();
           
            return P2PBase.SynchStore(key, data);
        }

        /// <summary>
        /// Retrieves the latest version of a given in key from the DHT. This method will block until a response has been received.
        /// </summary>
        /// <param name="key">key to retrieve</param>
        /// <exception cref="NotConnectedException">Will be thrown if the P2P system is not connected</exception>
        /// <returns>byte array containing the data</returns>
        public static byte[] Retrieve(string key)
        {
            if (!IsConnected)
                throw new NotConnectedException();
            
            return P2PBase.SynchRetrieve(key);
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
            if (!IsConnected)
                throw new NotConnectedException();

            return P2PBase.SynchRemove(key);
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
            if (!IsConnected)
                throw new NotConnectedException();

            return P2PBase.VersionedDht.Store(callback, key, data, asyncState);
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
            if (!IsConnected)
                throw new NotConnectedException();

            return P2PBase.Dht.Retrieve(callback, key, asyncState);
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
            if (!IsConnected)
                throw new NotConnectedException();

            return P2PBase.VersionedDht.Remove(callback, key, asyncState);
        }

        #endregion DHT operations (non-blocking)

        #region Connection methods

        /// <summary>
        /// Connect to the peer-to-peer network.
        /// <see cref="ConnectionManager">OnP2PConnectionStateChangeOccurred will be fired when the connection state changes.</see>
        /// </summary>
        public static void Connect()
        {
            ConnectionManager.Connect();
        }

        /// <summary>
        /// Disconnect from the peer-to-peer network.
        /// <see cref="ConnectionManager">OnP2PConnectionStateChangeOccurred will be fired when the connection state changes.</see>
        /// </summary>
        public static void Disconnect()
        {
            ConnectionManager.Disconnect();
        }

        /// <summary>
        /// Boolean which indicates, if the peer-to-peer network is currently connected and not in a connect/disconnect attempt.
        /// </summary>
        public static bool IsConnected
        {
            get { return P2PBase.IsConnected && !IsConnecting; }
        }

        /// <summary>
        /// Boolean which indicates, if the peer-to-peer network is currently in a connect/disconnect attempt.
        /// </summary>
        public static bool IsConnecting
        {
            get { return ConnectionManager.IsConnecting; }
        }

        #endregion
    }
}