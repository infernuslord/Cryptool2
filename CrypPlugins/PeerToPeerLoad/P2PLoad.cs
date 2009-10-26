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
using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using Cryptool.PluginBase.Tool;

namespace Cryptool.Plugins.PeerToPeer
{
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "P2PLoad", "Loading DHT value", "", "PeerToPeerLoad/ct2_p2p_load_medium.png")]
    public class P2PLoad : IThroughput
    {
        #region In and Output

        private string sDhtKey;
        [PropertyInfo(Direction.InputData, "Key Name", "Key Name of DHT Entry in the P2P-System", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string DhtKey
        {
            get
            {
                return this.sDhtKey;
            }
            set
            {
                this.sDhtKey = value;
                OnPropertyChanged("DhtKey");
            }
        }

        private byte[] byteValue;
        [PropertyInfo(Direction.OutputData, "Value Data", "Value Data of DHT Entry in the P2P-System", "",DisplayLevel.Beginner)]
        public byte[] DhtValue
        {
            get
            {
                return this.byteValue;
            }
            set
            {
                this.byteValue = value;
                OnPropertyChanged("DhtValue");
            }
        }

        #endregion
        
        #region Variables

        P2PBase p2pBase;
        private P2PLoadSettings settings;
        /// <summary>
        /// dirty workaround = dirty!!! Because PreExecute is fired once after Starting CT.
        /// </summary>
        private static bool bolFirstInitalisation = true;

        #endregion

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        public P2PLoad()
        {
            this.settings = new P2PLoadSettings(this);
            this.p2pBase = new P2PBase();
        }

        public ISettings Settings
        {
            set { this.settings = (P2PLoadSettings)value; }
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

        /// <summary>
        /// Wird bei Play einmal ausgeführt
        /// </summary>
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
                //p2pBase.Initialize("Manager", "distributedkeysearch");
                this.p2pBase.SynchStart();
            }
        }

        public void Execute()
        {
            if (DhtKey != null)
            {
                // nicht in PreExecute möglich, da schon beim Laden von CT2 gefeuert. Input-Data werden erst NACH PreExecute gesetzt!
                //p2pBase.Initialize(settings.P2PPeerName, settings.P2PWorldName);
                //p2pBase.Start();

                DhtValue = p2pBase.SynchRetrieve(DhtKey);
                if(DhtValue != null)
                    GuiLogMessage("KeyValue of DHT Entry '" + DhtKey + "' retrieved: " + DhtValue.ToString(), NotificationLevel.Info);
                else
                    GuiLogMessage("KeyValue of DHT Entry '" + DhtKey + "' is null. Possible cause: Key doesn't exist", NotificationLevel.Info);
            }
            else
            {
                GuiLogMessage("No key in input", NotificationLevel.Error);
            }
        }

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Pause()
        {
            //throw new NotImplementedException();
        }

        public void Stop()
        {
            //throw new NotImplementedException();
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            this.p2pBase.SynchStop();
        }

        public void LogInternalState() 
        {
            this.p2pBase.LogInternalState();
        }


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
    }
}
