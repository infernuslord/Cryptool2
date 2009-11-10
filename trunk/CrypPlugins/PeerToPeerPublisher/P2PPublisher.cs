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
 * - independent reverse counter per subscriber / timestamp dict (Key = SubID, Value = TimeStamp | SecondChanceStep)
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
        #region PrivVariables (Lists, Timer, etc)

        private string sDHTSettingsPostfix = "Settings";

        private P2PPublisherSettings settings;
        private IP2PControl p2pMaster;
        /*
        private List<byte[]> lstSubscribers = new List<byte[]>();
        private List<byte[]> lstWaitingForPong = new List<byte[]>();
        private List<byte[]> lstAliveArrived = new List<byte[]>();
        */
        private SubscriberInfo subManagement;
        private Timer timerWaitingForAliveMsg;
        private long aliveMessageInterval;

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

        public P2PPublisher()
        {
            this.settings = new P2PPublisherSettings(this);
            this.settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
        }

        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // storing settings for subscribers in the DHT, so they can load them there
            if (e.PropertyName == "SendAliveMessageInterval")
            {
                this.p2pMaster.DHTstore(settings.TaskName + this.sDHTSettingsPostfix, 
                    System.BitConverter.GetBytes(this.settings.SendAliveMessageInterval));
            }
            // if TaskName has changed, clear the Lists, because the Subscribers must reconfirm registering
            if (e.PropertyName == "TaskName")
            {
                /*
                this.lstSubscribers.Clear();
                this.lstAliveArrived.Clear();
                this.lstWaitingForPong.Clear();
                */
                GuiLogMessage("Taskname has changed, so all subscribers must reconfirm registering!",NotificationLevel.Warning);
            }
        }

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
            aliveMessageInterval = (long)this.settings.SendAliveMessageInterval * 1000;
            this.subManagement = new SubscriberInfo(aliveMessageInterval);
            this.subManagement.OnSubscriberRemoved += new SubscriberInfo.SubscriberRemoved(subManagement_OnSubscriberRemoved);
        }

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
            if (this.timerWaitingForAliveMsg != null)
            {
                this.timerWaitingForAliveMsg.Dispose();
                this.timerWaitingForAliveMsg = null;
            }
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
            if (this.settings.TaskName != null)
            {
                string sActualPeerName;

                // publish own PeerID to the DHT Entry with the key "TaskName", so every subscriber
                // can retrieve the name and send a register-message to the publisher
                byte[] bytePeerId = P2PMaster.GetPeerID(out sActualPeerName);

                sActualPeerName = P2PMaster.ConvertPeerId(bytePeerId);

                // Question: Why casting to String isn't possible?
                P2PMaster.DHTstore(this.settings.TaskName, bytePeerId);
                P2PMaster.DHTstore(this.settings.TaskName + this.sDHTSettingsPostfix,
                    System.BitConverter.GetBytes(aliveMessageInterval));

                GuiLogMessage("Peer ID '" + P2PMaster.ConvertPeerId(bytePeerId) + "' is published to DHT -Entry-Key '" + this.settings.TaskName + "'", NotificationLevel.Info);
            }
            else
            {
                GuiLogMessage("There is no input and/or empty Settings. Storing isn't possible.", NotificationLevel.Error);
                return;
            }
            if (this.Inputvalue != null)
            {
                PublishText(this.Inputvalue);
            }
        }

        private void PublishText(string sText)
        {
            Dictionary<byte[], DateTime> lstSubscribers = this.subManagement.GetAllSubscribers();
            foreach (byte[] byteSubscriber in lstSubscribers.Keys)
            {
                this.P2PMaster.SendToPeer(sText, byteSubscriber);
            }
        }

        private void p2pMaster_OnPeerReceivedMsg(byte[] byteSourceAddr, string sData)
        {
            if (sData.Trim() == "regi")
            {
                if (this.subManagement.Add(byteSourceAddr))
                    GuiLogMessage("REGISTERED: Peer with ID " + P2PMaster.ConvertPeerId(byteSourceAddr), NotificationLevel.Info);
                else
                    GuiLogMessage("ALREADY REGISTERED peer with ID " + P2PMaster.ConvertPeerId(byteSourceAddr), NotificationLevel.Info);
            }
            else
            {
                if (this.subManagement.Update(byteSourceAddr))
                    GuiLogMessage("RECEIVED: " + sData.Trim() + " Message from " + P2PMaster.ConvertPeerId(byteSourceAddr), NotificationLevel.Info);
                else
                    GuiLogMessage("UPDATE FAILED for " + P2PMaster.ConvertPeerId(byteSourceAddr) + " because it hasn't registered first.", NotificationLevel.Info);
                if (sData.Trim() == "ping")
                    this.P2PMaster.SendToPeer("pong", byteSourceAddr);
            }
            if(timerWaitingForAliveMsg == null)
                timerWaitingForAliveMsg = new Timer(OnWaitingForAliveMsg, null, this.settings.SendAliveMessageInterval * 1000,
                    this.settings.SendAliveMessageInterval * 1000);
        }

        private void OnWaitingForAliveMsg(object state)
        {
            List<byte[]> lstOutdatedSubscribers = this.subManagement.CheckVitality();
            foreach (byte[] outdatedSubscriber in lstOutdatedSubscribers)
            {
                P2PMaster.SendToPeer("ping", outdatedSubscriber);
                GuiLogMessage("PING outdated peer " + P2PMaster.ConvertPeerId(outdatedSubscriber), NotificationLevel.Info);
            }
        }

        private void subManagement_OnSubscriberRemoved(byte[] byPeerId)
        {
            GuiLogMessage("REMOVED subscriber " + P2PMaster.ConvertPeerId(byPeerId), NotificationLevel.Info);
        }

        /*OLD SOLUTION - WORKS, BUT NOT THE BEST WAY...*/
        /*
        void p2pMaster_OnPeerReceivedMsg(byte[] byteSourceAddr, string sData)
        {
            if (sData.Trim() == "regi")
            {
                if (!lstSubscribers.Contains(byteSourceAddr))
                {
                    lstSubscribers.Add(byteSourceAddr);
                    GuiLogMessage("REGISTERED: Peer with ID " + P2PMaster.ConvertPeerId(byteSourceAddr), NotificationLevel.Info);
                    StartWaitingTimer();
                }
            }
            if (sData.Trim() == "aliv")
            {
                if (!lstAliveArrived.Contains(byteSourceAddr))
                {
                    lstAliveArrived.Add(byteSourceAddr);
                    GuiLogMessage("RECEIVED: Alive Message from " + P2PMaster.ConvertPeerId(byteSourceAddr), NotificationLevel.Info);
                    // if Alive Msg arrived to late, the subscriber was removed from list
                    // of active subscribers. So we must add him again.
                    if (!lstSubscribers.Contains(byteSourceAddr))
                        lstSubscribers.Add(byteSourceAddr);
                    StartWaitingTimer();
                }
            }
            if (sData.Trim() == "pong")
            {
                GuiLogMessage("RECEIVED: Pong Message from " + P2PMaster.ConvertPeerId(byteSourceAddr), NotificationLevel.Info);
                if (lstWaitingForPong.Contains(byteSourceAddr))
                    lstWaitingForPong.Remove(byteSourceAddr);
            }
        }

        private void StartWaitingTimer()
        {
            // check every 30 seconds if the subscribers are still alive
            // Timer gets started not until retrieving the first register message from a Subscriber
            if (timerWaitingForAliveMsg == null)
                timerWaitingForAliveMsg = new Timer(OnWaitingForAliveMsg, null, 30000, 30000);
        }

        private void OnWaitingForAliveMsg(object state)
        {
            GuiLogMessage("Checking vitality of Subscribers", NotificationLevel.Info);
            for (int i = 0; i < this.lstSubscribers.Count; i++)
            {
                if (!this.lstAliveArrived.Contains(this.lstSubscribers[i]))
                {
                    // only if Subscriber had not send a Alive message
                    // remove him from the subscriber list
                    GuiLogMessage("DEAD: Subscriber " + P2PMaster.ConvertPeerId(this.lstSubscribers[i]) + " is removed from SubList!", NotificationLevel.Info);
                    this.lstSubscribers.Remove(this.lstSubscribers[i]);
                }
                else
                {
                    this.lstAliveArrived.Remove(this.lstSubscribers[i]);
                }
            }
            // if there are no more subscribers, stop Timer of
            // waiting for alive messages
            if (this.lstSubscribers.Count == 0)
            {
                timerWaitingForAliveMsg.Dispose();
                return;
            }
        }
        */

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
