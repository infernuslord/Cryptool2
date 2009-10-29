using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PDHTremoveSettings : ISettings
    {
        private bool hasChanges = false;
        private P2P_DHTremove p2pPlugin;

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


        public P2PDHTremoveSettings(P2P_DHTremove p2pPlugin)
	    {
            this.p2pPlugin = p2pPlugin;
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
