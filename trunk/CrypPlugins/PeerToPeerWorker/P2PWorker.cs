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
using System.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.Plugins.KeySearcher_IControl;
using KeySearcher;

namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// This PlugIn only works, when its connected with a P2P_Peer object.
    /// </summary>
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "P2P_Worker", "Creates a new Working-Peer", "", "PeerToPeerWorker/worker_medium_neutral.png", "PeerToPeerWorker/worker_medium_working.png", "PeerToPeerWorker/worker_medium_finished.png")]
    public class P2PWorker : IInput
    {
        private P2PWorkerSettings settings;
        private IP2PControl p2pControl;
        private IControlKeySearcher keySearcherControl;
        private P2PWorkerBase p2pWorker;

        #region Constructor and setting stuff
        public P2PWorker()
        {
            this.settings = new P2PWorkerSettings(this);
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
                if (this.p2pWorker != null)
                {
                    this.p2pWorker.Stop(PubSubMessageType.Unregister);
                    GuiLogMessage("Worker unregistered from Publisher!", NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Manager isn't initialized, so this action isn't possible.", NotificationLevel.Info);
                }
            }
            if (e.PropertyName == "BtnRegister")
            {
                RegisterSubscriber();
                GuiLogMessage("Worker registers with Publisher!", NotificationLevel.Info);
            }
            if (e.PropertyName == "BtnSolutionFound")
            {
                if (this.p2pWorker != null)
                {
                    this.p2pWorker.SolutionFound("");
                    GuiLogMessage("Solution found message sent to Manager.", NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Manager isn't initialized, so this action isn't possible.", NotificationLevel.Info);
                }
            }
        }
        #endregion

        #region In and Output

        [PropertyInfo(Direction.ControlMaster, "KeySearcher Master", "Connect the KeySearcher-PlugIn", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public IControlKeySearcher KeySearcherControl
        {
            get
            {
                return this.keySearcherControl;
            }
            set
            {
                if (this.keySearcherControl != null)
                {
                    this.keySearcherControl.OnStatusChanged -= KeySearcherControl_OnStatusChanged;
                }
                if (value != null)
                {
                    this.keySearcherControl = (KeySearcherMaster)value;
                    this.keySearcherControl.OnStatusChanged +=new IControlStatusChangedEventHandler(KeySearcherControl_OnStatusChanged);
                    OnPropertyChanged("KeySearcherControl");
                }
                else
                {
                    this.keySearcherControl = null;
                }
            }
        }

        private void KeySearcherControl_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            if (readyForExecution)
                GuiLogMessage("KeySearcherControl_OnStatusChanged thrown, readyForExecution = true",NotificationLevel.Info);
                //this.KeySearcherControl.bruteforcePattern();
        }

        /// <summary>
        /// Catches the completely configurated, initialized and joined P2P object from the P2PPeer-Slave-PlugIn.
        /// </summary>
        [PropertyInfo(Direction.ControlMaster, "P2P Master", "Input the P2P-Peer-PlugIn", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

        private string sOutputvalue;
        [PropertyInfo(Direction.OutputData, "Data from subscribed Publisher", "When you're subscribed to an alive Publisher, receive published data here", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string Outputvalue
        {
            get
            {
                return this.sOutputvalue;
            }
            set
            {
                if (value != this.sOutputvalue)
                {
                    this.sOutputvalue = value;
                    OnPropertyChanged("Outputvalue");
                }
            }
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
            set { this.settings = (P2PWorkerSettings)value; }
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
            if(this.p2pWorker != null)
                this.p2pWorker.Stop(PubSubMessageType.Stop);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
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
            if (this.settings.TopicName != null)
            {
                RegisterSubscriber();
            }
            else
            {
                GuiLogMessage("The settings are empty. Operation isn't possible.", NotificationLevel.Error);
            }
        }

        private void RegisterSubscriber()
        {
            if (this.p2pWorker == null)
            {
                this.p2pWorker = new P2PWorkerBase(this.P2PControl, this.KeySearcherControl);
                this.p2pWorker.OnGuiMessage += new P2PWorkerBase.GuiMessage(p2pWorker_OnGuiMessage);
                this.p2pWorker.OnTextArrivedFromPublisher += new P2PWorkerBase.TextArrivedFromPublisher(p2pWorker_OnTextArrivedFromPublisher);
                this.p2pWorker.OnKeyPatternReceived += new P2PWorkerBase.KeyPatternReceived(p2pWorker_OnKeyPatternReceived);
                this.p2pWorker.OnFinishedBruteforcingThePattern += new P2PWorkerBase.FinishedBruteforcingThePattern(p2pWorker_OnFinishedBruteforcingThePattern);
                this.p2pWorker.OnReceivedStopMessageFromPublisher += new P2PSubscriberBase.ReceivedStopFromPublisher(p2pWorker_OnReceivedStopMessageFromPublisher);

                this.p2pWorker.Register(this.settings.TopicName, (long)(this.settings.CheckPublishersAvailability * 1000),
                    (long)(this.settings.PublishersReplyTimespan * 1000));
            }
            else
            {
                this.p2pWorker.Register(this.settings.TopicName, (long)(this.settings.CheckPublishersAvailability * 1000),
                    (long)(this.settings.PublishersReplyTimespan * 1000));
            }
        }

        #region Handle Worker/Subscriber Events

        // Only three enum-Types valid: Stop, Unregister and Solution!
        void p2pWorker_OnReceivedStopMessageFromPublisher(PubSubMessageType stopType, string sData)
        {
            switch (stopType)
            {
                case PubSubMessageType.Stop:
                case PubSubMessageType.Unregister:
                    this.settings.WorkerStatusChanged(P2PWorkerSettings.WorkerStatus.Neutral);
                    break;
                case PubSubMessageType.Solution:
                    this.settings.WorkerStatusChanged(P2PWorkerSettings.WorkerStatus.Finished);
                    break;
                default:
                    break;
            }
        }

        void p2pWorker_OnFinishedBruteforcingThePattern(KeyPattern pattern)
        {
            this.settings.WorkerStatusChanged(P2PWorkerSettings.WorkerStatus.Finished);
        }

        void p2pWorker_OnKeyPatternReceived(KeyPattern pattern)
        {
            this.settings.WorkerStatusChanged(P2PWorkerSettings.WorkerStatus.Working);
        }

        void p2pWorker_OnTextArrivedFromPublisher(string sData, PeerId pid)
        {
            this.Outputvalue = sData;
        }

        void p2pWorker_OnGuiMessage(string sData, NotificationLevel notificationLevel)
        {
            GuiLogMessage(sData, notificationLevel);
        }

        #endregion

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