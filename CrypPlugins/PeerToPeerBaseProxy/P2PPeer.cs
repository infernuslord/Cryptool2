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
using System.Linq;
using System.Text;
using System.Threading;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.Plugins.PeerToPeer.Internal;
using Cryptool.P2P;
using Cryptool.P2P.Internal;

namespace Cryptool.Plugins.PeerToPeerProxy
{
    [Author("Paul Lelgemann", "lelgemann@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "P2P_Peer_Proxy", "Creates a new Peer. Uses the CrypTool2 built-in P2P network and can be used as a replacement for P2P_Peer.", "", "PeerToPeerBaseProxy/icons/peer_inactive.png", "PeerToPeerBaseProxy/icons/peer_connecting.png", "PeerToPeerBaseProxy/icons/peer_online.png", "PeerToPeerBaseProxy/icons/peer_error.png")]
    public class P2PPeer : IIOMisc
    {
        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        public event P2PBase.P2PMessageReceived OnPeerMessageReceived;

        #region Variables

        private P2PPeerSettings settings;
        private IP2PControl p2pSlave;

        #endregion

        #region Standard functionality

        public P2PPeer()
        {
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
        public bool PeerStarted()
        {
            return P2PManager.Instance.P2PConnected();
        }

        public void StartPeer()
        {
            this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Connecting);
            P2PManager.Instance.P2PBase.OnP2PMessageReceived += new P2PBase.P2PMessageReceived(p2pBase_OnP2PMessageReceived);

            if (P2PManager.Instance.P2PConnected())
            {
                GuiLogMessage("P2P connected.", NotificationLevel.Info);
                this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Online);
            }
            else
            {
                GuiLogMessage("P2P network must be configured and connecting using the world button.", NotificationLevel.Warning);
                this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Error);

                // TODO use appropriate exception / abort procedure
                throw new ApplicationException("P2P unavailable.");
            }
        }

        public void StopPeer()
        {

            GuiLogMessage("Peer cannot be stopped, it is running in CrypTool!", NotificationLevel.Info);
        }
        #endregion Start and Stop Peer

        public void LogInternalState()
        {
            P2PManager.Instance.P2PBase.LogInternalState();
        }
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

            P2PManager.Instance.P2PBase.OnSystemJoined += new P2PBase.SystemJoined(p2pBase_OnSystemJoined);
            P2PManager.Instance.OnPeerMessageReceived += new P2PBase.P2PMessageReceived(p2pPeer_OnPeerMessageReceived);
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
            return this.p2pPeer.PeerStarted();
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
            return P2PManager.Instance.P2PConnected();
        }

        #region IP2PControl Members

        public bool DHTstore(string sKey, byte[] byteValue)
        {
            if (P2PManager.Instance.P2PConnected())
                return P2PManager.Store(sKey, byteValue);
            return false;
        }

        public bool DHTstore(string sKey, string sValue)
        {
            if (P2PManager.Instance.P2PConnected())
                return P2PManager.Store(sKey, sValue);
            return false;
        }

        public byte[] DHTload(string sKey)
        {
            if (P2PManager.Instance.P2PConnected())
                return P2PManager.Retrieve(sKey);
            return null;
        }

        public bool DHTremove(string sKey)
        {
            if (P2PManager.Instance.P2PConnected())
                return P2PManager.Remove(sKey);
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
                    this.peerID = P2PManager.Instance.P2PBase.GetPeerID(out this.sPeerName);
                }
                sPeerName = this.sPeerName;
                return this.peerID;
            }
            sPeerName = this.sPeerName;
            return null;
        }

        public PeerId GetPeerID(byte[] byteId)
        {
            return P2PManager.Instance.P2PBase.GetPeerID(byteId);
        }

        private void SendReadilyMessage(byte[] data, PeerId destinationAddress)
        {
            if (SystemJoinedCompletely())
                P2PManager.Instance.P2PBase.SendToPeer(data, destinationAddress.ToByteArray());
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
