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
using System.Threading;

/*bearbeitung
 * TODO:
 * - Order the results when they arrived as a message! (MessageReceived method)
 * - Fire event, when all Pattern were distributed and ALL there results had arrived
 */

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PManagerBase : P2PPublisherBase
    {
        #region Events and Delegates

        public delegate void FinishedDistributingPatterns(LinkedList<KeySearcher.KeySearcher.ValueKey> lstTopList);
        /// <summary>
        /// This event will be thrown, when all patterns were done and all results were received
        /// </summary>
        public event FinishedDistributingPatterns OnFinishedDistributingPatterns;

        public delegate void FinishedOnePattern(string wildCardKey, double firstCoeffResult, string firstKeyResult, string workerId);
        /// <summary>
        /// Will be thrown when ONE pattern is bruteforced successfully
        /// </summary>
        public event FinishedOnePattern OnFinishedOnePattern;

        public delegate void ProcessProgress(double progressInPercent);
        public event ProcessProgress OnProcessProgress;

        #endregion

        #region Variables

        private const int MAX_IN_TOP_LIST = 10;

        private bool managerStarted = false;
        public bool ManagerStarted 
        {
            get { return this.managerStarted; }
            private set { this.managerStarted = value; } 
        }

        // 10.000 = 2048 Keys; 100.000 = 256 Keys; 400.000 = 64 Keys; 1.000.000 = 32 Keys - Angaben je für AES und 6 Wildcards...
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
        /// Global TopList, which will be actualized every time, when a new sub-result arrives. Consists only the top10 results.
        /// </summary>
        LinkedList<KeySearcher.KeySearcher.ValueKey> globalTopList;

        #endregion

        public P2PManagerBase(IP2PControl p2pControl)
            : base(p2pControl)
        {
            this.leftKeyPatterns = new Queue<KeyPattern>();
            this.allocatedPatterns = new Dictionary<string, KeyPattern>();
            this.patternResults = new Dictionary<KeyPattern, string>();
            this.globalTopList = getDummyTopList(MAX_IN_TOP_LIST);
        }

        protected override void MessageReceived(PeerId sourceAddr, string sData)
        {
            PubSubMessageType msgType = this.p2pControl.GetMsgType(sData);

            if (msgType != PubSubMessageType.NULL)
            {
                // ignore Solution case, because other worker could work on...
                if (msgType != PubSubMessageType.Solution)
                    // base class handles all administration cases (register, alive, unregister, ping, pong, ...)
                    base.MessageReceived(sourceAddr, sData);
            }
            else
            {
                ProcessPatternResult(sData, sourceAddr);
            }
        }

        /// <summary>
        /// This method decides whether the Data is an actual pattern result. Than deserializes data, display it,
        /// actualize the Top-10-List of ALL results and sets the worker free again.
        /// </summary>
        /// <param name="sPatternResult">serialized pattern result</param>
        /// <param name="workerId">ID of the actual worker</param>
        /// <returns></returns>
        private bool ProcessPatternResult(string sPatternResult, PeerId workerId)
        {
            // if sending peer is in the allocatedPatterns Dict, its an (intermediate) result
            if (this.allocatedPatterns.ContainsKey(workerId.stringId))
            {
                GuiLogging("Result from worker '" + workerId.stringId + "' received. Data: " + sPatternResult, NotificationLevel.Debug);

                KeyPattern actualPattern = this.allocatedPatterns[workerId.stringId];

                LinkedList<KeySearcher.KeySearcher.ValueKey> lstResults = DeserializeKeySearcherResult(sPatternResult);

                if (lstResults != null)
                {
                    KeySearcher.KeySearcher.ValueKey firstResult = lstResults.First<KeySearcher.KeySearcher.ValueKey>();

                    GuiLogging("Result was deserialized. Coeff.Value: " + firstResult.value.ToString() + ", Key: '" + firstResult.key + "'", NotificationLevel.Debug);

                    // if patternResult already contains a result for this pattern, compare the results
                    // and take the better one
                    if (this.patternResults.ContainsKey(actualPattern))
                    {
                        // TODO: compare results and take the better one
                        this.patternResults[actualPattern] = sPatternResult;
                        GuiLogging("New result for the same pattern (Value: " + firstResult.value.ToString() + ", Key: '" + firstResult.key + "') received. So it was updated.", NotificationLevel.Debug);
                    }
                    else
                    {
                        this.patternResults.Add(actualPattern, sPatternResult);
                        GuiLogging("Received FIRST result for the pattern (Value: " + firstResult.value.ToString() + ", Key: '" + firstResult.key + "')", NotificationLevel.Debug);
                    }
                    // TODO: Compare, actualize and display top 10 list of all results received from the different workers
                    ActualizeGlobalTopList(lstResults);

                    GuiLogging("(Intermediate) result found (Value: " + firstResult.value.ToString() + ", Key: '" + firstResult.key + "')", NotificationLevel.Info);

                    // send information to the Plugin to display the first result
                    if (OnFinishedOnePattern != null)
                        OnFinishedOnePattern(actualPattern.WildcardKey, firstResult.value, firstResult.key, workerId.stringId);

                    //remove, because task is solved and stored in Dictionary patternResults
                    this.allocatedPatterns.Remove(workerId.stringId);
                    // because result-sending worker is again free, set it to free sin the management, so it will get a new KeyPattern if available
                    ((WorkersManagement)this.peerManagement).SetBusyWorkerToFree(workerId);
                }
                else
                {
                    GuiLogging("Deserializing result canceled: '" + sPatternResult + "'.", NotificationLevel.Error);
                }
                if (this.leftKeyPatterns.Count == 0)
                {
                    if (OnFinishedDistributingPatterns != null)
                        OnFinishedDistributingPatterns(this.globalTopList);
                }

                return true;
            }
            else
            {
                GuiLogging("Received Message from non-working peer. Data: " + sPatternResult + ", ID: " + workerId.stringId, NotificationLevel.Info);
            }
            return false;
        }

        /* serialization information: 3 fields per data set in the following order: 
         * 1) value (double) 
         * 2) key (string) 
         * 3) decryption (byte[]) */
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
            if (!this.managerStarted)
            {
                GuiLogging("Manager isn't started at present, so I can't disperse the patterns.", NotificationLevel.Error);
                throw (new Exception("Critical error in P2PManager. Manager isn't started yet, but there can register workers..."));
            }   
            // check if patterns are left
            if (this.leftKeyPatterns.Count != 0)
                DispersePatterns();
            else
                GuiLogging("No more patterns left. So wait for the last results, than close this task.", NotificationLevel.Debug);
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
            int iCycle = 0;
            int iFreePatternAmount = leftKeyPatterns.Count;

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

                    GuiLogging("Pattern sent to peer (" + subscriber.stringId + "), Pattern: " + actualKeyPattern.WildcardKey, NotificationLevel.Debug);

                    // set free worker to busy in the peerManagement class
                    ((WorkersManagement)this.peerManagement).SetFreeWorkerToBusy(subscriber);

                    GuiLogging("Worker was set to busy. (Id: " + subscriber.stringId + ")", NotificationLevel.Debug);

                    iCycle++;
                }
                else
                {
                    // no more patterns to disperse, so leave foreach
                    // TODO: inform all workers that no more patterns must be bruteforced, they only have to finish the last assigned Patterns
                    ((WorkersManagement)this.peerManagement).OnFreeWorkersAvailable -= peerManagement_OnFreeWorkersAvailable;
                    GuiLogging("All patterns were dispersed. So waiting for the last results.",NotificationLevel.Debug);
                    break;
                }
            } // end foreach




            GuiLogging("Bruteforcing progress: " + GetProgressInformation() + "%.", NotificationLevel.Debug);



            GuiLogging(iCycle.ToString() + " pattern(s) dispersed. Patterns left: " + this.leftKeyPatterns.Count.ToString(), NotificationLevel.Info);
            return iCycle;
        }

        private double GetProgressInformation()
        {
            int leftPatterns = this.leftKeyPatterns.Count;
            int finishedPatterns = this.patternResults.Count;
            int patternsInProcess = this.allocatedPatterns.Count;
            int patternAmount = leftPatterns + finishedPatterns + patternsInProcess;
            double patternProgressInPercent;
            if (finishedPatterns > 0 && patternsInProcess > 0)
                patternProgressInPercent = 40 * (patternsInProcess / patternAmount) + 100 * (finishedPatterns / patternAmount);
            else if (patternsInProcess > 0)
                patternProgressInPercent = 40 * (patternsInProcess / patternAmount);
            else
                patternProgressInPercent = 0.0;

            // throw progress result to the GUI
            if (OnProcessProgress != null)
            {
                OnProcessProgress(patternProgressInPercent);
            }
            return patternProgressInPercent;
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
        /// <param name="keyPatternSize">KeyPatternSize in hundred thousand!</param>
        /// <returns></returns>
        public bool StartManager(string sTopic, long aliveMessageInterval, KeyPattern keyPattern, int keyPatternSize)
        {
            this.KeyPatternPartSize = (BigInteger)(keyPatternSize * 100000);
            return StartManager(sTopic, aliveMessageInterval, keyPattern);
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

            GuiLogging("Begin building a KeyPatternPool with KeyPatternPartSize " + this.KeyPatternPartSize.ToString(), NotificationLevel.Debug);
            List<KeyPattern> arrKeyPatternPool = keyPattern.makeKeySearcherPool(this.KeyPatternPartSize);
            GuiLogging("Enqueue " + arrKeyPatternPool.Count + " KeyPattern-Parts to the JobList.", NotificationLevel.Debug);
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

        #region Global Top-10-List

        double lastGlobalValue = 0.0;
        // leaned on KeySearcher.updateTopList
        private void ActualizeGlobalTopList(LinkedList<KeySearcher.KeySearcher.ValueKey> newTopList)
        {
            double firstNewValue = newTopList.First.Value.value;
            if (lastGlobalValue > firstNewValue)
                return;

            LinkedListNode<KeySearcher.KeySearcher.ValueKey> globalNode;
            LinkedListNode<KeySearcher.KeySearcher.ValueKey> newNode;

            newNode = newTopList.First;
            while(newNode != null)
            {
                globalNode = this.globalTopList.First;
                while (globalNode != null)
                {
                    if (newNode.Value.value > globalNode.Value.value)
                    {
                        this.globalTopList.AddBefore(globalNode, newNode.Value);
                        this.globalTopList.RemoveLast();
                        lastGlobalValue = this.globalTopList.Last.Value.value;
                        break;
                    }
                    globalNode = globalNode.Next;
                }
                newNode = newNode.Next;
            }
        }

        // leaned on KeySearcher.fillListWithDummies
        private LinkedList<KeySearcher.KeySearcher.ValueKey> getDummyTopList(int maxInList)
        {
            LinkedList<KeySearcher.KeySearcher.ValueKey> returnList = new LinkedList<KeySearcher.KeySearcher.ValueKey>();

            KeySearcher.KeySearcher.ValueKey valueKey = new KeySearcher.KeySearcher.ValueKey();

            valueKey.value = double.MinValue;
            valueKey.key = "dummykey";
            valueKey.decryption = new byte[0];

            lastGlobalValue = valueKey.value;
            LinkedListNode<KeySearcher.KeySearcher.ValueKey> node = returnList.AddFirst(valueKey);
            for (int i = 1; i < maxInList; i++)
            {
                node = returnList.AddAfter(node, valueKey);
            }
            return returnList;
        }

        public string GetGlobalTopList(LinkedList<KeySearcher.KeySearcher.ValueKey> topList)
        {
            StringBuilder sbRet = new StringBuilder();
            LinkedListNode<KeySearcher.KeySearcher.ValueKey> node = topList.First;
            while (node != null)
            {
                sbRet.AppendLine(node.Value.value.ToString());
                sbRet.AppendLine(node.Value.key);
                sbRet.AppendLine("-------------------");
                node = node.Next;
            }
            return sbRet.ToString();
        }

        #endregion
    }
}
