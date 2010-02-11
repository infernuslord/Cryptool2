﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.Plugins.PeerToPeer;
using Cryptool.PluginBase.Miscellaneous;

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
    /// Message types for maintaining the Publish/Subscriber systems
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

    /// <summary>
    /// necessary index for all p2p messages, which have to be processed by ct2
    /// </summary>
    public enum P2PMessageIndex
    {
        /// <summary>
        /// indicates, that a PubSubMessageType will follow
        /// </summary>
        PubSub = 0,
        /// <summary>
        /// indicates, that any kind of payload data will follow
        /// </summary>
        Payload = 1
    }
    #endregion

    public delegate void P2PPayloadMessageReceived(PeerId sender, byte[] data);
    public delegate void P2PSystemMessageReceived(PeerId sender, PubSubMessageType msgType);

    public interface IP2PControl : IControl
    {
        //event P2PBase.P2PMessageReceived OnPeerReceivedMsg;
        event P2PPayloadMessageReceived OnPayloadMessageReceived;
        event P2PSystemMessageReceived OnSystemMessageReceived;

        bool DHTstore(string sKey, byte[] byteValue);
        bool DHTstore(string sKey, string sValue);
        byte[] DHTload(string sKey);
        bool DHTremove(string sKey);

        bool PeerStarted();

        PeerId GetPeerID(out string sPeerName);
        PeerId GetPeerID(byte[] byteId);

        /// <summary>
        /// Sends data to the specified peer
        /// </summary>
        /// <param name="data">only send PAYLOAD data as an byte-array (attention: 
        /// don't add an index by yourselve, index will be added internally)</param>
        /// <param name="destinationAddress">the address of the destination peer</param>
        void SendToPeer(byte[] data, PeerId destinationAddress);
        /// <summary>
        /// Sends data to the specified peer
        /// </summary>
        /// <param name="sData">only send PAYLOAD data as a string</param>
        /// <param name="destinationAddress">the address of the destination peer</param>
        void SendToPeer(string sData, PeerId destinationAddress);
        /// <summary>
        /// Sends data to the specified peer
        /// </summary>
        /// <param name="msgType">a PubSub-System message</param>
        /// <param name="sDestinationAddress">the address of the destination peer</param>
        void SendToPeer(PubSubMessageType msgType, PeerId sDestinationAddress);
    }
}