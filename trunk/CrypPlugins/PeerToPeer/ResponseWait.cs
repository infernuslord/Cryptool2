using System.Threading;
using PeersAtPlay.P2PStorage.DHT;

namespace Cryptool.Plugins.PeerToPeer.Internal
{
    public class ResponseWait
    {
        public byte[] Message;
        public AutoResetEvent WaitHandle;
        public string key;

        public bool success;
        public byte[] value;
        public OperationStatus operationStatus;
    }
}