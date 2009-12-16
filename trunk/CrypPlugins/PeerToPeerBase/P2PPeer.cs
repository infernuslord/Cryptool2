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
        /// <summary>
        /// dirty workaround!!!
        /// </summary>
        private static bool bolFirstInitalisation = true;
        private P2PPeerSettings settings;
        private IP2PControl p2pSlave;

        #endregion

        #region Standard functionality

        public P2PPeer()
        {
            this.p2pBase = new P2PBase();
            // to forward event from overlay/dht MessageReceived-Event from P2PBase
            this.p2pBase.OnP2PMessageReceived += new P2PBase.P2PMessageReceived(p2pBase_OnP2PMessageReceived);
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
        private void p2pBase_OnP2PMessageReceived(PeerId sourceAddr, string sData)
        {
            if (OnPeerMessageReceived != null)
                OnPeerMessageReceived(sourceAddr, sData);
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
            // nicht in PreExecute möglich, da schon beim Laden von CT2 gefeuert. Input-Data werden erst NACH PreExecute gesetzt!
            if (bolFirstInitalisation)
            {
                bolFirstInitalisation = false;
            }
            else
            {
                StartPeer();
            }
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
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
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

                this.p2pBase.Initialize(this.settings.P2PPeerName, this.settings.P2PWorldName,
                    (P2PLinkManagerType)this.settings.P2PLinkMngrType, (P2PBootstrapperType)this.settings.P2PBSType,
                    (P2POverlayType)this.settings.P2POverlType, (P2PDHTType)this.settings.P2PDhtType);
                this.PeerStarted = this.p2pBase.SynchStart();

                if (this.PeerStarted)
                {
                    this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Online);
                    GuiLogMessage("Successfully joined the P2P System", NotificationLevel.Info);
                }
                else
                {
                    this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.Error);
                    GuiLogMessage("Joining to P2P System failed!", NotificationLevel.Error);
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
                    this.settings.PeerStatusChanged(P2PPeerSettings.PeerStatus.NotConnected);
                    GuiLogMessage("Peer stopped: " + !this.PeerStarted, NotificationLevel.Info);
                }
            }
            else
            {
                GuiLogMessage("Peer is already stopped!", NotificationLevel.Info);
            }
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

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        public event P2PBase.P2PMessageReceived OnPeerReceivedMsg;
        private void p2pPeer_OnPeerMessageReceived(PeerId sourceAddr, string sData)
        {
            if (OnPeerReceivedMsg != null)
                OnPeerReceivedMsg(sourceAddr, sData);
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

        public void SendToPeer(string sData, PeerId destinationAddress)
        {
            if (SystemJoinedCompletely())
                this.p2pPeer.p2pBase.SendToPeer(sData, destinationAddress);
        }
        public void SendToPeer(PubSubMessageType msgType, PeerId destinationAddress)
        {
            if (SystemJoinedCompletely())
                this.p2pPeer.p2pBase.SendToPeer(msgType, destinationAddress);
        }
        public void SendToPeer(string sData, byte[] destinationAddress)
        {
            if (SystemJoinedCompletely())
                this.p2pPeer.p2pBase.SendToPeer(sData, destinationAddress);
        }

        /// <summary>
        /// Converts a string to the PubSubMessageType if possible. Otherwise return null.
        /// </summary>
        /// <param name="sData">Data</param>
        /// <returns>PubSubMessageType if possible. Otherwise null.</returns>
        public PubSubMessageType GetMsgType(string sData)
        {
            // Convert one byte data to PublishSubscribeMessageType-Enum
            int iMsg;
            if (sData.Trim().Length == 1 && Int32.TryParse(sData.Trim(), out iMsg))
            {
                return (PubSubMessageType)iMsg;
            }
            else
            {
                // because Enum is non-nullable, I used this workaround
                return PubSubMessageType.NULL;
            }
        }

        #endregion
    }
}
