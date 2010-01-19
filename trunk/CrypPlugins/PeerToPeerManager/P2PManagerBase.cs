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
using Cryptool.PluginBase.IO;

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

        public delegate void FinishedOnePattern(string wildCardKey, double firstCoeffResult, string firstKeyResult, PeerId worker);
        /// <summary>
        /// Will be thrown when ONE pattern is bruteforced successfully
        /// </summary>
        public event FinishedOnePattern OnFinishedOnePattern;

        public delegate void ProcessProgress(double progressInPercent);
        public event ProcessProgress OnProcessProgress;

        #endregion

        #region Variables

        private const int MAX_IN_TOP_LIST = 10;
        //private const string DHT_ENCRYPTED_TEXT = "EncryptedText";
        //private const string DHT_INIT_VECTOR = "InitializationVector";

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
        /// When the Manager is started, this variable must be set.
        /// </summary>
        private string sTopic = String.Empty;

        /// <summary>
        /// Stack of all left key spaces of the given Key Pattern
        /// </summary>
        private Stack<KeyPattern> leftKeyPatterns;
        /// <summary>
        /// Key = PeerId, Value = Pattern (key space of key pattern)
        /// </summary>
        Dictionary<PeerId, KeyPattern> allocatedPatterns;
        /// <summary>
        /// Key = KeyPattern (key space), Value = Result
        /// </summary>
        Dictionary<KeyPattern, LinkedList<KeySearcher.KeySearcher.ValueKey>> patternResults;
        /// <summary>
        /// Global TopList, which will be actualized every time, when a new sub-result arrives. Consists only the top10 results.
        /// </summary>
        LinkedList<KeySearcher.KeySearcher.ValueKey> globalTopList;

        #endregion

        public P2PManagerBase(IP2PControl p2pControl)
            : base(p2pControl)
        {
            this.leftKeyPatterns = new Stack<KeyPattern>();
            this.allocatedPatterns = new Dictionary<PeerId, KeyPattern>();
            this.patternResults = new Dictionary<KeyPattern, LinkedList<KeySearcher.KeySearcher.ValueKey>>();
            this.globalTopList = getDummyTopList(MAX_IN_TOP_LIST);
        }

        protected override void p2pControl_OnPayloadMessageReceived(PeerId sender, byte[] data)
        {
            ProcessPatternResult(data, sender);
        }

        protected override void p2pControl_OnSystemMessageReceived(PeerId sender, PubSubMessageType msgType)
        {
            // ignore Solution case, because other worker could work on...
            if (msgType != PubSubMessageType.Solution)
                // base class handles all administration cases (register, alive, unregister, ping, pong, ...)
                base.p2pControl_OnSystemMessageReceived(sender, msgType);
        }


        /// <summary>
        /// This method decides whether the Data is an actual pattern result. Than deserializes data, display it,
        /// actualize the Top-10-List of ALL results and sets the worker free again.
        /// </summary>
        /// <param name="patternResult">serialized pattern result</param>
        /// <param name="workerId">ID of the actual worker</param>
        /// <returns></returns>
        private bool ProcessPatternResult(byte[] patternResult, PeerId workerId)
        {
            // if sending peer is in the allocatedPatterns Dict, its an (intermediate) result
            if (this.allocatedPatterns.ContainsKey(workerId))
            {
                GuiLogging("Result from worker '" + workerId + "' received. Data: " + UTF8Encoding.UTF8.GetString(patternResult,0,400), NotificationLevel.Debug);

                KeyPattern actualPattern = this.allocatedPatterns[workerId];

                KeySearcherResult keySearcherResult = new KeySearcherResult();
                LinkedList<KeySearcher.KeySearcher.ValueKey> lstResults = keySearcherResult.DeserializeResult(patternResult);

                if (lstResults != null)
                {
                    KeySearcher.KeySearcher.ValueKey firstResult = lstResults.First<KeySearcher.KeySearcher.ValueKey>();

                    GuiLogging("Result was deserialized. First Value: " + firstResult.value.ToString() + ", Key: '" + firstResult.key + "'", NotificationLevel.Debug);

                    // if patternResult already contains a result for this pattern, compare the results
                    // and take the better one
                    if (this.patternResults.ContainsKey(actualPattern))
                    {
                        // TODO: compare results and take the better one
                        this.patternResults[actualPattern] = lstResults;
                        GuiLogging("New result for the same pattern (Value: " + firstResult.value.ToString() + ", Key: '" + firstResult.key + "') received. So it was updated.", NotificationLevel.Debug);
                    }
                    else
                    {
                        this.patternResults.Add(actualPattern, lstResults);
                        GuiLogging("Received FIRST result for the pattern (Value: " + firstResult.value.ToString() + ", Key: '" + firstResult.key + "')", NotificationLevel.Debug);
                    }
                    ActualizeGlobalTopList(lstResults);

                    GuiLogging("(Intermediate) result found (Value: " + firstResult.value.ToString() + ", Key: '" + firstResult.key + "')", NotificationLevel.Info);

                    // send information to the Plugin to display the first result
                    if (OnFinishedOnePattern != null)
                        OnFinishedOnePattern(actualPattern.WildcardKey, firstResult.value, firstResult.key, workerId);

                    //remove, because task is solved and stored in Dictionary patternResults
                    this.allocatedPatterns.Remove(workerId);
                    // because result-sending worker is again free, set it to free sin the management, so it will get a new KeyPattern if available
                    ((WorkersManagement)this.peerManagement).SetBusyWorkerToFree(workerId);
                }
                else
                {
                    GuiLogging("Deserializing result canceled: '" + UTF8Encoding.UTF8.GetString(patternResult) + "'.", NotificationLevel.Error);
                }
                if (this.leftKeyPatterns.Count == 0)
                {
                    if (OnFinishedDistributingPatterns != null)
                        OnFinishedDistributingPatterns(this.globalTopList);
                }
                GetProgressInformation();

                return true;
            }
            else
            {
                GuiLogging("Received Message from non-working peer. Data: " + UTF8Encoding.UTF8.GetString(patternResult) + ", ID: " + workerId, NotificationLevel.Info);
            }
            return false;
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
            GetProgressInformation();
        }

        private void peerManagement_OnSubscriberRemoved(PeerId peerId)
        {
            if (this.allocatedPatterns.ContainsKey(peerId))
            {
                // because actual processing worker was removed, its job must be added to LeftKeyPatterns-List
                this.leftKeyPatterns.Push(this.allocatedPatterns[peerId]);
                bool removeResult = this.allocatedPatterns.Remove(peerId);
                GuiLogging("REMOVED worker with ID '" + peerId + "' and enqueue its pattern to the Pattern queue (" + removeResult + " ).", NotificationLevel.Info);
            }
            else
            {
                GuiLogging("REMOVED worker " + peerId, NotificationLevel.Info);
            }
            GetProgressInformation();
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
                    KeyPattern actualKeyPattern = this.leftKeyPatterns.Pop();
                    this.allocatedPatterns.Add(subscriber, actualKeyPattern);

                    // send job (Keyspace) to the actual worker peer
                    this.p2pControl.SendToPeer(actualKeyPattern.Serialize(), subscriber);

                    GuiLogging("Pattern sent to peer (" + subscriber + "), WildCardKey: " + actualKeyPattern.WildcardKey, NotificationLevel.Debug);

                    // set free worker to busy in the peerManagement class
                    ((WorkersManagement)this.peerManagement).SetFreeWorkerToBusy(subscriber);

                    GuiLogging("Worker was set to busy. (Id: " + subscriber + ")", NotificationLevel.Debug);

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

            GetProgressInformation();

            GuiLogging(iCycle.ToString() + " pattern(s) dispersed. Patterns left: " + this.leftKeyPatterns.Count.ToString(), NotificationLevel.Info);
            return iCycle;
        }

        /// <summary>
        /// returns the percentual progress information of the whole job
        /// </summary>
        /// <returns>the percentual progress information of the whole job</returns>
        private double GetProgressInformation()
        {
            double leftPatterns = this.leftKeyPatterns.Count;
            double finishedPatterns = this.patternResults.Count;
            double patternsInProcess = this.allocatedPatterns.Count;
            double patternAmount = leftPatterns + finishedPatterns + patternsInProcess;
            double patternProgressInPercent;
            if (finishedPatterns > 0 && patternsInProcess > 0)
                patternProgressInPercent = 30 * (patternsInProcess / patternAmount) + 100 * (finishedPatterns / patternAmount);
            else if (patternsInProcess > 0)
                patternProgressInPercent = 30 * (patternsInProcess / patternAmount);
            else if (finishedPatterns > 0)
                patternProgressInPercent = 100 * (finishedPatterns / patternAmount);
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
        public bool StartManager(string sTopic, long aliveMessageInterval, KeyPattern keyPattern, CryptoolStream encryptedData, byte[] initVector, int keyPatternSize)
        {
            this.KeyPatternPartSize = (BigInteger)(keyPatternSize * 100000);
            return StartManager(sTopic, aliveMessageInterval, keyPattern,encryptedData,initVector);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sTopic"></param>
        /// <param name="aliveMessageInterval"></param>
        /// <param name="keyPattern">Already initialized (!!!) key pattern</param>
        /// <param name="encryptedData">The OutputStream from an Encryption PlugIn</param>
        /// <param name="initVector">The initialization vector, which the Encryption PlugIn had used</param>
        /// <returns></returns>
        public bool StartManager(string sTopic, long aliveMessageInterval, KeyPattern keyPattern, CryptoolStream encryptedData, byte[] initVector)
        {
            this.sTopic = sTopic;

            bool bolStartResult = base.Start(sTopic, aliveMessageInterval);

            GuiLogging("Begin building a KeyPatternPool with KeyPatternPartSize " + this.KeyPatternPartSize.ToString(), NotificationLevel.Debug);

            
            /* TODO: Implement Stack instead of Queue later
             * At present: workaround this shit */

            //List<KeyPattern> arrKeyPatternPool = keyPattern.makeKeySearcherPool(this.KeyPatternPartSize);
            //GuiLogging("Enqueue " + arrKeyPatternPool.Count + " KeyPattern-Parts to the JobList.", NotificationLevel.Debug);
            //foreach (KeyPattern keyPatternPart in arrKeyPatternPool)
            //{
            //    this.leftKeyPatterns.Enqueue(keyPatternPart);
            //}

            leftKeyPatterns = keyPattern.makeKeySearcherPool(this.keyPatternPartSize);
            GuiLogging("Enqueue " + leftKeyPatterns.Count + " KeyPattern-Parts to the JobList.", NotificationLevel.Debug);
            //int keyCount = keyPatternPool.Count;
            //for (int j = 0; j < keyCount; j++)
            //{
            //    KeyPattern newPattern = keyPatternPool.Pop();
            //    if (newPattern != null)
            //        this.leftKeyPatterns.Enqueue(newPattern);
            //    else
            //        break;
            //}
            
            /* ************************************** */
            /* BEGIN: Only for debugging reasons - to delete */      
            //StreamWriter debugWriter = DebugToFile.GetDebugStreamWriter();
            //int i = 0;
            //foreach (KeyPattern patternPart in this.leftKeyPatterns.ToList<KeyPattern>())
            //{
            //    debugWriter.WriteLine(i + "\t" + patternPart.ToString());
            //    i++;
            //}
            //debugWriter.Dispose();

            /* END: Only for debugging reasons - to delete */
            /* ************************************** */


            #region Storing EncryptedData and InitializationVector in DHT
            bool encryptedTextStored = DHT_CommonManagement.SetEncryptedData(ref this.p2pControl, sTopic, encryptedData);
            bool initVectorStored = DHT_CommonManagement.SetInitializationVector(ref this.p2pControl, sTopic, initVector);

            //CryptoolStream newEncryptedData = new CryptoolStream();
            //newEncryptedData.OpenRead(encryptedData.FileName);
            //if (newEncryptedData.CanRead)
            //{
            //    // Convert CryptoolStream to an byte Array and store it in the DHT
            //    if (newEncryptedData.Length > Int32.MaxValue)
            //        throw (new Exception("Encrypted Data are too long for this PlugIn. The maximum size of Data is " + Int32.MaxValue + "!"));
            //    byte[] byteEncryptedData = new byte[newEncryptedData.Length];
            //    int k = newEncryptedData.Read(byteEncryptedData, 0, (int)newEncryptedData.Length - 1);
            //    encryptedTextStored = this.p2pControl.DHTstore(sTopic + DHT_ENCRYPTED_TEXT, byteEncryptedData);
            //}
            //else
            //{
            //    GuiLogging("Reading CryptoolStream wasn't possible.", NotificationLevel.Error);
            //    return false;
            //}
            //// Convert  to an byte Array and store Initialization Vector it in the DHT
            //initVectorStored = this.p2pControl.DHTstore(sTopic + DHT_INIT_VECTOR, initVector);

            GuiLogging("DHT: Encrypted data stored: '" + encryptedTextStored + "', Initialization vector stored: '" + initVectorStored + "'", NotificationLevel.Debug);

            #endregion

            this.ManagerStarted = true;

            GetProgressInformation();

            return bolStartResult;
        }

        public override void Stop(PubSubMessageType msgType)
        {
            base.Stop(msgType);
            // Remove all Manager specific DHT entries
            bool deletedMngrDHT = DHT_CommonManagement.DeleteAllManagersEntries(ref this.p2pControl, this.sTopic);

            if (deletedMngrDHT)
                GuiLogging("Removed all Manager specific DHT entries", NotificationLevel.Debug);
            else
                GuiLogging("WARNING: The Manager specific DHT entries weren't removed", NotificationLevel.Debug);

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

        #endregion

        #region WPF-Display stuff

        public LinkedList<KeySearcher.KeySearcher.ValueKey> GetGlobalTop10List()
        {
            return this.globalTopList;
        }
        public int LeftPatterns 
        {
            get { return this.leftKeyPatterns.Count; } 
        }
        public int FinishedPatterns 
        {
            get { return this.patternResults.Count; }
        }
        public int PatternsInProcess
        {
            get { return this.allocatedPatterns.Count; }
        }
        public int PatternAmount
        {
            get { return LeftPatterns + FinishedPatterns + PatternsInProcess; }
        }

        public int FreeWorkers
        {
            get { return ((WorkersManagement)this.peerManagement).GetFreeWorkersAmount();  }
        }

        public int BusyWorkers
        {
            get { return ((WorkersManagement)this.peerManagement).GetBusyWorkersAmount(); }
        }

        #endregion
    }
}
