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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.PeerToPeer.Jobs;
using System.Timers;

/* TWO DIFFERENT STOPPING CASE:
 * 1) When P2P-Admin is stopped, deregister WorkerControl-Events, so
 *    the (unfinished) JobPart-Result won't be sent to the Manager
 *    - Copy registering the WorkerControl-Events to "StartWorkerControl"
 *    - Unregister WorkerControl-Events in "StopWorkerControl"
 *    
 * 2) When P2P-Admin is stopped, the WorkerControl sends the (unfinished)
 *    JobPart-Result to the Manager (the Manager can handle this case without
 *    any problem).
 *    - Copy registering the WorkerControl-Events to the constructor
 *    - Comment the unregistering of the WorkerControl-Events in the
 *      "StopWorkerControl" method
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PJobAdminBase : P2PSubscriberBase
    {
        #region Events for PlugIn-Color-Status

        public delegate void StartWorking();
        public delegate void SuccessfullyEnded();
        public delegate void CanceledWorking();
        public delegate void WorkerStopped();

        public event StartWorking OnStartWorking;
        public event SuccessfullyEnded OnSuccessfullyEnded;
        public event CanceledWorking OnCanceledWorking;
        public event WorkerStopped OnWorkerStopped;

        #endregion

        private IControlWorker workerControl;
        private static int WAITING_FOR_JOBS = 5000;

        /// <summary>
        /// if worker sends a "free" msg to the manager, but it doesn't react on this,
        /// although there are some jobs to distribute...
        /// </summary>
        Timer timerWaitingForJobs = new Timer(WAITING_FOR_JOBS);

        /// <summary>
        /// if more than one job arrived at the same time, buffer it in this dictionary
        /// (can't happen under normal circumstances, but when it happens, we are prepared)
        /// </summary>
        Stack<byte[]> waitingJobStack;
        private bool isWorking = false;
        public bool IsWorking 
        {
            get { return this.isWorking; }
            private set { this.isWorking = value; }
        }
        ///// <summary>
        ///// everytime, when start processing a new job, set
        ///// its jobId in this variable, so processing the
        ///// same job several times can be avoided
        ///// </summary>
        //private BigInteger actualJobId = null;

        #region Constructor and Event Handling

        public P2PJobAdminBase(IP2PControl p2pControl, IControlWorker controlWorker) : base(p2pControl)
        {
            this.timerWaitingForJobs.Elapsed += new ElapsedEventHandler(timerWaitingForJobs_Elapsed);
            this.workerControl = controlWorker;

            // see comment above, to know why the following lines are commented
            //this.workerControl.OnProcessingCanceled += new ProcessingCanceled(workerControl_OnProcessingCanceled);
            //this.workerControl.OnProcessingSuccessfullyEnded += new ProcessingSuccessfullyEnded(workerControl_OnProcessingSuccessfullyEnded);
            //this.workerControl.OnInfoTextReceived += new InfoText(workerControl_OnInfoTextReceived);

            this.waitingJobStack = new Stack<byte[]>();
        }

        void workerControl_OnInfoTextReceived(string sText, NotificationLevel notLevel)
        {
            base.GuiLogging(sText, notLevel);
        }

        public void StartWorkerControl(string sTopicName, long lCheckPublishersAvailability, long lPublishersReplyTimespan)
        {
            if (!base.Started)
            {
                // see comment above, to know why the following lines are uncommented
                this.workerControl.OnProcessingCanceled += new ProcessingCanceled(workerControl_OnProcessingCanceled);
                this.workerControl.OnProcessingSuccessfullyEnded += new ProcessingSuccessfullyEnded(workerControl_OnProcessingSuccessfullyEnded);
                this.workerControl.OnInfoTextReceived += new InfoText(workerControl_OnInfoTextReceived);

                // starts subscriber
                base.Start(sTopicName, lCheckPublishersAvailability, lPublishersReplyTimespan);
            }
            else
                base.GuiLogging("P2PJobAdmin is already started.", NotificationLevel.Info);
        }

        public void StopWorkerControl(PubSubMessageType msgType)
        {
            this.IsWorking = false;

            if (base.Started)
            {
                // see comment above, to know why the following lines are uncommented
                this.workerControl.OnProcessingCanceled -= workerControl_OnProcessingCanceled;
                this.workerControl.OnProcessingSuccessfullyEnded -= workerControl_OnProcessingSuccessfullyEnded;
                this.workerControl.OnInfoTextReceived -= workerControl_OnInfoTextReceived;

                this.timerWaitingForJobs.Stop();

                base.Stop(msgType);

                // delete the waiting Job List, so after re-registering, this worker
                // will process the new incoming jobs and not old jobs, which were
                // already pushed to the global Job List of the Manager after receiving
                // the unregister message from this worker.
                if(this.waitingJobStack != null && this.waitingJobStack.Count > 0)
                    this.waitingJobStack.Clear();

                if (this.workerControl != null)
                    this.workerControl.StopProcessing();
                GuiLogging("P2P-Job-Admin is successfully stopped (Unregistering with Manager, Processing of the Worker is stopped)",NotificationLevel.Info);
            }
            else
                GuiLogging("P2P-Job-Admin isn't started yet. So stopping-events won't be executed.", NotificationLevel.Info);
        }

        void P2PWorker_OnReceivedStopMessageFromPublisher(PubSubMessageType stopType, string sData)
        {
            switch (stopType)
            {
                case PubSubMessageType.Stop:
                case PubSubMessageType.Unregister:
                    if (OnWorkerStopped != null)
                        OnWorkerStopped();
                    break;
                case PubSubMessageType.Solution:
                    if (OnSuccessfullyEnded != null)
                        OnSuccessfullyEnded();
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region THE RELEVANT PART

        // this method only processes Payload Data, Internal-organisation Data will be handled internally!
        protected override void HandleIncomingData(PeerId senderId, byte[] data)
        {
            if (JobMessages.GetMessageJobType(data[0]) == MessageJobType.JobPart)
            {
                //added by Arnold 2010.03.22
                this.timerWaitingForJobs.Stop(); //when receiving a new job, time can be stopped

                BigInteger jobId = null;
                GuiLogging("Received a JobPart from '" + senderId.ToString() + "'", NotificationLevel.Debug);
                byte[] serializedRawJobPartData = JobMessages.GetJobPartMessage(data, out jobId);
                StartProcessing(senderId, serializedRawJobPartData);
            }
            else
            {
                GuiLogging("Received some strange data (no JobPart) from peer '" + senderId.ToString() 
                    + "'. Data: " + Encoding.UTF8.GetString(data), NotificationLevel.Debug);
            }
        }

        /// <summary>
        /// Starts processing the incoming JobPart, when the waiting Stack has no elements
        /// (it will be priorized).
        /// </summary>
        /// <param name="senderId">Sender Id</param>
        /// <param name="data">already DistributableJob-"unpacked" serialized JobPart data</param>
        private void StartProcessing(PeerId senderId, byte[] data)
        {
            if (this.workerControl == null) // eventually check if sender is the actual publisher, too...
            {
                GuiLogging("Processing a new job isn't possible, because IWorkerControl isn't initialized yet.", NotificationLevel.Info);
                return;
            }

            BigInteger jobId;            

            if (this.IsWorking) // if it's still working, add Job to a waiting stack
            {                
                this.waitingJobStack.Push(data);
                GuiLogging("New incoming job will be pushed to the 'waitingJobStack', because the Worker is still processing a job.", NotificationLevel.Debug);
            }
            else
            {
                this.IsWorking = true;
                bool result = this.workerControl.StartProcessing(data, out jobId);

                // visualize Working-Status in PlugIn, if it has registered with this event
                if(result)
                {
                    if (OnStartWorking != null)
                        OnStartWorking();
                }

                byte[] jobAcceptanceStatus = JobMessages.CreateJobAcceptanceMessage(jobId, result);
                this.p2pControl.SendToPeer(jobAcceptanceStatus, senderId);

                this.IsWorking = result;

                GuiLogging("Processing started? " + result + ". JobId: " + jobId.ToString(), NotificationLevel.Debug);
            }
        }

        /// <summary>
        /// When IWorkerControl throws this event, processing is (successfully) ended. So send the
        /// Result to the Manager and decide whether to process a "stacked" job or ask for new Jobs
        /// </summary>
        /// <param name="jobId">JobId of the (successfully) ended Result</param>
        /// <param name="result">serialized Result data</param>
        private void workerControl_OnProcessingSuccessfullyEnded(BigInteger jobId, byte[] result)
        {
            //GuiLogging("Sending job result to Manager. JobId: " + jobId.ToString() + ". Mngr-Id: '" + base.ActualPublisher.ToString() + "'.", NotificationLevel.Info);
            GuiLogging("Sending job result to Manager. JobId: " + jobId.ToString() + ". Mngr-Id: '" + base.ActualPublisher.ToString() + "'.", NotificationLevel.Info);
            this.p2pControl.SendToPeer(JobMessages.CreateJobResultMessage(jobId, result), base.ActualPublisher);

            // set working flag to false, so processing a new job is possible
            this.IsWorking = false;
            // visualizes the Working-Status, if PlugIn registered with this event
            if (OnSuccessfullyEnded != null)
                OnSuccessfullyEnded();

            CheckIfAnyJobsLeft();   
        }

        private void CheckIfAnyJobsLeft()
        {
            if (this.waitingJobStack.Count > 0)
            {
                GuiLogging("There's a job in the 'waitingJob'-Stack, so process it before processing new incoming jobs from the Manager.", NotificationLevel.Info);
                StartProcessing(base.ActualPublisher, this.waitingJobStack.Pop());
            }
            else
            {
                // no more jobs in the waiting stack, so send Mngr the information, that Worker is waiting for new jobs now
                this.p2pControl.SendToPeer(JobMessages.CreateFreeWorkerStatusMessage(true), base.ActualPublisher);
                GuiLogging("No jobs in the 'waitingJob'-Stack, so send 'free'-information to the Manager. Mngr-Id: '" + base.ActualPublisher.ToString() + "'.", NotificationLevel.Info);
                // If this timer elapses, it will check if the isWorking flag is true. Than it will stop the timer.
                // Otherwise it will send a new free msg to the Manager, if the last free msg got lost
                this.timerWaitingForJobs.Start();
            }
        }

        // Added by Arnold - 2010.03.22
        /// <summary>
        /// this method is only necessary when the regular free message got lost on the way to the manager.
        /// In this case the Worker won't get any more jobs, so it sends a free message 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timerWaitingForJobs_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!isWorking)
            {
                this.timerWaitingForJobs.Interval += 2000; // every time this event is thrown, heighten the timer interval
                base.SendRegMsg();
                this.p2pControl.SendToPeer(JobMessages.CreateFreeWorkerStatusMessage(true), base.ActualPublisher);
                GuiLogging("Because the last 'free worker'-Message got lost, try again.", NotificationLevel.Info);
            }
            else
            {
                // reset timer interval - it could be heighten in the if brace...
                this.timerWaitingForJobs.Interval = WAITING_FOR_JOBS;
                // when Worker is working, than the time can be stopped
                this.timerWaitingForJobs.Stop();
            }
        }

        void workerControl_OnProcessingCanceled(byte[] result)
        {
            if (OnCanceledWorking != null)
                OnCanceledWorking();
            CheckIfAnyJobsLeft();
        }

        #endregion
    }
}
