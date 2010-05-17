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
using System.Threading;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.Plugins.PeerToPeer.Internal;

namespace Cryptool.Plugins.PeerToPeer
{
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "P2P_Peer", "Creates a new Peer", "", "PeerToPeerBase/icons/peer_inaktiv.png", "PeerToPeerBase/icons/peer_connecting.png", "PeerToPeerBase/icons/peer_online.png", "PeerToPeerBase/icons/peer_error.png")]
    public class P2PPeer : IIOMisc
    {
        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        public event P2PBase.P2PMessageReceived OnPeerMessageReceived;

        #region Variables

        public P2PBase p2pBase;
        private P2PPeerSettings settings;
        private IP2PControl p2pSlave;

        #endregion

        #region Standard functionality

        public P2PPeer()
        {
            this.p2pBase = new P2PBase();
            this.settings = new P2PPeerSettings(this);
            this.settings.TaskPaneAttributeChanged += new TaskPaneAttributeChangedHandler(settings_TaskPaneAttributeChanged);
            this.settings.OnPluginStatusChanged += new StatusChangedEventHandler(settings_OnPluginStatusChanged);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) 
                OnPluginStatusChanged(this, args);
        }

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        private void p2pBase_OnP2PMessageReceived(PeerId sourceAddr, byte[] data)
        {
            if (OnPeerMessageReceived != null)
                OnPeerMessageReceived(sourceAddr, data);
        }

        void settings_TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        public ISettings Settings
        {
            set { this.settings = (P2PPeerSettings)value; }
            get { return this.settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            StartPeer();
        }

        public void Execute()
        {
            // TODO: For future use copy functionality to Execute instead of PreExecute
            //       so we don't need the workaround anymore!!!
            // StartPeer();
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            StopPeer();
        }

        #endregion

        #region IPlugin Members

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion 

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            // for evaluation issues only
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p + "(" + DebugToFile.GetTimeStamp() + ")", this, notificationLevel));
            //EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion

        #region In and Output

        [PropertyInfo(Direction.ControlSlave, "Master Peer", "One peer to rule them all", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public IP2PControl P2PControlSlave
        {
            get
            {
                if (this.p2pSlave == null)
                    // to commit the settings of the plugin to the IControl
                    this.p2pSlave = new P2PPeerMaster(this);
                return this.p2pSlave;
            }
        }

        #endregion

        #region Start and Stop Peer

        /// <summary>
        /// Status flag for starting and stopping peer only once.
        /// </summary>
        private bool peerStarted = false;
        /// <summary>
        /// Status flag for starting and stopping peer only once.
        /// </summary>
        public bool PeerStarted 
        {
            get { return this.peerStarted; }
            private set { this.peerStarted = value; }
        }

        public void StartPeer()
        {
            if (!this.PeerStarted)
            {
                if (this.p2pBase == null)
                {
                    this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Error); 
                    GuiLogMessage("Starting Peer failed, because Base-Object is null.",NotificationLevel.Error);
                    return;
                }

                this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Connecting);

                this.p2pBase.AllowLoggingToMonitor = this.settings.Log2Monitor;

                // to forward event from overlay/dht MessageReceived-Event from P2PBase
                this.p2pBase.OnP2PMessageReceived += new P2PBase.P2PMessageReceived(p2pBase_OnP2PMessageReceived);

                if (CheckAndInstallPAPCertificates())
                {
                    this.p2pBase.Initialize(this.settings.P2PPeerName, this.settings.P2PWorldName,
                        (P2PLinkManagerType)this.settings.P2PLinkMngrType, (P2PBootstrapperType)this.settings.P2PBSType,
                        (P2POverlayType)this.settings.P2POverlType, (P2PDHTType)this.settings.P2PDhtType);
                    this.PeerStarted = this.p2pBase.SynchStart();

                    if (this.PeerStarted)
                    {
                        this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Online);
                    }
                    string joiningStatus = this.PeerStarted == true ? "successful" : "canceled";
                    GuiLogMessage("Status of joining the P2P System: " + joiningStatus, NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Because not all p2p certificates were installed, you can't start the p2p system!", NotificationLevel.Error);
                }
            }
            else
            {
                GuiLogMessage("Peer is already started!", NotificationLevel.Info);
            }
        }

        public void StopPeer()
        {
            if (this.PeerStarted && this.p2pBase != null)
            {
                this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Connecting);

                this.PeerStarted = !this.p2pBase.SynchStop();

                if (this.PeerStarted)
                {
                    this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Online);
                    GuiLogMessage("Peer stopped: " + !this.PeerStarted, NotificationLevel.Warning);
                }
                else
                {
                    this.p2pBase.OnP2PMessageReceived -= p2pBase_OnP2PMessageReceived;
                    this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.NotConnected);
                    GuiLogMessage("Peer stopped: " + !this.PeerStarted, NotificationLevel.Info);
                }
            }
            else
            {
                GuiLogMessage("Peer is already stopped!", NotificationLevel.Info);
            }
        }

        /// <summary>
        /// Checks if all certificates for using the pap p2p system are installed.
        /// Otherwise it tries to install the missing certificates. If all operations
        /// succeed, return value is true. Only when value is true, you can try
        /// to initialize the PAP System.
        /// </summary>
        /// <returns>If all operations succeed, return value is true. Only when value 
        /// is true, you can try to initialize the PAP System.</returns>
        private bool CheckAndInstallPAPCertificates()
        {
            bool retValue = false;

            // get exe directory, because there resides the certificate directory
            System.Reflection.Assembly assemb = System.Reflection.Assembly.GetEntryAssembly();
            string applicationDir = System.IO.Path.GetDirectoryName(assemb.Location);
            // check if all necessary certs are installed
            GuiLogMessage("Check installation of all certificates, which are necessary to run the p2p system", NotificationLevel.Info);
            List<PAPCertificate.PAP_Certificates> lstMissingCerts = PAPCertificate.CheckAvailabilityOfPAPCertificates(applicationDir);
            if (lstMissingCerts.Count == 0)
            {
                GuiLogMessage("All neccessary p2p certificates are installed.", NotificationLevel.Info);
                retValue = true;
            }
            else
            {
                StringBuilder sbMissingCerts = new StringBuilder();
                for (int i = 0; i < lstMissingCerts.Count; i++)
                {
                    sbMissingCerts.AppendLine(Enum.GetName(typeof(PAPCertificate.PAP_Certificates),lstMissingCerts[i]));
                }
                GuiLogMessage("Following certificates are missing. They will be installed now.\n" + sbMissingCerts.ToString(), NotificationLevel.Info);

                // try/catch neccessary because the CT-Editor doesn't support the whole exception display process (e.g. shows only "unknown error.")
                try
                {
                    if (PAPCertificate.InstallMissingCertificates(lstMissingCerts, applicationDir))
                    {
                        GuiLogMessage("Installation of all missing certificates was successful.", NotificationLevel.Info);
                        retValue = true;
                    }
                    else
                    {
                        GuiLogMessage("No/not all missing certificates were installed successful.", NotificationLevel.Error);
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Error occured while installing certificates. Exception: " + ex.ToString(), NotificationLevel.Error);
                }
            }
            return retValue;
        }

        public void LogInternalState()
        {
            if(this.p2pBase != null)
                this.p2pBase.LogInternalState();
        }
        #endregion
    }

    public class P2PPeerMaster : IP2PControl
    {
        private AutoResetEvent systemJoined;
        private P2PPeer p2pPeer;
        private PeerId peerID;
        private string sPeerName;
        // used for every encoding stuff
        private Encoding enc = UTF8Encoding.UTF8;

        public P2PPeerMaster(P2PPeer p2pPeer)
        {
            this.p2pPeer = p2pPeer;
            this.systemJoined = new AutoResetEvent(false);

            this.p2pPeer.p2pBase.OnSystemJoined += new P2PBase.SystemJoined(p2pBase_OnSystemJoined);
            this.p2pPeer.OnPeerMessageReceived += new P2PBase.P2PMessageReceived(p2pPeer_OnPeerMessageReceived);
            this.OnStatusChanged += new IControlStatusChangedEventHandler(P2PPeerMaster_OnStatusChanged);
        }

        #region Events and Event-Handling
        
        private void p2pBase_OnSystemJoined()
        {
            systemJoined.Set();
        }

        // to forward event from overlay MessageReceived-Event from P2PBase
        // analyzes the type of message and throws depend upon this anaysis an event
        public event P2PPayloadMessageReceived OnPayloadMessageReceived;
        public event P2PSystemMessageReceived OnSystemMessageReceived;
        private void p2pPeer_OnPeerMessageReceived(PeerId sourceAddr, byte[] data)
        {
            switch (GetMessageType(data[0])) //analyses the first byte of data (index, which represents the MessageType)
            {
                case P2PMessageIndex.PubSub:
                    if (data.Length == 2)
                    {
                        if(OnSystemMessageReceived != null)
                            OnSystemMessageReceived(sourceAddr, GetPubSubType(data[1]));
                    }
                    else
                    {
                        throw (new Exception("Data seems to be from type 'PubSub', but is to long for it... Data: '" + enc.GetString(data) + "'"));
                    }
                    break;
                case P2PMessageIndex.Payload:
                    if(OnPayloadMessageReceived != null)
                        OnPayloadMessageReceived(sourceAddr, GetMessagePayload(data));
                    break;
                default:
                    // not implemented. System ignores these messages completely at present
                    break;
            }
        }

        public event IControlStatusChangedEventHandler OnStatusChanged;
        private void P2PPeerMaster_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            if (OnStatusChanged != null)
                OnStatusChanged(sender, readyForExecution);
        }

        #endregion

        public bool PeerStarted()
        {
            return this.p2pPeer.PeerStarted;
        }

        /// <summary>
        /// workaround method. If the PAP functions are used, but the PAP system isn't
        /// started yet. This could happen because of the plugin hierarchy and
        /// when a p2p-using plugin uses PAP functions in the PreExecution method,
        /// this could run into a race condition (peer plugin not computed by the CT-system 
        /// yet, but p2p-using plugin is already executed)
        /// </summary>
        /// <returns></returns>
        private bool SystemJoinedCompletely()
        {
            if (!this.p2pPeer.PeerStarted)
            {
                this.p2pPeer.StartPeer();
                this.systemJoined.WaitOne();
            }
            return true;
        }

        #region IP2PControl Members

        public bool DHTstore(string sKey, byte[] byteValue)
        {
            if(SystemJoinedCompletely())
                return this.p2pPeer.p2pBase.SynchStore(sKey, byteValue);
            return false;
        }

        public bool DHTstore(string sKey, string sValue)
        {
            if (SystemJoinedCompletely())
                return this.p2pPeer.p2pBase.SynchStore(sKey, sValue);
            return false;
        }

        public byte[] DHTload(string sKey)
        {
            if (SystemJoinedCompletely())
                return this.p2pPeer.p2pBase.SynchRetrieve(sKey);
            return null;
        }

        public bool DHTremove(string sKey)
        {
            if (SystemJoinedCompletely())
                return this.p2pPeer.p2pBase.SynchRemove(sKey);
            return false;
        }

        /// <summary>
        /// This method only contacts the p2p system, if the peerID wasn't requested before
        /// </summary>
        /// <param name="sPeerName">returns the Peer Name</param>
        /// <returns>returns the Peer ID</returns>
        public PeerId GetPeerID(out string sPeerName)
        {
            if (SystemJoinedCompletely())
            {
                if (this.peerID == null)
                {
                    this.peerID = this.p2pPeer.p2pBase.GetPeerID(out this.sPeerName);
                }
                sPeerName = this.sPeerName;
                return this.peerID;
            }
            sPeerName = this.sPeerName;
            return null;
        }

        public PeerId GetPeerID(byte[] byteId)
        {
            return p2pPeer.p2pBase.GetPeerID(byteId);
        }

        private void SendReadilyMessage(byte[] data, PeerId destinationAddress)
        {
            if (SystemJoinedCompletely())
                this.p2pPeer.p2pBase.SendToPeer(data, destinationAddress.ToByteArray());
        }

        // adds the P2PMessageIndex to the given byte-array
        public void SendToPeer(byte[] data, PeerId destinationAddress)
        {
            byte[] newData = GenerateMessage(data, P2PMessageIndex.Payload);
            SendReadilyMessage(newData, destinationAddress);
        }

        public void SendToPeer(string sData, PeerId destinationAddress)
        {
            byte[] data = GenerateMessage(sData, P2PMessageIndex.Payload);
            SendReadilyMessage(data, destinationAddress);
        }
        public void SendToPeer(PubSubMessageType msgType, PeerId destinationAddress)
        {
            byte[] data = GenerateMessage(msgType);
            SendReadilyMessage(data, destinationAddress);
        }

        #region Communication protocol

        /// <summary>
        /// generates a ct2- and p2p-compatible and processable message
        /// </summary>
        /// <param name="payload">payload data in bytes</param>
        /// <param name="msgIndex">type of message (system message, simple payload for a special use case, etc.)</param>
        /// <returns>the message, which is processable by the ct2/p2p system</returns>
        private byte[] GenerateMessage(byte[] payload, P2PMessageIndex msgIndex)
        {
            // first byte is the index, if it is payload or Publish/Subscriber stuff
            byte[] retByte = new byte[1 + payload.Length];
            retByte[0] = (byte)msgIndex;
            payload.CopyTo(retByte, 1);
            return retByte;
        }

        /// <summary>
        /// generates a ct2- and p2p-compatible and processable message
        /// </summary>
        /// <param name="sPayload">payload data as a string</param>
        /// <param name="msgIndex">type of message (system message, simple payload for a special use case, etc.)</param>
        /// <returns>the message, which is processable by the ct2/p2p system</returns>
        private byte[] GenerateMessage(string sPayload, P2PMessageIndex msgIndex)
        {
            return GenerateMessage(enc.GetBytes(sPayload), msgIndex);
        }

        /// <summary>
        /// generates a ct2- and p2p-compatible and processable message
        /// </summary>
        /// <param name="pubSubData">PubSubMessageType</param>
        /// <returns>the message, which is processable by the ct2/p2p system<</returns>
        private byte[] GenerateMessage(PubSubMessageType pubSubData)
        {
            byte[] bytePubSubData = new byte[] { (byte)pubSubData };
            return GenerateMessage(bytePubSubData, P2PMessageIndex.PubSub);
        }

        /// <summary>
        /// returns the message type, e.g. PubSub or Payload message
        /// </summary>
        /// <param name="msgType">the FIRST byte of a raw message, received by the system</param>
        /// <returns>the message type</returns>
        private P2PMessageIndex GetMessageType(byte msgType)
        {
            try
            {
                return (P2PMessageIndex)msgType;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// returns the message type, e.g. PubSub or Payload message (to accelarate this process, only assign first byte of the whole array message)
        /// </summary>
        /// <param name="message">the whole message as an byte array</param>
        /// <returns>the message type</returns>
        private P2PMessageIndex GetMessageType(byte[] message)
        {
            try
            {
                return (P2PMessageIndex)message[0];
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// returns only the payload part of the message
        /// </summary>
        /// <param name="message">the raw message, received by the system, as an byte array (with the first index byte!!!)</param>
        /// <returns>only the payload part of the message</returns>
        private byte[] GetMessagePayload(byte[] message)
        {
            if (message.Length > 1)
            {
                byte[] retMsg = new byte[message.Length - 1];
                // workaround because CopyTo doesn't work...
                //for (int i = 0; i < message.Length-1; i++)
                //{
                //    retMsg[i] = message[i + 1];
                //}
                Buffer.BlockCopy(message, 1, retMsg, 0, retMsg.Length);
                return retMsg;
            }
            return null;
        }

        #endregion


        /// <summary>
        /// Converts a string to the PubSubMessageType if possible. Otherwise return null.
        /// </summary>
        /// <param name="sData">Data</param>
        /// <returns>PubSubMessageType if possible. Otherwise null.</returns>
        private PubSubMessageType GetPubSubType(byte data)
        {
            // Convert one byte data to PublishSubscribeMessageType-Enum
            try
            {
                return (PubSubMessageType)data;
            }
            catch (Exception ex)
            {
                throw(ex);
            }
        }

        #endregion
    }
}
