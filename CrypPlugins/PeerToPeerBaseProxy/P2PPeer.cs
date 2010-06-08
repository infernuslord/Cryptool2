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
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using Cryptool.P2P.Worker;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.PeerToPeer.Internal;

namespace Cryptool.Plugins.PeerToPeerProxy
{
    [Author("Paul Lelgemann", "lelgemann@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "P2P_Peer_Proxy",
        "Creates a new Peer. Uses the CrypTool2 built-in P2P network and can be used as a replacement for P2P_Peer.", ""
        , "PeerToPeerBaseProxy/icons/peer_inactive.png", "PeerToPeerBaseProxy/icons/peer_connecting.png",
        "PeerToPeerBaseProxy/icons/peer_online.png", "PeerToPeerBaseProxy/icons/peer_error.png")]
    public class P2PPeer : IIOMisc
    {
        #region IIOMisc Members

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        public event P2PBase.P2PMessageReceived OnPeerMessageReceived;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            // for evaluation issues only
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this,
                                       new GuiLogEventArgs(p + "(" + DebugToFile.GetTimeStamp() + ")", this,
                                                           notificationLevel));
        }

        #region In and Output

        [PropertyInfo(Direction.ControlSlave, "Master Peer", "One peer to rule them all", "", true, false,
            DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public IP2PControl P2PControlSlave
        {
            get { return _p2PSlave ?? (_p2PSlave = new P2PPeerMaster(this)); }
        }

        #endregion

        #region Start and Stop Peer

        /// <summary>
        /// Status flag for starting and stopping peer only once.
        /// </summary>
        public bool PeerStarted()
        {
            return P2PManager.Instance.IsP2PConnected() && !P2PManager.Instance.IsP2PConnecting;
        }

        public void StartPeer()
        {
            _settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Connecting);
            P2PManager.Instance.P2PBase.OnP2PMessageReceived += P2PBaseOnP2PMessageReceived;

            if (P2PManager.Instance.IsP2PConnected())
            {
                GuiLogMessage("P2P connected.", NotificationLevel.Info);
                _settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Online);
            }
            else if (!P2PManager.Instance.IsP2PConnected() && P2PManager.Instance.IsP2PConnecting)
            {
                HandleAlreadyConnecting();
            }
            else
            {
                if (_settings.Autoconnect)
                {
                    HandleAutoconnect();
                }
                else
                {
                    GuiLogMessage("P2P network offline. Please check the autoconnect checkbox of the proxy plugin or connect manually first.",
                                      NotificationLevel.Error);
                    _settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Error);
                    throw new ApplicationException("Workaround for wrong error handling... Workspace should be stopped now.");
                }
            }
        }

        private void HandleAlreadyConnecting()
        {
            P2PManager.OnP2PConnectionStateChangeOccurred += HandleConnectionStateChange;
            _connectResetEvent = new AutoResetEvent(false);
            _connectResetEvent.WaitOne();

            if (P2PManager.Instance.IsP2PConnected())
            {
                GuiLogMessage("P2P connected.", NotificationLevel.Info);
                _settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Online);
            }
            else
            {
                GuiLogMessage("P2P network could not be connected.",
                              NotificationLevel.Error);
                _settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Error);
                throw new ApplicationException("Workaround for wrong error handling... Workspace should be stopped now.");
            }
        }

        private void HandleAutoconnect()
        {
            P2PManager.OnP2PConnectionStateChangeOccurred += HandleConnectionStateChange;
            _connectResetEvent = new AutoResetEvent(false);

            new ConnectionWorker(P2PManager.Instance.P2PBase).Start();

            _connectResetEvent.WaitOne();

            if (P2PManager.Instance.IsP2PConnected())
            {
                GuiLogMessage("P2P network was connected due to plugin setting.",
                              NotificationLevel.Info);
                _settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Online);
            }
            else
            {
                GuiLogMessage("P2P network could not be connected.",
                              NotificationLevel.Error);
                _settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Error);
                throw new ApplicationException("Workaround for wrong error handling... Workspace should be stopped now.");
            }
        }

        void HandleConnectionStateChange(object sender, bool newState)
        {
            _connectResetEvent.Set();
        }

        public void StopPeer()
        {
            P2PManager.Instance.P2PBase.OnP2PMessageReceived -= P2PBaseOnP2PMessageReceived;
            GuiLogMessage("Removed event registration, but peer cannot be stopped, it is running in CrypTool!", NotificationLevel.Info);
        }

        #endregion Start and Stop Peer

        #region Variables

        private IP2PControl _p2PSlave;
        private P2PPeerSettings _settings;
        private AutoResetEvent _connectResetEvent;

        #endregion

        #region Standard functionality

        public P2PPeer()
        {
            _settings = new P2PPeerSettings();
            _settings.OnPluginStatusChanged += SettingsOnPluginStatusChanged;
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public ISettings Settings
        {
            set { _settings = (P2PPeerSettings) value; }
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            StartPeer();
        }

        public void Execute()
        {
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

        private void SettingsOnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
                OnPluginStatusChanged(this, args);
        }

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        private void P2PBaseOnP2PMessageReceived(PeerId sourceAddr, byte[] data)
        {
            if (OnPeerMessageReceived != null)
                OnPeerMessageReceived(sourceAddr, data);
        }

        private static void SettingsTaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
        }

        #endregion
    }


    public class P2PPeerMaster : IP2PControl
    {
        private readonly Encoding _enc = Encoding.UTF8;
        private readonly P2PPeer _p2PPeer;
        private readonly AutoResetEvent _systemJoined;
        private PeerId _peerId;
        private string _sPeerName;
        // used for every encoding stuff

        public P2PPeerMaster(P2PPeer p2PPeer)
        {
            _p2PPeer = p2PPeer;
            _systemJoined = new AutoResetEvent(false);

            P2PManager.Instance.P2PBase.OnSystemJoined += P2PBaseOnSystemJoined;
            P2PManager.Instance.OnPeerMessageReceived += P2PPeerOnPeerMessageReceived;
            OnStatusChanged += P2PPeerMaster_OnStatusChanged;
        }

        #region Events and Event-Handling

        // to forward event from overlay MessageReceived-Event from P2PBase
        // analyzes the type of message and throws depend upon this anaysis an event
        public event P2PPayloadMessageReceived OnPayloadMessageReceived;
        public event P2PSystemMessageReceived OnSystemMessageReceived;

        public event IControlStatusChangedEventHandler OnStatusChanged;

        private void P2PBaseOnSystemJoined()
        {
            _systemJoined.Set();
        }

        private void P2PPeerOnPeerMessageReceived(PeerId sourceAddr, byte[] data)
        {
            switch (GetMessageType(data[0])) //analyses the first byte of data (index, which represents the MessageType)
            {
                case P2PMessageIndex.PubSub:
                    if (data.Length == 2)
                    {
                        if (OnSystemMessageReceived != null)
                            OnSystemMessageReceived(sourceAddr, GetPubSubType(data[1]));
                    }
                    else
                    {
                        throw (new Exception("Data seems to be from type 'PubSub', but is to long for it... Data: '" +
                                             _enc.GetString(data) + "'"));
                    }
                    break;
                case P2PMessageIndex.Payload:
                    if (OnPayloadMessageReceived != null)
                        OnPayloadMessageReceived(sourceAddr, GetMessagePayload(data));
                    break;
                default:
                    // not implemented. System ignores these messages completely at present
                    break;
            }
        }

        private void P2PPeerMaster_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            if (OnStatusChanged != null)
                OnStatusChanged(sender, readyForExecution);
        }

        #endregion

        #region IP2PControl Members

        public bool PeerStarted()
        {
            return _p2PPeer.PeerStarted();
        }

        public bool DHTstore(string sKey, byte[] byteValue)
        {
            return P2PManager.Instance.IsP2PConnected() && P2PManager.Store(sKey, byteValue);
        }

        public bool DHTstore(string sKey, string sValue)
        {
            return P2PManager.Instance.IsP2PConnected() && P2PManager.Store(sKey, sValue);
        }

        public byte[] DHTload(string sKey)
        {
            return P2PManager.Instance.IsP2PConnected() ? P2PManager.Retrieve(sKey) : null;
        }

        public bool DHTremove(string sKey)
        {
            return P2PManager.Instance.IsP2PConnected() && P2PManager.Remove(sKey);
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
                if (_peerId == null)
                {
                    _peerId = P2PManager.Instance.P2PBase.GetPeerId(out _sPeerName);
                }
                sPeerName = _sPeerName;
                return _peerId;
            }

            sPeerName = _sPeerName;
            return null;
        }

        public PeerId GetPeerID(byte[] byteId)
        {
            return P2PManager.Instance.P2PBase.GetPeerId(byteId);
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

        #endregion

        #region Communication protocol

        /// <summary>
        /// generates a ct2- and p2p-compatible and processable message
        /// </summary>
        /// <param name="payload">payload data in bytes</param>
        /// <param name="msgIndex">type of message (system message, simple payload for a special use case, etc.)</param>
        /// <returns>the message, which is processable by the ct2/p2p system</returns>
        private static byte[] GenerateMessage(byte[] payload, P2PMessageIndex msgIndex)
        {
            // first byte is the index, if it is payload or Publish/Subscriber stuff
            var retByte = new byte[1 + payload.Length];
            retByte[0] = (byte) msgIndex;
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
            return GenerateMessage(_enc.GetBytes(sPayload), msgIndex);
        }

        /// <summary>
        /// generates a ct2- and p2p-compatible and processable message
        /// </summary>
        /// <param name="pubSubData">PubSubMessageType</param>
        /// <returns>the message, which is processable by the ct2/p2p system</returns>
        private static byte[] GenerateMessage(PubSubMessageType pubSubData)
        {
            var bytePubSubData = new[] {(byte) pubSubData};
            return GenerateMessage(bytePubSubData, P2PMessageIndex.PubSub);
        }

        /// <summary>
        /// returns the message type, e.g. PubSub or Payload message
        /// </summary>
        /// <param name="msgType">the FIRST byte of a raw message, received by the system</param>
        /// <returns>the message type</returns>
        private static P2PMessageIndex GetMessageType(byte msgType)
        {
            return (P2PMessageIndex) msgType;
        }

        /// <summary>
        /// returns only the payload part of the message
        /// </summary>
        /// <param name="message">the raw message, received by the system, as an byte array (with the first index byte!!!)</param>
        /// <returns>only the payload part of the message</returns>
        private static byte[] GetMessagePayload(byte[] message)
        {
            if (message.Length > 1)
            {
                var retMsg = new byte[message.Length - 1];
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
        /// workaround method. If the PAP functions are used, but the PAP system isn't
        /// started yet. This could happen because of the plugin hierarchy and
        /// when a p2p-using plugin uses PAP functions in the PreExecution method,
        /// this could run into a race condition (peer plugin not computed by the CT-system 
        /// yet, but p2p-using plugin is already executed)
        /// </summary>
        /// <returns></returns>
        private static bool SystemJoinedCompletely()
        {
            return P2PManager.Instance.IsP2PConnected() && !P2PManager.Instance.IsP2PConnecting;
        }

        private static void SendReadilyMessage(byte[] data, PeerId destinationAddress)
        {
            if (SystemJoinedCompletely())
                P2PManager.Instance.P2PBase.SendToPeer(data, destinationAddress.ToByteArray());
        }

        /// <summary>
        /// Converts a string to the PubSubMessageType if possible. Otherwise return null.
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>PubSubMessageType if possible. Otherwise null.</returns>
        private static PubSubMessageType GetPubSubType(byte data)
        {
            // Convert one byte data to PublishSubscribeMessageType-Enum
            return (PubSubMessageType) data;
        }
    }
}