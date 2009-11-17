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

/*
 * IDEAS:
 * - Publisher takes subscriber list out of the DHT and registered 
 *   itself with all subscribers pro-active (handle Register-Msg in Subscriber!)
 * 
 * TODO:
 * - Handle "publisher-changed" case (reconfirm registration, etc.)
 */

namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// This PlugIn only works, when its connected with a P2P_Peer object.
    /// </summary>
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(true, "P2P_Subscriber", "Creates a new Subscribing-Peer", "", "PeerToPeerSubscriber/ct2_p2p_sub_medium.png")]
    public class P2PSubscriber : IInput
    {
        #region Variables

        private string sDHTSettingsPostfix = "Settings";
        private long sendAliveMessageInterval;
        private long checkPublishersAvailability;

        private P2PSubscriberSettings settings;
        private IP2PControl p2pMaster;
        /// <summary>
        /// If the DHT Entry of the given Task is empty, continous try to 
        /// find a meanwhile inscribed Publishers PeerID
        /// </summary>
        private Timer timerRegisteringNotPossible;
        /// <summary>
        /// For informing the publisher pro-active, that this subscriber
        /// is still interested in this Task.
        /// </summary>
        private Timer timerSendingAliveMsg;
        /// <summary>
        /// checking liveness, availability and/or changes (new peer) of the Publisher
        /// </summary>
        private Timer timerCheckPubAvailability;
        /// <summary>
        /// this timer gets started when the availability of the publisher, 
        /// at which the subscriber had registered, is checked. If the timer
        /// callback is called and no Pong-message was received, the probability
        /// that the Publisher is down is high! 
        /// </summary>
        private Timer timeoutForPublishersPong;
        /// <summary>
        /// After register message is sent to publisher, this timer gets started.
        /// If the publisher doesn't response with a RegisteringAccepted-Message,
        /// the probability that the publisher is down is high!
        /// </summary>
        private Timer timeoutForPublishersRegAccept;
        /// <summary>
        /// PeerID of the actual publisher. This ID is will be checked continious
        /// on liveliness and/or updated if Publisher had changed.
        /// </summary>
        private string actualPublisher;

        #endregion

        #region In and Output

        /// <summary>
        /// Catches the completely configurated, initialized and joined P2P object from the P2PPeer-Slave-PlugIn.
        /// </summary>
        [PropertyInfo(Direction.ControlMaster,"P2P Slave","Input the P2P-Peer-PlugIn","",true,false,DisplayLevel.Beginner,QuickWatchFormat.Text,null)]
        public IP2PControl P2PMaster 
        {
            get
            {
                return this.p2pMaster;
            }
            set
            {
                if (this.p2pMaster != null)
                {
                    this.p2pMaster.OnPeerReceivedMsg -= P2PMaster_OnPeerReceivedMsg;
                    this.p2pMaster.OnStatusChanged -= P2PMaster_OnStatusChanged;
                }
                if (value != null)
                {
                    this.p2pMaster = (P2PPeerMaster)value;
                    this.p2pMaster.OnPeerReceivedMsg += new P2PBase.P2PMessageReceived(P2PMaster_OnPeerReceivedMsg);
                    this.p2pMaster.OnStatusChanged += new IControlStatusChangedEventHandler(P2PMaster_OnStatusChanged);
                    OnPropertyChanged("P2PMaster");
                }
                else
                {
                    this.p2pMaster = null;
                }
            }
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

        private void P2PMaster_OnPeerReceivedMsg(string sSourceAddr, string sData)
        {
            MessageReceived(sSourceAddr, sData);
        }

        private void P2PMaster_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            //throw new NotImplementedException();
        }

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region Standard functionality
        public P2PSubscriber()
        {
            this.settings = new P2PSubscriberSettings(this);
            this.settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            this.settings.TaskPaneAttributeChanged += new TaskPaneAttributeChangedHandler(settings_TaskPaneAttributeChanged);
        }

        void settings_TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            // throw new NotImplementedException();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BtnUnregister")
            {
                StopActiveWork();
                GuiLogMessage("Subscriber unregistered from Publisher!", NotificationLevel.Info);
            }
            if (e.PropertyName == "BtnRegister")
            {
                Register();
                GuiLogMessage("Subscriber registers with Publisher!", NotificationLevel.Info);
            }
            if (e.PropertyName == "BtnSolutionFound")
            {
                SendMessage(actualPublisher, PubSubMessageType.Solution);
                GuiLogMessage("Solution found message sent to Publisher.", NotificationLevel.Info);
            }
        }

        public ISettings Settings
        {
            set { this.settings = (P2PSubscriberSettings)value; }
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
            StopActiveWork();
        }

        public void Stop()
        {
            StopActiveWork();
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
            if (P2PMaster == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }
            if (this.settings.TopicName != null)
            {
                Register();
            }
            else
            {
                GuiLogMessage("The settings are empty. Operation isn't possible.", NotificationLevel.Error);
            }
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

        #region Subscriber methods

        /// <summary>
        /// if true, check whether a new Publisher is the actual one
        /// and renew Settings
        /// </summary>
        private bool bolStopped = true;

        private void Register()
        {
            string sPubId = CheckPublisherAvailability();
            // if DHT Entry for Task is empty, there exist no Publisher at present.
            // The method PublisherIsAlive starts a Timer for this case to continous proof Publisher-DHT-Entry
            if (sPubId != null)
            {
                SendMessage(sPubId, PubSubMessageType.Register);
                long interval = this.settings.PublishersReplyTimespan * 1000;
                if (this.timeoutForPublishersRegAccept == null)
                    this.timeoutForPublishersRegAccept = new Timer(OnTimeoutRegisteringAccepted, null, interval, interval);
                this.bolStopped = false;
            }
        }

        private void MessageReceived(string sSourceAddr, string sData)
        {
            if (sSourceAddr != actualPublisher)
            {
                GuiLogMessage("RECEIVED message from third party peer (not the publisher!): " + sData.Trim() + ", ID: " + sSourceAddr, NotificationLevel.Info);
                return;
            }

            PubSubMessageType msgType = this.P2PMaster.GetMsgType(sData);

            switch (msgType)
            {
                case PubSubMessageType.RegisteringAccepted:
                    GuiLogMessage("REGISTERING ACCEPTED received from publisher!", NotificationLevel.Info);
                    if (this.timeoutForPublishersRegAccept != null)
                    {
                        this.timeoutForPublishersRegAccept.Dispose();
                        this.timeoutForPublishersRegAccept = null;
                    }
                    break;
                case PubSubMessageType.Ping:
                    SendMessage(sSourceAddr, PubSubMessageType.Pong);
                    GuiLogMessage("REPLIED to a ping message from " + sSourceAddr, NotificationLevel.Info);
                    break;
                case PubSubMessageType.Register:
                case PubSubMessageType.Unregister:
                    /* can't work the right way, because at present the
                     * publisher can't remove information of the DHT 
                     * (point of entry for subscribers) */
                    GuiLogMessage(msgType.ToString().ToUpper() + " received from PUBLISHER.", NotificationLevel.Warning);
                    // continuously try to get a unregister and than re-register with publisher
                    StopActiveWork();
                    Register();
                    break;
                case PubSubMessageType.Solution:
                    StopActiveWork();
                    GuiLogMessage("Another Subscriber had found the solution!",NotificationLevel.Info);
                    break;
                case PubSubMessageType.Stop:
                    StopActiveWork();
                    GuiLogMessage("STOP received from publisher. Subscriber is stopped!", NotificationLevel.Warning);
                    break;
                case PubSubMessageType.Pong:
                    if (this.timeoutForPublishersPong != null)
                    {
                        this.timeoutForPublishersPong.Dispose();
                        this.timeoutForPublishersPong = null;
                    }
                    break;
                // if the received Data couldn't be casted to enum, 
                // it must be text-data
                case PubSubMessageType.NULL:
                    GuiLogMessage("RECEIVED: Message from '" + sSourceAddr
                    + "' with data: '" + sData + "'", NotificationLevel.Info);
                    Outputvalue = sData;
                    break;
                case PubSubMessageType.Alive:
                default:
                    // not possible at the moment
                    break;
            }
        }

        private void SendMessage(string pubPeerId, PubSubMessageType msgType)
        {
            if (timerSendingAliveMsg == null && !this.bolStopped)
                timerSendingAliveMsg = new Timer(OnSendAliveMessage, null, sendAliveMessageInterval, sendAliveMessageInterval);

            switch (msgType)
            {
                case PubSubMessageType.Register:
                    // stop "RegisteringNotPossibleTimer
                    if (timerRegisteringNotPossible != null)
                    {
                        timerRegisteringNotPossible.Dispose();
                        timerRegisteringNotPossible = null;
                    }
                    break;
                case PubSubMessageType.Alive:
                case PubSubMessageType.Ping:
                case PubSubMessageType.Pong:
                case PubSubMessageType.Unregister:
                    break;
                case PubSubMessageType.Solution:
                    StopActiveWork();
                    break;
                default:
                    GuiLogMessage("No Message sent, because MessageType wasn't supported: " + msgType.ToString(),NotificationLevel.Warning);
                    return;
            }
            this.P2PMaster.SendToPeer(msgType, pubPeerId);

            // don't show every single alive message
            if(msgType != PubSubMessageType.Alive)
                GuiLogMessage(msgType.ToString() + " message sent to Publisher", NotificationLevel.Info);
        }

        // registering isn't possible if no publisher has stored
        // his ID in the DHT Entry with the Key TaskName
        private void OnRegisteringNotPossible(object state)
        {
            string sPubId = CheckPublisherAvailability();
            // if DHT Entry for Task is empty, there exist no Publisher at present.
            // The method PublisherIsAlive starts a Timer for this case to continous proof Publisher-DHT-Entry
            if (sPubId != null)
                SendMessage(sPubId, PubSubMessageType.Register);
        }

        private void OnSendAliveMessage(object state)
        {
            SendMessage(actualPublisher, PubSubMessageType.Alive);
        }

        private string CheckPublisherAvailability()
        {
            byte[] bytePubId = P2PMaster.DHTload(this.settings.TopicName);
            if (bytePubId == null)
            {
                if (timerRegisteringNotPossible == null && !this.bolStopped)
                {
                    // if DHT value doesn't exist at this moment, wait for 10 seconds and try again
                    timerRegisteringNotPossible = new Timer(OnRegisteringNotPossible, null, 10000, 10000);
                }
                return null;
            }

            byte[] byteISettings = P2PMaster.DHTload(this.settings.TopicName + this.sDHTSettingsPostfix);
            if (byteISettings == null)
            {
                GuiLogMessage("Can't find settings from Publisher for the Subscriber.", NotificationLevel.Error);
                return null;
            }
            sendAliveMessageInterval = System.BitConverter.ToInt32(byteISettings, 0);

            string sPubId = UTF8Encoding.UTF8.GetString(bytePubId);
            GuiLogMessage("RECEIVED: Publishers' peer name '" + sPubId + "', Alive-Msg-Interval: " + sendAliveMessageInterval / 1000 + " sec!", NotificationLevel.Info);

            if (actualPublisher == null) //first time initialization
                actualPublisher = sPubId;

            checkPublishersAvailability = this.settings.CheckPublishersAvailability * 1000;

            // setting timer to check periodical the availability of the publishing peer
            if (timerCheckPubAvailability == null && !this.bolStopped)
                timerCheckPubAvailability = new Timer(OnCheckPubAvailability, null, checkPublishersAvailability, checkPublishersAvailability);

            return sPubId;
        }

        private void OnCheckPubAvailability(object state)
        {
            string sNewPubId = CheckPublisherAvailability();

            if (sNewPubId == null)
            {
                GuiLogMessage("Publisher wasn't found in DHT or settings didn't stored on the right way.", NotificationLevel.Warning);
                return;
            }
            if (sNewPubId != actualPublisher)
            {
                //Handle case, when publisher changed or isn't active at present (don't reply on response)
                GuiLogMessage("CHANGED: Publisher from '" + actualPublisher
                    + "' to '" + sNewPubId + "'!", NotificationLevel.Info);
                actualPublisher = sNewPubId;
                // because the publisher has changed, send a new register msg
                SendMessage(actualPublisher, PubSubMessageType.Register);
            }
            else
            {
                // Timer will be only stopped, when OnMessageReceived-Event received 
                // a Pong-Response from the publisher! 
                SendMessage(actualPublisher, PubSubMessageType.Ping);
                if (timeoutForPublishersPong == null)
                {
                    long interval = this.settings.PublishersReplyTimespan * 1000;
                    timeoutForPublishersPong = new Timer(OnTimeoutPublishersPong, null, interval, interval);
                }
            }
        }

        /// <summary>
        /// This callback only get fired, when the publisher didn't sent a response on the register message.
        /// </summary>
        /// <param name="state"></param>
        private void OnTimeoutRegisteringAccepted(object state)
        {
            GuiLogMessage("TIMEOUT: Waiting for registering accepted message from publisher!", NotificationLevel.Warning);
            // TODO: anything
        }

        /// <summary>
        /// This callback only get fired, when the publisher didn't sent a response on the ping message.
        /// </summary>
        /// <param name="state"></param>
        private void OnTimeoutPublishersPong(object state)
        {
            GuiLogMessage("Publisher didn't answer on Ping in the given time span!", NotificationLevel.Warning);
            timeoutForPublishersPong.Dispose();
            timeoutForPublishersPong = null;
            // try to get an active publisher and re-register
            CheckPublisherAvailability();
        }

        /// <summary>
        /// Will stop all timers, so Subscriber ends with sending
        /// Register-, Alive- and Pong-messages. Furthermore an
        /// unregister message will be send to the publisher
        /// </summary>
        private void StopActiveWork()
        {
            this.bolStopped = true;
            if(actualPublisher != null)
                SendMessage(actualPublisher,PubSubMessageType.Unregister);

            #region stopping all timers, if they are still active
            if (this.timerSendingAliveMsg != null)
            {
                this.timerSendingAliveMsg.Dispose();
                this.timerSendingAliveMsg = null;
            }
            if (this.timerRegisteringNotPossible != null)
            {
                this.timerRegisteringNotPossible.Dispose();
                this.timerRegisteringNotPossible = null;
            }
            if (this.timerCheckPubAvailability != null)
            {
                this.timerCheckPubAvailability.Dispose();
                this.timerCheckPubAvailability = null;
            }
            if (this.timeoutForPublishersRegAccept != null)
            {
                this.timeoutForPublishersRegAccept.Dispose();
                this.timeoutForPublishersRegAccept = null;
            }
            if (this.timeoutForPublishersPong != null)
            {
                this.timeoutForPublishersPong.Dispose();
                this.timeoutForPublishersPong = null;
            }
            #endregion
        }
        #endregion
    }
}
