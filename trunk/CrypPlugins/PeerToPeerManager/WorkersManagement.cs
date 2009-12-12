using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* 
 * CLASS INFORMATION
 * This class is derived from SubscriberManagement. The whole availability- and update-functionality
 * is assumed unchanged. Only Adding- and Removing-functionalities for subscriber/worker are extended,
 * because of an easier management it must handle the two lists freeWorkers and busyWorkers.
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public class WorkersManagement : SubscriberManagement
    {
        public delegate void FreeWorkersAvailable();
        /// <summary>
        /// When a worker switches from busy to free, this event will be thrown
        /// </summary>
        public event FreeWorkersAvailable OnFreeWorkersAvailable;

        private Dictionary<string,PeerId> freeWorkers;
        private Dictionary<string,PeerId> busyWorkers;

        #region own methods

        public WorkersManagement(long expirationTime) : base(expirationTime)
        {
            freeWorkers = new Dictionary<string,PeerId>();
            busyWorkers = new Dictionary<string,PeerId>();
        }

        public List<PeerId> GetFreeWorkers()
        {
            return this.freeWorkers.Values.ToList<PeerId>();
        }

        public List<PeerId> GetBusyWorkers()
        {
            return this.busyWorkers.Values.ToList<PeerId>();
        }

        /// <summary>
        /// When free Worker gets allocated to a job, this method manages the correct mapping/classfication between free and busy workers
        /// </summary>
        /// <param name="worker"></param>
        /// <returns>true, if the given worker is free. Otherwise false, because a already busy worker can't be allocated another time!</returns>
        public bool SetFreeWorkerToBusy(PeerId worker)
        {
            if(this.busyWorkers.ContainsKey(worker.stringId))
                return false;

            if (this.freeWorkers.ContainsKey(worker.stringId))
            {
                lock(this.freeWorkers)
                {
                    this.busyWorkers.Add(worker.stringId, worker);
                    this.freeWorkers.Remove(worker.stringId);
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// When busy Worker completes a job, this method manages the correct mapping/classfication between free and busy workers
        /// </summary>
        /// <param name="worker"></param>
        /// <returns>true, if the given worker is busy. Otherwise false, because a already free worker couldn't set free another time!</returns>
        public bool SetBusyWorkerToFree(PeerId worker)
        {
            if (this.freeWorkers.ContainsKey(worker.stringId))
                return false;

            if (this.busyWorkers.ContainsKey(worker.stringId))
            {
                lock (this.freeWorkers)
                {
                    this.freeWorkers.Add(worker.stringId, worker);
                    this.busyWorkers.Remove(worker.stringId);
                }
                CheckAvailabilityOfFreeWorkers();
            }
            else
            {
                return false;
            }
            return true;
        }

        // additional functionality for inherited method Remove and RemoveSubscriberEverywhere
        private void RemoveWorker(PeerId workerId)
        {
            if (this.freeWorkers.ContainsKey(workerId.stringId))
                this.freeWorkers.Remove(workerId.stringId);

            if (this.busyWorkers.ContainsKey(workerId.stringId))
            {
                this.busyWorkers.Remove(workerId.stringId);
            }
        }

        private void CheckAvailabilityOfFreeWorkers()
        {
            lock (this.freeWorkers)
            {
                if (this.freeWorkers.Count > 0)
                {
                    if (OnFreeWorkersAvailable != null)
                        OnFreeWorkersAvailable();
                }
            }
        }

        #endregion

        #region overrided methods from base class (SubscriberManagement)

        public override bool Add(PeerId subscriberId)
        {
            bool bolBaseAdd = base.Add(subscriberId);

            // additional: fill freeWorker-List with new Worker
            if(!this.freeWorkers.ContainsKey(subscriberId.stringId))
                this.freeWorkers.Add(subscriberId.stringId,subscriberId);

            CheckAvailabilityOfFreeWorkers();

            return bolBaseAdd;
        }

        public override bool Remove(PeerId subscriberId)
        {
            bool baseResult = base.Remove(subscriberId);

            RemoveWorker(subscriberId);

            return baseResult;
        }

        protected override bool RemoveSubscriberEverywhere(PeerId subId)
        {
            bool result = base.RemoveSubscriberEverywhere(subId);

            RemoveWorker(subId);

            return result;
        }

        public override void Dispose()
        {
            base.Dispose();

            freeWorkers.Clear();
            freeWorkers = null;
            busyWorkers.Clear();
            busyWorkers = null;
        }

        #endregion 

        public override string ToString()
        {
            if (this.freeWorkers != null && this.busyWorkers != null)
            {
                return base.ToString() + "; WorkersManagement. FreeWorkers: " + this.freeWorkers.Count.ToString() + ", BusyWorkers: " + this.busyWorkers.Count.ToString();
            }
            else
                return base.ToString();
        }
    }
}
