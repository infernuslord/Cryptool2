using Cryptool.P2P.Types;

namespace Cryptool.P2P.Interfaces
{
    public interface IConnectionManager
    {
        event Delegates.P2PConnectionStateChangeEventHandler OnP2PConnectionStateChangeOccurred;
        event Delegates.P2PTryConnectingStateChangeEventHandler OnP2PTryConnectingStateChangeOccurred;
        bool Disconnected { get; }
        bool IsConnecting { get; }
        void Connect();
        void Disconnect();
        bool IsReadyToConnect();
        void FireConnectionStatusChange();
    }
}