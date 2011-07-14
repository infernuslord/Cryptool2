using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

/*
 * TODO:
 * - Only Update registered Subscribers, don't add nonregistered subscribers - DONE
 * - ContainsKey doesn't work. I don't know why. Similar problem with comparing two byte-arrays
 *   Dirty Workaround: Loop byte-array and construct a string...
 */
namespace Cryptool.Plugins.PeerToPeer
{
    public class SubscriberInfo
    {
        public delegate void SubscriberRemoved(byte[] byPeerId);
        public event SubscriberRemoved OnSubscriberRemoved;

        private DateTime dateTimeNow;
        /// <summary>
        /// contains all active subscribers
        /// </summary>
        private Dictionary<byte[], DateTime> subList;
        /// <summary>
        /// when a peer is in this list, it will be deleted on the next vitality check
        /// </summary>
        private List<byte[]> secondChanceList;

        private long expirationTime;
        /// <summary>
        /// Timespan in which subscriber gets marked secondChance first and twice is removed from Subscriber list.
        /// Latency of 5 seconds will be added because of network latency.
        /// </summary>
        public long ExpirationTime 
        {
            get { return this.expirationTime * 5000;  }
            set { this.expirationTime = value; } 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expirationTime">expiration Time of a subscriber in milliseconds</param>
        public SubscriberInfo(long expirationTime)
        {
            this.dateTimeNow = new DateTime();
            this.subList = new Dictionary<byte[], DateTime>();
            this.secondChanceList = new List<byte[]>();
            this.ExpirationTime = expirationTime;
        }

        /// <summary>
        /// Add a subscriber to the subscriber list if it doesn't already exist
        /// </summary>
        /// <param name="subscriberId">ID of the Subscriber</param>
        /// <returns>true if subscriber wasn't in List and is added, otherwise false</returns>
        public bool Add(byte[] subscriberId)
        {
            if (!this.subList.ContainsKey(subscriberId))
            {
                this.dateTimeNow = DateTime.Now;
                this.subList.Add(subscriberId, this.dateTimeNow);
                return true;
            }
            else
                return false;

        }

        public bool Update(byte[] subscriberId)
        {
            /*Add peers which haven't registered or being removed from SUbscriber List*/
            /*
            this.dateTimeNow = DateTime.Now;
            // only for test issues
            if (!this.subList.ContainsKey(subscriberId))
            {
                this.subList.Add(subscriberId, dateTimeNow);
            }
            this.subList[subscriberId] = this.dateTimeNow;
            // remove subscriber from second chance list, because it's updated
            if (this.secondChanceList.Contains(subscriberId))
                this.secondChanceList.Remove(subscriberId);
            return true;
            */
            this.dateTimeNow = DateTime.Now;
            if (this.subList.ContainsKey(subscriberId))
            {
                this.subList[subscriberId] = this.dateTimeNow;
                if (this.secondChanceList.Contains(subscriberId))
                    this.secondChanceList.Remove(subscriberId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all Subscribers which are long-rated outdated (expiration time is considered). 
        /// The recently outdated subscribers will be added to the returned second chance list.
        /// </summary>
        /// <returns>all recently outdated subscribers</returns>
        public List<byte[]> CheckVitality()
        {
            this.dateTimeNow = DateTime.Now;

            List<byte[]> removeSubscribersFromDict = new List<byte[]>();

            foreach (KeyValuePair<byte[], DateTime> entry in this.subList)
            {
                byte[] key = entry.Key;
                DateTime valueWithExpirationTime = entry.Value.AddMilliseconds(ExpirationTime);

                if (this.dateTimeNow > valueWithExpirationTime && secondChanceList.Contains(key))
                {
                    removeSubscribersFromDict.Add(key);
                }
                else if (this.dateTimeNow > valueWithExpirationTime)
                {
                    this.secondChanceList.Add(key);
                }
            }
            foreach (byte[] removeSub in removeSubscribersFromDict)
            {
                this.secondChanceList.Remove(removeSub);
                this.subList.Remove(removeSub);
                if (OnSubscriberRemoved != null)
                    OnSubscriberRemoved(removeSub);
            }

            return this.secondChanceList;
        }

        private string ConvertByteToString(byte[] bytePeerId)
        {
            string sRet = String.Empty;
            for (int i = 0; i < bytePeerId.Length; i++)
            {
                sRet += bytePeerId[i].ToString() + ":";
            }
            return sRet.Substring(0, sRet.Length - 1);
        }

        public Dictionary<byte[],DateTime> GetAllSubscribers()
        {
            return this.subList;
        }

        public void Dispose()
        {
            this.subList = null;
            this.secondChanceList = null;
        }

        /*WORKAROUND AREA*/
        public string ConvertBytePeerId(byte[] bytePeerId)
        {
            string sRet = String.Empty;
            for (int i = 0; i < bytePeerId.Length; i++)
            {
                sRet += bytePeerId[i].ToString() + ":";
            }
            return sRet.Substring(0, sRet.Length - 1);
        }
        //public byte[] ConvertStringPeerId(string sPeerId)
        //{
        //    string[] sParts = sPeerId.Split(":",StringSplitOptions.RemoveEmptyEntries);
        //    ArrayList arrBytePeerId = new ArrayList();
        //    foreach (string sPart in sParts)
        //    {
        //        if (sPart != ":")
        //            arrBytePeerId.Add(sPart);
        //    }
        //    byte[] byteRet = new byte[arrBytePeerId.Count);
        //    // to be continued...
        //}
    }
}
