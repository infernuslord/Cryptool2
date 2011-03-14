using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.P2P;
using Cryptool.P2P.Internal;

namespace KeySearcher.P2P
{
    /// <summary>
    /// This is a static Wrapper class for P2PManager.
    /// It is needed to enable Unit testing
    /// </summary>
    class KSP2PManager
    {
        public static P2PManagerWrapper wrapper = new P2PManagerWrapper();

        public static RequestResult Retrieve(string key)
        {
            return wrapper.Retrieve(key);
        }

        public static RequestResult Store(string key, byte[] data)
        {
            return wrapper.Store(key, data);
        }

        public static RequestResult Remove(string key)
        {
            return wrapper.Remove(key);
        }
    }

    class P2PManagerWrapper
    {
        public virtual RequestResult Retrieve(string key)
        {
            return P2PManager.Retrieve(key);
        }

        public virtual RequestResult Store(string key, byte[] data)
        {
            return P2PManager.Store(key, data);
        }

        public virtual RequestResult Remove(string key)
        {
            return P2PManager.Remove(key);
        }
    }
}
