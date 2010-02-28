using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using Cryptool.PluginBase.Control;

namespace Cryptool.Plugins.PeerToPeer
{
    public class SubscriberManagement
    {
        public delegate void SubscriberRemoved(PeerId peerId);
        /// <summary>
        /// this event is thrown when a subscriber/worker has been removed from managing list
        /// </summary>
        public event SubscriberRemoved OnSubscriberRemoved;

        private DateTime dateTimeNow;
        /// <summary>
        /// contains all active subscribers
        /// </summary>
        private Dictionary<PeerId, DateTime> checkList;
        /// <summary>
        /// when a peer is in this list, it will be deleted on the next vitality check
        /// </summary>
        private HashSet<PeerId> secondChanceList;

        private long expirationTime;
        /// <summary>
        /// Timespan in which subscriber gets marked secondChance first and twice is removed from Subscriber list.
        /// Latency of 3 seconds will be added because of network latency.
        /// </summary>
        public long ExpirationTime 
        {
            get { return this.expirationTime + 3000;  }
            set { this.expirationTime = value; } 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expirationTime">expiration Time of a subscriber in milliseconds</param>
        public SubscriberManagement(long expirationTime)
        {
            this.dateTimeNow = new DateTime();
            this.checkList = new Dictionary<PeerId, DateTime>();
            this.secondChanceList = new HashSet<PeerId>();
            this.ExpirationTime = expirationTime;
        }

        /// <summary>
        /// Add a subscriber to the subscriber list if it doesn't already exist
        /// </summary>
        /// <param name="subscriberId">ID of the Subscriber</param>
        /// <returns>true if subscriber wasn't in List and is added, otherwise false</returns>
        public virtual bool Add(PeerId subscriberId)
        {
            bool retValue = false;

            // locking checkList instead of activeSubsList, because all other functions work on checkList, not on activeSubsList
            lock (this.checkList)
            {
                if (!this.checkList.ContainsKey(subscriberId))
                {
                    this.dateTimeNow = DateTime.Now;
                    this.checkList.Add(subscriberId, this.dateTimeNow);
                    retValue = true;
                }
            } // end lock
            return retValue;

        }

        /// <summary>
        /// Updates the Timestamp of the given subscriber if it exists.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public bool Update(PeerId subscriberId)
        {
            this.dateTimeNow = DateTime.Now;
            if (this.checkList.ContainsKey(subscriberId))
            {
                this.checkList[subscriberId] = this.dateTimeNow;
                // remove subscriber from this list, because it's updated now and hence alive!
                if (this.secondChanceList.Contains(subscriberId))
                    this.secondChanceList.Remove(subscriberId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes Subscriber/Worker from all managment lists
        /// </summary>
        /// <param name="subscriberId">ID of the removed subscriber/worker</param>
        /// <returns></returns>
        public virtual bool Remove(PeerId subscriberId)
        {
            bool result = false;
            lock (this.checkList)
            {
                if (this.secondChanceList.Contains(subscriberId))
                    this.secondChanceList.Remove(subscriberId);
                if (this.checkList.ContainsKey(subscriberId))
                {
                    this.checkList.Remove(subscriberId);
                    result = true;
                }

                if (result && OnSubscriberRemoved != null)
                    OnSubscriberRemoved(subscriberId);
            }
            return result;
        }

        /// <summary>
        /// Removes all Subscribers which are long-rated outdated (expiration time is considered). 
        /// The recently outdated subscribers will be added to the returned second chance list.
        /// </summary>
        /// <returns>all recently outdated subscribers</returns>
        public List<PeerId> CheckVitality()
        {
            this.dateTimeNow = DateTime.Now;

            List<PeerId> removeSubscribersFromDict = new List<PeerId>();

            lock (this.checkList)
            {
                // added try/catch because checkList could be changed while iterating on it = boom!
                try
                {
                    foreach (KeyValuePair<PeerId, DateTime> entry in this.checkList)
                    {
                        DateTime valueWithExpirationTime = entry.Value.AddMilliseconds(ExpirationTime);

                        // if time is expired AND the ID is already in the secondChanceList --> Add to remove list
                        if (this.dateTimeNow > valueWithExpirationTime && secondChanceList.Contains(entry.Key))
                        {
                            removeSubscribersFromDict.Add(entry.Key);
                        }
                        else if (this.dateTimeNow > valueWithExpirationTime) //otherwise give a second chance
                        {
                            this.secondChanceList.Add(entry.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // don't handle this case, because outdated Peers will be added to the 2ndChanceList
                    // or will be removed the next time
                }
            } //end lock(this.checkList)

            foreach (PeerId removeSub in removeSubscribersFromDict)
            {
                Remove(removeSub);
            }

            return this.secondChanceList.ToList<PeerId>();
        }

        public List<PeerId> GetAllSubscribers()
        {
            return this.checkList.Keys.ToList<PeerId>();
        }

        public virtual void Dispose()
        {
            this.checkList.Clear();
            this.checkList = null;
            this.secondChanceList.Clear();
            this.secondChanceList = null;
        }

        private Encoding enc = Encoding.UTF8;
        /// <summary>
        /// serializes only the active subscribers list,
        /// either the second chance list or the timestamps
        /// are nonrelevant, because after Deserializing
        /// this stuff, the availablity is obsolete and have
        /// to be additionally checked 
        /// </summary>
        /// <returns></returns>
        public virtual byte[] Serialize()
        {
            byte[] ret = null; 
            lock (this.checkList)
            {
                if (this.checkList == null || this.checkList.Count == 0)
                {
                    return null;
                }
                List<PeerId> lstActivePeers = this.checkList.Keys.ToList<PeerId>();
                using (MemoryStream memStream = new MemoryStream())
                {
                    // first write dataset count to list
                    memStream.WriteByte(Convert.ToByte(lstActivePeers.Count));
                    // than write every peer id as an byte array - first byte is the length of the id
                    foreach (PeerId pid in lstActivePeers)
                    {
                        byte[] byPid = pid.ToByteArray();
                        byte[] enhancedByte = new byte[byPid.Length + 1];
                        //additional store PeerId length to ease reconstructing
                        enhancedByte[0] = Convert.ToByte(byPid.Length);
                        Buffer.BlockCopy(byPid, 0, enhancedByte, 1, byPid.Length);
                        memStream.Write(enhancedByte, 0, enhancedByte.Length);
                    }
                    ret = memStream.ToArray();
                }
            }
            return ret;
        }

        /// <summary>
        /// Deserializes a Subscriber/Worker list and reconstructs the PeerIds. Returns
        /// the deserialized PeerId, so you can ping the peers to check whether they are 
        /// available anymore.
        /// </summary>
        /// <param name="serializedPeerIds">the already serialized peerId byte-array</param>
        /// <param name="p2pControl">a reference to the p2pControl to convert deserialized byte arrays to valid PeerIds</param>
        /// <returns></returns>
        public virtual List<PeerId> Deserialize(byte[] serializedPeerIds, ref IP2PControl p2pControl)
        {
            if (serializedPeerIds == null || serializedPeerIds.Length < 2)
            {
                throw (new Exception("Invalid byte[] input - deserialization not possible"));
            }
            List<PeerId> deserializedPeers = new List<PeerId>();

            MemoryStream memStream = new MemoryStream(serializedPeerIds);
            try
            {
                int peerIdAmount = Convert.ToInt32(memStream.ReadByte());
                int peerIdLen, readResult;
                PeerId pid;
                for (int i = 0; i < peerIdAmount; i++)
                {
                    peerIdLen = Convert.ToInt32(memStream.ReadByte());
                    byte[] byPeerId = new byte[peerIdLen];
                    readResult = memStream.Read(byPeerId, 0, byPeerId.Length);
                    if (readResult == 0)
                        throw (new Exception("Deserialization process of the byte[] was canceled, because byte[] didn't achieve to the conventions."));
                    // create a new PeerId Object and add it to the list
                    pid = p2pControl.GetPeerID(byPeerId);
                    //deserializedPeers.Add(byPeerId);
                    deserializedPeers.Add(pid);
                }
            }
            catch (Exception ex)
            {
                memStream.Flush();
                memStream.Close();
                memStream.Dispose();
                deserializedPeers.Clear();
                deserializedPeers = null;
                throw new Exception("Deserialization process of byte[] was canceled, because byte[] didn't achieve to the conventions.", ex);
            }
            return deserializedPeers;
        }
    }
}
