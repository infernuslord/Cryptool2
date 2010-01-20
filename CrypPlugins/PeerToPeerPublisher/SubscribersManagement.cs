using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

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
            if (!this.checkList.ContainsKey(subscriberId))
            {
                this.dateTimeNow = DateTime.Now;
                // locking checkList instead of activeSubsList, because all other functions work on checkList, not on activeSubsList
                lock (this.checkList)
                {
                    this.checkList.Add(subscriberId, this.dateTimeNow);
                }
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
    }
}
