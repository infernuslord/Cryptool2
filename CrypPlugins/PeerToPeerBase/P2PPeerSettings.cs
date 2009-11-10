using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using System.Windows;

namespace Cryptool.Plugins.PeerToPeer
{
    class P2PPeerSettings : ISettings
    {
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private bool hasChanges = false;
        private P2PBase p2pBase;

        #region ISettings Members

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        #endregion

        #region taskPane

        public P2PPeerSettings (P2PBase p2pBase)
	    {
            if(TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStop", Visibility.Hidden)));
            this.p2pBase = p2pBase;
            ChangePluginIcon(PeerStatus.NotConnected);
	    }

        #endregion

        #region Start- and Stop-Buttons incl. functionality

        [TaskPane("Start", "Initializes and starts Peer", null, 3, false, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnStart()
        {
            PeerStarted = !this.peerStarted;
        }

        private bool peerStarted = false;
        /// <summary>
        /// If peer isn't started by clicking the Button, it will be started by setting to true
        /// </summary>
        public bool PeerStarted
        {
            get { return this.peerStarted; }
            set
            {
                if (!this.peerStarted)
                {
                    if (P2PPeerName != null && P2PWorldName != null)
                    {
                        ChangePluginIcon(PeerStatus.Connecting);
                        this.p2pBase.Initialize(P2PPeerName, P2PWorldName, (P2PLinkManagerType)P2PLinkMngrType,
                            (P2PBootstrapperType)P2PBSType, (P2POverlayType)P2POverlType, (P2PDHTType)P2PDhtType);
                        this.p2pBase.SynchStart();
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStart", Visibility.Collapsed)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStop", Visibility.Visible)));
                        ChangePluginIcon(PeerStatus.Online);
                    }
                    else
                    {
                        ChangePluginIcon(PeerStatus.Error);
                        // can not initialize Peer, because P2PUserName and/or P2PWorldName are missing
                        throw (new Exception("You must set P2PPeerName and/or P2PWorldName, otherwise starting the peer isn't possible"));
                    }
                }
                if (value != this.peerStarted)
                {
                    this.peerStarted = value;
                    //use the private Var instead of the PeerStopped-Property, because you will run into a recursive loop!!!
                    this.peerStopped = !value; 
                    OnPropertyChanged("PeerStarted");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("Stop", "Stops the Peer", null, 4, false, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnStop()
        {
            PeerStopped = !this.peerStopped;
        }

        private bool peerStopped = true;
        /// <summary>
        /// if peer is already started, it will be stopped by setting to true
        /// </summary>
        public bool PeerStopped
        {
            get { return this.peerStopped; }
            set
            {
                if (this.peerStarted)
                {
                    ChangePluginIcon(PeerStatus.Connecting);
                    this.p2pBase.SynchStop();
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStart", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStop", Visibility.Collapsed)));
                    ChangePluginIcon(PeerStatus.NotConnected);
                }
                if (value != this.peerStopped)
                {
                    //don't use the PeerStarted-Property, because you will run into a recursive loop!!!
                    this.peerStarted = !value;
                    this.peerStopped = value;
                    OnPropertyChanged("PeerStopped");
                    HasChanges = true;
                }
            }
        }

        #endregion

        #region Setting-Fields

        private string p2pPeerName;
        [TaskPane("P2P Peer Name", "P2P Name of the Peer.", null, 0, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string P2PPeerName
        {
            get { return this.p2pPeerName; }
            set
            {
                if (value != this.p2pPeerName)
                {
                    this.p2pPeerName = value;
                    OnPropertyChanged("P2PPeerName");
                    HasChanges = true;
                }
            }
        }

        private string p2pWorldName;
        [TaskPane("P2P World", "P2P Name of the world.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string P2PWorldName
        {
            get { return this.p2pWorldName; }
            set
            {
                if (value != this.p2pWorldName)
                {
                    this.p2pWorldName = value;
                    OnPropertyChanged("P2PWorldName");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("Log internal state of peer", "Log internal state of peer", null, 2, false, DisplayLevel.Beginner, ControlType.Button)]
        public void btnTest()
        {
            this.p2pBase.LogInternalState();
        }

        private P2PLinkManagerType p2pLinkManagerType = P2PLinkManagerType.Snal;
        [TaskPane("LinkManager-Type", "Select the LinkManager-Type", "P2P Settings", 2, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Snal" })]
        public int P2PLinkMngrType
        {
            get { return (int)this.p2pLinkManagerType; }
            set
            {
                if ((P2PLinkManagerType)value != this.p2pLinkManagerType)
                {
                    this.p2pLinkManagerType = (P2PLinkManagerType)value;
                    OnPropertyChanged("P2PLinkManagerType");
                    HasChanges = true;
                }
            }
        }

        private P2PBootstrapperType p2pBSType = P2PBootstrapperType.LocalMachineBootstrapper;
        [TaskPane("Bootstrapper-Type", "Select the Bootstrapper-Type", "P2P Settings", 3, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "LocalMachineBootstrapper", "IrcBootstrapper" })]
        public int P2PBSType
        {
            get { return (int)this.p2pBSType; }
            set
            {
                if ((P2PBootstrapperType)value != this.p2pBSType)
                {
                    this.p2pBSType = (P2PBootstrapperType)value;
                    OnPropertyChanged("P2PBSType");
                    HasChanges = true;
                }
            }
        }

        private P2POverlayType p2pOverlayType = P2POverlayType.FullMeshOverlay;
        [TaskPane("Overlay-Type", "Select the Overlay-Type", "P2P Settings", 4, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "FullMeshOverlay" })]
        public int P2POverlType
        {
            get { return (int)this.p2pOverlayType; }
            set
            {
                if ((P2POverlayType)value != this.p2pOverlayType)
                {
                    this.p2pOverlayType = (P2POverlayType)value;
                    OnPropertyChanged("P2POverlType");
                    HasChanges = true;
                }
            }
        }

        private P2PDHTType p2pDhtType = P2PDHTType.FullMeshDHT;
        [TaskPane("DHT-Type", "Select the DHT-Type", "P2P Settings", 5, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "FullMeshDHT" })]
        public int P2PDhtType
        {
            get { return (int)this.p2pDhtType;  }
            set
            {
                if ((P2PDHTType)value != this.p2pDhtType)
                {
                    this.p2pDhtType = (P2PDHTType)value;
                    OnPropertyChanged("P2PDhtType");
                    HasChanges = true;
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

        // Index depends on icon-position in P2PPeer-Class properties
        private enum PeerStatus
        {
            Connecting = 1,
            Online = 2,
            Error = 3,
            NotConnected = 0
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(PeerStatus peerStatus)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, 
                new StatusEventArgs(StatusChangedMode.ImageUpdate, (int)peerStatus));
        }
    }
}
