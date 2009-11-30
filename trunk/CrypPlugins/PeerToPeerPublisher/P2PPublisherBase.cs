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
        public virtual event GuiMessage OnGuiMessage;

        #region Variables

        protected IP2PControl p2pControl;
        protected SubscriberManagement peerManagement;
        private string topic = String.Empty;
        private string sDHTSettingsPostfix = "Settings";        
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

        #region public methods (Start, Publish, Stop)

        protected virtual void AssignManagement(long aliveMessageInterval)
        {
            this.peerManagement = new SubscriberManagement(aliveMessageInterval);
            this.peerManagement.OnSubscriberRemoved += new SubscriberManagement.SubscriberRemoved(peerManagement_OnSubscriberRemoved);
        }

        /// <summary>
        /// Starts the publisher and checks whether there is already a publisher for this topic (than returns false)
        /// </summary>
        /// <param name="sTopic">Topic, at which the subscribers can register themselves</param>
        /// <param name="aliveMessageInterval">Declare interval (in sec) in which every subscriber has to send an alive message to the publisher</param>
        /// <returns>true, if writing all necessary information was written in DHT, otherwise false</returns>
        public bool Start(string sTopic, long aliveMessageInterval)
        {
            /* BEGIN: CHECKING WHETHER THERE HAS ALREADY EXIST ANOTHER PUBLISHER */
            this.topic = sTopic;
            this.aliveMessageInterval = aliveMessageInterval * 1000;
            AssignManagement(this.aliveMessageInterval);

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
                    GuiLogging("Can't store Publisher in the DHT because the Entry was already occupied.", NotificationLevel.Error);
                    return false;
                }
            }
            /* END: CHECKING WHETHER THERE HAS ALREADY EXIST ANOTHER PUBLISHER */

            bool bolTopicStored = this.p2pControl.DHTstore(sTopic, myPeerId.byteId);
            bool bolSettingsStored = this.p2pControl.DHTstore(sTopic + this.sDHTSettingsPostfix,
                System.BitConverter.GetBytes(this.aliveMessageInterval));

            if (!bolTopicStored || !bolSettingsStored)
            {
                GuiLogging("Storing Publishers ID or Publishers Settings wasn't possible.", NotificationLevel.Error);
                return false;
            }

            GuiLogging("Peer ID '" + myPeerId.stringId + "' is published to DHT -Entry-Key '" + this.topic + "'", NotificationLevel.Info);
            this.Started = true;

            return true;
        }

        /// <summary>
        /// Publish text to ALL active subscribers (subscribers of the second chance list included)
        /// </summary>
        /// <param name="sText"></param>
        /// <returns>Amount of Subscribers, to which the message was sent</returns>
        public virtual int Publish(string sText)
        {
            int i = 0;
            List<PeerId> lstSubscribers = this.peerManagement.GetAllSubscribers();

            foreach (PeerId subscriber in lstSubscribers)
            {
                this.p2pControl.SendToPeer(sText, subscriber);
                i++;
            }
            return i;
        }

        /// <summary>
        /// Sends a given message to all subscribers, deletes publishers information from DHT and stops all timers.
        /// </summary>
        /// <param name="msgType">Choose an according MessageType (for example: Unregister or Stop), so every subscriber can handle
        /// the LAST message from the publisher correctly!</param>
        public virtual void Stop(PubSubMessageType msgType)
        {
            // send unregister message to all subscribers
            int i = SendInternalMsg(msgType);
            GuiLogging("Unregister messages were sent to " + i.ToString() + " subscribers!", NotificationLevel.Info);

            //Still an error in dhtRemove... so at present ignored...
            if (this.p2pControl != null)
            {

                GuiLogging("Begin removing the information from the DHT", NotificationLevel.Info);

                bool removeTopic = this.p2pControl.DHTremove(this.topic);
                bool removeSettings = this.p2pControl.DHTremove(this.topic + this.sDHTSettingsPostfix);
                string removeInfo = String.Empty;
                if (removeTopic && removeSettings)
                    removeInfo = "Topic and settings";
                else if (removeTopic)
                    removeInfo = "Topic";
                else if (removeSettings)
                    removeInfo = "Settings";
                if (removeInfo != String.Empty)
                    GuiLogging(removeInfo + " successfully removed from DHT.", NotificationLevel.Info);
                else
                    GuiLogging("Neither Topic nor settings were removed from DHT.", NotificationLevel.Info);
            }

            GuiLogging("Stopping all timers.", NotificationLevel.Info);

            if (this.timerWaitingForAliveMsg != null)
            {
                this.timerWaitingForAliveMsg.Dispose();
                this.timerWaitingForAliveMsg = null;
            }

            GuiLogging("Publisher completely stopped!", NotificationLevel.Info);

            this.Started = false;
        }

        #endregion

        #region private Methods (MessageReceived, SendInternalMsg, OnWaitingForAliveMsg, peerManagement_OnSubscriberRemoved)

        protected virtual void MessageReceived(PeerId sourceAddr, string sData)
        {
            PubSubMessageType msgType = this.p2pControl.GetMsgType(sData);

            if (msgType != PubSubMessageType.NULL)
            {
                switch (msgType)
                {
                    case PubSubMessageType.Register:
                        if (this.peerManagement.Add(sourceAddr))
                        {
                            GuiLogging("REGISTERED: Peer with ID " + sourceAddr.stringId, NotificationLevel.Info);
                            this.p2pControl.SendToPeer(PubSubMessageType.RegisteringAccepted, sourceAddr);
                        }
                        else
                        {
                            GuiLogging("ALREADY REGISTERED peer with ID " + sourceAddr.stringId, NotificationLevel.Info);
                        }
                        break;
                    case PubSubMessageType.Unregister:
                        if (this.peerManagement.Remove(sourceAddr))
                            GuiLogging("REMOVED subscriber " + sourceAddr.stringId + " because it had sent an unregister message", NotificationLevel.Info);
                        else
                            GuiLogging("ALREADY REMOVED or had not registered anytime. ID " + sourceAddr.stringId, NotificationLevel.Info);
                        break;
                    case PubSubMessageType.Alive:
                    case PubSubMessageType.Pong:
                        if (this.peerManagement.Update(sourceAddr))
                        {
                            GuiLogging("RECEIVED: " + msgType.ToString() + " Message from " + sourceAddr.stringId, NotificationLevel.Info);
                        }
                        else
                        {
                            GuiLogging("UPDATE FAILED for " + sourceAddr.stringId + " because it hasn't registered first. " + msgType.ToString(), NotificationLevel.Info);
                        }
                        break;
                    case PubSubMessageType.Ping:
                        this.p2pControl.SendToPeer(PubSubMessageType.Pong, sourceAddr);
                        GuiLogging("REPLIED to a ping message from subscriber " + sourceAddr.stringId, NotificationLevel.Info);
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
                GuiLogging("RECEIVED message from non subscribed peer: " + sData.Trim() + ", ID: " + sourceAddr.stringId, NotificationLevel.Warning);
            }
        }

        /// <summary>
        /// This functions can only send infrastructure-supporting messages to all subscribers
        /// </summary>
        /// <param name="msgType"></param>
        /// <returns>Amount of Subscribers, to which a message was sent</returns>
        private int SendInternalMsg(PubSubMessageType msgType)
        {
            int i = 0;
            List<PeerId> lstSubscribers = this.peerManagement.GetAllSubscribers();
            foreach (PeerId subscriber in lstSubscribers)   
            {
                this.p2pControl.SendToPeer(msgType, subscriber);
                i++;
            }
            return i;
        }

        /// <summary>
        /// if peers are outdated (alive message doesn't arrive in the given interval)
        /// ping them to give them a second chance
        /// </summary>
        /// <param name="state"></param>
        private void OnWaitingForAliveMsg(object state)
        {
            // get second chance list from SubscribersManagement (Second chance list = The timespans of this subscribers are expired)
            List<PeerId> lstOutdatedSubscribers = this.peerManagement.CheckVitality();
            foreach (PeerId outdatedSubscriber in lstOutdatedSubscribers)
            {
                this.p2pControl.SendToPeer(PubSubMessageType.Ping, outdatedSubscriber);
                GuiLogging("PING outdated peer " + outdatedSubscriber, NotificationLevel.Info);
            }
        }

        private void peerManagement_OnSubscriberRemoved(PeerId peerId)
        {
            GuiLogging("REMOVED subscriber " + peerId.stringId, NotificationLevel.Info);
        }

        protected void GuiLogging(string sText, NotificationLevel notLev)
        {
            if(OnGuiMessage != null)
                OnGuiMessage(sText, notLev);
        }

        #endregion 
    }
}