using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Control;
//using System.Threading;
using System.Timers;
using System.IO;
using System.Threading;

/*
 * TODO:
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
        private System.Timers.Timer timerWaitingForAliveMsg;
        
        private PeerId ownPeerId;
        /// <summary>
        /// interval in milliseconds!!! Divide with 1000 to preserve seconds
        /// </summary>
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

        // Publisher-exchange extension - Arnie 2010.02.02
        /// <summary>
        /// Interval for waiting for other Publishers Pong in milliseconds!
        /// </summary>
        private const long INTERVAL_WAITING_FOR_OTHER_PUBS_PONG = 10000;
        private System.Timers.Timer waitingForOtherPublishersPong;
        /// <summary>
        /// if this value is set, you are between the TimeSpan of checking liveness of the other peer.
        /// If Timespan runs out without receiving a Pong-Msg from the other Publisher, assume its functionality
        /// </summary>
        private PeerId otherPublisherPeer = null;
        private bool otherPublisherHadResponded = false;

        /// <summary>
        /// counter, which counts the trials to assume publishers/managers functionality
        /// </summary>
        int cycleCheckCount = 0;

        #endregion

        public P2PPublisherBase(IP2PControl p2pControl)
        {
            this.p2pControl = p2pControl;

            this.timerWaitingForAliveMsg = new System.Timers.Timer();
            this.timerWaitingForAliveMsg.AutoReset = true;
            this.timerWaitingForAliveMsg.Elapsed += new ElapsedEventHandler(OnWaitingForAliveMsg);

            this.waitingForOtherPublishersPong = new System.Timers.Timer();
            this.waitingForOtherPublishersPong.AutoReset = false;
            this.waitingForOtherPublishersPong.Interval = INTERVAL_WAITING_FOR_OTHER_PUBS_PONG;
            this.waitingForOtherPublishersPong.Elapsed += new ElapsedEventHandler(OnWaitingForOtherPublishersPong);
        }

        // Publisher-exchange extension - Arnie 2010.02.02        
        /// <summary>
        /// Callback function for waitingForOtherPublishersPong-object. Will be only executed, when a
        /// different Publisher-ID was found in the DHT, to check if the "old" Publisher is still
        /// alive! When "old" Publisher is still alive, try as long as it leaves the network!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWaitingForOtherPublishersPong(object sender, ElapsedEventArgs e)
        {
            if (this.otherPublisherHadResponded)
            {
                //GuiLogging("Can't assume functionality of an alive Publishers. So starting this workspace isn't possible!", NotificationLevel.Error);
                //Stop(PubSubMessageType.PublisherRivalryProblem);
                cycleCheckCount++;
                GuiLogging("Can't assume functionality of an alive Publishers. So waiting till the other Manager leaves the network! Cycle: " + cycleCheckCount, NotificationLevel.Info);
                // wait one second longer with every trial
                Thread.Sleep(cycleCheckCount * 1000);
                Start(this.topic, this.aliveMessageInterval);
            }
            else
            {
                this.waitingForOtherPublishersPong.Stop();
                GuiLogging("First trial to assume functionality of the old Publisher.", NotificationLevel.Debug);
                // we have to delete all OLD Publishers entries to assume its functionality
                DHT_CommonManagement.DeleteAllPublishersEntries(ref this.p2pControl, this.topic);
                Start(this.topic, this.aliveMessageInterval);
            }
        }

        /// <summary>
        /// if peers are outdated (alive message doesn't arrive in the given interval)
        /// ping them to give them a second chance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWaitingForAliveMsg(object sender, ElapsedEventArgs e)
        {
            // get second chance list from SubscribersManagement (Second chance list = The timespans of this subscribers are expired)
            List<PeerId> lstOutdatedSubscribers = this.peerManagement.CheckVitality();
            foreach (PeerId outdatedSubscriber in lstOutdatedSubscribers)
            {
                this.p2pControl.SendToPeer(PubSubMessageType.Ping, outdatedSubscriber);
                GuiLogging("PING outdated peer " + outdatedSubscriber, NotificationLevel.Debug);
            }
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
        public void Start(string sTopic, long aliveMessageInterval)
        {
            this.p2pControl.OnPayloadMessageReceived -= p2pControl_OnPayloadMessageReceived;
            this.p2pControl.OnSystemMessageReceived -= p2pControl_OnSystemMessageReceived;
            this.p2pControl.OnPayloadMessageReceived += new P2PPayloadMessageReceived(p2pControl_OnPayloadMessageReceived);
            this.p2pControl.OnSystemMessageReceived += new P2PSystemMessageReceived(p2pControl_OnSystemMessageReceived);

            /* BEGIN: CHECKING WHETHER THERE HAS ALREADY EXIST ANOTHER PUBLISHER */
            this.topic = sTopic;
            this.aliveMessageInterval = aliveMessageInterval;
            AssignManagement(this.aliveMessageInterval);

            // publish own PeerID to the DHT Entry with the key "TaskName", so every subscriber
            // can retrieve the name and send a register-message to the publisher
            string sPeerName;
            PeerId myPeerId = this.p2pControl.GetPeerID(out sPeerName);
            this.ownPeerId = myPeerId;

            // before storing the publishers ID in the DHT, proof whether there already exists an entry
            PeerId byRead = DHT_CommonManagement.GetTopicsPublisherId(ref this.p2pControl, sTopic);

            // if byRead is not null, the DHT entry was already written
            if (byRead != null)
            {
                // if a different Publisher was found at the DHT entry, send a ping msg
                // and wait for Pong-Response. When this won't arrive in the given TimeSpan,
                // assume the Publishing functionality
                if (byRead != myPeerId)
                {   
                    this.otherPublisherHadResponded = false;
                    this.otherPublisherPeer = byRead;
                    this.waitingForOtherPublishersPong.Enabled = true;

                    this.p2pControl.SendToPeer(PubSubMessageType.Ping, byRead);

                    GuiLogging("Another Publisher was found. Waiting for Pong-Response for "
                        + INTERVAL_WAITING_FOR_OTHER_PUBS_PONG / 1000 + " seconds. When it won't response "
                        + "assume its functionality.", NotificationLevel.Debug);
                    return;
                }
            }
            /* END: CHECKING WHETHER THERE HAS ALREADY EXIST ANOTHER PUBLISHER */

            bool bolTopicStored = DHT_CommonManagement.SetTopicsPublisherId(ref this.p2pControl, sTopic, myPeerId);
            bool bolSettingsStored = DHT_CommonManagement.SetAliveMessageInterval(ref this.p2pControl, sTopic, this.aliveMessageInterval);

            if (!bolTopicStored || !bolSettingsStored)
            {
                GuiLogging("Storing Publishers ID and/or Publishers Settings wasn't possible.", NotificationLevel.Error);
                return;
            }

            GuiLogging("Peer ID '" + myPeerId + "' is published to DHT -Entry-Key '" + this.topic + "'", NotificationLevel.Info);
            this.Started = true;

            PeerCompletelyStarted();
        }

        /// <summary>
        /// Will only be thrown, when Publisher has successfully registered in the p2p network!
        /// </summary>
        protected virtual void PeerCompletelyStarted()
        {  }

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
            GuiLogging("Stopping all timers.", NotificationLevel.Debug);

            this.timerWaitingForAliveMsg.Stop();
            this.waitingForOtherPublishersPong.Stop();

            GuiLogging("Deregister message-received-events", NotificationLevel.Debug);
            this.p2pControl.OnPayloadMessageReceived -= p2pControl_OnPayloadMessageReceived;
            this.p2pControl.OnSystemMessageReceived -= p2pControl_OnSystemMessageReceived;

            // don't remove informations, when it occurs a rivalry problem between two
            // Publishers, because otherwise this will kill the whole topic-solution-network.
            if (this.p2pControl != null && this.p2pControl.PeerStarted() && msgType != PubSubMessageType.PublisherRivalryProblem)
            {
                GuiLogging("Begin removing the information from the DHT", NotificationLevel.Debug);

                bool removeSettings = DHT_CommonManagement.DeleteAllPublishersEntries(ref this.p2pControl, this.topic);

                if (removeSettings)
                    GuiLogging("Publishers/Managers ID and Alive Message Interval successfully removed from DHT.", NotificationLevel.Debug);
                else
                    GuiLogging("Neither Topic nor settings were removed from DHT.", NotificationLevel.Debug);

                // send unregister message to all subscribers
                int i = SendInternalMsg(msgType);
                GuiLogging("Unregister messages were sent to " + i.ToString() + " subscribers/workers!", NotificationLevel.Info);
            }

            GuiLogging("Publisher completely stopped!", NotificationLevel.Info);

            this.Started = false;
            cycleCheckCount = 0;
        }

        #endregion

        #region private Methods (MessageReceived, SendInternalMsg, OnWaitingForAliveMsg, peerManagement_OnSubscriberRemoved)

        protected virtual void p2pControl_OnSystemMessageReceived(PeerId sender, PubSubMessageType msgType)
        {
            if (sender == ownPeerId)
            {
                GuiLogging("Received Message from OWN Peer... Strange stuff.", NotificationLevel.Debug);
                return;
            }

            switch (msgType)
            {
                case PubSubMessageType.Register:
                    if (this.peerManagement.Add(sender))
                    {
                        if(!this.timerWaitingForAliveMsg.Enabled)
                            this.timerWaitingForAliveMsg.Start();
                        GuiLogging("REGISTERED: Peer with ID " + sender + "- RegExepted Msg was sent.", NotificationLevel.Info);
                        this.p2pControl.SendToPeer(PubSubMessageType.RegisteringAccepted, sender);
                    }
                    else
                    {
                        GuiLogging("ALREADY REGISTERED peer with ID " + sender, NotificationLevel.Info);
                        // this case only occurs, when some messages run into a misunderstanding,
                        // but reply with a RegAccepted Msg, so the sender won't send new Reg Msgs
                        // periodically.
                        this.p2pControl.SendToPeer(PubSubMessageType.RegisteringAccepted, sender);
                    }
                    break;
                case PubSubMessageType.Stop:
                case PubSubMessageType.Unregister:
                    if (!this.peerManagement.Remove(sender))
                        GuiLogging("ALREADY REMOVED or had not registered anytime. ID " + sender, NotificationLevel.Info);
                    break;
                case PubSubMessageType.Alive:
                case PubSubMessageType.Pong:
                    if (this.otherPublisherPeer != null && this.otherPublisherPeer == sender)
                    {
                        this.otherPublisherHadResponded = true;
                        this.otherPublisherPeer = null;
                    }
                    break;
                case PubSubMessageType.Ping:
                    this.p2pControl.SendToPeer(PubSubMessageType.Pong, sender);
                    GuiLogging("REPLIED to a ping message from peer " + sender, NotificationLevel.Debug);
                    break;
                case PubSubMessageType.Solution:
                    // Send solution msg to all subscriber peers and delete subList
                    Stop(msgType);
                    break;
                default:
                    throw (new NotImplementedException());
            } // end switch

            if (msgType != PubSubMessageType.Unregister && msgType != PubSubMessageType.Stop)
            {
                if(this.peerManagement.Update(sender))
                    GuiLogging("UPDATED Peer " + sender + " successfully.", NotificationLevel.Debug);
                else
                    GuiLogging("UPDATING Peer " + sender + " failed.", NotificationLevel.Debug);
            }
        }

        protected virtual void p2pControl_OnPayloadMessageReceived(PeerId sender, byte[] data)
        {
            GuiLogging("RECEIVED message from non subscribed peer: " + UTF8Encoding.UTF8.GetString(data) + ", ID: " + sender, NotificationLevel.Debug);
            // if sender is already registered, update its entry in either case
            if (this.peerManagement.Update(sender))
            {
                GuiLogging("UPDATED Peer " + sender + " successfully.", NotificationLevel.Debug);
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

        private void peerManagement_OnSubscriberRemoved(PeerId peerId)
        {
            GuiLogging("REMOVED peer " + peerId, NotificationLevel.Info);
        }

        protected void GuiLogging(string sText, NotificationLevel notLev)
        {
            if(OnGuiMessage != null)
                OnGuiMessage(sText, notLev);
        }

        #endregion 

        // Only for testing the (De-)Serialization of SubscribersManagement
        public void TestSerialization()
        {
            /* Get all Subs and serialize them manual */
            List<PeerId> originalSubList = this.peerManagement.GetAllSubscribers();

            /* Serialize and deserialize active subs automatically */
            byte[] byResult = this.peerManagement.Serialize();
            List<PeerId> pid = this.peerManagement.Deserialize(byResult, ref this.p2pControl);

            /* Comparing the deserialized list with the original SubList */
            bool result = true;

            if (pid.Count != originalSubList.Count)
            {
                result = false;
            }
            else
            {
                int f = 0;
                foreach (PeerId originalPeer in originalSubList)
                {
                    if (originalPeer != pid[f])
                    {
                        result = false;
                        break;
                    }
                    f++;
                }
            }
            GuiLogging("Result of serialization/deserialization: " + result, NotificationLevel.Debug);
        }
    }
}