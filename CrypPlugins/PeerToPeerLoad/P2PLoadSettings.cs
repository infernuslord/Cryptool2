using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.Plugins.PeerToPeer
{
    class P2PLoadSettings : ISettings
    {
        private bool hasChanges = false;
        private P2PLoad p2pPlugin;

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


        public P2PLoadSettings (P2PLoad p2pPlugin)
	    {
            this.p2pPlugin = p2pPlugin;
	    }

        [TaskPane("Log internal state of peer", "Log internal state of peer", null, 2, false, DisplayLevel.Beginner, ControlType.Button)]
        public void btnTest()
        {
            this.p2pPlugin.LogInternalState();
        }

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

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

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
