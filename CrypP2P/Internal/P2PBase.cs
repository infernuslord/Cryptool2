﻿/*
   Copyright 2010 Paul Lelgemann and Christian Arnold,
                  University of Duisburg-Essen

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
using System.Text;
using System.Threading;
using Cryptool.PluginBase;
using Cryptool.Plugins.PeerToPeer.Internal;
using Gears4Net;
using PeersAtPlay;
using PeersAtPlay.Monitoring;
using PeersAtPlay.P2PLink;
using PeersAtPlay.P2PLink.SnalNG;
using PeersAtPlay.P2POverlay;
using PeersAtPlay.P2POverlay.Bootstrapper;
using PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapperV2;
using PeersAtPlay.P2POverlay.Bootstrapper.LocalMachineBootstrapper;
using PeersAtPlay.P2POverlay.FullMeshOverlay;
using PeersAtPlay.P2PStorage.DHT;
using PeersAtPlay.P2PStorage.FullMeshDHT;
using PeersAtPlay.Util.Logging;

/* TODO:
 * - Delete UseNatTraversal-Flag and insert CertificateCheck and CertificateSetup
 * - Testing asynchronous methods incl. EventHandlers
 */

namespace Cryptool.P2P.Internal
{
    /// <summary>
    ///   Wrapper class to integrate peer@play environment into CrypTool. 
    ///   This class synchronizes asynchronous methods for easier usage in CT2.
    /// </summary>
    public class P2PBase
    {
        #region Variables

        private readonly AutoResetEvent _systemJoined;
        private readonly AutoResetEvent _systemLeft;
        private IBootstrapper _bootstrapper;
        private IP2PLinkManager _linkmanager;
        private P2POverlay _overlay;
        internal IDHT Dht;
        internal IVersionedDHT VersionedDht;

        /// <summary>
        ///   True if system was successfully joined, false if system is COMPLETELY left
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        ///   True if the underlying peer to peer system has been fully initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        #endregion

        #region Delegates

        public event P2PMessageReceived OnP2PMessageReceived;
        public delegate void P2PMessageReceived(PeerId sourceAddr, byte[] data);

        public event SystemJoined OnSystemJoined;
        public delegate void SystemJoined();

        public event SystemLeft OnSystemLeft;
        public delegate void SystemLeft();

        #endregion

        public P2PBase()
        {
            IsConnected = false;
            IsInitialized = false;

            _systemJoined = new AutoResetEvent(false);
            _systemLeft = new AutoResetEvent(false);
        }

        #region Basic P2P Methods (Init, Start, Stop)

