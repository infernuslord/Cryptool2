using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

/*
 * TODO:
 * - Everything but activeSubList works fine.
 *   --> Removing object doesn't work because two DIFFERENT OBJECTS with the SAME content!
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public class PeerValue
    {
        public PeerId peerId;
        public DateTime dateTime;

        public PeerValue(PeerId pid, DateTime dt)
        {
            this.peerId = pid;
            this.dateTime = dt;
        }
    }

    public class SubscriberManagement
    {
        public delegate void SubscriberRemoved(PeerId peerId);
        public event SubscriberRemoved OnSubscriberRemoved;

        private DateTime dateTimeNow;
        /// <summary>
        /// contains all active subscribers
        /// </summary>
        private Dictionary<string, PeerValue> checkList;


        /*TESTING*/
        private Dictionary<string, PeerId> activeSubsList;
        //private List<PeerId> activeSubsList;


        /// <summary>
        /// when a peer is in this list, it will be deleted on the next vitality check
        /// </summary>
        private Dictionary<string,PeerId> secondChanceList;

        private long expirationTime;
        /// <summary>
        /// Timespan in which subscriber gets marked secondChance first and twice is removed from Subscriber list.
        /// Latency of 2 seconds will be added because of network latency.
        /// </summary>
        public long ExpirationTime 
        {
            get { return this.expirationTime * 2000;  }
            set { this.expirationTime = value; } 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expirationTime">expiration Time of a subscriber in milliseconds</param>
        public SubscriberManagement(long expirationTime)
        {
            this.activeSubsList = new Dictionary<string, PeerId>();
            //this.activeSubsList = new List<PeerId>();

            this.dateTimeNow = new DateTime();
            this.checkList = new Dictionary<string, PeerValue>();
            this.secondChanceList = new Dictionary<string,PeerId>();
            this.ExpirationTime = expirationTime;
        }

        /// <summary>
        /// Add a subscriber to the subscriber list if it doesn't already exist
        /// </summary>
        /// <param name="subscriberId">ID of the Subscriber</param>
        /// <returns>true if subscriber wasn't in List and is added, otherwise false</returns>
        public bool Add(PeerId subscriberId)
        {
            if (!this.checkList.ContainsKey(subscriberId.stringId))
            {
                this.dateTimeNow = DateTime.Now;
                this.checkList.Add(subscriberId.stringId, new PeerValue(subscriberId, this.dateTimeNow));
                //TESTING
                this.activeSubsList.Add(subscriberId.stringId, subscriberId);
                //this.activeSubsList.Add(subscriberId);
                return true;
            }
            else
                return false;

        }

        /// <summary>
        /// Updates the Timestamp of the given subscriber if it exists.
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public bool Update(PeerId subscriberId)
        {
            this.dateTimeNow = DateTime.Now;
            if (this.checkList.ContainsKey(subscriberId.stringId))
            {
                this.checkList[subscriberId.stringId].dateTime = this.dateTimeNow;
                // remove subscriber from this list, because it's updated now and hence alive!
                if (this.secondChanceList.ContainsKey(subscriberId.stringId))
                    this.secondChanceList.Remove(subscriberId.stringId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes Subscriber from list
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public bool Remove(PeerId subscriberId)
        {
            if (this.checkList.ContainsKey(subscriberId.stringId))
            {
                this.checkList.Remove(subscriberId.stringId);
                // don't know if the parameter-element is the SAME object as the stored object...
                //this.activeSubsList.Remove(subscriberId);
                this.activeSubsList.Remove(subscriberId.stringId);
                return true;
            }
            return false;
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

            foreach (KeyValuePair<string, PeerValue> entry in this.checkList)
            {
                DateTime valueWithExpirationTime = entry.Value.dateTime.AddMilliseconds(ExpirationTime);

                // if time is expired AND the ID is already in the secondChanceList --> Add to remove list
                if (this.dateTimeNow > valueWithExpirationTime && secondChanceList.ContainsKey(entry.Key))
                {
                    removeSubscribersFromDict.Add(entry.Value.peerId);
                }
                else if (this.dateTimeNow > valueWithExpirationTime) //otherwise give a second chance
                {
                    this.secondChanceList.Add(entry.Key, entry.Value.peerId);
                }
            }
            foreach (PeerId removeSub in removeSubscribersFromDict)
            {
                this.secondChanceList.Remove(removeSub.stringId);
                this.checkList.Remove(removeSub.stringId);

                // remove entry from activeSubsList, don't know if it works...
                //this.activeSubsList.Remove(removeSub);
                this.activeSubsList.Remove(removeSub.stringId);

                if (OnSubscriberRemoved != null)
                    OnSubscriberRemoved(removeSub);
            }

            return this.secondChanceList.Values.ToList<PeerId>();
        }

        public List<PeerId> GetAllSubscribers()
        {
            return this.activeSubsList.Values.ToList<PeerId>();
            //return this.activeSubsList;
        }

        public void Dispose()
        {
            this.checkList.Clear();
            this.checkList = null;
            this.secondChanceList.Clear();
            this.secondChanceList = null;
            this.activeSubsList.Clear();
            this.activeSubsList = null;
        }
    }
}
