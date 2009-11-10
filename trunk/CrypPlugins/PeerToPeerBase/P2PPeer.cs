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

        public P2PPeer()
        {
            this.p2pBase = new P2PBase();
            // to forward event from overlay/dht MessageReceived-Event from P2PBase
            this.p2pBase.OnP2PMessageReceived += new P2PBase.P2PMessageReceived(p2pBase_OnP2PMessageReceived);
            this.settings = new P2PPeerSettings(p2pBase);
            this.settings.TaskPaneAttributeChanged += new TaskPaneAttributeChangedHandler(settings_TaskPaneAttributeChanged);
            this.settings.OnPluginStatusChanged += new StatusChangedEventHandler(settings_OnPluginStatusChanged);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, args);
        }

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        private void p2pBase_OnP2PMessageReceived(byte[] byteSourceAddr, string sData)
        {
            if (OnPeerMessageReceived != null)
                OnPeerMessageReceived(byteSourceAddr, sData);
        }

        void settings_TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        #region IPlugin Members

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

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
                if (!settings.PeerStarted)
                {
                    // starts peer in the settings class and enables/disables Controlbuttons
                    this.settings.PeerStarted = true;
                    GuiLogMessage("Successfully joined the P2P System", NotificationLevel.Info);
                }
            }
        }

        public void Execute()
        {
            // TODO: For future use copy functionality to Execute instead of PreExecute
            //       so we don't need the workaround anymore!!!
            //if (!settings.PeerStarted)
            //{
            //    // starts peer in the settings class and enables/disables Controlbuttons
            //    this.settings.PeerStarted = true;
            //}
        }

        public void process(IP2PControl sender)
        {
            GuiLogMessage("P2P Peer method 'process' is executed, because status of P2PSlave has changed!", NotificationLevel.Debug);
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
            //settings are already set to null in this Step...
            //unsolved design problem in CT2...
            //this.settings.PeerStopped = true;
            if (this.p2pBase != null)
            {
                this.p2pBase.SynchStop();
                this.p2pBase = null;
            }
        }

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
            set
            {
                if (this.p2pSlave != null)
                {
                    this.p2pSlave.OnStatusChanged -= p2pSlave_OnStatusChanged;
                }

                this.p2pSlave.OnStatusChanged += new IControlStatusChangedEventHandler(p2pSlave_OnStatusChanged);
                //Only when using asynchronous p2p-Start-method, add event handler for OnPeerJoinedCompletely
                //this.p2pSlave.OnPeerJoinedCompletely += new PeerJoinedP2P(OnPeerJoinedCompletely);

                if (this.p2pSlave != value)
                {
                    this.p2pSlave = (P2PPeerMaster)value;
                    OnPropertyChanged("P2PControlSlave");
                }
            }
        }

        void p2pSlave_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            if (readyForExecution)
                this.process((IP2PControl)sender);
        }

        #endregion
    }

    public class P2PPeerMaster : IP2PControl
    {
        private P2PPeer p2pPeer;
        private byte[] bytePeerID;
        private string sPeerID;
        private string sPeerName;

        public P2PPeerMaster(P2PPeer p2pPeer)
        {
            this.p2pPeer = p2pPeer;
            // to forward event from overlay/dht MessageReceived-Event from P2PBase
            this.p2pPeer.OnPeerMessageReceived += new P2PBase.P2PMessageReceived(p2pPeer_OnPeerMessageReceived);
            this.OnStatusChanged += new IControlStatusChangedEventHandler(P2PPeerMaster_OnStatusChanged);
        }

        #region Events and Event-Handling

        // to forward event from overlay/dht MessageReceived-Event from P2PBase
        public event P2PBase.P2PMessageReceived OnPeerReceivedMsg;
        private void p2pPeer_OnPeerMessageReceived(byte[] byteSourceAddr, string sData)
        {
            if (OnPeerReceivedMsg != null)
                OnPeerReceivedMsg(byteSourceAddr, sData);
        }

        public event IControlStatusChangedEventHandler OnStatusChanged;
        private void P2PPeerMaster_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            if (OnStatusChanged != null)
                OnStatusChanged(sender, readyForExecution);
        }

        #endregion

        #region IP2PControl Members

        public bool DHTstore(string sKey, byte[] byteValue)
        {
            return this.p2pPeer.p2pBase.SynchStore(sKey, byteValue);
        }

        public bool DHTstore(string sKey, string sValue)
        {
            return this.p2pPeer.p2pBase.SynchStore(sKey, sValue);
        }

        public byte[] DHTload(string sKey)
        {
            return this.p2pPeer.p2pBase.SynchRetrieve(sKey);
        }

        public bool DHTremove(string sKey)
        {
            // derzeit liegt wohl in peerq@play ein Fehler in der Methode...
            // erkennt den Übergabeparameter nicht an und wirft dann "ArgumentNotNullException"...
            // Problem an M.Helling und S.Holzapfel von p@p weitergegeben...
            return this.p2pPeer.p2pBase.SynchRemove(sKey);
            //return false;
        }

        /// <summary>
        /// This method only contacts the p2p system, if the peerID wasn't requested before
        /// </summary>
        /// <param name="sPeerName">returns the Peer Name</param>
        /// <returns>returns the Peer ID</returns>
        public byte[] GetPeerID(out string sPeerName)
        {
            if (this.bytePeerID == null)
            {
                this.bytePeerID = this.p2pPeer.p2pBase.GetPeerID(out this.sPeerName);
                this.sPeerID = ConvertPeerId(bytePeerID);
            }
            sPeerName = this.sPeerName;
            return this.bytePeerID;
        }

        public string ConvertPeerId(byte[] bytePeerId)
        {
            string sRet = String.Empty;
            for (int i = 0; i < bytePeerId.Length; i++)
            {
                sRet += bytePeerId[i].ToString() + ":";
            }
            return sRet.Substring(0, sRet.Length - 1);
        }

        public void SendToPeer(string sData, byte[] byteDestinationPeerAddress)
        {
            this.p2pPeer.p2pBase.SendToPeer(sData, byteDestinationPeerAddress);
        }

        #endregion
    }

    //public class P2PPeerMaster : IP2PControl
    //{
    //    private P2PBase p2pBase;

    //    public P2PPeerMaster(P2PBase p2pBase)
    //    {
    //        this.p2pBase = p2pBase;
    //        this.OnPeerReceivedMsg += new P2PBase.P2PMessageReceived(P2PPeerMaster_OnPeerReceivedMsg);
    //        this.OnStatusChanged += new IControlStatusChangedEventHandler(P2PPeerMaster_OnStatusChanged);
    //    }

    //    #region Events and Event-Handling

    //    public event P2PBase.P2PMessageReceived OnPeerReceivedMsg;
    //    private void P2PPeerMaster_OnPeerReceivedMsg(byte[] byteSourceAddr, string sData)
    //    {
    //        if (OnPeerReceivedMsg != null)
    //            OnPeerReceivedMsg(byteSourceAddr, sData);
    //    }

    //    public event IControlStatusChangedEventHandler OnStatusChanged;
    //    private void P2PPeerMaster_OnStatusChanged(IControl sender, bool readyForExecution)
    //    {
    //        if (OnStatusChanged != null)
    //            OnStatusChanged(sender, readyForExecution);
    //    }

    //    #endregion

    //    #region IP2PControl Members

    //    public bool DHTstore(string sKey, string sValue)
    //    {
    //        return this.p2pBase.SynchStore(sKey, sValue);
    //    }

    //    public byte[] DHTload(string sKey)
    //    {
    //        return this.p2pBase.SynchRetrieve(sKey);
    //    }

    //    public bool DHTremove(string sKey)
    //    {
    //        // derzeit liegt wohl in peerq@play ein Fehler in der Methode...
    //        // erkennt den Übergabeparameter nicht an und wirft dann "ArgumentNotNullException"...
    //        // Problem an M.Helling und S.Holzapfel von p@p weitergegeben...
    //        return this.p2pBase.SynchRemove(sKey);
    //        //return false;
    //    }

    //    public byte[] GetPeerName()
    //    {
    //        return this.p2pBase.GetPeerName();
    //    }

    //    public void SendToPeer(string sData, byte[] byteDestinationPeerAddress)
    //    {
    //        this.p2pBase.SendToPeer(sData, byteDestinationPeerAddress);
    //    }

    //    #endregion
    //}
}
