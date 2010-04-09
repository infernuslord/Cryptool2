/* Copyright 2009 Team CrypTool (Christian Arnold), Uni Duisburg-Essen

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
using System.Linq;
using System.Text;
using PeersAtPlay.P2PStorage.DHT;
using PeersAtPlay.P2PStorage.FullMeshDHT;
using PeersAtPlay.P2POverlay.Bootstrapper;
using PeersAtPlay.P2POverlay;
using PeersAtPlay.P2POverlay.Bootstrapper.LocalMachineBootstrapper;
using PeersAtPlay.P2POverlay.FullMeshOverlay;
using PeersAtPlay.P2PLink;
using PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapper;
using System.Threading;
using Cryptool.PluginBase.Control;
using System.ComponentModel;
using PeersAtPlay;
using PeersAtPlay.Util.Logging;
using Gears4Net;

/* TODO:
 * - Catch errors, which can occur when using the DHT (network-based errors)
 */

/* - Synchronous functions successfully tested (store, retrieve)
 * - The DHT has an integrated versioning system. When a peer wants
 *   to store data in an entry, which already holds data, the version
 *   number will be compared with the peers' version number. If the
 *   peer hasn't read/write the entry the last time, the storing instruction
 *   will be rejected. You must first read the actual data and than you can
 *   store your data in this entry...
 * 
 * INFO:
 * - Have considered the DHT-own versioning system in the SynchStore method.
 *   If this versioning system will be abolished, the SynchStore method must
 *   be change!
 * - Everything switched to SnalNG, SimpleSnal isn't used anymore, because
 *   certification stuff runs now
 * 
 * TODO:
 * - Delete UseNatTraversal-Flag and insert CertificateCheck and CertificateSetup
 * - Testing asynchronous methods incl. EventHandlers
 */
namespace Cryptool.Plugins.PeerToPeer
{
    /* Advantages of this wrapper class:
     * - The PeerAtPlay-Libraries are only referenced in this project
     *   --> so they're easy to update
     * - PeerAtPlay only works with asynchronous methods, so this class
     *   "synchronizes" this methods.
     * - The PeerToPeer-Layers are unimportant for CT2-Developers, so this
     *   issue is obfuscated by this wrapper class
     */
    /// <summary>
    /// Wrapper class to integrate peer@play environment into CrypTool. 
    /// This class synchronizes asynchronous methods for easier usage in CT2. For future
    /// </summary>
    public class P2PBase
    {
        #region Delegates and Events for asynchronous p2p functions

        public delegate void SystemJoined();
        public event SystemJoined OnSystemJoined;

        public delegate void SystemLeft();
        public event SystemLeft OnSystemLeft;

        public delegate void P2PMessageReceived(PeerId sourceAddr, byte[] data);
        public event P2PMessageReceived OnP2PMessageReceived;

        /// <summary>
        /// returns true if key-value-pair is successfully stored in the DHT
        /// </summary>
        /// <param name="result"></param>
        public delegate void DHTStoreCompleted(bool result);
        public event DHTStoreCompleted OnDhtStore_Completed;

        public delegate void DHTLoadCompleted(byte[] loadedData);
        public event DHTLoadCompleted OnDhtLoad_Completed;

        /// <summary>
        /// returns true if key was found and removed successfully from the DHT
        /// </summary>
        /// <param name="result"></param>
        public delegate void DHTRemoveCompleted(bool result);
        public event DHTRemoveCompleted OnDhtRemove_Completed;

        #endregion

        #region Variables

        private bool allowLoggingToMonitor;
        /// <summary>
        /// If true, all kinds of actions will be logged in the PeersAtPlay LogMonitor.
        /// </summary>
        public bool AllowLoggingToMonitor 
        {
            get { return this.allowLoggingToMonitor; }
            set { this.allowLoggingToMonitor = value; }
        }

        private const bool ALLOW_LOGGING_TO_MONITOR = true;

        private bool started = false;
        /// <summary>
        /// True if system was successfully joined, false if system is COMPLETELY left
        /// </summary>
        public bool Started 
        {
            get { return this.started; }
            private set { this.started = value; } 
        }

        private IDHT dht;
        private IP2PLinkManager linkmanager;
        private IBootstrapper bootstrapper;
        private P2POverlay overlay;
        private AutoResetEvent systemJoined;
        private AutoResetEvent systemLeft;

