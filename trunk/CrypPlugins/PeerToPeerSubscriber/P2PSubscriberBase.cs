using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using System.Timers;

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

        protected IP2PControl p2pControl;
        private long sendAliveMessageInterval;
        private long checkPublishersAvailability;
        private long publisherReplyTimespan;
        private string sTopic;

        /// <summary>
        /// Checking liveness, availability and/or changes (new peer) of the Publisher.
        /// Retrieves the required DHT entries and initiates the necessary steps
        /// </summary>
        private Timer timerCheckPubAvailability;
        /// <summary>
        /// To inform the publisher pro-active, that this subscriber
        /// is still alive and interested in this Task, send periodical 
        /// alive messages.
        /// </summary>
        private Timer timerSendingAliveMsg;
        /// <summary>
        /// This timer gets started when a DHT entry for a Publisher exists and
        /// we want to check the liveness of the publisher, 
        /// at which the subscriber had registered. Therefore we send a Ping message
        /// to the Publisher. If the timer callback is called and no Pong-Response was 
        /// received from the Publisher, the probability is high, that the Publisher is down! 
        /// </summary>
        private Timer timeoutForPublishersPong;
        /// <summary>
        /// After register message is sent to publisher, this timer is started.
        /// If the publisher doesn't responds with a RegisteringAccepted-Message,
        /// the probability is high, that the publisher is down!
        /// </summary>
        private Timer timeoutForPublishersRegAccept;
        /// <summary>
        /// PeerID of the actual publisher. This ID is will be checked continious
        /// on liveliness and/or updated if Publisher had changed.
        /// </summary>
        private PeerId actualPublisher;
        public PeerId ActualPublisher
        {
            get { return this.actualPublisher; }
            set { this.actualPublisher = value; }
        }

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
            get { return this.started; }
            private set { this.started = value; }
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

            this.timeoutForPublishersPong = new Timer();
            this.timeoutForPublishersPong.Elapsed += new ElapsedEventHandler(OnTimeoutPublishersPong);

            this.timeoutForPublishersRegAccept = new Timer();
            this.timeoutForPublishersRegAccept.Elapsed += new ElapsedEventHandler(OnTimeoutRegisteringAccepted);

            this.timerCheckPubAvailability = new Timer();
            this.timerCheckPubAvailability.Elapsed += new ElapsedEventHandler(OnCheckPubAvailability);

            this.timerSendingAliveMsg = new Timer();
            this.timerSendingAliveMsg.Elapsed += new ElapsedEventHandler(OnSendAliveMessage);
        }

        public void Start(string sTopic, long checkPublishersAvailability, long publishersReplyTimespan)
        {
            this.actualPublisher = null;

            this.sTopic = sTopic;
            this.checkPublishersAvailability = checkPublishersAvailability;
            this.publisherReplyTimespan = publishersReplyTimespan;

            #region Initialize network-maintanance-Timers

            double pubTimeResponseTimeout = Convert.ToDouble(this.publisherReplyTimespan);

            this.timerCheckPubAvailability.Interval = Convert.ToDouble(this.checkPublishersAvailability);
            this.timerCheckPubAvailability.AutoReset = true;
            this.timeoutForPublishersPong.Interval = pubTimeResponseTimeout;
            this.timeoutForPublishersRegAccept.Interval = pubTimeResponseTimeout;

            #endregion

            this.p2pControl.OnPayloadMessageReceived += new P2PPayloadMessageReceived(p2pControl_OnPayloadMessageReceived);
            this.p2pControl.OnSystemMessageReceived += new P2PSystemMessageReceived(p2pControl_OnSystemMessageReceived);

            if (this.p2pControl != null)
            {
                string sNonrelevant;
                PeerId myPeerId = this.p2pControl.GetPeerID(out sNonrelevant);
                GuiLogging("Started Subscriber with ID: '" + myPeerId.ToString() + "'", NotificationLevel.Info);
            }

            this.Started = true;

            CheckPublishersAvailability();
        }

        private void CheckPublishersAvailability()
        {
            // retrieve publisher information from the DHT
            PeerId pid = DHT_CommonManagement.GetTopicsPublisherId(ref this.p2pControl, this.sTopic);
            this.sendAliveMessageInterval = DHT_CommonManagement.GetAliveMessageInterval(ref this.p2pControl, this.sTopic);

            if (pid == null || this.sendAliveMessageInterval == 0)
            {
                GuiLogging("No Publisher/Manager information for registering found in the DHT.", NotificationLevel.Info);
            }
            else
            {
                this.timerSendingAliveMsg.Interval = Convert.ToDouble(this.sendAliveMessageInterval);
                this.timerSendingAliveMsg.Start();

                if (actualPublisher == null)
                {
                    GuiLogging("Found a Publisher/Manager with ID '" + pid.ToString() + ", so register with it.", NotificationLevel.Info);
                    SendMessage(pid, PubSubMessageType.Register);
                }
                else if (actualPublisher == pid)
                {
                    // Timer will be only stopped, when OnMessageReceived-Event received 
                    // a Pong-Response from the publisher! 
                    SendMessage(pid, PubSubMessageType.Ping);
                    this.timeoutForPublishersPong.Start();
                    GuiLogging("Successfully checked publishers'/managers' information in the DHT. To check liveness, a Ping message was sended to '" + pid.ToString() + "'.", NotificationLevel.Debug);
                }
                else
                {
                    GuiLogging("The Publisher/Manager had changed from '" + this.actualPublisher.ToString()
                        + "' to '" + pid.ToString() + "'. Register with the new Publisher/Manager.", NotificationLevel.Info);
                    SendMessage(pid, PubSubMessageType.Register);
                }
                this.actualPublisher = pid;
            }
            this.timerCheckPubAvailability.Enabled = true;
        }

        private void p2pControl_OnSystemMessageReceived(PeerId sender, PubSubMessageType msgType)
        {
            if (sender != this.actualPublisher)
            {
                GuiLogging("RECEIVED message from third party peer (not the publisher!): " + msgType.ToString() + ", ID: " + sender, NotificationLevel.Debug);
                return;
            }
            switch (msgType)
            {
                case PubSubMessageType.RegisteringAccepted:
                    GuiLogging("REGISTERING ACCEPTED received from publisher!", NotificationLevel.Info);
                    this.timeoutForPublishersRegAccept.Stop();
                    break;
                case PubSubMessageType.Ping:
                    SendMessage(sender, PubSubMessageType.Pong);
                    GuiLogging("REPLIED to a ping message from " + sender, NotificationLevel.Debug);
                    break;
                case PubSubMessageType.Register:
                case PubSubMessageType.Unregister:
                    GuiLogging(msgType.ToString().ToUpper() + " received from PUBLISHER.", NotificationLevel.Debug);
                    // continuously trying to get a unregister and than re-register with publisher
                    Stop(msgType);
                    CheckPublishersAvailability();
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
                    this.timeoutForPublishersPong.Stop();
                    break;
                case PubSubMessageType.Alive:
                default:
                    // not possible at the moment
                    break;
            }
            // workaround, because Timers.Timer doesn't contains a "Reset" method --> when receiving a
            // message from the Publisher, we can reset the "check pub availability"-interval time!
            this.timerCheckPubAvailability.Enabled = false;
            this.timerCheckPubAvailability.Enabled = true;
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
            switch (msgType)
            {
                case PubSubMessageType.Register:
                    // start waiting interval for RegAccept Message
                    this.timeoutForPublishersRegAccept.Start();
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
            // it doesn't care which message type was sent, but when it was sent to the publisher, reset this AliveMsg Timer
            if (pubPeerId == actualPublisher)
            {
                this.timerSendingAliveMsg.Enabled = false;
                this.timerSendingAliveMsg.Enabled = true;
            }

            this.p2pControl.SendToPeer(msgType, pubPeerId);

            GuiLogging(msgType.ToString() + " message sent to Publisher ID '" + pubPeerId.ToString() + "'.", NotificationLevel.Debug);
        }

        private void OnSendAliveMessage(object sender, ElapsedEventArgs e)
        {
            SendMessage(actualPublisher, PubSubMessageType.Alive);
        }

        /// <summary>
        /// Callback for timerCheckPubAvailability (adjustable parameter 
        /// in settings, usually every 60 seconds). If another Peer
        /// takes over Publishing the Task, this and many other things
        /// will be initiated here
        /// </summary>
        /// <param name="state"></param>
        private void OnCheckPubAvailability(object sender, ElapsedEventArgs e)
        {
            CheckPublishersAvailability();
        }

        /// <summary>
        /// This callback is only fired, when the publisher didn't sent a response on the register message.
        /// </summary>
        /// <param name="state"></param>
        private void OnTimeoutRegisteringAccepted(object sender, ElapsedEventArgs e)
        {
            GuiLogging("TIMEOUT: Waiting for registering accepted message from publisher!", NotificationLevel.Debug);
            // try to register again
            CheckPublishersAvailability();
        }

        /// <summary>
        /// This callback os only fired, when the publisher didn't sent a response on the ping message.
        /// </summary>
        /// <param name="state"></param>
        private void OnTimeoutPublishersPong(object sender, ElapsedEventArgs e)
        {
            GuiLogging("Publisher didn't respond on subscribers' ping message in the given time span!", NotificationLevel.Info);
            this.timeoutForPublishersPong.Stop();
            // try to get an active publisher and re-register

            CheckPublishersAvailability();
        }

        /// <summary>
        /// Will stop all timers, so Subscriber ends with sending
        /// Register-, Alive- and Pong-messages. Furthermore an
        /// unregister message will be send to the publisher
        /// </summary>
        public void Stop(PubSubMessageType msgType)
        {
            if (actualPublisher != null)
                SendMessage(actualPublisher, msgType);

            #region stopping all timers, if they are still active

            this.timeoutForPublishersPong.Stop();
            this.timeoutForPublishersRegAccept.Stop();
            this.timerSendingAliveMsg.Stop();

            //when Sub received a UnReg message, it haven't stop
            //the registering not possible worker, to connect to
            //a new incoming Publisher
            if (msgType == PubSubMessageType.Stop)
            {
                this.bolStopped = true;

                this.timerCheckPubAvailability.Stop();

                this.Started = false;
                this.p2pControl.OnSystemMessageReceived -= p2pControl_OnSystemMessageReceived;
                this.p2pControl.OnPayloadMessageReceived -= p2pControl_OnPayloadMessageReceived;
                GuiLogging("Subscriber/Worker is completely stopped", NotificationLevel.Debug);
            }
            else
            {
                GuiLogging("Publisher/Manager had left the network, waiting for its comeback or takeover by a new Publisher/Manager.", NotificationLevel.Info);
            }

            #endregion

            GuiLogging("All Timers were stopped successfully", NotificationLevel.Debug);
        }

        protected void GuiLogging(string sText, NotificationLevel notLev)
        {
            if (OnGuiMessage != null)
                OnGuiMessage(sText, notLev);
        }
    }
}