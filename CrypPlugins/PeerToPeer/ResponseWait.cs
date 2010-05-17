using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cryptool.Plugins.PeerToPeer.Internal
{
    public class ResponseWait
    {
        public AutoResetEvent WaitHandle = null;
        public byte[] Message = null;
        public string key = null;
        public byte[] value = null;

        public bool success = false;
    }

}
