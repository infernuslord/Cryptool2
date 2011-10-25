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
using System.Timers;
using Cryptool.P2P.Interfaces;
using Cryptool.P2P.Types;
using Cryptool.P2PDLL.Helper;
using Cryptool.P2PDLL.Internal;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using PeersAtPlay.P2PStorage.DHT;
using PeersAtPlay.Util.Threading;
using System.IO;

namespace Cryptool.P2PDLL
{
    public sealed class P2PManager
    {
        #region Variables

        public static ConnectionManager ConnectionManager { get; private set; }
        public static P2PBase P2PBase { get; private set; }
        public static bool IsAutoconnectConsoleOptionSet { get; set; }
        public static UInt64 NetSize { get; private set; }

        private static UInt64 NetSize_timestamp;
        private static Timer SENSTimer;

        #endregion

        #region Events

        public static event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        #endregion Events


        #region Singleton

        public static readonly P2PManager Instance = new P2PManager();

        private P2PManager()
        {
            P2PBase = new P2PBase();
            P2PBase.OnP2PMessageReceived += P2PBase_OnP2PMessageReceived;
            NetSize = 0;
            NetSize_timestamp = 0;
            P2P.P2PSettings.Default.NetSize = "-- offline --";

            Random rnd = new Random();
            SENSTimer = new Timer(rnd.Next(60,120)*1000); //initially 1-2 minutes
            SENSTimer.AutoReset = true;
            SENSTimer.Elapsed += SENSTimerElapsed;

            ConnectionManager = new ConnectionManager(P2PBase);
            ConnectionManager.OnP2PConnectionStateChangeOccurred += ConnectionManager_OnP2PConnectionStateChangeOccurred;

            SettingsHelper.ValidateSettings();
        }

        #endregion

        #region Peer counting methods


        private static void ConnectionManager_OnP2PConnectionStateChangeOccurred(object sender, bool newState)
        {
            if (newState)
            {
                if (P2P.P2PSettings.Default.Architecture == P2PArchitecture.FullMesh)
                {
                    SENSTimer.Start();
                    P2P.P2PSettings.Default.NetSize = "Estimating online peers..";    
                }
                else
                {
                    // SENS currently works only with FullMesh... 
                    P2P.P2PSettings.Default.NetSize = "-- online --"; 
                }
                
            }
            else
            {
                SENSTimer.Stop();
                P2P.P2PSettings.Default.NetSize = "-- offline --";
            }
        }

        private static void SENSTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            InitiateSENS();
        }

        private static void InitiateSENS()
        {
            string username;
            PeerId pid = P2PBase.GetPeerId(out username);

            GuiLogMessage(String.Format("Initiating SENS Protocol ... (MyID={0})",pid), NotificationLevel.Info);

            byte[] dst = incrementAdr(pid.ToByteArray());

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);

            binaryWriter.Write(new byte[2] { 0xAF, 0xFE }); // write some header to distinguish from other packets - should be extended by a checksum
            binaryWriter.Write(pid.ToByteArray());          // write initiator (unchanged)
            binaryWriter.Write(NetSize);                    // Write 8 Byte netSize
            binaryWriter.Write((ulong)1);                   // Write 8 Byte counter
            binaryWriter.Write(NetSize_timestamp);          // Write the 8-Byte logical timestamp from last collect of Netsize

