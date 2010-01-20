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
using KeySearcher;
using Cryptool.PluginBase.Control;

namespace Cryptool.Plugins.KeySearcher_IControl
{
    [Author("Christian Arnold", "arnold@cryptool.org", "Universität Duisburg-Essen, Fachgebiet Verteilte Systeme", "http://www.vs.uni-due.de")]
    [PluginInfo(false, "Peer-based KeySearcher", "Peer-driven KeySearcher. KeySearcher gets the search pattern from a managing peer.", null, "KeySearcher_IControl/P2PKeySearcher.png")]
    public class KeySearcher_IControl : KeySearcher.KeySearcher, IAnalysisMisc
    {
        private bool readyForExec = false;
        public bool ReadyForExec 
        {
            get { return this.readyForExec; }
            private set { this.readyForExec = value; }
        }

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

        private IControlKeySearcher controlKeySearcher;
        [PropertyInfo(Direction.ControlSlave, "Master KeySearcher", "For distributed bruteforcing", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public IControlKeySearcher ControlKeySearcher
        {
            get
            {
                if (this.controlKeySearcher == null)
                {
                    // to commit the settings of the plugin to the IControl
                    this.controlKeySearcher = new KeySearcherMaster(this);
                }
                return this.controlKeySearcher;
            }
        }

        #endregion

        /*
         * The original KeySearcher starts bruteforcing after the master control IEncryptionControl
         * had fired the event onStatusChanged. Because of repeatedly using this modded KeySearcher
         * we must establish an event which informs the KeySearcherMaster Control, that all other
         * necessary Controls (CostControl, EncryptControl) are already initialized.
         * Otherwise the direct call of bruteforcePattern could run into an Exception, because
         * Encryption Control isn't initialized already.
         */
        //public delegate void IsReadyForExecution();
        //public event IsReadyForExecution OnAllMasterControlsInitialized;

        public override void Execute()
        {
            //if(this.ControlMaster != null)
            //{
            //    if (OnAllMasterControlsInitialized != null) 
            //        OnAllMasterControlsInitialized();
            //}
        }
    }

    public class KeySearcherMaster : IControlKeySearcher
    {
        private KeySearcher_IControl keySearcher;

        public KeySearcherMaster(KeySearcher_IControl keysearcher)
        {
            this.keySearcher = keysearcher;
            this.keySearcher.OnBruteforcingEnded += new KeySearcher.KeySearcher.BruteforcingEnded(keySearcher_OnBruteforcingEnded);
        }

        #region IControlKeySearcher Members

        public IControlEncryption GetEncyptionControl()
        {
            return this.keySearcher.ControlMaster;
        }

        public IControlCost GetCostControl()
        {
            return this.keySearcher.CostMaster;
        }

        public void StartBruteforcing(KeyPattern pattern, byte[] encryptedData, byte[] initVector)
        {
            //because the KeySearcher object uses this property instead of the parameters in some internal methods... Dirty implementation...
            this.keySearcher.Pattern = pattern;
            //necessary, because the Pattern property seems to work incorrect
            this.keySearcher.Pattern.WildcardKey = pattern.WildcardKey;

            //New stuff because of changing the IControl data flow - Arnie 2010.01.18

            this.keySearcher.BruteforcePattern(pattern, encryptedData, initVector, this.keySearcher.ControlMaster, this.keySearcher.CostMaster);
        }

        public event KeySearcher.KeySearcher.BruteforcingEnded OnEndedBruteforcing;
        void keySearcher_OnBruteforcingEnded(LinkedList<KeySearcher.KeySearcher.ValueKey> top10List)
        {
            if (OnEndedBruteforcing != null)
                OnEndedBruteforcing(top10List);
        }

        public void StopBruteforcing()
        {
            //this.keySearcher.OnAllMasterControlsInitialized -= keySearcher_OnAllMasterControlsInitialized;
        }

        #endregion

        #region IControl Members

        public event IControlStatusChangedEventHandler OnStatusChanged;

        #endregion
    }
}
