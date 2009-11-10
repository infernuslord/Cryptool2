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
 * TODO:
 * - Receive "add"-Message from Publisher (2-way-handshake),
 *   so you can be sure that the Publisher exists at present
 * - Handle "publisher-changed" case (reconfirm registration, etc.)
 * - Check availability of Publisher periodically (make GuiLogMsg)
 * - Unregister subscriber on Stop-Action
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
        /// <summary>
        /// if true, check whether a new Publisher is the actual one
        /// and renew Settings
        /// </summary>
        private bool bolStopped = true;

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
        private byte[] actualPublisher;

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
            bolStopped = false;
        }

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
            bolStopped = true;
            try
            {
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
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.ToString(), NotificationLevel.Error);
            }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
        #endregion

        #region Methods, EventHandler, etc. for Subscriber

        public void Execute()
        {
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PMaster == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }
            if (this.settings.TaskName != null)
            {
                byte[] bytePubId = CheckPublisherAvailability();
                // if DHT Entry for Task is empty, there exist no Publisher at present.
                // The method PublisherIsAlive starts a Timer for this case to continous proof Publisher-DHT-Entry
                if (bytePubId != null)
                    SendMsgToPublisher(bytePubId, PubSubMessageType.Register);
            }
            else
            {
                GuiLogMessage("The settings are empty. Operation isn't possible.", NotificationLevel.Error);
            }
        }

        private void SendMsgToPublisher(byte[] pubPeerId, PubSubMessageType msgType)
        {
            if (timerSendingAliveMsg == null && !bolStopped)
                timerSendingAliveMsg = new Timer(OnSendAliveMessage, null, sendAliveMessageInterval, sendAliveMessageInterval);

            switch (msgType)
            {
                case PubSubMessageType.Register:
                    // stop "RegisteringNotPossibleTimer
                    if(timerRegisteringNotPossible != null)
                        timerRegisteringNotPossible.Dispose();

                    // send register message to the publisher peer
                    this.P2PMaster.SendToPeer("regi", pubPeerId);
                    GuiLogMessage("Register message sent to Publishing", NotificationLevel.Info);
                    break;
                case PubSubMessageType.Alive:
                    this.P2PMaster.SendToPeer("aliv", pubPeerId);
                    GuiLogMessage("Alive message sent to Publisher",NotificationLevel.Info);
                    break;
                case PubSubMessageType.Pong:
                    this.P2PMaster.SendToPeer("pong", pubPeerId);
                    GuiLogMessage("Pong message sent to Publisher", NotificationLevel.Info);
                    break;
                default:
                    break;
            }
        }

        // registering isn't possible if no publisher has stored
        // his ID in the DHT Entry with the Key TaskName
        private void OnRegisteringNotPossible(object state)
        {
            byte[] bytePubId = CheckPublisherAvailability();
            // if DHT Entry for Task is empty, there exist no Publisher at present.
            // The method PublisherIsAlive starts a Timer for this case to continous proof Publisher-DHT-Entry
            if (bytePubId != null)
                SendMsgToPublisher(bytePubId, PubSubMessageType.Register);
        }

        private void OnSendAliveMessage(object state)
        {
            SendMsgToPublisher(actualPublisher, PubSubMessageType.Alive);
        }

        private void P2PMaster_OnPeerReceivedMsg(byte[] byteSourceAddr, string sData)
        {
            if (sData.Trim() == "ping")
            {
                SendMsgToPublisher(byteSourceAddr, PubSubMessageType.Pong);
                GuiLogMessage("REPLIED to a ping message from the publisher", NotificationLevel.Info);
            }
            else
            {
                GuiLogMessage("RECEIVED: Message from '" + P2PMaster.ConvertPeerId(byteSourceAddr)
                    + "' with data: '" + sData + "'", NotificationLevel.Info);
                Outputvalue = sData;
            }
        }

        private byte[] CheckPublisherAvailability()
        {
            byte[] bytePubId = P2PMaster.DHTload(this.settings.TaskName);
            if (bytePubId == null)
            {
                if (timerRegisteringNotPossible == null && !bolStopped)
                {
                    // if DHT value doesn't exist at this moment, wait for 10 seconds and try again
                    timerRegisteringNotPossible = new Timer(OnRegisteringNotPossible, null, 10000, 10000);
                }
                return null;
            }
            byte[] byteISettings = P2PMaster.DHTload(this.settings.TaskName + this.sDHTSettingsPostfix);
            if (byteISettings == null)
            {
                GuiLogMessage("Can't find settings from Publisher for the Subscriber.", NotificationLevel.Error);
                return null;
            }
            sendAliveMessageInterval = System.BitConverter.ToInt32(byteISettings, 0);

            string sPublisherName = P2PMaster.ConvertPeerId(bytePubId);
            GuiLogMessage("RECEIVED: Publishers' peer name '" + sPublisherName + "', Alive-Msg-Interval: " + sendAliveMessageInterval / 1000 + " sec!", NotificationLevel.Info);

            if (actualPublisher == null) //first time initialization
                actualPublisher = bytePubId;

            checkPublishersAvailability = this.settings.CheckPublishersAvailability * 1000;

            // setting timer to check periodical the availability of the publishing peer
            if (timerCheckPubAvailability == null && !bolStopped)
                timerCheckPubAvailability = new Timer(OnCheckPubAvailability, null, checkPublishersAvailability, checkPublishersAvailability);

            return bytePubId;
        }

        private void OnCheckPubAvailability(object state)
        {
            byte[] newPubId = CheckPublisherAvailability();

            string sNewPubId = P2PMaster.ConvertPeerId(newPubId);
            string sActualPeerId = P2PMaster.ConvertPeerId(actualPublisher);
            if (sNewPubId != sActualPeerId)
            {
                //Handle case, when publisher changed or isn't active at present (don't reply on response)
                GuiLogMessage("CHANGED: Publisher from '" + sActualPeerId
                    + "' to '" + sNewPubId + "'!", NotificationLevel.Info);
            }
            SendMsgToPublisher(newPubId, PubSubMessageType.Ping);
            // TODO: handle asynchronous reply or timeout ...
            // bool unansweredPingToPub = true;
            // //Start Timer with settings-interval to check whether a pong arrived
            // this.settings.PublishersReplyTimespan
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