        /// <summary>
        ///   Initializes the underlying peer-to-peer system with settings configured in P2PSettings. This step is required in order to be able to establish a connection.
        /// </summary>
        public void Initialize()
        {
            Scheduler scheduler = new STAScheduler("pap");

            switch (P2PSettings.Default.LinkManager)
            {
                case P2PLinkManagerType.Snal:
                    LogToMonitor("Init LinkMgr: Using NAT Traversal stuff");

                    // NAT-Traversal stuff needs a different Snal-Version
                    _linkmanager = new Snal(scheduler);

                    var settings = new PeersAtPlay.P2PLink.SnalNG.Settings();
                    settings.LoadDefaults();
                    settings.ConnectInternal = true;
                    settings.LocalReceivingPort = P2PSettings.Default.LocalReceivingPort;
                    settings.UseLocalAddressDetection = P2PSettings.Default.UseLocalAddressDetection;
                    settings.AutoReconnect = false;
                    settings.NoDelay = false;
                    settings.ReuseAddress = false;
                    settings.UseNetworkMonitorServer = true;
                    settings.CloseConnectionAfterPingTimeout = false;
                        
                    switch(P2PSettings.Default.TransportProtocol)
                    {
                        case P2PTransportProtocol.UDP:
                            settings.TransportProtocol = TransportProtocol.UDP;
                            break;
                        case P2PTransportProtocol.TCP_UDP:
                            settings.TransportProtocol = TransportProtocol.TCP_UDP;
                            break;
                        default:
                            settings.TransportProtocol = TransportProtocol.TCP;
                            break;
                    }

                    _linkmanager.Settings = settings;
                    _linkmanager.ApplicationType = ApplicationType.CrypTool;

                    break;
                default:
                    throw (new NotImplementedException());
            }

            switch (P2PSettings.Default.Bootstrapper)
            {
                case P2PBootstrapperType.LocalMachineBootstrapper:
                    // LocalMachineBootstrapper = only local connection (runs only on one machine)
                    _bootstrapper = new LocalMachineBootstrapper();
                    break;
                case P2PBootstrapperType.IrcBootstrapper:
                    // setup nat traversal stuff
                    LogToMonitor("Init Bootstrapper: Using NAT Traversal stuff");
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapper.Settings.DelaySymmetricResponse = true;
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapper.Settings.IncludeSymmetricInResponse = false;
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapper.Settings.SymmetricResponseDelay = 6000;

                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapperV2.Settings.DelaySymmetricResponse = true;
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapperV2.Settings.IncludeSymmetricResponse = false;

                    _bootstrapper = new IrcBootstrapper(scheduler);
                    break;
                default:
                    throw (new NotImplementedException());
            }

            switch (P2PSettings.Default.Overlay)
            {
                case P2POverlayType.FullMeshOverlay:
                    // changing overlay example: this.overlay = new ChordOverlay();
                    _overlay = new FullMeshOverlay(scheduler);
                    break;
                default:
                    throw (new NotImplementedException());
            }

            switch (P2PSettings.Default.Dht)
            {
                case P2PDHTType.FullMeshDHT:
                    Dht = new FullMeshDHT(scheduler);
                    break;
                default:
                    throw (new NotImplementedException());
            }

            _overlay.MessageReceived += OverlayMessageReceived;
            Dht.SystemJoined += OnDhtSystemJoined;
            Dht.SystemLeft += OnDhtSystemLeft;

            VersionedDht = (IVersionedDHT) Dht;

            P2PManager.GuiLogMessage("Initializing DHT with world name " + P2PSettings.Default.WorldName,
                                                NotificationLevel.Info);
            Dht.Initialize(P2PSettings.Default.PeerName, string.Empty, P2PSettings.Default.WorldName, _overlay,
                            _bootstrapper,
                            _linkmanager, null);

            IsInitialized = true;
        }

        /// <summary>
        ///   Starts the P2P System. When the given P2P world doesn't exist yet, 
        ///   inclusive creating the and bootstrapping to the P2P network.
        ///   In either case joining the P2P world.
        ///   This synchronized method returns true not before the peer has 
        ///   successfully joined the network (this may take one or two minutes).
        /// </summary>
        /// <exception cref = "InvalidOperationException">When the peer-to-peer system has not been initialized. 
        /// After validating the settings, this can be done by calling Initialize().</exception>
        /// <returns>True, if the peer has completely joined the p2p network</returns>
        public bool SynchStart()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Peer-to-peer is not initialized.");
            }

            if (IsConnected)
            {
                return true;
            }

            Dht.BeginStart(BeginStartEventHandler);

            // Wait for event SystemJoined. When it's invoked, the peer completely joined the P2P system
            _systemJoined.WaitOne();
            P2PManager.GuiLogMessage("System join process ended.", NotificationLevel.Debug);

