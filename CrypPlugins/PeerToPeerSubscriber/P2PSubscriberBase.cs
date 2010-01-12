using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using System.Threading;

/*
 * All Subscriber functions work problem-free!
 * 
 * IDEAS:
 * - Publisher takes subscriber list out of the DHT and registered 
 *   itself with all subscribers pro-active (handle Register-Msg in Subscriber!)
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PSubscriberBase
    {
        public delegate void GuiMessage(string sData, NotificationLevel notificationLevel);
        public event GuiMessage OnGuiMessage;
        public delegate void TextArrivedFromPublisher(byte[] data, PeerId pid);
        public event TextArrivedFromPublisher OnTextArrivedFromPublisher;
        public delegate void ReceivedStopFromPublisher(PubSubMessageType stopType, string sData);
        /// <summary>
        /// fired when Manager sent "stop" message to the worker.
        /// </summary>
        public event ReceivedStopFromPublisher OnReceivedStopMessageFromPublisher;

        #region Variables

        private const string DHT_ALIVE_POSTFIX = "AliveMsg";

        private IP2PControl p2pControl;
        private long sendAliveMessageInterval;
        private long checkPublishersAvailability;
        private long publisherReplyTimespan;
        private string sTopic;

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
        private PeerId actualPublisher;
        /// <summary>
        /// if true, check whether a new Publisher is the actual one
        /// and renew Settings
        /// </summary>
        private bool bolStopped = true;

        private bool started = false;
        /// <summary>
        /// Status flag which contains the state of the Subscriber
        /// </summary>
        public bool Started 
        {
            get { return this.started;  }
            private set { this.started = value;  } 
        }

        #endregion

        /* BEGIN: Only for experimental cases */

        public void SolutionFound(byte[] solutionData)
        {
            SendMessage(actualPublisher, PubSubMessageType.Solution);
            this.p2pControl.SendToPeer(solutionData, actualPublisher);
        }

        /* END: Only for experimental cases */


        public P2PSubscriberBase(IP2PControl p2pControl)
        {
            this.p2pControl = p2pControl;
        }

        public void Start(string sTopic, long checkPublishersAvailability, long publishersReplyTimespan)
        {
            this.p2pControl.OnPayloadMessageReceived += new P2PPayloadMessageReceived(p2pControl_OnPayloadMessageReceived);
            this.p2pControl.OnSystemMessageReceived += new P2PSystemMessageReceived(p2pControl_OnSystemMessageReceived);

            this.sTopic = sTopic;
            this.checkPublishersAvailability = checkPublishersAvailability;
            this.publisherReplyTimespan = publishersReplyTimespan;
            Register();
        }

        private void Register()
        {
            // because CheckPublishersAvailability checks this value, set it for the first time here...
            // if bolStopped = true, the Timer for Checking Publishers liveliness doesn't start
            this.bolStopped = false;
            PeerId pubId = CheckPublisherAvailability();
            // if DHT Entry for Task is empty, there exist no Publisher at present.
            // The method PublisherIsAlive starts a Timer for this case to continous proof Publisher-DHT-Entry
            if (pubId != null)
            {
                SendMessage(pubId, PubSubMessageType.Register);
                if (this.timeoutForPublishersRegAccept == null)
                    this.timeoutForPublishersRegAccept = new Timer(OnTimeoutRegisteringAccepted, null, this.publisherReplyTimespan, this.publisherReplyTimespan);
                this.started = true;
            }
            else
            {
                this.Started = false;
                // if PubId is null, the Publisher isn't started!
                this.bolStopped = true;
            }
        }

        private void p2pControl_OnSystemMessageReceived(PeerId sender, PubSubMessageType msgType)
        {
            if (sender != actualPublisher)
            {
                GuiLogging("RECEIVED message from third party peer (not the publisher!): " + msgType.ToString() + ", ID: " + sender, NotificationLevel.Debug);
                return;
            }
            switch (msgType)
            {
                case PubSubMessageType.RegisteringAccepted:
                    GuiLogging("REGISTERING ACCEPTED received from publisher!", NotificationLevel.Info);
                    if (this.timeoutForPublishersRegAccept != null)
                    {
                        this.timeoutForPublishersRegAccept.Dispose();
                        this.timeoutForPublishersRegAccept = null;
                    }
                    break;
                case PubSubMessageType.Ping:
                    SendMessage(sender, PubSubMessageType.Pong);
                    GuiLogging("REPLIED to a ping message from " + sender, NotificationLevel.Debug);
                    break;
                case PubSubMessageType.Register:
                case PubSubMessageType.Unregister:
                    GuiLogging(msgType.ToString().ToUpper() + " received from PUBLISHER.", NotificationLevel.Debug);
                    // continuously try to get a unregister and than re-register with publisher
                    Stop(msgType);
                    Register();
                    break;
                case PubSubMessageType.Solution:
                    Stop(msgType);
                    GuiLogging("Another Subscriber had found the solution!", NotificationLevel.Info);
                    break;
                case PubSubMessageType.Stop:
                    Stop(msgType);
                    GuiLogging("STOP received from publisher. Subscriber is stopped!", NotificationLevel.Warning);
                    break;
                case PubSubMessageType.Pong:
                    if (this.timeoutForPublishersPong != null)
                    {
                        this.timeoutForPublishersPong.Dispose();
                        this.timeoutForPublishersPong = null;
                    }
                    break;
                case PubSubMessageType.Alive:
                default:
                    // not possible at the moment
                    break;
            }
        }

        private void p2pControl_OnPayloadMessageReceived(PeerId sender, byte[] data)
        {
            if (sender != actualPublisher)
            {
                GuiLogging("RECEIVED message from third party peer (not the publisher!): " + UTF8Encoding.UTF8.GetString(data) + ", ID: " + sender, NotificationLevel.Debug);
                return;
            }
            // functionality swapped for better inheritance
            HandleIncomingData(sender, data);
        }

        /// <summary>
        /// Incoming data will be printed in the information field and the OnTextArrivedEvent will be thrown
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="sData"></param>
        protected virtual void HandleIncomingData(PeerId senderId, byte[] data)
        {
            GuiLogging("RECEIVED: Message from '" + senderId
                    + "' with data: '" + UTF8Encoding.UTF8.GetString(data) + "'", NotificationLevel.Debug);

            if (OnTextArrivedFromPublisher != null)
                OnTextArrivedFromPublisher(data, senderId);
        }

        private void SendMessage(PeerId pubPeerId, PubSubMessageType msgType)
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
                case PubSubMessageType.Stop: 

                case PubSubMessageType.Solution:
                    break;
                //case PubSubMessageType.Solution:
                //    // when i send Solution to the Stop method, we will run into a recursive loop between SendMessage and Stop!
                //    Stop(PubSubMessageType.NULL);
                //    break;
                default:
                    GuiLogging("No Message sent, because MessageType wasn't supported: " + msgType.ToString(), NotificationLevel.Warning);
                    return;
            }
            this.p2pControl.SendToPeer(msgType, pubPeerId);

            // don't show every single alive message
            if (msgType != PubSubMessageType.Alive)
                GuiLogging(msgType.ToString() + " message sent to Publisher", NotificationLevel.Debug);
        }

        // registering isn't possible if no publisher has stored
        // his ID in the DHT Entry with the Key TaskName
        private void OnRegisteringNotPossible(object state)
        {
            PeerId pubId = CheckPublisherAvailability();
            // if DHT Entry for Task is empty, there exist no Publisher at present.
            // The method PublisherIsAlive starts a Timer for this case to continous proof Publisher-DHT-Entry
            if (pubId != null)
                SendMessage(pubId, PubSubMessageType.Register);
            else
                GuiLogging("No publisher for registering found.", NotificationLevel.Info);
        }

        private void OnSendAliveMessage(object state)
        {
            SendMessage(actualPublisher, PubSubMessageType.Alive);
        }

        /// <summary>
        /// Returns the actual Publishers ID or null, when a publisher wasn't found in the DHT. In the second case,
        /// a Timer will be started, to check periodically the DHT entry.
        /// When the publishers entry changed the Publishers ID, a Register-message will be send to the new Publisher.
        /// The Timer for periodically checking the Publishers availability is also started here.
        /// </summary>
        /// <returns>the actual Publishers ID or null, when a publisher wasn't found in the DHT</returns>
        private PeerId CheckPublisherAvailability()
        {
            PeerId pid;

            byte[] bytePubId = this.p2pControl.DHTload(this.sTopic);
            if (bytePubId == null)
            {
                if (timerRegisteringNotPossible == null && !this.bolStopped)
                {
                    // if DHT value doesn't exist at this moment, wait for 10 seconds and try again
                    timerRegisteringNotPossible = new Timer(OnRegisteringNotPossible, null, 10000, 10000);
                }
                GuiLogging("Publisher wasn't found in DHT or settings didn't stored on the right way.", NotificationLevel.Debug);
                //GuiLogging("Publisher wasn't found in DHT or settings didn't stored on the right way.", NotificationLevel.Warning);
                return null;
            }

            byte[] byteISettings = this.p2pControl.DHTload(this.sTopic + DHT_ALIVE_POSTFIX);
            if (byteISettings == null)
            {
                GuiLogging("Can't find AliveMsg-Settings from Publisher for the Subscriber.", NotificationLevel.Error);
                return null;
            }
            sendAliveMessageInterval = System.BitConverter.ToInt32(byteISettings, 0);

            pid = this.p2pControl.GetPeerID(bytePubId);
            if (actualPublisher == null) //first time initialization
            {
                actualPublisher = pid;
                GuiLogging("First time received publishers ID.", NotificationLevel.Debug);
            }
            else if (actualPublisher != pid)
            {
                GuiLogging("Publisher has been changed from ID '" +  actualPublisher + "' to '" + pid + "'", NotificationLevel.Debug);
                actualPublisher = pid;
                // because the publisher has changed, send a new register msg
                SendMessage(actualPublisher, PubSubMessageType.Register);
            }

            GuiLogging("RECEIVED: Publishers' peer ID '" + pid + "', Alive-Msg-Interval: " + sendAliveMessageInterval / 1000 + " sec!", NotificationLevel.Debug);

            // setting timer to check periodical the availability of the publishing peer
            if (timerCheckPubAvailability == null && !this.bolStopped)
                timerCheckPubAvailability = new Timer(OnCheckPubAvailability, null, this.checkPublishersAvailability, this.checkPublishersAvailability);

            return pid;
        }

        /// <summary>
        /// Callback for timerCheckPubAvailability (adjustable parameter 
        /// in settings, usually every 60 seconds). If another Peer
        /// takes over Publishing the Task, this will be handled in this callback, too.
        /// </summary>
        /// <param name="state"></param>
        private void OnCheckPubAvailability(object state)
        {
            PeerId newPubId = CheckPublisherAvailability();

            if (newPubId == actualPublisher)
            {
                // Timer will be only stopped, when OnMessageReceived-Event received 
                // a Pong-Response from the publisher! 
                SendMessage(actualPublisher, PubSubMessageType.Ping);
                if (timeoutForPublishersPong == null)
                {
                    timeoutForPublishersPong = new Timer(OnTimeoutPublishersPong, null, this.publisherReplyTimespan, this.publisherReplyTimespan);
                }
            }
        }

        /// <summary>
        /// This callback is only fired, when the publisher didn't sent a response on the register message.
        /// </summary>
        /// <param name="state"></param>
        private void OnTimeoutRegisteringAccepted(object state)
        {
            GuiLogging("TIMEOUT: Waiting for registering accepted message from publisher!", NotificationLevel.Warning);
            // try to register again
            Register();
        }

        /// <summary>
        /// This callback os only fired, when the publisher didn't sent a response on the ping message.
        /// </summary>
        /// <param name="state"></param>
        private void OnTimeoutPublishersPong(object state)
        {
            GuiLogging("Publisher didn't answer on Ping in the given time span!", NotificationLevel.Warning);
            if (timeoutForPublishersPong != null)
            {
                timeoutForPublishersPong.Dispose();
                timeoutForPublishersPong = null;
            }
            // try to get an active publisher and re-register
            CheckPublisherAvailability();
        }

        /// <summary>
        /// Will stop all timers, so Subscriber ends with sending
        /// Register-, Alive- and Pong-messages. Furthermore an
        /// unregister message will be send to the publisher
        /// </summary>
        public void Stop(PubSubMessageType msgType)
        {
            this.bolStopped = true;
            if (actualPublisher != null && msgType != PubSubMessageType.NULL)
                SendMessage(actualPublisher, msgType);

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
            GuiLogging("All Timers were stopped successfully", NotificationLevel.Debug);

            this.p2pControl.OnSystemMessageReceived -= p2pControl_OnSystemMessageReceived;
            this.p2pControl.OnPayloadMessageReceived -= p2pControl_OnPayloadMessageReceived;

            this.Started = false;
            GuiLogging("Subscriber is completely stopped",NotificationLevel.Debug);
        }

        protected void GuiLogging(string sText, NotificationLevel notLev)
        {
            if (OnGuiMessage != null)
                OnGuiMessage(sText, notLev);
        }
    }
}
