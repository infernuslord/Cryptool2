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
        private P2PPeer p2pPeer;

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

        public P2PPeerSettings (P2PPeer p2pPeer)
	    {
            if(TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStop", Visibility.Hidden)));
            this.p2pPeer = p2pPeer;
            ChangePluginIcon(PeerStatus.NotConnected);
	    }

        #endregion

        #region Start- and Stop-Buttons incl. functionality

        /** ATTENTION: Change property changeableWhileExecuting to false for the Start and Stop Button.
         * Only set to true for testing issues.*/

        //[TaskPane("Start", "Initializes and starts Peer", null, 2, false, DisplayLevel.Beginner, ControlType.Button)]
        [TaskPane("Start", "Connects to the P2P-system.", null, 2, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnStart()
        {
            if (P2PWorldName != null)
            {
                this.p2pPeer.StartPeer();
            }
            else
            {
                PeerStatusChanged(PeerStatus.Error);
                // can not initialize Peer, because P2PUserName and/or P2PWorldName are missing
                throw (new Exception("You must set the P2P username and worldname, otherwise starting the peer isn't possible"));
            }
        }

        //[TaskPane("Stop", "Stops the Peer", null, 3, false, DisplayLevel.Beginner, ControlType.Button)]
        [TaskPane("Stop", "Disconnects from the P2P-system", null, 3, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnStop()
        {
            this.p2pPeer.StopPeer();
         
            OnPropertyChanged("PeerStopped");
        }

        [TaskPane("Internal state dump", "Dumps the interla state of the P2P system to syslog.", "P2P Expert Settings", 0, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnLogInternalState()
        {
            this.p2pPeer.LogInternalState();
        }

        #endregion

        #region Setting-Fields

        private string p2pPeerName = PAPCertificate.CERTIFIED_PEER_NAME;
        [TaskPane("P2P Username", "Your username for the peers@play-system. Note that you need to have a corresponding certificate installed on your computer. If you don't know what this means, just leave the default \"CrypTool2\"-username and everything will be taken care of.", null, 0, false, DisplayLevel.Beginner, ControlType.TextBox)]
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

        //private bool p2pUseNatTraversal = false;
        //[TaskPane("Use NAT Traversal", "Activate/Deactivate NAT-Traversal (tunneling connections through NATs " +
        //    "and Firewalls, necessary to work in different networks in the same world)", "P2P Expert Settings", 0, false, 
        //    DisplayLevel.Beginner, ControlType.CheckBox)]
        //public bool P2PUseNatTraversal
        //{
        //    get { return this.p2pUseNatTraversal; }
        //    set 
        //    {
        //        if (value != this.p2pUseNatTraversal)
        //        {
        //            this.p2pUseNatTraversal = value;
        //            OnPropertyChanged("P2PUseNatTraversal");
        //            HasChanges = true;
        //        }
        //    }
        //}

        private bool log2Monitor = true;
        [TaskPane("Use syslogging", "Logs all p2p actions to the to a sylog-daemon on localhost.", "P2P Expert Settings", 1, false,
            DisplayLevel.Beginner, ControlType.CheckBox)]
        public bool Log2Monitor
        {
            get { return this.log2Monitor; }
            set
            {
                if (value != this.log2Monitor)
                {
                    this.log2Monitor = value;
                    OnPropertyChanged("Log2Monitor");
                    HasChanges = true;
                }
            }
        }

        private P2PLinkManagerType p2pLinkManagerType = P2PLinkManagerType.Snal;
        [TaskPane("LinkManager-Type", "Select the LinkManager-Type", "P2P Expert Settings", 2, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Snal" })]
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
        [TaskPane("Bootstrapper-Type", "Select the Bootstrapper-Type", "P2P Expert Settings", 3, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "LocalMachineBootstrapper", "IrcBootstrapper" })]
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
        [TaskPane("Overlay-Type", "Select the Overlay-Type", "P2P Expert Settings", 4, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "FullMeshOverlay" })]
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
        [TaskPane("DHT-Type", "Select the DHT-Type", "P2P Expert Settings", 5, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "FullMeshDHT" })]
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
        public enum PeerStatus
        {
            NotConnected = 0,
            Connecting = 1,
            Online = 2,
            Error = 3
        }

        /// <summary>
        /// Changes icon of P2PPeer and visibility of the control buttons in settings
        /// </summary>
        /// <param name="peerStat"></param>
        public void PeerStatusChanged(PeerStatus peerStat)
        {
            ChangePluginIcon(peerStat);
            // Only set visibility in final states!
            switch (peerStat)
            {
                case PeerStatus.Online:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(
                        new TaskPaneAttribteContainer("BtnStart", Visibility.Collapsed)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(
                        new TaskPaneAttribteContainer("BtnStop", Visibility.Visible)));
                    break;
                case PeerStatus.NotConnected:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(
                        new TaskPaneAttribteContainer("BtnStart", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(
                        new TaskPaneAttribteContainer("BtnStop", Visibility.Hidden)));
                    break;
                case PeerStatus.Error:
                case PeerStatus.Connecting:
                default:
                    break;
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(PeerStatus peerStatus)
        {
            if (OnPluginStatusChanged != null) 
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, (int)peerStatus));
        }
    }
}