        /// <summary>
        /// Dictionary for synchronizing asynchronous DHT retrieves.
        /// Cryptool doesn't offers an asynchronous environment, so this workaround is necessary
        /// </summary>
        private Dictionary<Guid, ResponseWait> waitDict;

        #endregion

        public P2PBase()
        {
            this.waitDict = new Dictionary<Guid, ResponseWait>();
            this.systemJoined = new AutoResetEvent(false);
            this.systemLeft = new AutoResetEvent(false);
        }

        #region Basic P2P Methods (Init, Start, Stop) - synch and asynch

        /// <summary>
        /// Initializing is the first step to build a new or access an existing p2p network
        /// </summary>
        /// <param name="sUserName">Choose an individual name for the user</param>
        /// <param name="sWorldName">fundamental: two peers are only in the SAME 
        /// P2P system, when they initialized the SAME WORLD!</param>
        /// <param name="bolUseNatTraversal">When you want to use NAT-Traversal #
        /// (tunneling the P2P connection through NATs and Firewalls), you have to
        /// set this flag to true</param>
        /// <param name="linkManagerType"></param>
        /// <param name="bsType"></param>
        /// <param name="overlayType"></param>
        /// <param name="dhtType"></param>
        public void Initialize(string sUserName, string sWorldName, P2PLinkManagerType linkManagerType, P2PBootstrapperType bsType, P2POverlayType overlayType, P2PDHTType dhtType)
        {
            #region Setting LinkManager, Bootstrapper, Overlay and DHT to the specified types

            Scheduler scheduler = new STAScheduler("pap");

            switch (linkManagerType)
            {
                case P2PLinkManagerType.Snal:
                    LogToMonitor("Init LinkMgr: Using NAT Traversal stuff");
                    // NAT-Traversal stuff needs a different Snal-Version
                    this.linkmanager = new PeersAtPlay.P2PLink.SnalNG.Snal(scheduler);

                    PeersAtPlay.P2PLink.SnalNG.Settings settings = new PeersAtPlay.P2PLink.SnalNG.Settings();
                    settings.LoadDefaults();
                    settings.ConnectInternal = true;
                    settings.LocalReceivingPort = 0;
                    settings.UseLocalAddressDetection = false;
                    settings.AutoReconnect = false;
                    settings.NoDelay = false;
                    settings.ReuseAddress = false;
                    settings.UseNetworkMonitorServer = true;

                    this.linkmanager.Settings = settings;
                    this.linkmanager.ApplicationType = PeersAtPlay.Monitoring.ApplicationType.CrypTool;

                    break;
                default:
                    throw (new NotImplementedException());
            }
            switch (bsType)
            {
                case P2PBootstrapperType.LocalMachineBootstrapper:
                    //LocalMachineBootstrapper = only local connection (runs only on one machine)
                    this.bootstrapper = new LocalMachineBootstrapper();
                    break;
                case P2PBootstrapperType.IrcBootstrapper:
                    // setup nat traversal stuff
                    LogToMonitor("Init Bootstrapper: Using NAT Traversal stuff");
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapper.Settings.DelaySymmetricResponse = true;
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapper.Settings.IncludeSymmetricInResponse = false;
                    PeersAtPlay.P2POverlay.Bootstrapper.IrcBootstrapper.Settings.SymmetricResponseDelay = 6000;

                    this.bootstrapper = new IrcBootstrapper(scheduler);
                    break;
                default:
                    throw (new NotImplementedException());
            }
            switch (overlayType)
            {
                case P2POverlayType.FullMeshOverlay:
                    // changing overlay example: this.overlay = new ChordOverlay();
                    this.overlay = new FullMeshOverlay(scheduler);
                    break;
                default:
                    throw (new NotImplementedException());
            }
            switch (dhtType)
            {
                case P2PDHTType.FullMeshDHT:
                    this.dht = new FullMeshDHT(scheduler);
                    break;
                default:
                    throw (new NotImplementedException());
            }
            #endregion

            this.overlay.MessageReceived += new EventHandler<OverlayMessageEventArgs>(overlay_MessageReceived);
            this.dht.SystemJoined += new EventHandler(OnDHT_SystemJoined);
            this.dht.SystemLeft += new EventHandler<SystemLeftEventArgs>(OnDHT_SystemLeft);

            this.dht.Initialize(sUserName, "", sWorldName, this.overlay, this.bootstrapper, this.linkmanager, null);
        }

