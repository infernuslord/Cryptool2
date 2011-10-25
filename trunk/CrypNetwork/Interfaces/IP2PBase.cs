using System;
using Cryptool.P2P.Types;

namespace Cryptool.P2P.Interfaces
{
    public interface IP2PBase
    {
        /// <summary>
        ///   True if system was successfully joined, false if system is COMPLETELY left
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///   True if the underlying peer to peer system has been fully initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        ///   Initializes the underlying peer-to-peer system with settings configured in P2PSettings. This step is required in order to be able to establish a connection.
        /// </summary>
        void Initialize();

        /// <summary>
        ///   Starts the P2P System. When the given P2P world doesn't exist yet, 
        ///   inclusive creating the and bootstrapping to the P2P network.
        ///   In either case joining the P2P world.
        ///   This synchronized method returns true not before the peer has 
        ///   successfully joined the network (this may take one or two minutes).
        /// </summary>
        /// <exception cref = "InvalidOperationException">When the peer-to-peer system has not been initialized. 
        /// After validating the settings, this can be done by calling Initialize().</exception>
        /// <returns>True, if the peer has completely joined the p2p network</returns>
        bool SynchStart();

        /// <summary>
        ///   Disconnects from the peer-to-peer system.
        /// </summary>
        /// <returns>True, if the peer has completely left the p2p network</returns>
        bool SynchStop();

        void SendToPeer(byte[] data, byte[] destinationPeer);

        /// <summary>
        ///   Stores a value in the DHT at the given key
        /// </summary>
        /// <param name = "key">Key of DHT Entry</param>
        /// <param name = "value">Value of DHT Entry</param>
        /// <returns>True, when storing is completed!</returns>
        IRequestResult SynchStore(string key, string value);

        /// <summary>
        ///   Stores a value in the DHT at the given key
        /// </summary>
        /// <param name = "key">Key of DHT Entry</param>
        /// <param name = "data">Value of DHT Entry</param>
        /// <returns>True, when storing is completed!</returns>
        IRequestResult SynchStore(string key, byte[] data);

        /// <summary>
        ///   Get the value of the given DHT Key or null, if it doesn't exist.
        /// </summary>
        /// <param name = "key">Key of DHT Entry</param>
        /// <returns>Value of DHT Entry</returns>
        IRequestResult SynchRetrieve(string key);

        /// <summary>
        ///   Removes a key/value pair out of the DHT
        /// </summary>
        /// <param name = "key">Key of the DHT Entry</param>
        /// <returns>True, when removing is completed!</returns>
        IRequestResult SynchRemove(string key);

        long TotalBytesSentOnAllLinks();
        long TotalBytesReceivedOnAllLinks();

        /// <summary>
        ///   To log the internal state in the Monitoring Software of P@play
        /// </summary>
        void LogInternalState();

        event Delegates.SystemJoined OnSystemJoined;
        event Delegates.SystemLeft OnSystemLeft;
    }
}