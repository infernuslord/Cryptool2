using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.P2P.Internal;

namespace KeySearcher.P2P.UnitTests
{
    class RandomP2PWrapper : P2PManagerWrapper
    {
        public override RequestResult Retrieve(string key)
        {
            return null;
        }

        public override RequestResult Store(string key, byte[] data)
        {
            return null;
        }

        public override RequestResult Remove(string key)
        {
            return null;
        }
    }
}