            return true;
        }

        private void BeginStartEventHandler(DHTEventArgs eventArgs)
        {
            P2PManager.GuiLogMessage("Received DHTEventArgs: " + eventArgs + ", state: " + eventArgs.State, NotificationLevel.Debug);
        }

        /// <summary>
        ///   Disconnects from the peer-to-peer system.
        /// </summary>
        /// <returns>True, if the peer has completely left the p2p network</returns>
        public bool SynchStop()
        {
            if (Dht == null) return false;

            Dht.BeginStop(null);

            if (!IsConnected)
            {
                return true;
            }

            // wait till systemLeft Event is invoked
            _systemLeft.WaitOne();

            return true;
        }

        #endregion

        #region Peer related method (GetPeerId, Send message to peer)

        /// <summary>
        ///   Get PeerName of the actual peer
        /// </summary>
        /// <param name = "sPeerName">out: additional peer information UserName on LinkManager</param>
        /// <returns>PeerID as a String</returns>
        public PeerId GetPeerId(out string sPeerName)
        {
            sPeerName = _linkmanager.UserName;
            return new PeerId(_overlay.LocalAddress);
        }

        /// <summary>
        ///   Construct PeerId object for a specific byte[] id
        /// </summary>
        /// <param name = "byteId">overlay address as byte array</param>
        /// <returns>corresponding PeerId for given byte[] id</returns>
        public PeerId GetPeerId(byte[] byteId)
        {
            LogToMonitor("GetPeerID: Converting byte[] to PeerId-Object");
            return new PeerId(_overlay.GetAddress(byteId));
        }

        // overlay.LocalAddress = Overlay-Peer-Address/Names
        public void SendToPeer(byte[] data, byte[] destinationPeer)
        {
            // get stack size of the pap use-data and add own use data (for optimizing Stack size)
            var realStackSize = _overlay.GetHeaderSize() + data.Length;

            var stackData = new ByteStack(realStackSize);
            stackData.Push(data);

            var destinationAddr = _overlay.GetAddress(destinationPeer);
            var overlayMsg = new OverlayMessage(MessageReceiverType.P2PBase, 
                                                _overlay.LocalAddress, destinationAddr, stackData);

            _overlay.Send(overlayMsg);
        }

        private void OverlayMessageReceived(object sender, OverlayMessageEventArgs e)
        {
            if (OnP2PMessageReceived == null) return;

            var pid = new PeerId(e.Message.Source);
            /* You have to fire this event asynchronous, because the main 
                 * thread will be stopped in this wrapper class for synchronizing
                 * the asynchronous stuff (AutoResetEvent) --> so this could run 
                 * into a deadlock, when you fire this event synchronous (normal Invoke)
                 * ATTENTION: This could change the invocation order!!! In my case 
                              no problem, but maybe in future cases... */

            // TODO: not safe: The delegate must have only one target
            // OnP2PMessageReceived.BeginInvoke(pid, e.Message.Data.PopBytes(e.Message.Data.CurrentStackSize), null, null);

            foreach (var del in OnP2PMessageReceived.GetInvocationList())
            {
                del.DynamicInvoke(pid, e.Message.Data.PopBytes(e.Message.Data.CurrentStackSize));
            }
        }

        #endregion

        #region Event Handling (System Joined, Left and Message Received)

        private void OnDhtSystemJoined(object sender, EventArgs e)
        {
            if (OnSystemJoined != null)
                OnSystemJoined();

            _systemJoined.Set();
            IsConnected = true;
        }

        private void OnDhtSystemLeft(object sender, SystemLeftEventArgs e)
        {
            if (OnSystemLeft != null)
                OnSystemLeft();

            IsConnected = false;
            IsInitialized = false;

            // Allow new connection to start and check for waiting / blocked tasks
            // TODO reset running ConnectionWorkers?
            _systemLeft.Set();
            _systemJoined.Set();

            LogToMonitor("CrypP2P left the system.");
        }

        #endregion

        #region Synchronous Methods + their Callbacks

        /// <summary>
        ///   Stores a value in the DHT at the given key
        /// </summary>
        /// <param name = "key">Key of DHT Entry</param>
        /// <param name = "data">Value of DHT Entry</param>
        /// <returns>True, when storing is completed!</returns>
        public bool SynchStore(string key, byte[] data)
        {
            var autoResetEvent = new AutoResetEvent(false);

            // LogToMonitor("testcrash" + Encoding.UTF8.GetString(new byte[5000]));
            LogToMonitor("Begin: SynchStore. Key: " + key + ", " + data.Length + " bytes");

            var responseWait = new ResponseWait {WaitHandle = autoResetEvent, key = key, value = data};
            VersionedDht.Store(OnSynchStoreCompleted, key, data, responseWait);

            // blocking till response
            autoResetEvent.WaitOne();

            LogToMonitor("End: SynchStore. Key: " + key + ". Success: " + responseWait.success);

            return responseWait.success;
        }

        /// <summary>
        ///   Stores a value in the DHT at the given key
        /// </summary>
        /// <param name = "key">Key of DHT Entry</param>
        /// <param name = "value">Value of DHT Entry</param>
        /// <returns>True, when storing is completed!</returns>
        public bool SynchStore(string key, string value)
        {
            return SynchStore(key, Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        ///   Callback for a the synchronized store method
        /// </summary>
        /// <param name = "storeResult">retrieved data container</param>
        private static void OnSynchStoreCompleted(StoreResult storeResult)
        {
            var responseWait = storeResult.AsyncState as ResponseWait;
            if (responseWait == null)
            {
                LogToMonitor("Received OnSynchStoreCompleted, but ResponseWait object is missing. Discarding.");
                return;
            }

            responseWait.success = storeResult.Status == OperationStatus.Success;
            responseWait.operationStatus = storeResult.Status;
            responseWait.Message = Encoding.UTF8.GetBytes(storeResult.Status.ToString());

            // unblock WaitHandle in the synchronous method
            responseWait.WaitHandle.Set();
        }

        /// <summary>
        ///   Get the value of the given DHT Key or null, if it doesn't exist.
        /// </summary>
        /// <param name = "key">Key of DHT Entry</param>
        /// <returns>Value of DHT Entry</returns>
        public byte[] SynchRetrieve(string key)
        {
            LogToMonitor("Begin: SynchRetrieve. Key: " + key);

            var autoResetEvent = new AutoResetEvent(false);
            var responseWait = new ResponseWait {WaitHandle = autoResetEvent, key = key};
            Dht.Retrieve(OnSynchRetrieveCompleted, key, responseWait);

            // blocking till response
            autoResetEvent.WaitOne();

            LogToMonitor("End: SynchRetrieve. Key: " + key + ". Success: " + responseWait.success);

            return responseWait.Message;
        }

        /// <summary>
        ///   Callback for a the synchronized retrieval method
        /// </summary>
        /// <param name = "retrieveResult"></param>
        private static void OnSynchRetrieveCompleted(RetrieveResult retrieveResult)
        {
            var responseWait = retrieveResult.AsyncState as ResponseWait;
            if (responseWait == null)
            {
                LogToMonitor("Received OnSynchRetrieveCompleted, but ResponseWait object is missing. Discarding.");
                return;
            }

            switch (retrieveResult.Status)
            {
                case OperationStatus.Success:
                    responseWait.success = true;
                    responseWait.Message = retrieveResult.Data;
                    break;
                case OperationStatus.KeyNotFound:
                    responseWait.success = true;
                    responseWait.Message = null;
                    break;
                default:
                    responseWait.success = false;
                    responseWait.Message = null;
                    break;
            }

            responseWait.operationStatus = retrieveResult.Status;

            // unblock WaitHandle in the synchronous method
            responseWait.WaitHandle.Set();
        }

        /// <summary>
        ///   Removes a key/value pair out of the DHT
        /// </summary>
        /// <param name = "key">Key of the DHT Entry</param>
        /// <returns>True, when removing is completed!</returns>
        public bool SynchRemove(string key)
        {
            LogToMonitor("Begin SynchRemove. Key: " + key);

            var autoResetEvent = new AutoResetEvent(false);
            var responseWait = new ResponseWait {WaitHandle = autoResetEvent, key = key};
            VersionedDht.Remove(OnSynchRemoveCompleted, key, responseWait);

            // blocking till response
            autoResetEvent.WaitOne();

            LogToMonitor("End: SynchRemove. Key: " + key + ". Success: " + responseWait.success);

            return responseWait.success;
        }

        /// <summary>
        ///   Callback for a the synchronized remove method
        /// </summary>
        /// <param name = "removeResult"></param>
        private static void OnSynchRemoveCompleted(RemoveResult removeResult)
        {
            var responseWait = removeResult.AsyncState as ResponseWait;
            if (responseWait == null)
            {
                LogToMonitor("Received OnSynchRemoveCompleted, but ResponseWait object is missing. Discarding.");
                return;
            }

            responseWait.success = removeResult.Status != OperationStatus.Failure &&
                                   removeResult.Status != OperationStatus.VersionMismatch;
            responseWait.operationStatus = removeResult.Status;
            responseWait.Message = Encoding.UTF8.GetBytes(removeResult.Status.ToString());

            // unblock WaitHandle in the synchronous method
            responseWait.WaitHandle.Set();
        }

        #endregion

        #region Log facility

        /// <summary>
        ///   To log the internal state in the Monitoring Software of P@play
        /// </summary>
        public void LogInternalState()
        {
            if (Dht != null)
            {
                Dht.LogInternalState();
            }
        }

        private static void LogToMonitor(string sTextToLog)
        {
            if (P2PSettings.Default.Log2Monitor)
                Log.Debug(sTextToLog);
        }

        #endregion
    }
}