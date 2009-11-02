using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using System.Windows;

/*
 * TODO:
 * - Standardwerte in P2P-Settings setzen
 * - Start- und Stop-Button fürs Beenden und Starten des Peers auch außerhalb des Starts des PlugIns
 * 
 * FRAGE:
 * - Wie kann ich statt StringArrays die Enum-Werte als Parameter wählen?
 */

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

        // Parse String-Value to a valid Enum-Value
        private static T StringToEnum<T>(string sName)
        {
            return (T)Enum.Parse(typeof(T), sName);
        }

        public P2PPeerSettings (P2PBase p2pBase)
	    {
            if(TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStop", Visibility.Hidden)));
            this.p2pBase = p2pBase;
	    }

        #endregion

        #region Start- and Stop-Buttons incl. functionality

        [TaskPane("Start", "Initializes and starts Peer", null, 3, false, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnStart()
        {
            PeerStarted = !this.peerStarted;
        }

        //private int iTest = 2;
        //[PropertySaveOrder(4)]
        //[ContextMenu("CaptionText","ContextToolTip",4,DisplayLevel.Beginner,ContextMenuControlType.ComboBox,new int[]{1,2},"Welt","Hallo")]
        //[TaskPane("Testfunktion","",null,4,false,DisplayLevel.Beginner,ControlType.ComboBox,new string[]{"Welt","Hallo"})]
        //public int Test 
        //{ 
        //    get {return this.iTest;}
        //    set
        //    {
        //        if (value != this.iTest)
        //            this.iTest = value;
        //    }
        //}

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
                        this.p2pBase.InitializeAll(P2PPeerName, P2PWorldName, P2PLinkMngrType, P2PBSType, P2POverlType, P2PDhtType);
                        this.p2pBase.SynchStart();
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStart", Visibility.Collapsed)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStop", Visibility.Visible)));
                    }
                    else
                    {
                        // can not initialize Peer, because P2PUserName and/or P2PWorldName are missing
                        throw (new Exception("You must set P2PPeerName and/or P2PWorldName, otherwise starting the peer isn't possible"));
                    }
                }
                if (value != this.peerStarted)
                {
                    this.peerStarted = value;
                    //don't use the PeerStopped-Property, because you will run into a recursive loop!!!
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
                    this.p2pBase.SynchStop();
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStart", Visibility.Visible)));
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStop", Visibility.Collapsed)));
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
        public P2PLinkManagerType P2PLinkMngrType
        {
            get { return this.p2pLinkManagerType; }
            set
            {
                if (value != this.p2pLinkManagerType)
                {
                    this.p2pLinkManagerType = StringToEnum<P2PLinkManagerType>(value.ToString());
                    OnPropertyChanged("P2PLinkManagerType");
                    HasChanges = true;
                }
            }
        }

        //private P2PBootstrapperType p2pBSType = P2PBootstrapperType.LocalMachineBootstrapper;
        private P2PBootstrapperType p2pBSType;
        [TaskPane("Bootstrapper-Type", "Select the Bootstrapper-Type", "P2P Settings", 3, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "LocalMachineBootstrapper", "IrcBootstrapper" })]
        public P2PBootstrapperType P2PBSType
        {
            get { return this.p2pBSType; }
            set
            {
                if (value != this.p2pBSType)
                {
                    this.p2pBSType = StringToEnum<P2PBootstrapperType>(value.ToString());
                    OnPropertyChanged("P2PBSType");
                    HasChanges = true;
                }
            }
        }

        private P2POverlayType p2pOverlayType = P2POverlayType.FullMeshOverlay;
        [TaskPane("Overlay-Type", "Select the Overlay-Type", "P2P Settings", 4, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "FullMeshOverlay" })]
        public P2POverlayType P2POverlType
        {
            get { return this.p2pOverlayType; }
            set
            {
                if (value != this.p2pOverlayType)
                {
                    this.p2pOverlayType = StringToEnum<P2POverlayType>(value.ToString());
                    OnPropertyChanged("P2POverlType");
                    HasChanges = true;
                }
            }
        }

        private P2PDHTType p2pDhtType = P2PDHTType.FullMeshDHT;
        [TaskPane("DHT-Type", "Select the DHT-Type", "P2P Settings", 5, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "FullMeshDHT" })]
        public P2PDHTType P2PDhtType
        {
            get { return this.p2pDhtType;  }
            set
            {
                if (value != this.p2pDhtType)
                {
                    this.p2pDhtType = StringToEnum<P2PDHTType>(value.ToString());
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
    }
}
