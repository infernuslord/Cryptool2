using System.Threading;
using Cryptool.P2P;
using Cryptool.PluginBase;

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
                keySearcher.GuiLogMessage("P2P network not connected and autoconnect disabled. Cannot compute job.",
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
                keySearcher.GuiLogMessage("P2P network was connected due to plugin setting.",
                                          NotificationLevel.Info);
            }
            else
            {
                keySearcher.GuiLogMessage("P2P network could not be connected.",
                                          NotificationLevel.Error);
            }
        }

        void HandleConnectionStateChange(object sender, bool newState)
        {
            connectResetEvent.Set();
        }
    }
}
