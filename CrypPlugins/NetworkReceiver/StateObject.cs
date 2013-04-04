using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetworkReceiver
{
    class StateObject
    {
        public Socket workSocket = null;

        public const int BufferSize = 2048;

        public byte[] DataToReceive = new byte[BufferSize];
    }
}
