/* Copyright 2009 Team CrypTool (Christian Arnold), Uni Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/* CLASS INFORMATION
 * This class is derived from SubscriberManagement. The whole availability- and update-functionality
 * is assumed unchanged. Only Adding- and Removing-functionalities for subscriber/worker were extended,
 * because of an easier management, it must handle the two lists freeWorkers and busyWorkers. */

namespace Cryptool.Plugins.PeerToPeer
{
    public class WorkersManagement : SubscriberManagement
    {
        public delegate void FreeWorkersAvailable();
        /// <summary>
        /// When a worker switches from busy to free, this event will be thrown
        /// </summary>
        public event FreeWorkersAvailable OnFreeWorkersAvailable;

        private HashSet<PeerId> freeWorkers;
        private HashSet<PeerId> busyWorkers;

        #region own methods

        public WorkersManagement(long expirationTime)
            : base(expirationTime)
        {
            freeWorkers = new HashSet<PeerId>();
            busyWorkers = new HashSet<PeerId>();
        }

        public List<PeerId> GetFreeWorkers()
        {
            return this.freeWorkers.ToList<PeerId>();
        }

        public List<PeerId> GetBusyWorkers()
        {
            return this.busyWorkers.ToList<PeerId>();
        }

        /// <summary>
        /// When free Worker gets allocated to a job, this method manages the correct mapping/classfication between free and busy workers
        /// </summary>
        /// <param name="worker"></param>
        /// <returns>true, if the given worker is free. Otherwise false, because a already busy worker can't be allocated another time!</returns>
        public bool SetFreeWorkerToBusy(PeerId worker)
        {
            bool ret = false;
            lock (this.freeWorkers)
            {
                if (!this.busyWorkers.Contains(worker) && this.freeWorkers.Contains(worker))
                {
                    this.busyWorkers.Add(worker);
                    this.freeWorkers.Remove(worker);
                    ret = true;
                }
            }
            return ret;
        }

        /// <summary>
        /// When busy Worker completes a job, this method manages the correct mapping/classfication between free and busy workers
        /// </summary>
        /// <param name="worker"></param>
        /// <returns>true, if the given worker is busy. Otherwise false, because a already free worker couldn't set free another time!</returns>
        public bool SetBusyWorkerToFree(PeerId worker)
        {
            bool ret = false;
            lock (this.freeWorkers)
            {
                if (!this.freeWorkers.Contains(worker) && this.busyWorkers.Contains(worker))
                {
                    this.freeWorkers.Add(worker);
                    this.busyWorkers.Remove(worker);
                    CheckAvailabilityOfFreeWorkers();
                    ret = true;
                }
            }
            return ret;
        }

        // additional functionality for inherited method Remove and RemoveSubscriberEverywhere
        private void RemoveWorker(PeerId workerId)
        {
            lock (this.freeWorkers)
            {
                if (this.freeWorkers.Contains(workerId))
                    this.freeWorkers.Remove(workerId);

                if (this.busyWorkers.Contains(workerId))
                    this.busyWorkers.Remove(workerId);
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

            // fill freeWorker-List with new Worker too
            if (bolBaseAdd && !this.freeWorkers.Contains(subscriberId))
            {
                this.freeWorkers.Add(subscriberId);
                CheckAvailabilityOfFreeWorkers();
            }

            return bolBaseAdd;
        }

        public override bool Remove(PeerId subscriberId)
        {
            RemoveWorker(subscriberId);
            // execute base method here, because the OnPeerRemoved-Event
            // will be fired in this method - catched and handled by 
            // the Manager
            bool baseResult = base.Remove(subscriberId);

            return baseResult;
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

        public int GetFreeWorkersAmount()
        {
            return this.freeWorkers.Count();
        }

        public int GetBusyWorkersAmount()
        {
            return this.busyWorkers.Count();
        }
    }
}