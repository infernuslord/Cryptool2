using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

/* 
 * Everything works fine.
 * @CheckVitality: Don't know what is better. Run through two loops and save copying dict or copying dict and save a second loop...
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
        /// <summary>
        /// this event is thrown when a subscriber/worker has been removed from managing list
        /// </summary>
        public event SubscriberRemoved OnSubscriberRemoved;

        private DateTime dateTimeNow;
        /// <summary>
        /// contains all active subscribers
        /// </summary>
        private Dictionary<string, PeerValue> checkList;


        /*TESTING*/
        protected Dictionary<string, PeerId> activeSubsList;


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
        public virtual bool Add(PeerId subscriberId)
        {
            if (!this.checkList.ContainsKey(subscriberId.stringId))
            {
                this.dateTimeNow = DateTime.Now;
                // locking checkList instead of activeSubsList, because all other functions work on checkList, not on activeSubsList
                lock (this.checkList)
                {
                    this.checkList.Add(subscriberId.stringId, new PeerValue(subscriberId, this.dateTimeNow));
                    this.activeSubsList.Add(subscriberId.stringId, subscriberId);
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
        public virtual bool Remove(PeerId subscriberId)
        {
            return RemoveSubscriberEverywhere(subscriberId);
        }

        protected virtual bool RemoveSubscriberEverywhere(PeerId subId)
        {
            bool result = false;
            lock(this.checkList)
            {
                if(this.secondChanceList.ContainsKey(subId.stringId))
                    this.secondChanceList.Remove(subId.stringId);
                if (this.checkList.ContainsKey(subId.stringId))
                {
                    this.checkList.Remove(subId.stringId);
                    result = true;
                }
                if (this.activeSubsList.ContainsKey(subId.stringId))
                {
                    this.activeSubsList.Remove(subId.stringId);
                    result = true;
                }

                if (result && OnSubscriberRemoved != null)
                    OnSubscriberRemoved(subId);
            }
            return result;
        }

        /* see alternative method */

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
            } //end lock(this.checkList)

            foreach (PeerId removeSub in removeSubscribersFromDict)
            {
                // functionality swapped for inheritance matters
                RemoveSubscriberEverywhere(removeSub);
            }

            return this.secondChanceList.Values.ToList<PeerId>();
        }

        /* alternative method, works with a copy of checkList, saves a foreach-loop, but produces load for copying */
        /*
        /// <summary>
        /// Removes all Subscribers which are long-rated outdated (expiration time is considered). 
        /// The recently outdated subscribers will be added to the returned second chance list.
        /// </summary>
        /// <returns>all recently outdated subscribers</returns>
        public List<PeerId> CheckVitality()
        {
            this.dateTimeNow = DateTime.Now;

            lock (this.checkList)
            {
                //don't know what is better. Run through two loops and save copying dict or copying dict and save a second loop...
                Dictionary<string, PeerValue> checkListCopy = this.checkList;
                foreach (KeyValuePair<string, PeerValue> entry in this.checkListCopy)
                {
                    DateTime valueWithExpirationTime = entry.Value.dateTime.AddMilliseconds(ExpirationTime);

                    // if time is expired AND the ID is already in the secondChanceList --> Add to remove list
                    if (this.dateTimeNow > valueWithExpirationTime && secondChanceList.ContainsKey(entry.Key))
                    {
                        this.secondChanceList.Remove(removeSub.stringId);
                        // remove entry from ORIGINAL dictionary!
                        this.checkList.Remove(removeSub.stringId);
                        this.activeSubsList.Remove(removeSub.stringId);

                        if (OnSubscriberRemoved != null)
                            OnSubscriberRemoved(removeSub);
                    }
                    else if (this.dateTimeNow > valueWithExpirationTime) //otherwise give a second chance
                    {
                        this.secondChanceList.Add(entry.Key, entry.Value.peerId);
                    }
                }
            } //end lock(this.checkList)

            return this.secondChanceList.Values.ToList<PeerId>();
        }
        */ 

        public List<PeerId> GetAllSubscribers()
        {
            return this.activeSubsList.Values.ToList<PeerId>();
        }

        public virtual void Dispose()
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
