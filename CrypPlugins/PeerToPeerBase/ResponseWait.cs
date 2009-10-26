using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cryptool.Plugins.PeerToPeer
{
    public class ResponseWait
    {
        public AutoResetEvent WaitHandle = null;
        public byte[] Message = null;
    }

}