            P2PBase.SendToPeer(memoryStream.ToArray(), dst);
        }


        private static void P2PBase_OnP2PMessageReceived(PeerId sourceAddr, byte[] data)
        {
            GuiLogMessage("P2P Message received from " + sourceAddr, NotificationLevel.Debug);

            if (data.Length > 2 && data[0] == 0xAF && data[1] == 0xFE)
            {
                // Correct header - so hopefully it is a counting message (this should be checked with an additional hash
                // Header: 2B, Initiator: 20Byte, NetSize (ulong): 8 Byte, NodeCounter (ulong) 8 Byte, Timestamp (ulong) 8 Byte
                var binaryReader = new BinaryReader(new MemoryStream(data));
                binaryReader.ReadBytes(2); //skip the header
                PeerId initiator = P2PBase.GetPeerId(binaryReader.ReadBytes(20));
                var netSize = binaryReader.ReadUInt64();
                var nodeCounter = binaryReader.ReadUInt64();
                var timestamp = binaryReader.ReadUInt64();

                //check if the message is from us
                string username;
                PeerId myPeerID = P2PBase.GetPeerId(out username);

                Random rnd = new Random();

                if (myPeerID.ToString() == initiator.ToString())
                {
                    GuiLogMessage("Received SENS message from ourselves: NetSize=" + netSize + "; NodeCounter=" + nodeCounter + "; Timestamp=" + timestamp, NotificationLevel.Debug);
                    SENSTimer.Interval = rnd.Next(300, 600)*1000; // timer reset
                    NetSize = nodeCounter;
                    NetSize_timestamp = timestamp + 1;
                    P2P.P2PSettings.Default.NetSize = NetSize + " peers online";
                    GuiLogMessage("SENS finished with a new estimation of " + NetSize + " peer(s) online.", NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Received SENS message from " + initiator + ": NetSize=" + netSize + "; NodeCounter=" + nodeCounter + "; Timestamp=" + timestamp, NotificationLevel.Debug);
                    if ((netSize > 0) && (timestamp > NetSize_timestamp))
                    {
                        SENSTimer.Interval = rnd.Next(300, 600) * 1000; // timer reset
                        NetSize = netSize;
                        NetSize_timestamp = timestamp;
                        P2P.P2PSettings.Default.NetSize = NetSize + " peers online";
                        GuiLogMessage("Found updated estimation of " + NetSize + " peer(s) online.",NotificationLevel.Info);
                    }
                    else
                    {
                        netSize = NetSize;
                        timestamp = NetSize_timestamp;
                    }
                    nodeCounter++;

                    // finally send to next node..
                    byte[] dst = incrementAdr(myPeerID.ToByteArray());

                    var memoryStream = new MemoryStream();
                    var binaryWriter = new BinaryWriter(memoryStream);

                    binaryWriter.Write(new byte[2]{0xAF, 0xFE}); // write header
                    binaryWriter.Write(initiator.ToByteArray()); // wirte initiator (unchanged)
                    binaryWriter.Write(netSize);                 // Write 8 Byte netSize
                    binaryWriter.Write(nodeCounter);             // Write 8 Byte counter
                    binaryWriter.Write(timestamp);               // Write 8 Byte time information

                    P2PBase.SendToPeer(memoryStream.ToArray(), dst);

                }
            }

        }

        /// <summary>
        /// This is one reason, why the entire SENSE protocol should be integrated in the overlay
        /// incrementing the address is probably overlay-address specific.. 
        /// </summary>
        /// <param name="adr"></param>
        /// <returns></returns>
        private static byte[] incrementAdr(byte[] adr)
        {
            byte[] result = (byte[])adr.Clone();

            for (int i = (result.Length-1); i >= 0; i--)
            {
                result[i]++;
                if (result[i] != 0)
                    return result;
            }

            return result;
        }


        #endregion


        #region CrypWin helper methods

        public static void HandleConnectOnStartup()
        {
            var isAutoconnectConfiguredOrRequested = P2P.P2PSettings.Default.ConnectOnStartup || IsAutoconnectConsoleOptionSet;
            var isReadyToConnect = ConnectionManager.IsReadyToConnect();

            if (isAutoconnectConfiguredOrRequested && isReadyToConnect)
            {
                GuiLogMessage("Connect on startup enabled. Establishing connection...", NotificationLevel.Info);
                ConnectionManager.Connect();
            }
        }

        public static void HandleDisconnectOnShutdown()
        {
            Disconnect();
        }

        public static IRequestResult GetSuccessfullRequestResult()
        {
            return new RequestResult {Status = RequestResultType.Success};
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
        public static IRequestResult Store(string key, byte[] data)
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
        public static IRequestResult Store(string key, string data)
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
        public static IRequestResult Retrieve(string key)
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
        public static IRequestResult Remove(string key)
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