        /// <summary>
        /// Starts the P2P System. When the given P2P world doesn't exist yet, 
        /// inclusive creating the and bootstrapping to the P2P network.
        /// In either case joining the P2P world.
        /// This synchronized method returns true not before the peer has 
        /// successfully joined the network (this may take one or two minutes).
        /// </summary>
        /// <returns>True, if the peer has completely joined the p2p network</returns>
        public bool SynchStart()
        {
            //Start != system joined
            //Only starts the system asynchronous, the possible callback is useless, 
            //because it's invoked before the peer completly joined the P2P system
            this.dht.BeginStart(null);
            //Wait for event SystemJoined. When it's invoked, the peer completly joined the P2P system
            this.systemJoined.WaitOne();
            return true;
        }

        /// <summary>
        /// Disjoins the peer from the system. The P2P system survive while one peer is still in the network.
        /// </summary>
        /// <returns>True, if the peer has completely disjoined the p2p network</returns>
        public bool SynchStop()
        {
            if (this.dht != null)
            {
                this.dht.BeginStop(null);
                //don't stop anything else, because BOOM

                //wait till systemLeft Event is invoked
                this.systemLeft.WaitOne();
            }
            return true;
        }

        /// <summary>
        /// Asynchronously starting the peer. When the given P2P world doesn't 
        /// exist yet, inclusive creating the and bootstrapping to the P2P network.
        /// In either case joining the P2P world. To ensure that peer has successfully
        /// joined the p2p world, catch the event OnSystemJoined.
        /// </summary>
        public void AsynchStart()
        {
            // no callback usefull, because starting and joining isn't the same
            // everything else is done by the EventHandler OnDHT_SystemJoined
            this.dht.BeginStart(null);
        }

        /// <summary>
        /// Asynchronously disjoining the actual peer of the p2p system. To ensure
        /// disjoining, catch the event OnDHT_SystemLeft.
        /// </summary>
        public void AsynchStop()
        {
            if (this.dht != null)
            {
                // no callback usefull.
                // Everything else is done by the EventHandler OnDHT_SystemLeft
                this.dht.BeginStop(null);
            }
        }

        #endregion

        /// <summary>
        /// Get PeerName of the actual peer
        /// </summary>
        /// <param name="sPeerName">out: additional peer information UserName on LinkManager</param>
        /// <returns>PeerID as a String</returns>
        public PeerId GetPeerID(out string sPeerName)
        {
            sPeerName = this.linkmanager.UserName;
            return new PeerId(this.overlay.LocalAddress);
        }

        /// <summary>
        /// Construct PeerId object for a specific byte[] id
        /// </summary>
        /// <param name="byteId">overlay address as byte array</param>
        /// <returns>corresponding PeerId for given byte[] id</returns>
        public PeerId GetPeerID(byte[] byteId)
        {
            LogToMonitor("GetPeerID: Converting byte[] to PeerId-Object");
            return new PeerId(this.overlay.GetAddress(byteId));
        }

        // overlay.LocalAddress = Overlay-Peer-Address/Names
        public void SendToPeer(byte[] data, byte[] destinationPeer)
        {
            // get stack size of the pap use-data and add own use data (for optimizing Stack size)
            int realStackSize = this.overlay.GetHeaderSize() + data.Length;
            ByteStack stackData = new ByteStack(realStackSize);

            stackData.Push(data);

            OverlayAddress destinationAddr = this.overlay.GetAddress(destinationPeer);
            OverlayMessage overlayMsg = new OverlayMessage(MessageReceiverType.P2PBase,
                this.overlay.LocalAddress, destinationAddr, stackData);
            this.overlay.Send(overlayMsg);
        }

        private void overlay_MessageReceived(object sender, OverlayMessageEventArgs e)
        {
            if (OnP2PMessageReceived != null)
            {
                PeerId pid = new PeerId(e.Message.Source);
                /* You have to fire this event asynchronous, because the main 
                 * thread will be stopped in this wrapper class for synchronizing
                 * the asynchronous stuff (AutoResetEvent) --> so this could run 
                 * into a deadlock, when you fire this event synchronous (normal Invoke)
                 * ATTENTION: This could change the invocation order!!! In my case 
                              no problem, but maybe in future cases... */
                OnP2PMessageReceived.BeginInvoke(pid, e.Message.Data.PopBytes(e.Message.Data.CurrentStackSize),null,null);
                //OnP2PMessageReceived(pid, e.Message.Data.PopUTF8String());
            }
        }

