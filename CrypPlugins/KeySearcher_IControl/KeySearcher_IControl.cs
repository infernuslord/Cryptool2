/*
   Copyright 2009 Christian Arnold, Universität Duisburg-Essen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using Cryptool.PluginBase.Analysis;
// KeyPattern kicked out of this project and will be sourced from the namespace KeySearcher
using Cryptool.PluginBase.Control;
using Cryptool.Plugins.PeerToPeer;
using Cryptool.Plugins.PeerToPeer.Jobs;
using System.Threading;
using System.Numerics;

namespace Cryptool.Plugins.KeySearcher_IControl
{
    [Author("Christian Arnold", "arnold@cryptool.org", "Universität Duisburg-Essen, Fachgebiet Verteilte Systeme", "http://www.vs.uni-due.de")]
    [PluginInfo(false, "Peer-based KeySearcher", "Peer-driven KeySearcher. KeySearcher gets the search pattern from a managing peer.", null, "KeySearcher_IControl/P2PKeySearcher.png")]
    public class KeySearcher_IControl : KeySearcher.KeySearcher, IAnalysisMisc
    {
        //only change: mandatory = false!!!
        [PropertyInfo(Direction.InputData, "Encrypted Data", "Encrypted data out of an Encryption PlugIn", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, "")]
        public override byte[] EncryptedData
        {
            get
            {
                return base.EncryptedData;
            }
            set
            {
                base.EncryptedData = value;
            }
        }

        //only change: mandatory = false!!!
        [PropertyInfo(Direction.InputData, "Initialization Vector", "Initialization vector with which the data were encrypted", "",false,false, DisplayLevel.Beginner,QuickWatchFormat.Hex,"")]
        public override byte[] InitVector
        {
            get
            {
                return base.InitVector;
            }
            set
            {
                base.InitVector = value;
            }
        }

        #region IKeySearcherControl Members

        private IControlWorker controlWorker;
        [PropertyInfo(Direction.ControlSlave, "Master Worker", "For distributed job processing", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public IControlWorker ControlWorker
        {
            get
            {
                if (this.controlWorker == null)
                {
                    // to commit the settings of the plugin to the IControl
                    this.controlWorker = new KeySearcherMaster(this);
                }
                return this.controlWorker;
            }
        }

        #endregion
    }

    public class KeySearcherMaster : IControlWorker
    {
        #region processing and result variables and properties
        
        private BigInteger JobId;
        private DateTime dtStartProcessing;
        private DateTime dtEndProcessing;

        #endregion

        private KeySearcher_IControl keySearcher;

        public KeySearcherMaster(KeySearcher_IControl keysearcher)
        {
            this.keySearcher = keysearcher;
            this.keySearcher.OnBruteforcingEnded += new KeySearcher.KeySearcher.BruteforcingEnded(keySearcher_OnBruteforcingEnded);
        }

        public IControlEncryption GetEncyptionControl()
        {
            return this.keySearcher.ControlMaster;
        }

        public IControlCost GetCostControl()
        {
            return this.keySearcher.CostMaster;
        }

        void keySearcher_OnBruteforcingEnded(LinkedList<KeySearcher.KeySearcher.ValueKey> top10List)
        {
            //wacker: you need to test if BOTH conditions apply, hence &&
            //if (this.JobId != null || top10List != null)
            if (this.JobId != null && top10List != null)
            {
                this.dtEndProcessing = DateTime.Now;
                // Create a new JobResult
                TimeSpan processingTime = this.dtEndProcessing.Subtract(this.dtStartProcessing);
                KeyPatternJobResult jobResult =
                    new KeyPatternJobResult(this.JobId, top10List, processingTime);

                GuiLogging("Ended bruteforcing JobId '" + this.JobId.ToString() + "' in "
                    + processingTime.TotalMinutes.ToString() + " minutes", NotificationLevel.Info);

                // if registered, sending the serialized Job Result
                if (OnProcessingSuccessfullyEnded != null)
                {
                    OnProcessingSuccessfullyEnded(this.JobId, jobResult.Serialize());
                }
            }
            else
            {
                GuiLogging("Bruteforcing was canceled, because jobId and/or jobResult are null.", NotificationLevel.Info);
                if (OnProcessingCanceled != null)
                    OnProcessingCanceled(null);
            }
        }

        #region IControl Members

        public event IControlStatusChangedEventHandler OnStatusChanged;

        #endregion

        #region IControlWorker Members
        public event ProcessingSuccessfullyEnded OnProcessingSuccessfullyEnded;
        public event ProcessingCanceled OnProcessingCanceled;
        public event InfoText OnInfoTextReceived;

        string sTopicName = String.Empty;
        public string TopicName
        {
            get { return this.sTopicName; }
            set { this.sTopicName = value; }
        }

        public bool StartProcessing(byte[] job, out BigInteger jobId)
        {
            jobId = -1; //out parameter
            if (job != null)
            {
                KeyPatternJobPart jobPart = new KeyPatternJobPart(job);
                if (jobPart != null)
                {
                    this.JobId = jobPart.JobId;
                    // fill out parameter
                    jobId = this.JobId;
                    this.dtStartProcessing = DateTime.Now;

                    GuiLogging("Deserializing job with id '" + jobPart.JobId + "' was successful. Start bruteforcing the KeyPattern '" + jobPart.Pattern.WildcardKey + "'", NotificationLevel.Info);

                    // call bruteforcing method in a thread, so this method didn't block the flow
                    Thread bruteforcingThread = new Thread(StartBruteforcing);
                    bruteforcingThread.Start(jobPart);

                    return true;
                }
                else
                {
                    GuiLogging("The received job byte[] wasn't null, but couldn't be deserialized!", NotificationLevel.Warning);
                }
            }
            else
            {
                GuiLogging("Received job byte[] was null. Nothing to do.", NotificationLevel.Warning);
            }
            return false;
        }

        private void StartBruteforcing(object what)
        {
            if (what is KeyPatternJobPart)
            {
                KeyPatternJobPart jobPart = what as KeyPatternJobPart;
                this.keySearcher.BruteforcePattern(jobPart.Pattern, jobPart.EncryptData, jobPart.InitVector,
                            this.keySearcher.ControlMaster, this.keySearcher.CostMaster);
            }
            else
                throw(new Exception("Bruteforcing object wasn't from Type 'KeyPatternJobPart'!"));
        }

        public void StopProcessing()
        {
            this.keySearcher.Stop();
        }

        private void GuiLogging(string sText, NotificationLevel notLevel)
        {
            if (OnInfoTextReceived != null)
                OnInfoTextReceived(sText, notLevel);
        }

        #endregion
    }
}
