/* Copyright 2010 Team CrypTool (Christian Arnold), Uni Duisburg-Essen

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
using Cryptool.PluginBase.Control;
using Cryptool.Plugins.PeerToPeer.Jobs;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

/* TODO:
 * - Publisher-change is possible, but catch old Publishers subscriber list
 *   isn't implemented yet ((de)serialization of the subscribers is 
 *   implemented and tested)
 * - Make Manager-change possible ((de)serialization of job management lists)
 * - Benchmarking the working peers 
 *   (this.distributableJobControl.SetResult() returns the TimeSpan for the result)
 * - Insert internal Start-/Stop-Button, so Manager can stop its works without
 *   loosing any Job-Information (this happens at present, when pressing the Stop
 *   button of the CrypTool Workspace)
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PManagerBase_NEW : P2PPublisherBase
    {
        #region Events and Delegates

        public delegate void ProcessProgress(double progressInPercent);       
        public delegate void NewJobAllocated(BigInteger jobId);
        public delegate void ResultReceived(BigInteger jobId);
        public delegate void NoMoreJobsLeft();
        public delegate void AllJobResultsReceived(BigInteger lastJobId);
        public event ProcessProgress OnProcessProgress;
        /// <summary>
        /// When a new job was successfully allocated to a worker (after receiving 
        /// its "JobAccepted"-Message), this event is thrown
        /// </summary>
        public event NewJobAllocated OnNewJobAllocated;
        /// <summary>
        /// When a new job result was received (and accepted) this event is thrown
        /// </summary>
        public event ResultReceived OnResultReceived;
        /// <summary>
        /// When the last job from the DistributableJob-Stack is allocated, but
        /// the Manager is still waiting for some JobResults this event is thrown
        /// </summary>
        public event NoMoreJobsLeft OnNoMoreJobsLeft;
        /// <summary>
        /// When no more jobs left AND the last "ausstehendes" JobResult comes in,
        /// this event is thrown
        /// </summary>
        public event AllJobResultsReceived OnAllJobResultsReceived;

        #endregion

        #region Variables

        /// <summary>
        /// this control contains a JobStack and other special 
        /// management for a SPECIAL distributable Job
        /// </summary>
        private IDistributableJob distributableJobControl;
        /// <summary>
        /// this list contains all jobs, which were sent to workers,
        /// but the workers hadn't accept/decline the Job at present
        /// </summary>
        private Dictionary<BigInteger, PeerId> jobsWaitingForAcceptanceInfo;
        /// <summary>
        /// this dict contains all jobs/workers, who were successfully
        /// distributed (so the manager already had received a JobAccepted Msg)
        /// </summary>
        private Dictionary<BigInteger, PeerId> jobsInProgress;

        private bool managerStarted = false;
        public bool ManagerStarted
        {
            get { return this.managerStarted; }
            private set { this.managerStarted = value; }
        }

        /// <summary>
        /// When the Manager is started, this variable must be set.
        /// </summary>
        private string sTopic = String.Empty;
        public string TopicName 
        {
            get { return this.sTopic; }
            private set { this.sTopic = value; }
        }

        private long lAliveMessageInterval;
        public long AliveMesageInterval 
        {
            get { return this.lAliveMessageInterval ; }
            set { this.lAliveMessageInterval = value; }
        }

        private DateTime startWorkingTime = DateTime.MinValue;
        /// <summary>
        /// This value will be initialized after allocating the first job to a worker.
        /// Before initialization this is MinValue! Used for end time approximation
        /// </summary>
        public DateTime StartWorkingTime 
        {
            get { return this.startWorkingTime; } 
        }

        #endregion

        public P2PManagerBase_NEW(IP2PControl p2pControl, IDistributableJob distributableJob) : base(p2pControl)
        {
            this.distributableJobControl = distributableJob;

            this.jobsWaitingForAcceptanceInfo = new Dictionary<BigInteger, PeerId>();
            this.jobsInProgress = new Dictionary<BigInteger, PeerId>();
        }

        public void StartManager(string sTopic, long aliveMessageInterval)
        {
            // only when the main manager plugin is connected with a Peer-PlugIn
            // and a IWorkerControl-PlugIn, this Manager can start its work
            if (this.distributableJobControl != null && this.p2pControl != null)
            {
                //set value to null, when restarting the manager
                this.startWorkingTime = DateTime.MinValue; 
                this.TopicName = sTopic;
                this.AliveMesageInterval = aliveMessageInterval;
                base.Start(this.TopicName, this.AliveMesageInterval);
            }
            else
            {
                GuiLogging("Manager can't be started, because P2P-Peer- or Distributable-Job-PlugIn isn't connected with the Manager or the connection is broken...", NotificationLevel.Warning);
            }
        }

        protected override void PeerCompletelyStarted()
        {
            base.PeerCompletelyStarted();

            this.ManagerStarted = true;
            GetProgressInformation();
            GuiLogging("P2PManager is started right now.", NotificationLevel.Info);
        }

        public override void Stop(PubSubMessageType msgType)
        {
            base.Stop(msgType);

            this.ManagerStarted = false;
            ((WorkersManagement)this.peerManagement).OnFreeWorkersAvailable -= peerManagement_OnFreeWorkersAvailable;
            ((WorkersManagement)this.peerManagement).OnSubscriberRemoved -= peerManagement_OnSubscriberRemoved;

            GuiLogging("P2PManager was stopped successully.", NotificationLevel.Info);
        }

        /// <summary>
        /// because the manager needs additional peer information for all workers,
        /// this method is overwritten. WorkersManagement throws events, when
        /// a Worker leaves or joins the "solution network", so we can re-add or
        /// allocate a job.
        /// </summary>
        /// <param name="aliveMessageInterval"></param>
        protected override void AssignManagement(long aliveMessageInterval)
        {
            this.peerManagement = new WorkersManagement(aliveMessageInterval);
            this.peerManagement.OnSubscriberRemoved +=new SubscriberManagement.SubscriberRemoved(peerManagement_OnSubscriberRemoved);
            // waiting for new workers joining the manager or already joined worker, who were set to "free" again
            ((WorkersManagement)this.peerManagement).OnFreeWorkersAvailable += new WorkersManagement.FreeWorkersAvailable(peerManagement_OnFreeWorkersAvailable);
        }

        /// <summary>
        /// only accepts DistributableJob-specific messages (created, checked and transformed by
        /// the static class JobMessages). All other message are dropped!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        protected override void p2pControl_OnPayloadMessageReceived(PeerId sender, byte[] data)
        {
            if (!JobMessages.IsJobMessageType(data[0]))
            {
                GuiLogging("Received an undefined message (not a job accepted message or a job result).", NotificationLevel.Debug);
                return;
            } 
            switch (JobMessages.GetMessageJobType(data[0]))
            {
                case MessageJobType.JobAcceptanceInfo:
                    HandleJobAcceptanceMessage(sender, data);
                    break;
                case MessageJobType.JobResult:
                    GuiLogging("Received JobResult message from Peer '" + sender.ToString() + "'. Beginning to set result now.", NotificationLevel.Debug);
                    HandleJobResultMessage(sender, data);
                    break;
                case MessageJobType.Free:
                    HandleFreeMessage(sender, data);
                    break;
                default:
                    GuiLogging("Obscure Message (" + Encoding.UTF8.GetString(data) + ") received from '" + sender.ToString() + "'.", NotificationLevel.Info);
                    break;
            } // end switch
            GetProgressInformation();
        }

        /// <summary>
        /// This method is only overwritten because we have to ignore the Solution-case in
        /// the System-Message-Handling (a Peer mustn't send a Solution message, which influences
        /// the working status of the Manager, because it havn't the overview of the JobParts)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msgType"></param>
        protected override void p2pControl_OnSystemMessageReceived(PeerId sender, PubSubMessageType msgType)
        {
            // ignore Solution case, because other worker could work on...
            if (msgType != PubSubMessageType.Solution)
                // base class handles all administration cases (register, alive, unregister, ping, pong, ...)
                base.p2pControl_OnSystemMessageReceived(sender, msgType);
        }

        #region Handle different DistributableJob-specific, incoming messages

        /// <summary>
        /// Handles the two job-acceptance cases (accepted or declined). Adds accepted jobs
        /// to the "inProgress" Dictionary, sets a busy declined worker to free (when message
        /// is JobDeclined) and removes the job in every case from the waitingForAcceptance list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void HandleJobAcceptanceMessage(PeerId sender, byte[] data)
        {
            BigInteger jobId = null;
            if (JobMessages.GetJobAcceptanceMessage(data, out jobId))
            {
                this.distributableJobControl.JobAccepted(jobId);
                lock (this.jobsInProgress)
                {
                    if (!this.jobsInProgress.ContainsKey(jobId))
                    {
                        // add to jobs in progress, because P2PJobAdmin has accepted the job!
                        this.jobsInProgress.Add(jobId, sender);
                        if (OnNewJobAllocated != null)
                            OnNewJobAllocated(jobId);
                    }
                    //else
                    //    throw (new Exception("Received a JobAccepted message for a already accepted JobId... JobId: " + jobId.ToString()));
                }
                GuiLogging("JobId '" + jobId.ToString() + "' was accepted by Peer '" + sender.ToString() + "'.", NotificationLevel.Info);
            }
            else // if AcceptanceInfo is declined
            {
                this.distributableJobControl.JobDeclined(jobId);

                // set busy worker to free, because he delined the job

                // TODO: maybe create a "black list" for peers, who had declined this kind of Job twice...
                ((WorkersManagement)this.peerManagement).SetBusyWorkerToFree(sender);
                GuiLogging("JobId '" + jobId.ToString() + "' was declined by Peer '" + sender.ToString() + "'.", NotificationLevel.Info);
            }
            // in every case remove the job from thew waiting Dictionary
            lock (this.jobsWaitingForAcceptanceInfo)
            {
                if (this.jobsWaitingForAcceptanceInfo.ContainsKey(jobId))
                {
                    this.jobsWaitingForAcceptanceInfo.Remove(jobId);
                }
                //else
                //    throw (new Exception("Received a JobAcceptance-Message for a jobId, which isn't in the waitingForAcceptance-List... JobId: " + jobId.ToString()));
            }
        }

        /// <summary>
        /// Sets the incoming result in the DistributableJob class, removes the job from
        /// the JobsInProgress Dictionary and throws the OnResultReceivedEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void HandleJobResultMessage(PeerId sender, byte[] data)
        {
            BigInteger jobId;

            byte[] serializedJobResult = JobMessages.GetJobResult(data, out jobId);
            TimeSpan jobProcessingTime = this.distributableJobControl.SetResult(jobId, serializedJobResult);

            if (OnResultReceived != null)
                OnResultReceived(jobId);

            GuiLogging("JobResult for Job '" + jobId.ToString() + "' received. Processing Time: "
                + jobProcessingTime.TotalMinutes.ToString() + " minutes. Worker-Id: '" + sender.ToString() + "'.", NotificationLevel.Info);

            lock (this.jobsInProgress)
            {
                if (this.jobsInProgress.ContainsKey(jobId))
                    this.jobsInProgress.Remove(jobId);
                //dirty workaround because P2PJobAdmin sends the result msg twice...
                //else
                //    throw (new Exception("Received a valid job result, which wasn't allocated before!!!"));
            }
        }

        /// <summary>
        /// If message content declares the sender as a free worker,
        /// set this worker from busy to free, otherwise do nothing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        private void HandleFreeMessage(PeerId sender, byte[] data)
        {
            // only handle the "true"-case, because otherwise there is nothing to do
            if (JobMessages.GetFreeWorkerStatusMessage(data))
            {
                GuiLogging("Received a 'free'-message from Peer '" + sender.ToString() + "'.", NotificationLevel.Debug);
                // only if worker already exists in the "busy list", it will set to free and event will be thrown
                ((WorkersManagement)this.peerManagement).SetBusyWorkerToFree(sender);
            }
        }

        #endregion

        #region Worker-action-handling

        /// <summary>
        /// every time when new workers are available, continue distribution of Jobs (if any JobParts left)
        /// </summary>
        private void peerManagement_OnFreeWorkersAvailable()
        {
            if (!this.ManagerStarted)
            {
                GuiLogging("Manager isn't started at present, so I can't disperse the patterns.", NotificationLevel.Error);
                bool removeSettings = DHT_CommonManagement.DeleteAllPublishersEntries(ref this.p2pControl, this.TopicName);
                if (removeSettings)
                    GuiLogging("Manager is stopped, but DHT entries were still existing, so they were deleted!", NotificationLevel.Info);
                else
                    throw (new Exception("Critical error in P2PManager. Manager isn't started yet, but the workers can register... Even removing DHT entries weren't possible..."));
            }
            else
            {
                /* edited by Arnold - 2010.02.23 */
                // because parallel incoming free workers could run
                // into concurrence in this method, so some workers
                // could get more than one job - so they have to
                // queue the additional jobs.
                lock (this)
                {
                    AllocateJobs();
                }
            }

            GetProgressInformation();
        }

        /// <summary>
        /// When a Worker leaves the network, its (maybe) allocated JobParts have to
        /// be pushed back to the main Jobstack
        /// </summary>
        /// <param name="peerId"></param>
        private void peerManagement_OnSubscriberRemoved(PeerId peerId)
        {
            GuiLogging("REMOVED worker " + peerId, NotificationLevel.Info);

            // necessary lock, because the amount of jobs in Progress could change while traversing this list
            lock (this.jobsInProgress)
            {
                // push job back and remove list entries for "jobs in progress"
                List<BigInteger> allJobsForRemovedPeer = (from k in this.jobsInProgress where k.Value == peerId select k.Key).ToList<BigInteger>();

                BigInteger jobId;
                for (int i = 0; i < allJobsForRemovedPeer.Count; i++)
                {
                    jobId = allJobsForRemovedPeer[i];
                    this.distributableJobControl.Push(jobId);
                    this.jobsInProgress.Remove(jobId);
                    GuiLogging("Pushed job '" + jobId.ToString() + "' back to the stack, because peer left the network.", NotificationLevel.Debug);
                }
            }

            // necessary lock, because the amount of jobs in Progress could change while traversing this list
            lock (this.jobsWaitingForAcceptanceInfo)
            {
                // Set the JobDeclined-status for all jobs of the removed peer, which are still waiting 
                // for an acceptance information. Than remove all jobs from the "jobs waiting for acceptance info" List
                List<BigInteger> allWaitingEntriesForRemovedPeer = (from k in this.jobsWaitingForAcceptanceInfo where k.Value == peerId select k.Key).ToList<BigInteger>();

                for (int i = 0; i < allWaitingEntriesForRemovedPeer.Count; i++)
                {
                    this.distributableJobControl.JobDeclined(allWaitingEntriesForRemovedPeer[i]);
                    this.jobsWaitingForAcceptanceInfo.Remove(allWaitingEntriesForRemovedPeer[i]);
                    GuiLogging("Declined job '" + allWaitingEntriesForRemovedPeer[i].ToString() + "', because peer left the network.", NotificationLevel.Debug);
                }
            }

            GetProgressInformation();
        }
        
        /// <summary>
        /// Allocates new JobParts to new registered or calling-for-jobs Workers.
        /// Additionally it adds the allocated job to a waitingForAcceptance Dictionary,
        /// so it can be checked, if the Worker respond to the Job-allocation
        /// </summary>
        private void AllocateJobs()
        {
            int i = 0;
            BigInteger temp_jobId = null;
            List<PeerId> freePeers = ((WorkersManagement)this.peerManagement).GetAllSubscribers();

            GuiLogging("Trying to allocate " + freePeers.Count + " job(s) to workers.", NotificationLevel.Debug);

            // set the start working time after allocating the FIRST job
            if (this.startWorkingTime == DateTime.MinValue && freePeers.Count > 0)
                this.startWorkingTime = DateTime.Now;

            foreach (PeerId worker in freePeers)
            {
                byte[] serializedNewJob = this.distributableJobControl.Pop(out temp_jobId);
                if (serializedNewJob != null) // if this is null, there are no more JobParts on the main stack!
                {
                    this.jobsWaitingForAcceptanceInfo.Add(temp_jobId, worker);
                    // get actual subscriber/worker and send the new job
                    base.p2pControl.SendToPeer(JobMessages.CreateJobPartMessage(temp_jobId, serializedNewJob), worker);

                    if (OnNewJobAllocated != null)
                        OnNewJobAllocated(temp_jobId);

                    // set free worker to busy in the peerManagement class
                    ((WorkersManagement)this.peerManagement).SetFreeWorkerToBusy(worker);

                    GuiLogging("Job '" + temp_jobId.ToString() + "' were sent to worker id '" + worker.ToString() + "'", NotificationLevel.Info);
                    i++;
                }
                else
                {
                    GuiLogging("No more jobs left. So wait for the last results, than close this task.", NotificationLevel.Debug);
                    if (OnNoMoreJobsLeft != null)
                        OnNoMoreJobsLeft();
                }
                GuiLogging(i + " Job(s) allocated to worker(s).", NotificationLevel.Debug);
            } // end foreach
        }

        #endregion

        /// <summary>
        /// returns the percental progress information of the whole job (value is between 0 and 100)
        /// </summary>
        /// <returns>the percental progress information of the whole job</returns>
        private double GetProgressInformation()
        {
            double jobProgressInPercent;
            double lFinishedAmount = (double)this.distributableJobControl.FinishedAmount.LongValue();
            double lAllocatedAmount = (double)this.distributableJobControl.AllocatedAmount.LongValue();
            double lTotalAmount = (double)this.distributableJobControl.TotalAmount.LongValue();

            if (lFinishedAmount > 0 && lAllocatedAmount > 0)
            {
                jobProgressInPercent = 30 * (lAllocatedAmount / lTotalAmount) + 100 * (lFinishedAmount / lTotalAmount);
            }
            else if (lAllocatedAmount > 0)
            {
                jobProgressInPercent = 30 * (lAllocatedAmount / lTotalAmount);
            }
            else if (lFinishedAmount > 0)
            {
                jobProgressInPercent = 100 * (lFinishedAmount / lTotalAmount);
            }
            else
            {
                jobProgressInPercent = 0.0;
            }

            if (OnProcessProgress != null)
                OnProcessProgress(jobProgressInPercent);

            return jobProgressInPercent;
        }

        /// <summary>
        /// returns the estimated end time (correlation between Start Time, Total amount of jobs and finished jobs).
        /// When no job is finished yet, it returns an empty timespan
        /// </summary>
        /// <returns></returns>
        public DateTime EstimatedEndTime()
        {
            DateTime retTime = DateTime.MaxValue;
            if (this.distributableJobControl.FinishedAmount.LongValue() > 0)
            {
                TimeSpan bruteforcingTime = DateTime.Now.Subtract(this.StartWorkingTime);
                double jobsPerSecond = bruteforcingTime.TotalSeconds / this.distributableJobControl.FinishedAmount.LongValue();
                double restSeconds = jobsPerSecond * 
                    (this.distributableJobControl.TotalAmount - this.distributableJobControl.FinishedAmount).LongValue();
                //retTime.TotalSeconds = jobsPerSecond * (2 - (progressInPercent / 100));
                retTime = DateTime.Now.AddSeconds(restSeconds);
            }
            return retTime;
        }

        #region Forward PeerManagement Values

        public int FreeWorkers() { return ((WorkersManagement)peerManagement).GetFreeWorkersAmount(); }
        public int BusyWorkers() { return ((WorkersManagement)peerManagement).GetBusyWorkersAmount(); }

        #endregion
    }
}