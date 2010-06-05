using System.ComponentModel;
using System.Windows;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.PeerToPeerProxy
{
    internal class P2PPeerSettings : ISettings
    {
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
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this,
                                         new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStop",
                                                                                                             Visibility.
                                                                                                                 Hidden)));
            ChangePluginIcon(PeerStatus.NotConnected);
        }

        #endregion

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
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