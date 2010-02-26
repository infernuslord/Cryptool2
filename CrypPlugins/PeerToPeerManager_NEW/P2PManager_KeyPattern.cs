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
using System.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using KeySearcher;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.Plugins.PeerToPeer.Jobs;
using System.Timers;
using System.Windows.Media;

/*TODO: 
 * - Execute: If InitVector is null, try to create a fitting InitVector with the format 0...0 
 * - 2 Output values (Top1-Key and Top1-Decryption)
 */

namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// This PlugIn only works, when its connected with a P2P_Peer object.
    /// </summary>
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "P2P_Manager_KeyPattern", "Creates a new Manager-Peer for distributable KeyPattern-Jobs", "", "PeerToPeerManager_NEW/manager_medium_neutral.png", "PeerToPeerManager_NEW/manager_medium_working.png", "PeerToPeerManager_NEW/manager_medium_finished.png")]
    public class P2PManager_KeyPattern : IInput
    {
        private P2PManagerBase_NEW p2pManager;
        private P2PManager_KeyPatternSettings settings;
        // IInput
        private CryptoolStream decryptedData;
        private byte[] initVector;
        // IControls        
        private IControlEncryption encryptionControl;
        private IP2PControl p2pControl;

        private KeyPattern pattern = null;
        public KeyPattern Pattern
        {
            get
            {
                return pattern;
            }
            set
            {
                pattern = value;
                if ((settings.Key == null) || ((settings.Key != null) && !pattern.testWildcardKey(settings.Key)))
                    settings.Key = pattern.giveInputPattern();
            }
        }

        /// <summary>
        /// after starting the execution, set this value, so after receiving
        /// a Job result, the WPF UpdateQuickWatch can use this value, without
        /// accessing the Settings every time.
        /// </summary>
        private int bytesToUseForDecryption = 0;

        #region In and Output

        [PropertyInfo(Direction.InputData, "Encrypted Data", "Encrypted data out of an Encryption PlugIn", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, "")]
        public CryptoolStream DecryptedData
        {
            get { return this.decryptedData; }
            set
            {
                if (value != this.decryptedData)
                {
                    this.decryptedData = value;
                }
            }
        }

        [PropertyInfo(Direction.InputData, "Initialization Vector", "Initialization vector with which the data were encrypted", "", DisplayLevel.Beginner)]
        public byte[] InitVector
        {
            get { return this.initVector; }
            set
            {
                if (value != this.initVector)
                    this.initVector = value;
            }
        }

        [PropertyInfo(Direction.ControlMaster, "Control Master", "Used for bruteforcing", "", DisplayLevel.Beginner)]
        public IControlEncryption EncryptionControl
        {
            get { return this.encryptionControl; }
            set
            {
                if (this.encryptionControl != null)
                {
                    this.encryptionControl.keyPatternChanged -= encryptionControl_keyPatternChanged;
                    this.encryptionControl.OnStatusChanged -= encryptionControl_onStatusChanged;
                }
                if (value != null)
                {
                    Pattern = new KeyPattern(value.getKeyPattern());
                    value.keyPatternChanged += encryptionControl_keyPatternChanged;
                    value.OnStatusChanged += encryptionControl_onStatusChanged;
                    this.encryptionControl = value;
                    OnPropertyChanged("ControlMaster");

                }
                else
                    this.encryptionControl = null;
            }
        }

        private void encryptionControl_keyPatternChanged()
        {
            Pattern = new KeyPattern(this.encryptionControl.getKeyPattern());
        }
        private void encryptionControl_onStatusChanged(IControl sender, bool readyForExecution)
        {
            // obsolete stuff
            if (readyForExecution)
            {
                this.process((IControlEncryption)sender);
            }
        }

        /// <summary>
        /// Catches the completely configurated, initialized and joined P2P object from the P2PPeer-Slave-PlugIn.
        /// </summary>
        [PropertyInfo(Direction.ControlMaster, "P2P Slave", "Input the P2P-Peer-PlugIn", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public IP2PControl P2PControl
        {
            get
            {
                return this.p2pControl;
            }
            set
            {
                if (this.p2pControl != null)
                {
                    this.p2pControl.OnStatusChanged -= P2PControl_OnStatusChanged;
                }
                if (value != null)
                {
                    this.p2pControl = (P2PPeerMaster)value;
                    this.p2pControl.OnStatusChanged += new IControlStatusChangedEventHandler(P2PControl_OnStatusChanged);
                    OnPropertyChanged("P2PMaster");
                }
                else
                {
                    this.p2pControl = null;
                }
            }
        }

        private void P2PControl_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region Standard PlugIn-Functionality

        public P2PManager_KeyPattern()
        {
            this.settings = new P2PManager_KeyPatternSettings();
            this.settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            this.settings.TaskPaneAttributeChanged += new TaskPaneAttributeChangedHandler(settings_TaskPaneAttributeChanged);
            this.settings.OnPluginStatusChanged += new StatusChangedEventHandler(settings_OnPluginStatusChanged);

            QuickWatchPresentation = new P2PManagerPresentation();
        }

        #region QuickWatchPresentation Stuff

        public UserControl QuickWatchPresentation
        {
            get;
            private set;
        }

        public UserControl Presentation
        {
            get { return QuickWatchPresentation; }
        }

        private void timerProcessingTimeReset()
        {
            this.timerProcessingTime.Stop();
            this.timerProcessingTime.Close();
            this.timerProcessingTime.Dispose();
            this.timerProcessingTime = null;

            if (QuickWatchPresentation.IsVisible)
            {
                ((P2PManagerPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {                   
                    //((P2PManagerPresentation)QuickWatchPresentation).txtTimeInProcess.Text = "not started";
                    ((P2PManagerPresentation)QuickWatchPresentation).txtProgressInPercent.Text = "not started";
                    //((P2PManagerPresentation)QuickWatchPresentation).txtEstimatedEndTime.Text = "no finished jobs";
                    //((P2PManagerPresentation)QuickWatchPresentation).PrgChunks.JobCount = 0;
                }, null);
            }
        }

        void timerProcessingTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (QuickWatchPresentation.IsVisible)
            {
                ((P2PManagerPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    // calculate and display current processing time span
                    TimeSpan processTime = DateTime.Now.Subtract(this.p2pManager.StartWorkingTime);
                    StringBuilder sbProcess = new StringBuilder();
                    if(processTime.Days != 0)
                        sbProcess.Append(processTime.Days.ToString() + " Days ");
                    if (processTime.Hours <= 9)
                        sbProcess.Append("0");
                    sbProcess.Append(processTime.Hours.ToString() + ":");
                    if (processTime.Minutes <= 9)
                        sbProcess.Append("0");
                    sbProcess.Append(processTime.Minutes.ToString() + ":");
                    if (processTime.Seconds <= 9)
                        sbProcess.Append("0");
                    sbProcess.Append(processTime.Seconds.ToString());

                    ((P2PManagerPresentation)QuickWatchPresentation).txtTimeInProcess.Text = sbProcess.ToString() + " (hh:mm:ss)";
                    sbProcess = null;

                    // change color of jobs in progress
                    Brush evenClr = System.Windows.Media.Brushes.Black;
                    Brush oddClr = System.Windows.Media.Brushes.Yellow;
                    try
                    {
                        if((Math.Round(processTime.TotalSeconds,0) % 2) == 0)
                        {
                            for (int i = 0; i < ((P2PManagerPresentation)QuickWatchPresentation).PrgChunks.JobCount; i++)
			                {
            			        if(((P2PManagerPresentation)QuickWatchPresentation).PrgChunks[i] == oddClr)
                                    ((P2PManagerPresentation)QuickWatchPresentation).PrgChunks[i] = evenClr;
			                }
                        }
                        else
                        {
                            for (int i = 0; i < ((P2PManagerPresentation)QuickWatchPresentation).PrgChunks.JobCount; i++)
			                {
            			        if(((P2PManagerPresentation)QuickWatchPresentation).PrgChunks[i] == evenClr)
                                    ((P2PManagerPresentation)QuickWatchPresentation).PrgChunks[i] = oddClr;
			                }
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.ToString(), NotificationLevel.Warning);
                    }
                }, null);
            }
        }

        private void UpdateQuickWatch(double progressInPercent)
        {
            UpdateQuickWatch(this.distributableKeyPatternJob.GlobalResultList, this.distributableKeyPatternJob.TotalAmount,
                this.distributableKeyPatternJob.AllocatedAmount, this.distributableKeyPatternJob.FinishedAmount, 
                progressInPercent, this.p2pManager.FreeWorkers(), this.p2pManager.BusyWorkers());
        }

        private void UpdateQuickWatch(LinkedList<KeySearcher.KeySearcher.ValueKey> globalTop10List,
            BigInteger jobsTotalAmount, BigInteger jobsInProgress, BigInteger jobsFinished, double progressInPercent,
            int freeWorkers, int busyWorkers)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            LinkedListNode<KeySearcher.KeySearcher.ValueKey> listNode;

            if (QuickWatchPresentation.IsVisible)
            {
                ((P2PManagerPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((P2PManagerPresentation)QuickWatchPresentation).txtProgressInPercent.Text = "" + Math.Round(progressInPercent, 2) + "%";
                    ((P2PManagerPresentation)QuickWatchPresentation).txtTotal.Text = "" + jobsTotalAmount.ToString();
                    ((P2PManagerPresentation)QuickWatchPresentation).txtInProgress.Text = "" + jobsInProgress.ToString();
                    ((P2PManagerPresentation)QuickWatchPresentation).txtLeft.Text = "" + new BigInteger((jobsTotalAmount - jobsInProgress - jobsFinished)).ToString();
                    ((P2PManagerPresentation)QuickWatchPresentation).txtFinished.Text = "" + jobsFinished.ToString();

                    ((P2PManagerPresentation)QuickWatchPresentation).txtTotalWorker.Text = "" + (freeWorkers + busyWorkers);
                    ((P2PManagerPresentation)QuickWatchPresentation).txtFreeWorker.Text = "" + freeWorkers;
                    ((P2PManagerPresentation)QuickWatchPresentation).txtBusyWorker.Text = "" + busyWorkers;

                    /* START approximation of end time */
                    // this value is MinValue until the first job is allocated to a worker
                    DateTime estimatedDateTime = this.p2pManager.EstimatedEndTime();
                    if(estimatedDateTime != DateTime.MaxValue)
                    {
                        ((P2PManagerPresentation)QuickWatchPresentation).txtEstimatedEndTime.Text = estimatedDateTime.ToString();
                    }
                    /* END approximation of end time */

                    ((P2PManagerPresentation)QuickWatchPresentation).entries.Clear();
                    listNode = globalTop10List.First;

                    int i = 0;
                    while (listNode != null)
                    {
                        i++;

                        ResultEntry entry = new ResultEntry();
                        entry.Ranking = "" + i;
                        entry.Value = "" + Math.Round(listNode.Value.value, 3);
                        entry.Key = listNode.Value.key;
                        // remove all linebreaks, tabs and so on
                        string decryptText = enc.GetString(listNode.Value.decryption, 0, this.bytesToUseForDecryption);
                        decryptText = decryptText.Replace("\n", String.Empty);
                        decryptText = decryptText.Replace("\t", String.Empty);
                        decryptText = decryptText.Replace("\r", String.Empty);
                        entry.Text = decryptText;//enc.GetString(listNode.Value.decryption, 0, this.bytesToUseForDecryption);

                        ((P2PManagerPresentation)QuickWatchPresentation).entries.Add(entry);
                        listNode = listNode.Next;
                    }
                    // to resize the WPF Presentation, so it will fit in the PlugIn-Borders
                    ((P2PManagerPresentation)QuickWatchPresentation).P2PManagerPresentation_SizeChanged(null, null);
                }, null);
            }
        }

        private void UpdateProgressChunk(BigInteger jobId, System.Windows.Media.Brush color)
        {
            // new Progress Chunk - Arnold 2010.02.23
            if (jobId.LongValue() <= Int32.MaxValue)
            {
                int iJobId = (int)jobId.LongValue();
                if (QuickWatchPresentation.IsVisible)
                {
                    ((P2PManagerPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        try
                        {
                            if (((P2PManagerPresentation)QuickWatchPresentation).PrgChunks != null && ((P2PManagerPresentation)QuickWatchPresentation).PrgChunks.JobCount != 0)
                                ((P2PManagerPresentation)QuickWatchPresentation).PrgChunks[iJobId] = color;
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage(ex.ToString(), NotificationLevel.Warning);
                        }
                    }, null);
                }
            }
        }

        private void SetProgressChunkJobCount(BigInteger bigInteger)
        {
            if (bigInteger.LongValue() <= Int32.MaxValue)
            {
                int count = (int)bigInteger.LongValue();
                if (QuickWatchPresentation.IsVisible)
                {
                    ((P2PManagerPresentation)QuickWatchPresentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        if(((P2PManagerPresentation)QuickWatchPresentation).PrgChunks != null)
                            ((P2PManagerPresentation)QuickWatchPresentation).PrgChunks.JobCount = count;
                    }, null);
                }
            }
        }

        #endregion

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
                OnPluginStatusChanged(this, args);
        }

        void settings_TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Key")
            {
                if (this.EncryptionControl != null)
                {
                    KeyPattern checkPattern = new KeyPattern(this.EncryptionControl.getKeyPattern());
                    if (!checkPattern.testWildcardKey(this.settings.Key))
                    {
                        GuiLogMessage("The set Pattern doesn't fit to the encryption plugin connected with this manager!", NotificationLevel.Warning);
                    }
                    else
                        GuiLogMessage("Successfully changed the KeyPattern in the settings.", NotificationLevel.Info);
                }
            }

            if (this.p2pControl == null)
                return;
            if (e.PropertyName == "BtnUnregister")
            {
                if (this.p2pManager != null && this.p2pManager.Started)
                {
                    Stop();
                    //this.p2pManager.Stop(PubSubMessageType.Unregister);
                    GuiLogMessage("Unregister button pressed, Publisher has stopped!", NotificationLevel.Info);
                }
            }
            if (e.PropertyName == "BtnRegister")
            {
                if (this.p2pManager == null || !this.p2pManager.Started)
                {
                    this.process(this.EncryptionControl);
                    GuiLogMessage("Register button pressed, Publisher has been started!", NotificationLevel.Info);
                }
            }
            if (e.PropertyName == "BtnSolutionFound")
            {
                if (this.p2pManager != null && this.p2pManager.Started)
                {
                    Stop();
                    //this.p2pManager.Stop(PubSubMessageType.Solution);
                    GuiLogMessage("TEST: Emulate Solution-Found-message", NotificationLevel.Info);
                }
            }
        }

        public ISettings Settings
        {
            set { this.settings = (P2PManager_KeyPatternSettings)value; }
            get { return this.settings; }
        }

        // Pre-Execute Method is below this region

        // Execute-Method is below this region

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Pause()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {

            if (this.p2pManager != null && this.p2pManager.Started)
            {
                //stop processTime Timer
                timerProcessingTimeReset();

                this.p2pManager.Stop(PubSubMessageType.Unregister);
                this.settings.MngStatusChanged(P2PManager_KeyPatternSettings.MngStatus.Neutral);
                this.p2pManager.OnGuiMessage -= p2pManager_OnGuiMessage;
                this.p2pManager.OnProcessProgress -= p2pManager_OnProcessProgress;
                this.p2pManager.OnNewJobAllocated -= p2pManager_OnNewJobAllocated;
                this.p2pManager.OnNoMoreJobsLeft -= p2pManager_OnNoMoreJobsLeft;
                this.p2pManager.OnResultReceived -= p2pManager_OnResultReceived;
                // set Manager to null, so after restarting the Workspace,
                // a new Distributable stop will be initialized with (maybe)
                // changed settings
                this.p2pManager = null;
            }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        public void PreExecution()
        {
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PControl == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }
        }

        void p2pManager_OnProcessProgress(double progressInPercent)
        {
            ProgressChanged(progressInPercent, 100.0);
            UpdateQuickWatch(progressInPercent);
        }

        void p2pManager_OnGuiMessage(string sData, NotificationLevel notificationLevel)
        {
            GuiLogMessage(sData, notificationLevel);
        }

        DistributableKeyPatternJob distributableKeyPatternJob;
        public void Execute()
        {
            if (this.DecryptedData != null)
            {
                // TODO: dirty hack because of a missing Initialization vector
                // it can't be null, because serialization will throw an exception in this case
                if (this.InitVector == null)
                {
                    this.InitVector = new byte[8]{0,0,0,0,0,0,0,0};
                    GuiLogMessage("Initialization vector not set, so set a standard value - dirty hack!", NotificationLevel.Info);
                }
                this.process(this.EncryptionControl);
            }                
        }

        private void process(IControlEncryption iControlEncryption)
        {
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PControl == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }
            if (iControlEncryption == null)
            {
                GuiLogMessage("No Encryption Control connected with this PlugIn", NotificationLevel.Error);
                return;
            }

            if (this.p2pManager == null)
            {
                byte[] byteEncryptedData = null;
                CryptoolStream newEncryptedData = new CryptoolStream();
                try
                {
                    newEncryptedData.OpenRead(this.DecryptedData.FileName);
                    // Convert CryptoolStream to an byte Array and store it in the DHT
                    if (newEncryptedData.Length > Int32.MaxValue)
                        throw (new Exception("Encrypted Data are too long for this PlugIn. The maximum size of Data is " + Int32.MaxValue + "!"));
                    byteEncryptedData = new byte[newEncryptedData.Length];
                    int k = newEncryptedData.Read(byteEncryptedData, 0, byteEncryptedData.Length);
                    if (k < byteEncryptedData.Length)
                        throw (new Exception("Read Data are shorter than byteArrayLen"));
                }
                catch (Exception ex)
                {
                    throw (new Exception("Fatal error while reading the CryptoolStream. Exception: " + ex.ToString()));
                }
                finally
                {
                    newEncryptedData.Close();
                    newEncryptedData.Dispose();
                }
                             
                string pattern = this.encryptionControl.getKeyPattern();
                KeyPattern kp = new KeyPattern(pattern);
                if (kp.testWildcardKey(this.settings.Key))
                {
                    kp.WildcardKey = this.settings.Key;
                }
                else
                {
                    GuiLogMessage("The Key Pattern in the settings isn't valid for the given Encryption!", NotificationLevel.Error);
                    return;
                }

                // create a new DistributableJob instance
                distributableKeyPatternJob = new DistributableKeyPatternJob
                    (kp, this.settings.KeyPatternSize * 10000, byteEncryptedData, this.InitVector);

                //set progress chunk job count
                SetProgressChunkJobCount(this.distributableKeyPatternJob.TotalAmount);

                this.p2pManager = new P2PManagerBase_NEW(this.P2PControl, distributableKeyPatternJob);
                this.p2pManager.OnGuiMessage += new P2PPublisherBase.GuiMessage(p2pManager_OnGuiMessage);
                this.p2pManager.OnProcessProgress += new P2PManagerBase_NEW.ProcessProgress(p2pManager_OnProcessProgress);
                this.p2pManager.OnNewJobAllocated += new P2PManagerBase_NEW.NewJobAllocated(p2pManager_OnNewJobAllocated);
                this.p2pManager.OnNoMoreJobsLeft += new P2PManagerBase_NEW.NoMoreJobsLeft(p2pManager_OnNoMoreJobsLeft);
                this.p2pManager.OnResultReceived += new P2PManagerBase_NEW.ResultReceived(p2pManager_OnResultReceived);
                this.p2pManager.OnJobCanceled += new P2PManagerBase_NEW.JobCanceled(p2pManager_OnJobCanceled);
                this.p2pManager.OnAllJobResultsReceived += new P2PManagerBase_NEW.AllJobResultsReceived(p2pManager_OnAllJobResultsReceived);
            }

            this.bytesToUseForDecryption = this.settings.BytesToUse;

            this.p2pManager.StartManager(this.settings.TopicName, this.settings.SendAliveMessageInterval * 1000);

            //added 2010.02.26 for displaying actual processing time - this variables have to be reset when restarting manager
            firstTimeJobAllocated = true;

            this.settings.MngStatusChanged(P2PManager_KeyPatternSettings.MngStatus.Working);
        }

        void p2pManager_OnAllJobResultsReceived(BigInteger lastJobId)
        {
            timerProcessingTimeReset();
        }

        void p2pManager_OnJobCanceled(BigInteger jobId)
        {
            UpdateProgressChunk(jobId, System.Windows.Media.Brushes.Red);
        }

        void p2pManager_OnResultReceived(BigInteger jobId)
        {
            UpdateProgressChunk(jobId, System.Windows.Media.Brushes.Green);

            this.settings.MngStatusChanged(P2PManager_KeyPatternSettings.MngStatus.Finished);
        }

        void p2pManager_OnNoMoreJobsLeft()
        {
            this.settings.MngStatusChanged(P2PManager_KeyPatternSettings.MngStatus.Finished);
        }


        private bool firstTimeJobAllocated = true;
        private System.Timers.Timer timerProcessingTime;
        void p2pManager_OnNewJobAllocated(BigInteger jobId)
        {
            if (firstTimeJobAllocated)
            {
                timerProcessingTime = new System.Timers.Timer(1000);
                timerProcessingTime.Elapsed += new ElapsedEventHandler(timerProcessingTime_Elapsed);
                timerProcessingTime.Start();
                firstTimeJobAllocated = false;
            }
            UpdateProgressChunk(jobId, System.Windows.Media.Brushes.Yellow);

            this.settings.MngStatusChanged(P2PManager_KeyPatternSettings.MngStatus.Working);
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion
    }

}