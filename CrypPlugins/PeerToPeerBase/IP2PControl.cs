using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{    
    public interface IP2PControl : IControl
    {
        bool DHTstore(string sKey, string sValue);
        byte[] DHTload(string sKey);
        bool DHTremove(string sKey);

        /// <summary>
        /// delegate is defined in Cryptool.PluginBase.Delegates.cs
        /// </summary>
        //event PeerJoinedP2P OnPeerJoinedCompletely;
    }
}
