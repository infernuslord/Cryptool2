using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
using System.Threading;

/*
 * TODO:
 * - Removing excluded in Stop-Method, because it throws an exception... --> to be fixed by M.Helling (PAP)
 * - FUTURE: dual data management of subscriber list (on local peer and in DHT)
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PPublisherBase
    {
        public delegate void GuiMessage(string sData, NotificationLevel notificationLevel);
        public event GuiMessage OnGuiMessage;

        #region Variables

        private IP2PControl p2pControl;
        private string topic = String.Empty;
        private string sDHTSettingsPostfix = "Settings";
        private SubscriberManagement subManagement;
        private Timer timerWaitingForAliveMsg;
        private PeerId ownPeerId;
        private long aliveMessageInterval;

        private bool started = false;
        /// <summary>
        /// Status flag which contains the state of the Publisher
        /// </summary>
        public bool Started 
        {
            get { return this.started; }
            private set { this.started = value; }
        }

        #endregion

        public P2PPublisherBase(IP2PControl p2pControl)
        {
            this.p2pControl = p2pControl;
            this.p2pControl.OnPeerReceivedMsg +=new P2PBase.P2PMessageReceived(MessageReceived);
        }

        public bool Start(string sTopic, long aliveMessageInterval)
        {
            /* BEGIN: CHECKING WHETHER THERE HAS ALREADY EXIST ANOTHER PUBLISHER */
            this.topic = sTopic;
            this.aliveMessageInterval = aliveMessageInterval * 1000;
            this.subManagement = new SubscriberManagement(aliveMessageInterval);
            this.subManagement.OnSubscriberRemoved += new SubscriberManagement.SubscriberRemoved(subManagement_OnSubscriberRemoved);

            // publish own PeerID to the DHT Entry with the key "TaskName", so every subscriber
            // can retrieve the name and send a register-message to the publisher
            string sPeerName;
            PeerId myPeerId = this.p2pControl.GetPeerID(out sPeerName);
            this.ownPeerId = myPeerId;

            // before storing the publishers ID in the DHT, proof whether there already exist an entry
            byte[] byRead = this.p2pControl.DHTload(sTopic);
            // if byRead is not null, the DHT entry was already written
            if (byRead != null)
            {
                // if sRead equals sPeerId this Publisher with the same topic had written 
                // this entry - no problem! Otherwise abort Starting the publisher!
                if (byRead != myPeerId.byteId)
                {
                    OnGuiMessage("Can't store Publisher in the DHT because the Entry was already occupied.", NotificationLevel.Error);
                    return false;
                }
            }
            /* END: CHECKING WHETHER THERE HAS ALREADY EXIST ANOTHER PUBLISHER */

            bool bolTopicStored = this.p2pControl.DHTstore(sTopic, myPeerId.byteId);
            bool bolSettingsStored = this.p2pControl.DHTstore(sTopic + this.sDHTSettingsPostfix,
                System.BitConverter.GetBytes(this.aliveMessageInterval));

            if (!bolTopicStored || !bolSettingsStored)
            {
                OnGuiMessage("Storing Publishers ID or Publishers Settings wasn't possible.", NotificationLevel.Error);
                return false;
            }

            OnGuiMessage("Peer ID '" + myPeerId.stringId + "' is published to DHT -Entry-Key '" + this.topic + "'", NotificationLevel.Info);
            this.Started = true;

            return true;
        }

        private void MessageReceived(PeerId sourceAddr, string sData)
        {
            PubSubMessageType msgType = this.p2pControl.GetMsgType(sData);

            if (msgType != PubSubMessageType.NULL)
            {
                switch (msgType)
                {
                    case PubSubMessageType.Register:
                        if (this.subManagement.Add(sourceAddr))
                        {
                            OnGuiMessage("REGISTERED: Peer with ID " + sourceAddr.stringId, NotificationLevel.Info);
                            this.p2pControl.SendToPeer(PubSubMessageType.RegisteringAccepted, sourceAddr);
                        }
                        else
                        {
                            OnGuiMessage("ALREADY REGISTERED peer with ID " + sourceAddr.stringId, NotificationLevel.Info);
                        }
                        break;
                    case PubSubMessageType.Unregister:
                        if (this.subManagement.Remove(sourceAddr))
                            OnGuiMessage("REMOVED subscriber " + sourceAddr.stringId + " because it had sent an unregister message", NotificationLevel.Info);
                        else
                            OnGuiMessage("ALREADY REMOVED or had not registered anytime. ID " + sourceAddr.stringId, NotificationLevel.Info);
                        break;
                    case PubSubMessageType.Alive:
                    case PubSubMessageType.Pong:
                        if (this.subManagement.Update(sourceAddr))
                        {
                            OnGuiMessage("RECEIVED: " + msgType.ToString() + " Message from " + sourceAddr.stringId, NotificationLevel.Info);
                        }
                        else
                        {
                            OnGuiMessage("UPDATE FAILED for " + sourceAddr.stringId + " because it hasn't registered first. " + msgType.ToString(), NotificationLevel.Info);
                        }
                        break;
                    case PubSubMessageType.Ping:
                        this.p2pControl.SendToPeer(PubSubMessageType.Pong, sourceAddr);
                        OnGuiMessage("REPLIED to a ping message from subscriber " + sourceAddr.stringId, NotificationLevel.Info);
                        break;
                    case PubSubMessageType.Solution:
                        // Send solution msg to all subscriber peers and delete subList
                        Stop(msgType);
                        break;
                    default:
                        throw (new NotImplementedException());
                } // end switch
                if (timerWaitingForAliveMsg == null)
                    timerWaitingForAliveMsg = new Timer(OnWaitingForAliveMsg, null, this.aliveMessageInterval * 1000,
                        this.aliveMessageInterval * 1000);
            }
            // Received Data aren't PubSubMessageTypes or rather no action-relevant messages
            else
            {
                OnGuiMessage("RECEIVED message from non subscribed peer: " + sData.Trim() + ", ID: " + sourceAddr.stringId, NotificationLevel.Warning);
            }
        }

        public void Publish(string sText)
        {
            List<PeerId> lstSubscribers = this.subManagement.GetAllSubscribers();

            PubSubMessageType msgType = this.p2pControl.GetMsgType(sText);
            if (msgType == PubSubMessageType.NULL)
            {
                foreach (PeerId subscriber in lstSubscribers)
                {
                    this.p2pControl.SendToPeer(sText, subscriber);
                }
            }
            else
            {
                foreach (PeerId subscriber in lstSubscribers)
                {
                    this.p2pControl.SendToPeer(msgType, subscriber);
                }
            }
        }

        /// <summary>
        /// if peers are outdated (alive message doesn't arrive in the given interval)
        /// ping them to give them a second chance
        /// </summary>
        /// <param name="state"></param>
        private void OnWaitingForAliveMsg(object state)
        {
            List<PeerId> lstOutdatedSubscribers = this.subManagement.CheckVitality();
            foreach (PeerId outdatedSubscriber in lstOutdatedSubscribers)
            {
                this.p2pControl.SendToPeer(PubSubMessageType.Ping, outdatedSubscriber);
                OnGuiMessage("PING outdated peer " + outdatedSubscriber, NotificationLevel.Info);
            }
        }

        private void subManagement_OnSubscriberRemoved(PeerId peerId)
        {
            OnGuiMessage("REMOVED subscriber " + peerId.stringId, NotificationLevel.Info);
        }

        public void Stop(PubSubMessageType msgType)
        {

            OnGuiMessage("Send unregister message to all subscribers", NotificationLevel.Info);

            // send unregister message to all subscribers
            Publish(((int)msgType).ToString());

            //Still an error in dhtRemove... so at present ignored...
            if (this.p2pControl != null)
            {

                OnGuiMessage("Begin removing the information from the DHT", NotificationLevel.Info);

                bool removeTopic = this.p2pControl.DHTremove(this.topic);
                bool removeSettings = this.p2pControl.DHTremove(this.topic + this.sDHTSettingsPostfix);
                string removeInfo = String.Empty;
                if (removeTopic && removeSettings)
                    removeInfo = "Topic and settings";
                else if (removeTopic)
                    removeInfo = "Topic";
                else if (removeSettings)
                    removeInfo = "Settings";
                if(removeInfo != String.Empty)
                    OnGuiMessage(removeInfo + " successfully removed from DHT.", NotificationLevel.Info);
                else
                    OnGuiMessage("Neither Topic nor settings were removed from DHT.", NotificationLevel.Info);
            }

            OnGuiMessage("Stopping all timers.",NotificationLevel.Info);

            if (this.timerWaitingForAliveMsg != null)
            {
                this.timerWaitingForAliveMsg.Dispose();
                this.timerWaitingForAliveMsg = null;
            }

            OnGuiMessage("Publisher completely stopped!", NotificationLevel.Info);

            this.Started = false;
        }
    }
}
