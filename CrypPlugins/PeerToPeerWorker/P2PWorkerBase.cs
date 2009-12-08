using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using System.Threading;
using KeySearcher;

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
        /// <summary>
        /// only goal: validate incoming KeyPatterns
        /// </summary>
        private KeyPattern pattern;
        private IControlKeySearcher keySearcherControl;
        private IControlCost costControl;
        private IControlEncryption encryptControl;

        #endregion

        public P2PWorkerBase(IP2PControl p2pControl, IControlKeySearcher keySearcherControl)
            : base(p2pControl)
        {
            this.keySearcherControl = keySearcherControl;
            this.keySearcherControl.OnEndedBruteforcing += new KeySearcher.KeySearcher.BruteforcingEnded(keySearcherControl_OnEndedBruteforcing);
            this.encryptControl = this.keySearcherControl.GetEncyptionControl();
            this.costControl = this.keySearcherControl.GetCostControl();
            if (!InitializeTestKeyPattern(encryptControl))
                throw (new Exception("P2PWorkerBase: Encryption Type not supported"));
        }

        void keySearcherControl_OnEndedBruteforcing(LinkedList<KeySearcher.KeySearcher.ValueKey> top10List)
        {
            if (top10List != null)
            {
                KeySearcher.KeySearcher.ValueKey bestResult = top10List.First<KeySearcher.KeySearcher.ValueKey>();
                GuiLogging("Bruteforcing Ended. Best key result: '" + bestResult.key + "'. Coefficient value: "
                    + bestResult.value + ". Decrypted result: '" + UTF8Encoding.UTF8.GetString(bestResult.decryption)
                    + "'", NotificationLevel.Info);
                // TODO: send the top-10-List to the Manager Peer --> SolutionFound
                SolutionFound(SerializeKeySearcherResult(top10List));
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
        private string SerializeKeySearcherResult(LinkedList<KeySearcher.KeySearcher.ValueKey> top10List)
        {
            StringBuilder sbRet = new StringBuilder();
            foreach (KeySearcher.KeySearcher.ValueKey valKey in top10List)
            {
                sbRet.Append(valKey.value + seperator + valKey.key + seperator + UTF8Encoding.UTF8.GetString(valKey.decryption) + dataSetSeperator);
            }
            string sRet = sbRet.ToString();
            // cut off last dataSetSeperator
            sRet = sRet.Substring(0, sRet.Length - dataSetSeperator.Length);
            return sRet;
        }

        private bool InitializeTestKeyPattern(IControlEncryption encryptControl)
        {
            bool result = false;
            string sPattern = encryptControl.getKeyPattern();
            pattern = new KeyPattern(sPattern);
            int len = sPattern.ToString().Length;

            if (len == (int)KeyPatternLength.AES)
            {
                // "30-30-30-30-30-30-30-30-30-30-30-30-30-**-**-**"
                string sKeyPattern = GetKeyForInit(EncryptionPatternLength.AES, 3);
                this.pattern.WildcardKey = sKeyPattern;
                result = true;
            }
            else if (len == (int)KeyPatternLength.DES)
            {
                // "30-30-30-30-30-**-**-**"
                string sKeyPattern = GetKeyForInit(EncryptionPatternLength.DES, 3);
                this.pattern.WildcardKey = sKeyPattern;
                result = true;
            }
            return result;
        }

        protected override void HandleIncomingData(PeerId senderId, string sData)
        {
            // returns null if the data aren't a valid KeyPattern.
            KeyPattern receivedKeyPattern = pattern.DeserializeFromString(sData);
            if (receivedKeyPattern != null)
            {
                GuiLogging("Starting Bruteforcing the incoming pattern: '" + receivedKeyPattern.ToString() + "'", NotificationLevel.Info);

                if (OnKeyPatternReceived != null)
                    OnKeyPatternReceived(receivedKeyPattern);
                // Commit pattern to the KeySearcherControl and wait for result(s)
                this.keySearcherControl.StartBruteforcing(receivedKeyPattern);
                // to display the incoming KeyPattern in the OutputText
                base.HandleIncomingData(senderId, sData);
            }
            else
            {
                base.HandleIncomingData(senderId, sData);
            }
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
    }
}
