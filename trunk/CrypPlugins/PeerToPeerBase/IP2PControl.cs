using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{
    #region P2P Initialisation Enums
    public enum P2PLinkManagerType
    {
        Snal
    }

    public enum P2PBootstrapperType
    {
        LocalMachineBootstrapper,
        IrcBootstrapper
    }

    public enum P2POverlayType
    { 
        FullMeshOverlay
    }

    public enum P2PDHTType
    {
        FullMeshDHT
    }
    #endregion

    public interface IP2PControl : IControl
    {
        bool DHTstore(string sKey, string sValue);
        byte[] DHTload(string sKey);
        bool DHTremove(string sKey);

        string GetPeerName();

        /// <summary>
        /// delegate is defined in Cryptool.PluginBase.Delegates.cs
        /// </summary>
        //event PeerJoinedP2P OnPeerJoinedCompletely;
    }
}