        #region Event Handling (System Joined, Left and Message Received)

        private void OnDHT_SystemJoined(object sender, EventArgs e)
        {
            if (OnSystemJoined != null)
                OnSystemJoined();
            this.systemJoined.Set();
            Started = true;
        }

        private void OnDHT_SystemLeft(object sender, SystemLeftEventArgs e)
        {
            if (OnSystemLeft != null)
                OnSystemLeft();
            // as an experiment
            this.dht = null;
            this.systemLeft.Set();
            Started = false;

            LogToMonitor("OnDHT_SystemLeft has nulled the dht and setted the systemLeft Waithandle");
        }

        #endregion

        /* Attention: The asynchronous methods are not tested at the moment */
        #region Asynchronous Methods incl. Callbacks

        /// <summary>
        /// Asynchronously retrieving a key from the DHT. To get value, catch 
        /// event OnDhtLoad_Completed.
        /// </summary>
        /// <param name="sKey">Existing key in DHT</param>
        public void AsynchRetrieve(string sKey)
        {
            Guid g = this.dht.Retrieve(OnAsynchRetrieve_Completed, sKey);
        }
        private void OnAsynchRetrieve_Completed(RetrieveResult rr)
        {
            if (OnDhtLoad_Completed != null)
            {
                OnDhtLoad_Completed(rr.Data);
            }
        }

        /// <summary>
        /// Asynchronously storing a Key-Value-Pair in the DHT. To ensure that 
        /// storing is completed, catch event OnDhtStore_Completed.
        /// </summary>
        /// <param name="sKey"></param>
        /// <param name="sValue"></param>
        public void AsynchStore(string sKey, string sValue)
        {
            //this.dht.Store(OnAsynchStore_Completed, sKey, UTF8Encoding.UTF8.GetBytes(sValue), IGNORE_DHT_VERSIONING_SYSTEM);
            this.dht.Store(OnAsynchStore_Completed, sKey, UTF8Encoding.UTF8.GetBytes(sValue));
        }

        private void OnAsynchStore_Completed(StoreResult sr)
        {
            if (OnDhtStore_Completed != null)
            {
                if (sr.Status == OperationStatus.Success)
                    OnDhtStore_Completed(true);
                else
                    OnDhtStore_Completed(false);
            }
                
        }

        /// <summary>
        /// Asynchronously removing an existing key out of the DHT. To ensure
        /// that removing is completed, catch event OnDhtRemove_Completed.
        /// </summary>
        /// <param name="sKey"></param>
        public void AsynchRemove(string sKey)
        {
            //this.dht.Remove(OnAsynchRemove_Completed, sKey, IGNORE_DHT_VERSIONING_SYSTEM);
            this.dht.Remove(OnAsynchRemove_Completed, sKey);
        }
        private void OnAsynchRemove_Completed(RemoveResult rr)
        {
            if (OnDhtRemove_Completed != null)
            {
                if(rr.Status == OperationStatus.Success)
                    OnDhtRemove_Completed(true);
                else
                    OnDhtRemove_Completed(false);
            }
        }

        #endregion

        #region Synchronous Methods incl. Callbacks

        #region SynchStore incl.Callback and SecondTrialCallback

        /// <summary>
        /// Stores a value in the DHT at the given key
        /// </summary>
        /// <param name="sKey">Key of DHT Entry</param>
        /// <param name="byteData">Value of DHT Entry</param>
        /// <returns>True, when storing is completed!</returns>
        public bool SynchStore(string sKey, byte[] byteData)
        {
            LogToMonitor("Begin: SynchStore. Key: " + sKey + ", Data: " + Encoding.UTF8.GetString(byteData));
            AutoResetEvent are = new AutoResetEvent(false);
            // this method returns always a GUID to distinguish between asynchronous actions
            Guid g = this.dht.Store(OnSynchStoreCompleted, sKey, byteData);

            ResponseWait rw = new ResponseWait() { WaitHandle = are, key=sKey , value = byteData };

            waitDict.Add(g, rw);
            //blocking till response
            are.WaitOne();

            LogToMonitor("End: SynchStore. Key: " + sKey + ". Success: " + rw.success.ToString());

            return rw.success;
        }

