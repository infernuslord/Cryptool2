using System.Threading;
using Cryptool.P2P;
using Cryptool.PluginBase;
using KeySearcher.Properties;

namespace KeySearcher.P2P.Helper
{
    public class ConnectionHelper
    {
        private readonly KeySearcher keySearcher;
        private readonly KeySearcherSettings settings;
        private AutoResetEvent connectResetEvent;

        public ConnectionHelper(KeySearcher keySearcher, KeySearcherSettings settings)
        {
            this.keySearcher = keySearcher;
            this.settings = settings;
        }

        public void ValidateConnectionToPeerToPeerSystem()
        {
            if (P2PManager.IsConnected)
            {
                return;
            }

            if (settings.AutoconnectPeerToPeer)
            {
                HandleAutoconnect();
            }
            else
            {
                keySearcher.GuiLogMessage(Resources.P2P_network_not_connected_and_autoconnect_disabled__Cannot_compute_job_,
                                          NotificationLevel.Error);
            }
        }

        private void HandleAutoconnect()
        {
            P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred += HandleConnectionStateChange;
            connectResetEvent = new AutoResetEvent(false);

            P2PManager.Connect();

            connectResetEvent.WaitOne();

            if (P2PManager.IsConnected)
            {
                keySearcher.GuiLogMessage(Resources.P2P_network_was_connected_due_to_plugin_setting_,
                                          NotificationLevel.Info);
            }
            else
            {
                keySearcher.GuiLogMessage(Resources.P2P_network_could_not_be_connected_,
                                          NotificationLevel.Error);
            }
        }

        void HandleConnectionStateChange(object sender, bool newState)
        {
            connectResetEvent.Set();
        }
    }
}
