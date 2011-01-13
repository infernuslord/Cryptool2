using System.ComponentModel;
using System.Windows;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.PeerToPeerProxy
{
    internal class P2PPeerSettings : ISettings
    {
        private readonly P2PProxySettings _settings;

        #region PeerStatus enum

        public enum PeerStatus
        {
            NotConnected = 0,
            Connecting = 1,
            Online = 2,
            Error = 3
        }

        #endregion

        #region ISettings Members

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasChanges { get; set; }

        #endregion

        #region taskPane

        public P2PPeerSettings()
        {
            ChangePluginIcon(PeerStatus.NotConnected);
            _settings = P2PProxySettings.Default;
        }

        #endregion

        [TaskPane("Autoconnect P2P network", "Autoconnect to the P2P network, when the workspace is executed.", null, 0, true,
            ControlType.CheckBox)]
        public bool Autoconnect
        {
            get { return _settings.Autoconnect; }
            set
            {
                if (value != _settings.Autoconnect)
                {
                    _settings.Autoconnect = value;
                    OnPropertyChanged("Autoconnect");
                    HasChanges = true;
                }
            }
        }

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }

            P2PProxySettings.Default.Save();
        }

        /// <summary>
        /// Changes icon of P2PPeer and visibility of the control buttons in settings
        /// </summary>
        /// <param name="peerStat"></param>
        public void PeerStatusChanged(PeerStatus peerStat)
        {
            ChangePluginIcon(peerStat);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void ChangePluginIcon(PeerStatus peerStatus)
        {
            if (OnPluginStatusChanged != null)
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, (int) peerStatus));
        }
    }
}