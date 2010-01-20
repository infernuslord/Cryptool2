using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using System.Threading;
using KeySearcher;

/* Nice to have:
 * - Enable sending intermediate results in a user-defined interval */

namespace Cryptool.Plugins.PeerToPeer
{
    public enum EncryptionPatternLength
    {
        AES = 16,
        //SDES = x,
        DES = 8
    }

    public enum KeyPatternLength
    {
        AES = 271,
        //SDES = x,
        DES = 135
    }

    public class P2PWorkerBase : P2PSubscriberBase
    {
        #region events and variables

        // Events
        public delegate void KeyPatternReceived(KeyPattern pattern);
        /// <summary>
        /// Every time when this worker receives a new KeyPattern from the Manager, this event will be fired.
        /// </summary>
        public event KeyPatternReceived OnKeyPatternReceived;
        public delegate void FinishedBruteforcingThePattern(KeyPattern pattern);
        /// <summary>
        /// fired when bruteforcing the actual pattern - assigned from the Manager - was finisher. 
        /// Attention: No indication that the worker doesn't have to bruteforce any other assigned Patterns!
        /// </summary>
        public event FinishedBruteforcingThePattern OnFinishedBruteforcingThePattern;

        // Variables
        private string sTopicName = String.Empty;
        /// <summary>
        /// only goal: validate incoming KeyPatterns
        /// </summary>
        private KeyPattern patternForValidateIncomingPatterns;
        /// <summary>
        /// this value is always the actual pattern, which is in progress
        /// </summary>
        private KeyPattern actualProcessingPattern;
        private IControlKeySearcher keySearcherControl;
        private IControlCost costControl;
        private IControlEncryption encryptControl;
        /// <summary>
        /// Flag to check if this worker is currently working on a Pattern.
        /// So you must wait till current job is finished.
        /// </summary>
        private bool currentlyWorking = false;
        /// <summary>
        /// waiting job Queue, if jobs received, but the worker is still working on another job
        /// </summary>
        private Queue<KeyPattern> waitingJobList;

        #endregion

        public P2PWorkerBase(IP2PControl p2pControl, IControlKeySearcher keySearcherControl, string sTopicName)
            : base(p2pControl)
        {
            this.sTopicName = sTopicName;
            this.keySearcherControl = keySearcherControl;
            this.keySearcherControl.OnEndedBruteforcing += new KeySearcher.KeySearcher.BruteforcingEnded(keySearcherControl_OnEndedBruteforcing);
            this.encryptControl = this.keySearcherControl.GetEncyptionControl();
            this.costControl = this.keySearcherControl.GetCostControl();
            if (!InitializeTestKeyPattern(encryptControl))
                throw (new Exception("P2PWorkerBase: Encryption Type not supported"));
            waitingJobList = new Queue<KeyPattern>();
        }

        #region Only for testing reasons (Build a TestPattern to check the validity of the incoming KeyPattern)

        private bool InitializeTestKeyPattern(IControlEncryption encryptControl)
        {
            bool result = false;
            string sPattern = encryptControl.getKeyPattern();
            patternForValidateIncomingPatterns = new KeyPattern(sPattern);
            int len = sPattern.ToString().Length;

            if (len == (int)KeyPatternLength.AES)
            {
                // "30-30-30-30-30-30-30-30-30-30-30-30-30-**-**-**"
                string sKeyPattern = GetKeyForInit(EncryptionPatternLength.AES, 3);
                this.patternForValidateIncomingPatterns.WildcardKey = sKeyPattern;
                result = true;
            }
            else if (len == (int)KeyPatternLength.DES)
            {
                // "30-30-30-30-30-**-**-**"
                string sKeyPattern = GetKeyForInit(EncryptionPatternLength.DES, 3);
                this.patternForValidateIncomingPatterns.WildcardKey = sKeyPattern;
                result = true;
            }
            return result;
        }

        private string GetKeyForInit(EncryptionPatternLength encryptLen, int wildCardAmount)
        {
            int iEncryptLen = (int)encryptLen;

            if (iEncryptLen < wildCardAmount)
                return null;

            StringBuilder sbRet = new StringBuilder("30");
            for (int i = 0; i < iEncryptLen - wildCardAmount - 1; i++)
            {
                sbRet.Append("-30");
            }
            for (int i = 0; i < wildCardAmount; i++)
            {
                sbRet.Append("-**");
            }
            return sbRet.ToString();
        }

