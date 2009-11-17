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
 * - FUTURE: dual data management of subscriber list (on local peer and in DHT)
 */

namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// This PlugIn only works, when its connected with a P2P_Peer object.
    /// </summary>
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "P2P_Publisher", "Creates a new Publishing-Peer", "", "PeerToPeerPublisher/ct2_p2p_pub_medium.png")]
    public class P2PPublisher : IInput
    {
        private P2PPublisherSettings settings;
        private IP2PControl p2pMaster;
        private P2PPublisherBase p2pPublisher;

        public P2PPublisher()
        {
            this.settings = new P2PPublisherSettings(this);
            this.settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            this.settings.TaskPaneAttributeChanged += new TaskPaneAttributeChangedHandler(settings_TaskPaneAttributeChanged);
        }

        #region SettingEvents

        private void settings_TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // storing settings for subscribers in the DHT, so they can load them there
            if (e.PropertyName == "SendAliveMessageInterval")
            {
                this.p2pMaster.DHTstore(settings.TopicName + "Settings",
                    System.BitConverter.GetBytes(this.settings.SendAliveMessageInterval));
            }
            // if TaskName has changed, clear the Lists, because the Subscribers must reconfirm registering
            if (e.PropertyName == "TopicName")
            {
                GuiLogMessage("Topic Name has changed, so all subscribers must reconfirm registering!", NotificationLevel.Warning);
                // stop active publisher and tell all subscribers that topic name isn't valid anymore
                this.p2pPublisher.Stop(PubSubMessageType.Unregister);
                // start publisher for the changed topic
                this.p2pPublisher.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval);
            }
            if (e.PropertyName == "BtnUnregister")
            {
                this.p2pPublisher.Stop(PubSubMessageType.Unregister);
                GuiLogMessage("Unregister button pressed, Publisher has stopped!", NotificationLevel.Info);
            }
            if (e.PropertyName == "BtnRegister")
            {
                this.p2pPublisher.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval);
                GuiLogMessage("Register button pressed, Publisher has been started!", NotificationLevel.Info);
            }
            if (e.PropertyName == "BtnSolutionFound")
            {
                this.p2pPublisher.Stop(PubSubMessageType.Solution);
                GuiLogMessage("TEST: Emulate Solution-Found-message", NotificationLevel.Info);
            }
        }

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
                    this.p2pMaster.OnPeerReceivedMsg -= p2pMaster_OnPeerReceivedMsg;
                    this.p2pMaster.OnStatusChanged -= P2PMaster_OnStatusChanged;
                }
                if (value != null)
                {
                    this.p2pMaster = (P2PPeerMaster)value;
                    this.p2pMaster.OnStatusChanged += new IControlStatusChangedEventHandler(P2PMaster_OnStatusChanged);
                    this.p2pMaster.OnPeerReceivedMsg += new P2PBase.P2PMessageReceived(p2pMaster_OnPeerReceivedMsg);
                    OnPropertyChanged("P2PMaster");
                }
                else
                {
                    this.p2pMaster = null;
                }
            }
        }

        private void P2PMaster_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            //throw new NotImplementedException();
        }

        private string sInputvalue;
        [PropertyInfo(Direction.InputData, "Publish-String", "Publish this string to all DHT Subscribers", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string Inputvalue
        {
            get
            {
                return this.sInputvalue;
            }
            set
            {
                this.sInputvalue = value;
                OnPropertyChanged("Inputvalue");
            }
        }

        #endregion

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region Standard PlugIn-Functionality

        public ISettings Settings
        {
            set { this.settings = (P2PPublisherSettings)value; }
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

        public void PreExecution()
        {
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PMaster == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }

            if (this.p2pPublisher == null)
            {
                this.p2pPublisher = new P2PPublisherBase(this.P2PMaster);
                this.p2pPublisher.OnGuiMessage += new P2PPublisherBase.GuiMessage(p2pPublisher_OnGuiMessage);
                this.p2pPublisher.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval);
            }
        }

        public void Execute()
        {
            if (this.settings == null && this.settings.TopicName == null)
            {
                GuiLogMessage("There is no input and/or empty Settings. Storing isn't possible.", NotificationLevel.Error);
                return;
            }

            if (this.Inputvalue != null)
            {
                this.p2pPublisher.Publish(this.Inputvalue);
            }
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
            if(this.p2pPublisher != null)
                this.p2pPublisher.Stop(PubSubMessageType.Unregister);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        private void p2pMaster_OnPeerReceivedMsg(string sSourceAddr, string sData)
        {
            //this.p2pPublisher.MessageReceived(sSourceAddr, sData);
        }

        private void p2pPublisher_OnGuiMessage(string sData, NotificationLevel notificationLevel)
        {
            GuiLogMessage(sData, notificationLevel);
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

        //#region Publisher methods

        //private string topic = String.Empty;
        //private string sDHTSettingsPostfix = "Settings";
        //private SubscriberManagement subManagement;
        //private Timer timerWaitingForAliveMsg;
        //private string sPeerName;
        //private long aliveMessageInterval;

        //private bool StartPublisher(string sTopic, long aliveMessageInterval)
        //{
        //    this.topic = sTopic;
        //    aliveMessageInterval = aliveMessageInterval * 1000;
        //    this.subManagement = new SubscriberManagement(aliveMessageInterval);
        //    this.subManagement.OnSubscriberRemoved += new SubscriberManagement.SubscriberRemoved(subManagement_OnSubscriberRemoved);

        //    string sActualPeerName;

        //    // publish own PeerID to the DHT Entry with the key "TaskName", so every subscriber
        //    // can retrieve the name and send a register-message to the publisher
        //    string sPeerId = P2PMaster.GetPeerID(out sPeerName);
        //    //byte[] bytePeerId = P2PMaster.GetPeerID(out sActualPeerName);

        //    sActualPeerName = sPeerId;

        //    // before storing the publishers ID in the DHT, proof whether there already exist an entry
        //    byte[] byRead = P2PMaster.DHTload(sTopic);
        //    string sRead;
        //    // if byRead is not null, the DHT entry was already written
        //    if (byRead != null)
        //    {
        //        sRead = UTF8Encoding.UTF8.GetString(byRead);
        //        // if sRead equals sPeerId this Publisher with the same topic had written 
        //        // this entry - no problem! Otherwise abort Starting the publisher!
        //        if (sRead != sPeerId)
        //        {
        //            GuiLogMessage("Can't store Publisher in the DHT because the Entry was already occupied.", NotificationLevel.Error);
        //            return false;
        //        }
        //    }
        //    bool bolTopicStored = P2PMaster.DHTstore(sTopic, sPeerId);
        //    bool bolSettingsStored = P2PMaster.DHTstore(sTopic + this.sDHTSettingsPostfix,
        //        System.BitConverter.GetBytes(aliveMessageInterval));

        //    if (!bolTopicStored || !bolSettingsStored)
        //    {
        //        GuiLogMessage("Storing Publishers ID or Publishers Settings wasn't possible.", NotificationLevel.Error);
        //        return false;
        //    }

        //    GuiLogMessage("Peer ID '" + sPeerId + "' is published to DHT -Entry-Key '" + this.settings.TopicName + "'", NotificationLevel.Info);
        //    return true;
        //}

        //private void MessageReceived(string sSourceAddr, string sData)
        //{
        //    PubSubMessageType msgType = this.P2PMaster.GetMsgType(sData);

        //    if (msgType != PubSubMessageType.NULL)
        //    {
        //        switch (msgType)
        //        {
        //            case PubSubMessageType.Register:
        //                if (this.subManagement.Add(sSourceAddr))
        //                {
        //                    GuiLogMessage("REGISTERED: Peer with ID " + sSourceAddr, NotificationLevel.Info);
        //                    this.P2PMaster.SendToPeer(PubSubMessageType.RegisteringAccepted, sSourceAddr);
        //                }
        //                else
        //                {
        //                    GuiLogMessage("ALREADY REGISTERED peer with ID " + sSourceAddr, NotificationLevel.Info);
        //                }
        //                break;
        //            case PubSubMessageType.Unregister:
        //                if (this.subManagement.Remove(sSourceAddr))
        //                    GuiLogMessage("REMOVED subscriber " + sSourceAddr + " because it had sent an unregister message", NotificationLevel.Info);
        //                else
        //                    GuiLogMessage("ALREADY REMOVED or had not registered anytime. ID " + sSourceAddr, NotificationLevel.Info);
        //                break;
        //            case PubSubMessageType.Alive:
        //            case PubSubMessageType.Pong:
        //                if (this.subManagement.Update(sSourceAddr))
        //                {
        //                    GuiLogMessage("RECEIVED: " + msgType.ToString() + " Message from " + sSourceAddr, NotificationLevel.Info);
        //                }
        //                else
        //                {
        //                    GuiLogMessage("UPDATE FAILED for " + sSourceAddr + " because it hasn't registered first. " + msgType.ToString(), NotificationLevel.Info);
        //                }
        //                break;
        //            case PubSubMessageType.Ping:
        //                this.P2PMaster.SendToPeer(PubSubMessageType.Pong, sSourceAddr);
        //                GuiLogMessage("REPLIED to a ping message from subscriber " + sSourceAddr, NotificationLevel.Info);
        //                break;
        //            case PubSubMessageType.Solution:
        //                // Send solution msg to all subscriber peers and delete subList
        //                StopPublisher(msgType);
        //                break;
        //            default:
        //                throw (new NotImplementedException());
        //        } // end switch
        //        if (timerWaitingForAliveMsg == null)
        //            timerWaitingForAliveMsg = new Timer(OnWaitingForAliveMsg, null, this.settings.SendAliveMessageInterval * 1000,
        //                this.settings.SendAliveMessageInterval * 1000);
        //    }
        //    // Received Data aren't PubSubMessageTypes or rather no action-relevant messages
        //    else
        //    {
        //        GuiLogMessage("RECEIVED message from non subscribed peer: " + sData.Trim() + ", ID: " + sSourceAddr, NotificationLevel.Warning);
        //    }
        //}

        //private void Publish(string sText)
        //{
        //    Dictionary<string, DateTime> lstSubscribers = this.subManagement.GetAllSubscribers();

        //    PubSubMessageType msgType = this.P2PMaster.GetMsgType(sText);
        //    if (msgType == PubSubMessageType.NULL)
        //    {
        //        foreach (string sSubscriber in lstSubscribers.Keys)
        //        {
        //            this.P2PMaster.SendToPeer(sText, sSubscriber);
        //        }
        //    }
        //    else
        //    {
        //        foreach (string sSubscriber in lstSubscribers.Keys)
        //        {
        //            this.P2PMaster.SendToPeer(msgType, sSubscriber);
        //        }
        //    }
        //}

        ///// <summary>
        ///// if peers are outdated (alive message doesn't arrive in the given interval)
        ///// ping them to give them a second chance
        ///// </summary>
        ///// <param name="state"></param>
        //private void OnWaitingForAliveMsg(object state)
        //{
        //    List<string> lstOutdatedSubscribers = this.subManagement.CheckVitality();
        //    foreach (string outdatedSubscriber in lstOutdatedSubscribers)
        //    {
        //        P2PMaster.SendToPeer(PubSubMessageType.Ping, outdatedSubscriber);
        //        GuiLogMessage("PING outdated peer " + outdatedSubscriber, NotificationLevel.Info);
        //    }
        //}

        //private void subManagement_OnSubscriberRemoved(string sPeerId)
        //{
        //    GuiLogMessage("REMOVED subscriber " + sPeerId, NotificationLevel.Info);
        //}

        //private void StopPublisher(PubSubMessageType msgType)
        //{
        //    if(this.P2PMaster != null)
        //        // send unregister message to all subscribers
        //        Publish(((int)msgType).ToString());
        //    if (this.P2PMaster != null)
        //    {
        //        this.P2PMaster.DHTremove(this.topic);
        //        this.P2PMaster.DHTremove(this.topic + this.sDHTSettingsPostfix);
        //    }
        //    if (this.timerWaitingForAliveMsg != null)
        //    {
        //        this.timerWaitingForAliveMsg.Dispose();
        //        this.timerWaitingForAliveMsg = null;
        //    }
        //}

        //#endregion
    }

}
