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
using Cryptool.Plugins.PeerToPeer.Jobs;
using Cryptool.Plugins.PeerToPeer.Internal;

namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// This PlugIn only works, when its connected with a P2P_Peer object.
    /// </summary>
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "P2P_JobAdmin", "Creates a new Job-Administration-Peer, which receives and forwards jobs ans jobresults", "", "PeerToPeerJobAdmin/worker_medium_neutral.png", "PeerToPeerJobAdmin/worker_medium_working.png", "PeerToPeerJobAdmin/worker_medium_finished.png")]
    public class P2PJobAdmin : IInput
    {
        private P2PJobAdminBase jobAdminBase;
        private P2PJobAdminSettings settings;
        private IControlWorker workerControl;
        private IP2PControl p2pControl;

        #region Constructor and setting stuff
        public P2PJobAdmin()
        {
            this.settings = new P2PJobAdminSettings();
            this.settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            this.settings.TaskPaneAttributeChanged += new TaskPaneAttributeChangedHandler(settings_TaskPaneAttributeChanged);
            this.settings.OnPluginStatusChanged += new StatusChangedEventHandler(settings_OnPluginStatusChanged);
        }

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
                OnPluginStatusChanged(this, args);
        }

        void settings_TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            // throw new NotImplementedException();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BtnUnregister")
            {
                if (this.jobAdminBase.Started)
                {
                    this.jobAdminBase.StopWorkerControl(PubSubMessageType.Unregister);
                    GuiLogMessage("Worker unregistered from Publisher!", NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Worker isn't started, so this action isn't possible.", NotificationLevel.Info);
                }
            }
            if (e.PropertyName == "BtnRegister")
            {
                this.jobAdminBase.StartWorkerControl(this.settings.TopicName, this.settings.CheckPublishersAvailability * 1000, this.settings.PublishersReplyTimespan * 1000);
                GuiLogMessage("Worker registers with Publisher!", NotificationLevel.Info);
            }
        }
        #endregion

        #region In and Output

        [PropertyInfo(Direction.ControlMaster, "Working Master", "Connect a WorkingMaster-PlugIn", "", true, false, QuickWatchFormat.Text, null)]
        public IControlWorker WorkerControl
        {
            get
            {
                return this.workerControl;
            }
            set
            {
                this.workerControl = value;
                OnPropertyChanged("WorkerControl");
            }
        }

        /// <summary>
        /// Catches the completely configurated, initialized and joined P2P object from the P2PPeer-Slave-PlugIn.
        /// </summary>
        [PropertyInfo(Direction.ControlMaster, "P2P Master", "Input the P2P-Peer-PlugIn", "", true, false, QuickWatchFormat.Text, null)]
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
                    this.p2pControl = (IP2PControl)value;
                    this.p2pControl.OnStatusChanged += new IControlStatusChangedEventHandler(P2PControl_OnStatusChanged);
                    OnPropertyChanged("P2PControl");
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

        #region Standard functionality

        public ISettings Settings
        {
            set { this.settings = (P2PJobAdminSettings)value; }
            get { return this.settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        //execution method is below this region

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            if (this.jobAdminBase != null && this.jobAdminBase.Started)
            {
                this.jobAdminBase.StopWorkerControl(PubSubMessageType.Stop);
            }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            if (this.jobAdminBase != null && this.jobAdminBase.Started)
            {
                this.jobAdminBase.StopWorkerControl(PubSubMessageType.Stop);
            }
        }
        #endregion

        public void Execute()
        {
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PControl == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }
            if (this.settings.TopicName != null && this.settings.PublishersReplyTimespan > 0 && this.settings.CheckPublishersAvailability > 0)
            {
                StartJobAdmin();
            }
            else
            {
                GuiLogMessage("The settings are empty. Operation isn't possible.", NotificationLevel.Error);
            }
        }

        void jobAdminBase_OnGuiMessage(string sData, NotificationLevel notificationLevel)
        {
            GuiLogMessage(sData, notificationLevel);
        }

        void jobAdminBase_OnWorkerStopped()
        {
            this.settings.WorkerStatusChanged(P2PJobAdminSettings.WorkerStatus.Neutral);            
        }

        void jobAdminBase_OnSuccessfullyEnded()
        {
            this.settings.WorkerStatusChanged(P2PJobAdminSettings.WorkerStatus.Finished);
        }

        void jobAdminBase_OnStartWorking()
        {
            this.settings.WorkerStatusChanged(P2PJobAdminSettings.WorkerStatus.Working);
        }

        void jobAdminBase_OnCanceledWorking()
        {
            this.settings.WorkerStatusChanged(P2PJobAdminSettings.WorkerStatus.Neutral);
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

        public void StartJobAdmin()
        {
            if(this.settings.TopicName == null || this.settings.CheckPublishersAvailability <= 0 || this.settings.PublishersReplyTimespan <= 0)
            {
                GuiLogMessage("Please set all settings before you want to start the P2PJobAdmin",NotificationLevel.Warning);
                return;
            }
            if (this.P2PControl == null || this.WorkerControl == null)
            {
                GuiLogMessage("Starting P2PJobAdmin isn't possible while P2PPeer and/or WorkerControl isn't connected with this PlugIn.", NotificationLevel.Error);
                return;
            }
            if (this.jobAdminBase == null)
            {
                this.jobAdminBase = new P2PJobAdminBase(this.P2PControl, this.WorkerControl);
                this.jobAdminBase.OnGuiMessage += new P2PSubscriberBase.GuiMessage(jobAdminBase_OnGuiMessage);
                this.jobAdminBase.OnStartWorking += new P2PJobAdminBase.StartWorking(jobAdminBase_OnStartWorking);
                this.jobAdminBase.OnSuccessfullyEnded += new P2PJobAdminBase.SuccessfullyEnded(jobAdminBase_OnSuccessfullyEnded);
                this.jobAdminBase.OnCanceledWorking += new P2PJobAdminBase.CanceledWorking(jobAdminBase_OnCanceledWorking);
                this.jobAdminBase.OnWorkerStopped += new P2PJobAdminBase.WorkerStopped(jobAdminBase_OnWorkerStopped);
            }
            if (!this.jobAdminBase.Started)
            {
                this.jobAdminBase.StartWorkerControl(this.settings.TopicName, this.settings.CheckPublishersAvailability * 1000,
                    this.settings.PublishersReplyTimespan * 1000);
            }
            else
            {
                GuiLogMessage("P2PJobAdmin is already started.", NotificationLevel.Info);
            }
        }
    }
}