        #endregion

        /// <summary>
        /// Checks whether the incoming data are a valid KeyPattern object. Then it checks
        /// if the worker is still working currently - than enqueue new Pattern to waitingJobList.
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="sData"></param>
        protected override void HandleIncomingData(PeerId senderId, byte[] data)
        {
            // returns null if the data aren't a valid KeyPattern.
            KeyPattern receivedKeyPattern = patternForValidateIncomingPatterns.Deserialize(data);
            if (receivedKeyPattern != null)
            {
                /* only one Pattern can be bruteforced concurrently, 
                 * so other incoming Patterns will be admitted to the 
                 * waitingJobList, will be processed immediately at
                 * the end of the current bruteforce-Job */
                if (this.currentlyWorking)
                {
                    this.waitingJobList.Enqueue(receivedKeyPattern);
                }
                else
                {
                    Thread processingThread = new Thread(new ParameterizedThreadStart(this.StartProcessing));
                    processingThread.Start(receivedKeyPattern);
                    //StartProcessing(receivedKeyPattern);
                }
            }
            else
            {
                base.HandleIncomingData(senderId, data);
            }
        }

        // necessary, because Thread-starting doesn't allow other parameters than object
        private void StartProcessing(object receivedKeyPattern)
        {
            if (receivedKeyPattern is KeyPattern)
                StartProcessing(receivedKeyPattern as KeyPattern);
        }

        /// <summary>
        /// Main method for processing a new job. Started in own thread!
        /// </summary>
        /// <param name="receivedKeyPattern">a valid and full initialized KeyPattern</param>
        private void StartProcessing(KeyPattern receivedKeyPattern)
        {
            GuiLogging("Start Bruteforcing the incoming Key: '" + receivedKeyPattern.getKey() + "', WildCardKey: '" + receivedKeyPattern.WildcardKey + "'", NotificationLevel.Info);

            if (OnKeyPatternReceived != null)
                OnKeyPatternReceived(receivedKeyPattern);
            this.currentlyWorking = true;
            // to access this pattern in every method, where it's meaningful for information or processing reasons
            this.actualProcessingPattern = receivedKeyPattern;

            /* Begin: New stuff because of changing the IControl data flow - Arnie 2010.01.18 */

            GuiLogging("BEGIN: Retrieving encryption information from the DHT.", NotificationLevel.Debug);
            byte[] encryptedData = DHT_CommonManagement.GetEncryptedData(ref this.p2pControl, sTopicName);
            byte[] initVector = DHT_CommonManagement.GetInitializationVector(ref this.p2pControl, sTopicName);
            GuiLogging("END: Retrieving encryption information from the DHT.", NotificationLevel.Debug);

            /* End: New stuff because of changing the IControl data flow */

            // Commit pattern to the KeySearcherControl and wait for result(s)
            this.keySearcherControl.StartBruteforcing(receivedKeyPattern, encryptedData, initVector);
        }

        private void keySearcherControl_OnEndedBruteforcing(LinkedList<KeySearcher.KeySearcher.ValueKey> top10List)
        {
            if (top10List != null)
            {
                KeySearcher.KeySearcher.ValueKey bestResult = top10List.First<KeySearcher.KeySearcher.ValueKey>();
                GuiLogging("Bruteforcing Ended. Best key result: '" + bestResult.key + "'. Coefficient value: "
                    + bestResult.value.ToString() + ". Decrypted result: '" + UTF8Encoding.UTF8.GetString(bestResult.decryption)
                    + "'", NotificationLevel.Info);

                KeySearcherResult keySearcherResult = new KeySearcherResult();
                SolutionFound(keySearcherResult.SerializeResult(top10List));

                if (OnFinishedBruteforcingThePattern != null)
                    OnFinishedBruteforcingThePattern(this.actualProcessingPattern);

                // because bruteforcing the actual pattern was successful, delete information
                this.actualProcessingPattern = null;
                GuiLogging("Serialized result list sended to Managing-Peer.", NotificationLevel.Debug);
            }
            // if there are any Jobs in the waiting list, process them now successively
            if (this.waitingJobList.Count > 0)
            {
                StartProcessing(this.waitingJobList.Dequeue());
            }
            else
                // set flag to false, because bruteforcing has been finished
                this.currentlyWorking = false;
        }
    }
}