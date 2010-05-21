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

        private readonly P2PPeer p2pPeer;

        #region ISettings Members

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasChanges { get; set; }

        #endregion

        #region taskPane

        public P2PPeerSettings(P2PPeer p2pPeer)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this,
                                         new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnStop",
                                                                                                             Visibility.
                                                                                                                 Hidden)));
            this.p2pPeer = p2pPeer;
            ChangePluginIcon(PeerStatus.NotConnected);
        }

        #endregion

        #region Start- and Stop-Buttons incl. functionality

        [TaskPane("Internal state dump", "Dumps the interla state of the P2P system to syslog.", "P2P Expert Settings",
            0, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnLogInternalState()
        {
            p2pPeer.LogInternalState();
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

        // Index depends on icon-position in P2PPeer-Class properties

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
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, (int) peerStatus));
        }
    }
}