        /// <summary>
        /// Stores a value in the DHT at the given key
        /// </summary>
        /// <param name="sKey">Key of DHT Entry</param>
        /// <param name="sValue">Value of DHT Entry</param>
        /// <returns>True, when storing is completed!</returns>
        public bool SynchStore(string sKey, string sData)
        {
            return SynchStore(sKey, UTF8Encoding.UTF8.GetBytes(sData));
        }
        /// <summary>
        /// Callback for a the synchronized store method
        /// </summary>
        /// <param name="rr"></param>
        private void OnSynchStoreCompleted(StoreResult sr)
        {
            ResponseWait rw;
            if (this.waitDict.TryGetValue(sr.Guid, out rw))
            {
                // if Status == Error, than the version of the value is out of date.
                // There is a versioning system in the DHT. So you must retrieve the
                // key and than store the new value
                if (sr.Status == OperationStatus.Failure)
                {
                    byte[] byteTemp = this.SynchRetrieve(rw.key);

                    // Only try a second time. When it's still not possible, abort storing
                    AutoResetEvent are = new AutoResetEvent(false);
                    Guid g = this.dht.Store(OnSecondTrialStoring, rw.key, rw.value);
                    ResponseWait rw2 = new ResponseWait() { WaitHandle = are, key = rw.key, value = rw.value };

                    waitDict.Add(g, rw2);
                    // blocking till response
                    are.WaitOne();
                    rw.success = rw2.success;
                    rw.Message = rw2.Message;
                }
                else
                {
                    rw.Message = UTF8Encoding.UTF8.GetBytes(sr.Status.ToString());
                    if (sr.Status == OperationStatus.KeyNotFound)
                        rw.success = false;
                    else
                        rw.success = true;
                }
            }
            //unblock WaitHandle in the synchronous method
            rw.WaitHandle.Set();
            // don't know if this accelerates the system...
            this.waitDict.Remove(sr.Guid);
        }

        private void OnSecondTrialStoring(StoreResult sr)
        {
            ResponseWait rw;
            if (this.waitDict.TryGetValue(sr.Guid, out rw))
            {
                if (sr.Status == OperationStatus.Failure)
                {
                    //Abort storing, because it's already the second trial
                    rw.Message = UTF8Encoding.UTF8.GetBytes("Storing also not possible on second trial.");
                    rw.success = false;
                }
                else
                {
                    //works the second trial, so it was the versioning system
                    rw.success = true;
                }
            }
            //unblock WaitHandle in the synchronous method
            rw.WaitHandle.Set();
            // don't know if this accelerates the system...
            this.waitDict.Remove(sr.Guid);
        }

        #endregion

        /// <summary>
        /// Get the value of the given DHT Key or null, if it doesn't exist.
        /// For synchronous environments use the Synch* methods.
        /// </summary>
        /// <param name="sKey">Key of DHT Entry</param>
        /// <returns>Value of DHT Entry</returns>
        public byte[] SynchRetrieve(string sKey)
        {
            LogToMonitor("ThreadId (P2PBase SynchRetrieve): " + Thread.CurrentThread.ManagedThreadId.ToString());

            AutoResetEvent are = new AutoResetEvent(false);
            // this method returns always a GUID to distinguish between asynchronous actions

            LogToMonitor("Begin: SynchRetrieve. Key: " + sKey);
             
            Guid g = this.dht.Retrieve(OnSynchRetrieveCompleted, sKey);
            
            ResponseWait rw = new ResponseWait() {WaitHandle = are };
            
            waitDict.Add(g,rw  );
            // blocking till response
            are.WaitOne();

            LogToMonitor("End: SynchRetrieve. Key: " + sKey + ". Success: " + rw.success.ToString());

            //Rückgabe der Daten
            return rw.Message;
        }

