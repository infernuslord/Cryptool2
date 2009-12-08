using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using KeySearcher;
using Cryptool.PluginBase.Miscellaneous;
using System.IO;

/*bearbeitung
 * TODO:
 * - Order the results when they arrived as a message! (MessageReceived method)
 * - Fire event, when all Pattern were distributed and ALL there results had arrived
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PManagerBase : P2PPublisherBase
    {
        public delegate void FinishedDistributingPatterns(List<object> lstTopList);
        public event FinishedDistributingPatterns OnFinishedDistributingPatterns;

        #region Variables

        private bool managerStarted = false;
        public bool ManagerStarted 
        {
            get { return this.managerStarted; }
            private set { this.managerStarted = value; } 
        }

        // 10.000 = 2048 Keys bei AES; 100.000 = 256 Keys bei AES; 400.000 = 64 Keys bei AES; 1.000.000 = xx Keys bei AES
        private BigInteger keyPatternPartSize = 1000000;
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
        /// <summary>
        /// When a working peer sends the information message "Solution Found" the 
        /// actual worked Pattern gets stored with the PeerId, so we can detect the
        /// following received serialized result list. Delete entry after receiving
        /// the result list!!!
        /// </summary>
        Dictionary<string, KeyPattern> solutionFound;

        #endregion

        public P2PManagerBase(IP2PControl p2pControl) : base(p2pControl)
        {
            this.leftKeyPatterns = new Queue<KeyPattern>();
            this.allocatedPatterns = new Dictionary<string, KeyPattern>();
            this.patternResults = new Dictionary<KeyPattern, string>();
            this.solutionFound = new Dictionary<string, KeyPattern>();
        }

        protected override void MessageReceived(PeerId sourceAddr, string sData)
        {
            PubSubMessageType msgType = this.p2pControl.GetMsgType(sData);

            if (msgType != PubSubMessageType.NULL)
                // before the worker send the result list, it sends a SolutionFound Message
                if (msgType == PubSubMessageType.Solution)
                {
                    // if solutionFound sending peer isn't already in the solutionFound list, but had been allocated to a pattern
                    if (!this.solutionFound.ContainsKey(sourceAddr.stringId) && this.allocatedPatterns.ContainsKey(sourceAddr.stringId))
                    {
                        GuiLogging("Solution found message received from '" + sourceAddr.stringId + "'", NotificationLevel.Info);
                        this.solutionFound.Add(sourceAddr.stringId, this.allocatedPatterns[sourceAddr.stringId]);
                    }
                }
                else
                    // base class handles all administration cases (register, alive, unregister, ping, pong, ...)
                    base.MessageReceived(sourceAddr, sData);
            else
            {
                // if sending peer is in the allocatedPatterns Dict, its an (intermediate) result
                if (this.allocatedPatterns.ContainsKey(sourceAddr.stringId) && this.solutionFound.ContainsKey(sourceAddr.stringId))
                {
                    GuiLogging("Result from worker '" + sourceAddr.stringId + "' received. Data: " + sData, NotificationLevel.Debug);

                    KeyPattern actualPattern = this.allocatedPatterns[sourceAddr.stringId];

                    LinkedList<KeySearcher.KeySearcher.ValueKey> lstResults = DeserializeKeySearcherResult(sData);
                    
                    GuiLogging("Result for Pattern '" + actualPattern.WildcardKey + "' was deserialized. Value: '" + lstResults.First.Value,NotificationLevel.Debug);

                    // if patternResult already contains a result for this pattern, compare the results
                    // and take the better one
                    if (this.patternResults.ContainsKey(actualPattern))
                    {
                        // TODO: compare results and take the better one
                        this.patternResults[actualPattern] = sData;
                        GuiLogging("New result for the same pattern (" + actualPattern.WildcardKey + ") received. So it was updated.", NotificationLevel.Debug);
                    }
                    else
                    {
                        this.patternResults.Add(actualPattern, sData);
                        GuiLogging("Received FIRST result for the pattern '" + actualPattern.WildcardKey + "'",NotificationLevel.Debug);
                    }
                    // TODO: Compare, actualize and display top 10 list of all results received from the different workers
                    // ActualizeTopList(DeserializeKeySearcherResult(sData));

                    KeySearcher.KeySearcher.ValueKey firstResult = lstResults.First();

                    // result was processed, so peer must be removed from solution list
                    this.solutionFound.Remove(sourceAddr.stringId);

                    GuiLogging("(Intermediate) result found for pattern: '" + actualPattern.ToString() + "'. Result: '" + firstResult.key + "' with Coefficient: " + firstResult.value.ToString(), NotificationLevel.Info);
                }
                else
                {
                    GuiLogging("Received Message from non-working peer. Data: " + sData + ", ID: " + sourceAddr.stringId,NotificationLevel.Info);
                }
            }
        }

        /*
         * serialization information: 3 fields per data set in the following order: 
         * 1) value (double) 
         * 2) key (string) 
         * 3) decryption (byte[])
         */
        private string seperator = "#;#";
        private string dataSetSeperator = "|**|";
        private LinkedList<KeySearcher.KeySearcher.ValueKey> DeserializeKeySearcherResult(string sSerializedResult)
        {
            LinkedList<KeySearcher.KeySearcher.ValueKey> lstRet = new LinkedList<KeySearcher.KeySearcher.ValueKey>();
            string[] serveralDataSets = sSerializedResult.Split(dataSetSeperator.ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < serveralDataSets.Length; i++)
            {
                string[] severalFieldsInDataSet = serveralDataSets[i].Split(seperator.ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                if(severalFieldsInDataSet.Length != 3)
                    return null;
                // build ValueKey from splitted string
                KeySearcher.KeySearcher.ValueKey valKey = new KeySearcher.KeySearcher.ValueKey();
                valKey.value = Convert.ToDouble(severalFieldsInDataSet[0]);
                valKey.key = severalFieldsInDataSet[1];
                valKey.decryption = UTF8Encoding.UTF8.GetBytes(severalFieldsInDataSet[2]);
                // add builded ValueKey to list
                lstRet.AddLast(valKey);
            }
            return lstRet;
        }

        // every time when new workers are available, continue dispersion of patterns
        private void peerManagement_OnFreeWorkersAvailable()
        {
            if (DispersePatterns() == 0)
            {
                int result = 0;
                while (!this.ManagerStarted && result == 0)
                {
                    result = DispersePatterns();
                }
            }
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
                    KeyPattern actualKeyPattern = this.leftKeyPatterns.Dequeue();
                    this.allocatedPatterns.Add(subscriber.stringId, actualKeyPattern);
                    // send job (Keyspace) to the actual worker peer
                    this.p2pControl.SendToPeer(actualKeyPattern.SerializeToString(), subscriber.byteId);
                    // set free worker to busy in the peerManagement class
                    ((WorkersManagement)this.peerManagement).SetFreeWorkerToBusy(subscriber);

                    iCycle++;
                }
                else
                {
                    // no more patterns to disperse, so leave foreach
                    // TODO: inform all workers that no more patterns must be bruteforced, they only have to finish the last assigned Patterns
                    ((WorkersManagement)this.peerManagement).OnFreeWorkersAvailable -= peerManagement_OnFreeWorkersAvailable;
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
            
            /* ************************************** */
            /* Only for debugging reasons - to delete */      
            StreamWriter debugWriter = DebugToFile.GetDebugStreamWriter();
            int i = 0;
            foreach (KeyPattern patternPart in this.leftKeyPatterns.ToList<KeyPattern>())
            {
                debugWriter.WriteLine(i + "\t" + patternPart.ToString());
                i++;
            }
            debugWriter.Dispose();

            /* Only for debugging reasons - to delete */
            /* ************************************** */
            

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
