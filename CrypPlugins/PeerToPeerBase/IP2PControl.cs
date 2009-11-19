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
        /// <summary>
        /// To register the subscriber with the publisher
        /// </summary>
        Register = 0,
        /// <summary>
        /// adequate response to a subscriber-sided 
        /// registering message
        /// </summary>
        RegisteringAccepted = 1,
        /// <summary>
        /// when peer wants to leave the publish/subscriber union
        /// </summary>
        Unregister = 2,
        /// <summary>
        /// To signalize the publisher that subscriber is still online/alive
        /// </summary>
        Alive = 3,
        /// <summary>
        /// active liveliness-request, the other side 
        /// must respond with a pong message
        /// </summary>
        Ping = 4, 
        /// <summary>
        /// adequate response to a 
        /// received ping message
        /// </summary>
        Pong = 5,
        /// <summary>
        /// subscriber sends this msg when solution was found
        /// </summary>
        Solution = 6,
        /// <summary>
        /// to immediately stop the subscribers work
        /// </summary>
        Stop = 7,
        /// <summary>
        /// because Enum is non-nullable, I used this workaround
        /// </summary>
        NULL = 666
    }
    #endregion

    public interface IP2PControl : IControl
    {
        bool DHTstore(string sKey, byte[] byteValue);
        bool DHTstore(string sKey, string sValue);
        byte[] DHTload(string sKey);
        bool DHTremove(string sKey);

        PeerId GetPeerID(out string sPeerName);
        //byte[] GetPeerID(out string sPeerName);

        void SendToPeer(string sData, PeerId destinationAddress);
        //void SendToPeer(string sData, byte[] sDestinationPeerAddress);
        //void SendToPeer(string sData, string sDestinationPeerAddress);
        void SendToPeer(PubSubMessageType msgType, PeerId sDestinationAddress);
        void SendToPeer(string sData, byte[] destinationAddress);
        //void SendToPeer(PubSubMessageType msgType, string sDestinationAddress);

        PubSubMessageType GetMsgType(string byteData);
        string ConvertIdToString(byte[] byteId);

        event P2PBase.P2PMessageReceived OnPeerReceivedMsg;
    }
}
