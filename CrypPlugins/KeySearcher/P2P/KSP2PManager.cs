using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.P2P;
using Cryptool.P2P.Interfaces;

namespace KeySearcher.P2P
{
    /// <summary>
    /// This is a static Wrapper class for P2PManager.
    /// It is needed to enable Unit testing
    /// </summary>
    class KSP2PManager
    {
        public static P2PManagerWrapper wrapper = new P2PManagerWrapper();

        public static IRequestResult Retrieve(string key)
        {
            return wrapper.Retrieve(key);
        }

        public static IRequestResult Store(string key, byte[] data)
        {
            return wrapper.Store(key, data);
        }

        public static IRequestResult Remove(string key)
        {
            return wrapper.Remove(key);
        }
    }

    class P2PManagerWrapper
    {
        public virtual IRequestResult Retrieve(string key)
        {
            return P2PManager.Retrieve(key);
        }

        public virtual IRequestResult Store(string key, byte[] data)
        {
            return P2PManager.Store(key, data);
        }

        public virtual IRequestResult Remove(string key)
        {
            return P2PManager.Remove(key);
        }
    }
}
