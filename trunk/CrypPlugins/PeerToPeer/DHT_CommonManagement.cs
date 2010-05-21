using System;
using System.Text;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.PeerToPeer.Internal
{
    public static class DHT_CommonManagement
    {
        private static Encoding enc = Encoding.UTF8;
        private static string PublishersTimeStamp = "PubTimeStamp";
        private static string AliveMsg = "AliveMsg";
        private static string EncryptedData = "EncryptedText";
        private static string InitializationVector = "InitializationVector";

        /// <summary>
        /// Store the ID of the Publisher/Manager, so all Subscribers/Workers can retrieve the
        /// Publishers/Managers ID from the DHT to register themselves.
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <param name="publishersId">PeerId of the Publisher/Manager</param>
        /// <returns></returns>
        public static bool SetTopicsPublisherId(ref IP2PControl p2pControl, string sTopicName, PeerId publishersId)
        {
            return p2pControl.DHTstore(sTopicName, publishersId.ToByteArray());
        }

        /// <summary>
        /// Loads the actual Publishers-/Managers-ID out of the DHT.
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <returns>PeerId, if already in the DHT, otherwise null</returns>
        public static PeerId GetTopicsPublisherId(ref IP2PControl p2pControl, string sTopicName)
        {
            PeerId pid = null;
            byte[] byteLoad = p2pControl.DHTload(sTopicName);
            if (byteLoad != null)
                pid = p2pControl.GetPeerID(byteLoad);
            return pid;
        }

        /// <summary>
        /// Store alive message interval in DHT - necessary for maintanance all Subscriber/Worker-Lists.
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <param name="aliveMessageInterval">AliveMessageInterval in milliseconds</param>
        /// <returns>true, if storing value in the DHT was successfull.</returns>
        public static bool SetAliveMessageInterval(ref IP2PControl p2pControl, string sTopicName,
                                                   long aliveMessageInterval)
        {
            return p2pControl.DHTstore(sTopicName + AliveMsg, BitConverter.GetBytes(aliveMessageInterval));
        }

        /// <summary>
        /// Alive message interval, prescribed by the Publisher/Manager.
        /// In this interval - in milliseconds - the Subscribers/Workers have to send a alive message, 
        /// otherwise they will be removed from the Subscriber-/Worker-List.
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <returns></returns>
        public static long GetAliveMessageInterval(ref IP2PControl p2pControl, string sTopicName)
        {
            long ret = 0;
            byte[] byteLoad = p2pControl.DHTload(sTopicName + AliveMsg);
            if (byteLoad != null)
                ret = BitConverter.ToInt64(byteLoad, 0);
            return ret;
        }

        /// <summary>
        /// Storing encrypted Data in the DHT. This is necessary for all workers, who want to bruteforce
        /// the encryption.
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <param name="cStream">CryptoolStream from the Encryption-PlugIn</param>
        /// <returns></returns>
        public static bool SetEncryptedData(ref IP2PControl p2pControl, string sTopicName, CryptoolStream cStream)
        {
            bool encryptedTextStored = false;
            if (cStream != null && sTopicName != null && sTopicName != String.Empty && p2pControl != null)
            {
                var newEncryptedData = new CryptoolStream();
                newEncryptedData.OpenRead(cStream.FileName);
                if (newEncryptedData.CanRead)
                {
                    // Convert CryptoolStream to an byte Array and store it in the DHT
                    if (newEncryptedData.Length > Int32.MaxValue)
                        throw (new Exception(
                            "Encrypted Data are too long for this PlugIn. The maximum size of Data is " + Int32.MaxValue +
                            "!"));
                    var byteEncryptedData = new byte[newEncryptedData.Length];
                    int k = newEncryptedData.Read(byteEncryptedData, 0, byteEncryptedData.Length);
                    if (k < byteEncryptedData.Length)
                        throw (new Exception("Read Data are shorter than byteArrayLen"));
                    encryptedTextStored = p2pControl.DHTstore(sTopicName + EncryptedData, byteEncryptedData);
                }
                else
                {
                    throw (new Exception("Reading the CryptoolStream wasn't possible"));
                }
            }
            return encryptedTextStored;
        }

        /// <summary>
        /// Functions necessary for P2PWorkers. You will get the EncryptedData, necessary
        /// for bruteforcing.
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <returns></returns>
        public static byte[] GetEncryptedData(ref IP2PControl p2pControl, string sTopicName)
        {
            byte[] ret = null;
            if (p2pControl != null && sTopicName != null && sTopicName != String.Empty)
            {
                return p2pControl.DHTload(sTopicName + EncryptedData);
            }
            return ret;
        }

        /// <summary>
        /// Storing initialization vector if neccessary (some cipher-block-mode (e.g. CBC, OFB))
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <param name="initVector">the init vector</param>
        /// <returns>true, if storing value in the DHT was successfull.</returns>
        public static bool SetInitializationVector(ref IP2PControl p2pControl, string sTopicName, byte[] initVector)
        {
            return p2pControl.DHTstore(sTopicName + InitializationVector, initVector);
        }

        /// <summary>
        /// Functions necessary for P2PWorkers. You will get the Initialization Vector, necessary 
        /// for some cipher-block-mode (e.g. CBC, OFB) decryption. 
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <returns>the initialization vector if available, otherwise null</returns>
        public static byte[] GetInitializationVector(ref IP2PControl p2pControl, string sTopicName)
        {
            return p2pControl.DHTload(sTopicName + InitializationVector);
        }

        /// <summary>
        /// Removes all entries out of the DHT, which maintain the Publish/Subscriber-Network
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <returns></returns>
        public static bool DeleteAllPublishersEntries(ref IP2PControl p2pControl, string sTopicName)
        {
            bool id = p2pControl.DHTremove(sTopicName);
            bool alive = p2pControl.DHTremove(sTopicName + AliveMsg);
            bool timestamp = p2pControl.DHTremove(sTopicName + PublishersTimeStamp);
            //if timestamp wasn't removed successfully, that's no problem!
            return (id && alive);
        }

        /// <summary>
        /// Removes all entries out of the DHT, which maintain the Manager/Workers-Network
        /// </summary>
        /// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        /// <param name="sTopicName">the topic name on which all Peers are registered</param>
        /// <returns></returns>
        public static bool DeleteAllManagersEntries(ref IP2PControl p2pControl, string sTopicName)
        {
            bool encrypt = p2pControl.DHTremove(sTopicName + EncryptedData);
            bool init = p2pControl.DHTremove(sTopicName + InitializationVector);
            return (encrypt && init);
        }

        // new stuff - Arnie 2010.02.02

        ///// <summary>
        ///// Sets or updates the TimeStamp for the actual Publisher
        ///// </summary>
        ///// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        ///// <param name="sTopicName">the topic name on which all Peers are registered</param>
        ///// <returns>true, if storing succeeds, otherwise false</returns>
        //public static bool SetPublishersTimeStamp(ref IP2PControl p2pControl, string sTopicName)
        //{
        //    byte[] timeStamp = enc.GetBytes(DateTime.Now.ToString());
        //    bool store = p2pControl.DHTstore(sTopicName + PublishersTimeStamp, timeStamp);
        //}

        ///// <summary>
        /////  Gets the TimeStamp for the actual Publisher, if it's stored in the DHT (when a Publisher
        /////  is active, this value have to be occupied). Otherwise, when no Publisher is active, returns null.
        /////  If the DHT value is occupied, but parsing to DateTime wasn't possible, an Exception will be thrown
        ///// </summary>
        ///// <param name="p2pControl">necessary P2PControl to use DHT operation(s)</param>
        ///// <param name="sTopicName">the topic name on which all Peers are registered</param>
        ///// <returns>DateTime, if DHT entry is occupied, otherwise false.</returns>
        //public static DateTime GetPublishersTimeStamp(ref IP2PControl p2pControl, string sTopicName)
        //{
        //    DateTime retTimeStamp = null;
        //    byte[] timeStamp = p2pControl.DHTload(sTopicName + PublishersTimeStamp);
        //    //when timeStamp is null, DHT entry isn't occupied --> No Publisher is active at present
        //    if (timeStamp != null)
        //    {
        //        string sTimeStamp = enc.GetString(timeStamp);
        //        // if parsing isn't possible though DHT entry is occupied, throw an exception
        //        if (!DateTime.TryParse(sTimeStamp, out retTimeStamp))
        //        {
        //            throw (new Exception("Parsing the DHT entry '" + sTopicName + PublishersTimeStamp 
        //                + "' to DateTime wasn't possible. Value: '" + sTimeStamp + "'"));
        //        }
        //    }
        //    return retTimeStamp;
        //}
    }
}