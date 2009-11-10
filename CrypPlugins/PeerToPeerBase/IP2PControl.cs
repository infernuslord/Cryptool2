using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.Plugins.PeerToPeer;

namespace Cryptool.PluginBase.Control
{
    #region P2P Initialisation Enums
    public enum P2PLinkManagerType
    {
        Snal = 0
    }

    public enum P2PBootstrapperType
    {
        LocalMachineBootstrapper = 0,
        IrcBootstrapper = 1
    }

    public enum P2POverlayType
    { 
        FullMeshOverlay = 0
    }

    public enum P2PDHTType
    {
        FullMeshDHT = 0
    }

    /// <summary>
    /// Message types for Publish/Subscriber systems
    /// </summary>
    public enum PubSubMessageType
    {
        Register = 0,
        Alive = 1,
        Ping = 2, 
        Pong = 3
    }
    #endregion

    public interface IP2PControl : IControl
    {
        bool DHTstore(string sKey, byte[] byteValue);
        bool DHTstore(string sKey, string sValue);
        byte[] DHTload(string sKey);
        bool DHTremove(string sKey);

        byte[] GetPeerID(out string sPeerName);

        string ConvertPeerId(byte[] bytePeerId);

        void SendToPeer(string sData, byte[] sDestinationPeerAddress);

        event P2PBase.P2PMessageReceived OnPeerReceivedMsg;
    }
}
