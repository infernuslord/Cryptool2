using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using KeySearcher;
using Cryptool.PluginBase.Miscellaneous;

/*bearbeitung
 * TODO:
 * - Order the results when they arrived as a message! (MessageReceived method)
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PManagerBase : P2PPublisherBase
    {
        #region Variables

        private bool managerStarted = false;
        public bool ManagerStarted 
        {
            get { return this.managerStarted; }
            private set { this.managerStarted = value; } 
        }

        private BigInteger keyPatternPartSize = 10000;
        /// <summary>
        /// Declare in how many parts the key space will be split
        /// </summary>
        public BigInteger KeyPatternPartSize 
        {
            get { return this.keyPatternPartSize; }
            private set 
            {
                // value is an even value?
                if ((value % 2) == 0)
                {
                    this.keyPatternPartSize = value;
                }
                else
                {
                    this.keyPatternPartSize = value + 1;
                }
            } 
        }

        /// <summary>
        /// Queue of all left key spaces of the given Key Pattern
        /// </summary>
        private Queue<KeyPattern> leftKeyPatterns;
        /// <summary>
        /// Key = PeerId, Value = Pattern (key space of key pattern)
        /// </summary>
        Dictionary<string, KeyPattern> allocatedPatterns;
        /// <summary>
        /// Key = KeyPattern (key space), Value = Result
        /// </summary>
        Dictionary<KeyPattern, string> patternResults;

        #endregion

        public P2PManagerBase(IP2PControl p2pControl) : base(p2pControl)
        {
            this.leftKeyPatterns = new Queue<KeyPattern>();
            this.allocatedPatterns = new Dictionary<string, KeyPattern>();
            this.patternResults = new Dictionary<KeyPattern, string>();
        }

        protected override void MessageReceived(PeerId sourceAddr, string sData)
        {
            PubSubMessageType msgType = this.p2pControl.GetMsgType(sData);

            if (msgType != PubSubMessageType.NULL)
                // base class handles all administration cases (register, alive, unregister, ping, pong, ...)
                base.MessageReceived(sourceAddr, sData);
            else
            {
                // if sending peer is in the allocatedPatterns Dict, its an (intermediate) result
                if (this.allocatedPatterns.ContainsKey(sourceAddr.stringId))
                {
                    KeyPattern actualPattern = this.allocatedPatterns[sourceAddr.stringId];
                    // if patternResult already contains a result for this pattern, compare the results
                    // and take the better one
                    if (this.patternResults.ContainsKey(actualPattern))
                    {
                        // TODO: compare results and take the better one
                        this.patternResults[actualPattern] = sData;
                    }
                    else
                    {
                        this.patternResults.Add(actualPattern, sData);
                    }
                    GuiLogging("(Intermediate) result found for pattern: '" + actualPattern.ToString() + "'. Result: '" + sData + "'", NotificationLevel.Debug);
                }
                else
                {
                    GuiLogging("Received Message from non-working peer. Data: " + sData + ", ID: " + sourceAddr.stringId,NotificationLevel.Info);
                }
            }
        }

        // every time when new workers are available, continue dispersion of patterns
        private void peerManagement_OnFreeWorkersAvailable()
        {
            DispersePatterns();
        }

        private void peerManagement_OnSubscriberRemoved(PeerId peerId)
        {
            if (this.allocatedPatterns.ContainsKey(peerId.stringId))
            {
                // because actual processing worker was removed, its job must be added to LeftKeyPatterns-List
                this.leftKeyPatterns.Enqueue(this.allocatedPatterns[peerId.stringId]);
                this.allocatedPatterns.Remove(peerId.stringId);
            }
            GuiLogging("REMOVED worker " + peerId.stringId, NotificationLevel.Info);
        }

        public int DispersePatterns()
        {
            if (!this.ManagerStarted)
            {
                GuiLogging("Manager isn't started at present, so I can't disperse the patterns.", NotificationLevel.Info);
                return 0;
            }

            int iCycle = 0;
            int iFreePatternAmount = leftKeyPatterns.Count;
            if (iFreePatternAmount == 0) 
                return 0;

            // gets only the free workers, which had register at this manager
            List<PeerId> lstSubscribers = ((WorkersManagement)this.peerManagement).GetFreeWorkers();
            foreach (PeerId subscriber in lstSubscribers)
            {
                if (iCycle <= iFreePatternAmount)
                {
                    KeyPattern actualPattern = this.leftKeyPatterns.Dequeue();
                    this.allocatedPatterns.Add(subscriber.stringId, actualPattern);
                    // send job (Keyspace) to the actual worker peer
                    this.p2pControl.SendToPeer(actualPattern.ToString(), subscriber.byteId);
                    // set free worker to busy in the peerManagement class
                    ((WorkersManagement)this.peerManagement).SetFreeWorkerToBusy(subscriber);

                    iCycle++;
                }
                else
                {
                    // no more patterns to disperse, so leave foreach
                    break;
                }
            } // end foreach

            GuiLogging(iCycle.ToString() + " patterns dispersed. " + lstSubscribers.Count + " free workers were available. Patterns left: " + this.leftKeyPatterns.Count.ToString(), NotificationLevel.Debug);
            return iCycle;
        }

        #region overrided methods with low functionality

        protected override void AssignManagement(long aliveMessageInterval)
        {
            this.peerManagement = new WorkersManagement(aliveMessageInterval);
            this.peerManagement.OnSubscriberRemoved += new SubscriberManagement.SubscriberRemoved(peerManagement_OnSubscriberRemoved);
            // waiting for new workers joining the manager or already joined worker, who were set to "free" again
            ((WorkersManagement)this.peerManagement).OnFreeWorkersAvailable += new WorkersManagement.FreeWorkersAvailable(peerManagement_OnFreeWorkersAvailable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sTopic"></param>
        /// <param name="aliveMessageInterval"></param>
        /// <param name="keyPattern">Already initialized (!!!) key pattern</param>
        /// <returns></returns>
        public bool StartManager(string sTopic, long aliveMessageInterval, KeyPattern keyPattern)
        {
            bool bolStartResult = base.Start(sTopic, aliveMessageInterval);

            List<KeyPattern> arrKeyPatternPool = keyPattern.makeKeySearcherPool(this.KeyPatternPartSize);
            foreach (KeyPattern keyPatternPart in arrKeyPatternPool)
            {
                this.leftKeyPatterns.Enqueue(keyPatternPart);
            }

            this.ManagerStarted = true;

            return bolStartResult;
            
        }

        public override void Stop(PubSubMessageType msgType)
        {
            base.Stop(msgType);

            this.ManagerStarted = false;
            ((WorkersManagement)this.peerManagement).OnFreeWorkersAvailable -= peerManagement_OnFreeWorkersAvailable;
        }

        #endregion
    }
}
