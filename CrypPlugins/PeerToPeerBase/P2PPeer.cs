/* Copyright 2009 Team CrypTool (Christian Arnold), Uni Duisburg-Essen

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
using System.Threading;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.PluginBase.IO;

namespace Cryptool.Plugins.PeerToPeer
{
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "P2P_Peer", "Creates a new Peer", "", "PeerToPeerBase/ct2_p2p_load_medium.png")]
    public class P2PPeer : IIOMisc
    {
        #region Variables

        /// <summary>
        /// dirty workaround!!!
        /// </summary>
        private static bool bolFirstInitalisation = true;
        private P2PPeerSettings settings;
        private P2PBase p2pBase;

        #endregion

        public P2PPeer()
        {
            this.p2pBase = new P2PBase();
            this.settings = new P2PPeerSettings(p2pBase);
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            set { this.settings = (P2PPeerSettings)value; }
            get { return this.settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public System.Windows.Controls.UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            // nicht in PreExecute möglich, da schon beim Laden von CT2 gefeuert. Input-Data werden erst NACH PreExecute gesetzt!
            if (bolFirstInitalisation)
            {
                bolFirstInitalisation = false;
            }
            else
            {
                this.p2pBase.Initialize(settings.P2PPeerName, settings.P2PWorldName);
                this.p2pBase.SynchStart();
            }
        }

        public void Execute()
        {

        }

        public void process(IP2PControl sender)
        {
            GuiLogMessage("P2P Peer method 'process' is executed!", NotificationLevel.Debug);
        }

        public void PostExecution()
        {
        }

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            p2pBase.SynchStop();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PluginProgressChangedEventHandler OnPluginProcessChanged;

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        #endregion

        #region In and Output

        private IP2PControl p2pMaster;
        [PropertyInfo(Direction.ControlMaster, "Master Peer", "One peer to rule them all", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public IP2PControl P2PControlMaster
        {
            get
            {
                if (this.p2pMaster == null)
                    // to commit the settings of the plugin to the IControl
                    this.p2pMaster = new P2PPeerMaster(p2pBase);
                return this.p2pMaster;
            }
            set
            {
                if (this.p2pMaster != null)
                    this.p2pMaster.OnStatusChanged -= p2pMaster_OnStatusChanged;
                //{
                //    //Only when using asynchronous p2p-Start-method, remove event handler for OnPeerJoinedCompletely
                //    this.p2pMaster.onPeerJoinedCompletely -= OnPeerJoinedCompletely;
                //}
                if (this.p2pMaster != value)
                {
                    this.p2pMaster.OnStatusChanged +=new IControlStatusChangedEventHandler(p2pMaster_OnStatusChanged);
                    //Only when using asynchronous p2p-Start-method, add event handler for OnPeerJoinedCompletely
                    //this.p2pMaster.OnPeerJoinedCompletely += new PeerJoinedP2P(OnPeerJoinedCompletely);
                    this.p2pMaster = value;
                    OnPropertyChanged("P2PControlMaster");
                }
            }
        }

        void p2pMaster_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            if (readyForExecution)
                this.process((IP2PControl)sender);
        }

        #endregion
    }

    public class P2PPeerMaster : IP2PControl
    {
        private P2PBase p2pBase;

        public P2PPeerMaster(P2PBase p2pBase)
        {
            this.p2pBase = p2pBase;
        }

        #region IP2PControl Members

        public bool DHTstore(string sKey, string sValue)
        {
            return this.p2pBase.SynchStore(sKey, sValue);
        }

        public byte[] DHTload(string sKey)
        {
            return this.p2pBase.SynchRetrieve(sKey);
        }

        public bool DHTremove(string sKey)
        {
            return this.p2pBase.SynchRemove(sKey);
        }

        public event PeerJoinedP2P OnPeerJoinedCompletely;

        #endregion

        #region IControl Members

        public event IControlStatusChangedEventHandler OnStatusChanged;

        #endregion
    }
}