        /// <summary>
        /// Callback for a the synchronized retrieval method
        /// </summary>
        /// <param name="rr"></param>
        private void OnSynchRetrieveCompleted(RetrieveResult rr)
        {
            LogToMonitor(rr.Guid.ToString());
            
            ResponseWait rw;

            LogToMonitor("ThreadId (P2PBase retrieve callback): " + Thread.CurrentThread.ManagedThreadId.ToString());

            if (this.waitDict.TryGetValue(rr.Guid, out rw))
            {
                // successful as long as no error occured
                rw.success = true;
                if (rr.Status == OperationStatus.Failure)
                {
                    rw.Message = null;
                    rw.success = false;
                }
                else if (rr.Status == OperationStatus.KeyNotFound)
                    rw.Message = null;
                else
                    rw.Message = rr.Data;

                //unblock WaitHandle in the synchronous method
                rw.WaitHandle.Set();
                // don't know if this accelerates the system...
                this.waitDict.Remove(rr.Guid);
            }
        }
        /// <summary>
        /// Removes a key/value pair out of the DHT
        /// </summary>
        /// <param name="sKey">Key of the DHT Entry</param>
        /// <returns>True, when removing is completed!</returns>
        public bool SynchRemove(string sKey)
        {
            LogToMonitor("Begin SynchRemove. Key: " + sKey);

            AutoResetEvent are = new AutoResetEvent(false);
            // this method returns always a GUID to distinguish between asynchronous actions
            Guid g = this.dht.Remove(OnSynchRemoveCompleted, sKey);
            //Guid g = this.dht.Remove(OnSynchRemoveCompleted, sKey, IGNORE_DHT_VERSIONING_SYSTEM);

            ResponseWait rw = new ResponseWait() { WaitHandle = are };

            waitDict.Add(g, rw);
            // blocking till response
            are.WaitOne();

            LogToMonitor("Ended SynchRemove. Key: " + sKey + ". Success: " + rw.success.ToString());

            return rw.success;
        }

        /// <summary>
        /// Callback for a the synchronized remove method
        /// </summary>
        /// <param name="rr"></param>
        private void OnSynchRemoveCompleted(RemoveResult rr)
        {
            ResponseWait rw;
            if (this.waitDict.TryGetValue(rr.Guid, out rw))
            {
                rw.Message = UTF8Encoding.UTF8.GetBytes(rr.Status.ToString());

                if (rr.Status == OperationStatus.Failure || rr.Status == OperationStatus.KeyNotFound)
                    rw.success = false;
                else
                    rw.success = true;

                //unblock WaitHandle in the synchronous method
                rw.WaitHandle.Set();
                // don't know if this accelerates the system...
                this.waitDict.Remove(rr.Guid);
            }
        }

        #endregion

        /// <summary>
        /// To log the internal state in the Monitoring Software of P@play
        /// </summary>
        public void LogInternalState()
        {
            if (this.dht != null)
            {
                this.dht.LogInternalState();
            }
        }

        public void LogToMonitor(string sTextToLog)
        {
            if(AllowLoggingToMonitor)
                Log.Debug(sTextToLog);
        }

    }

    public class PeerId
    {
        private readonly string stringId;
        private readonly byte[] byteId;

        private const uint OFFSET_BASIS = 2166136261;
        private const uint PRIME = 16777619; // 2^24 + 2^8 + 0x93
        private readonly int hashCode;

        public PeerId(OverlayAddress oAddress)
        {
            if (oAddress != null)
            {
                this.stringId = oAddress.ToString();
                this.byteId = oAddress.ToByteArray();

                // FNV-1 hashing
                uint fnvHash = OFFSET_BASIS;
                foreach (byte b in byteId)
                {
                    fnvHash = (fnvHash * PRIME) ^ b;
                }
                hashCode = (int)fnvHash;
            }
        }

        /// <summary>
        /// Returns true when the byteId content is identical
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        public override bool Equals(object right)
        {
            /* standard checks for reference types */

            if (right == null)
                return false;

            if (object.ReferenceEquals(this, right))
                return true;

            if (this.GetType() != right.GetType())
                return false;

            // actual content comparison
            return this == (PeerId)right;
        }

        public static bool operator ==(PeerId left, PeerId right)
        {
            // because it's possible that one parameter is null, catch this exception
            /* Begin add - Christian Arnold, 2009.12.16, must cast the parameters, otherwise --> recursion */
            if ((object)left == (object)right)
                return true;

            if ((object)left == null || (object)right == null)
                return false;
            /* End add */

            if (left.hashCode != right.hashCode)
                return false;

            if (left.byteId.Length != right.byteId.Length)
                return false;

            for (int i = 0; i < left.byteId.Length; i++)
            {
                // uneven pattern content
                if (left.byteId[i] != right.byteId[i])
                    return false;
            }

            return true;
        }

        public static bool operator !=(PeerId left, PeerId right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override string ToString()
        {
            return this.stringId;
        }

        public byte[] ToByteArray()
        {
            return this.byteId;
        }
    }
}
