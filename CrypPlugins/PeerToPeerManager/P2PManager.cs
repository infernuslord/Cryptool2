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
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using KeySearcher;

/*
 * TODO: 
 * - derzeit dasselbe wie PUBLISHER - umformen zum Manager!!! (arbeitsteilung)
 */

namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// This PlugIn only works, when its connected with a P2P_Peer object.
    /// </summary>
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo(false, "P2P_Manager", "Creates a new Manager-Peer", "", "PeerToPeerManager/manager_medium_neutral.png", "PeerToPeerManager/manager_medium_working.png", "PeerToPeerManager/manager_medium_finished.png")]
    public class P2PManager : IOutput
    {
        private P2PManagerBase p2pManager;
        private P2PManagerSettings settings;
        // IControls
        private IControlEncryption encryptionControl;
        private IP2PControl p2pControl;

        private KeyPattern pattern = null;
        public KeyPattern Pattern
        {
            get
            {
                return pattern;
            }
            set
            {
                pattern = value;
                if ((settings.Key == null) || ((settings.Key != null) && !pattern.testWildcardKey(settings.Key)))
                    settings.Key = pattern.giveInputPattern();
            }
        }

        #region In and Output
        [PropertyInfo(Direction.ControlMaster, "Control Master", "Used for bruteforcing", "", DisplayLevel.Beginner)]
        public IControlEncryption EncryptionControl
        {
            get { return this.encryptionControl; }
            set
            {
                if (this.encryptionControl != null)
                {
                    this.encryptionControl.keyPatternChanged -= encryptionControl_keyPatternChanged;
                    this.encryptionControl.OnStatusChanged -= encryptionControl_onStatusChanged;
                }
                if (value != null)
                {
                    Pattern = new KeyPattern(value.getKeyPattern());
                    value.keyPatternChanged += encryptionControl_keyPatternChanged;
                    value.OnStatusChanged += encryptionControl_onStatusChanged;
                    this.encryptionControl = value;
                    OnPropertyChanged("ControlMaster");

                }
                else
                    this.encryptionControl = null;
            }
        }

        private void encryptionControl_keyPatternChanged()
        {
            Pattern = new KeyPattern(this.encryptionControl.getKeyPattern());
        }
        private void encryptionControl_onStatusChanged(IControl sender, bool readyForExecution)
        {
            if (readyForExecution)
            {
                this.process((IControlEncryption)sender);
            }
        }

        /// <summary>
        /// Catches the completely configurated, initialized and joined P2P object from the P2PPeer-Slave-PlugIn.
        /// </summary>
        [PropertyInfo(Direction.ControlMaster,"P2P Slave","Input the P2P-Peer-PlugIn","",true,false,DisplayLevel.Beginner,QuickWatchFormat.Text,null)]
        public IP2PControl P2PControl 
        {
            get
            {
                return this.p2pControl;
            }
            set
            {
                if (this.p2pControl != null)
                {
                    this.p2pControl.OnStatusChanged -= P2PControl_OnStatusChanged;
                }
                if (value != null)
                {
                    this.p2pControl = (P2PPeerMaster)value;
                    this.p2pControl.OnStatusChanged += new IControlStatusChangedEventHandler(P2PControl_OnStatusChanged);
                    OnPropertyChanged("P2PMaster");
                }
                else
                {
                    this.p2pControl = null;
                }
            }
        }

        private void P2PControl_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            //throw new NotImplementedException();
        }

        private string sOutputvalue;
        [PropertyInfo(Direction.OutputData, "Managing Information", "Displays all distribution information", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
        public string Outputvalue
        {
            get
            {
                return this.sOutputvalue;
            }
            set
            {
                this.sOutputvalue = value;
                OnPropertyChanged("Outputvalue");
            }
        }

        #endregion

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region Standard PlugIn-Functionality

        public P2PManager()
        {
            this.settings = new P2PManagerSettings(this);
            this.settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            this.settings.TaskPaneAttributeChanged += new TaskPaneAttributeChangedHandler(settings_TaskPaneAttributeChanged);
            this.settings.OnPluginStatusChanged += new StatusChangedEventHandler(settings_OnPluginStatusChanged);
        }

        void settings_OnPluginStatusChanged(IPlugin sender, StatusEventArgs args)
        {
            if (OnPluginStatusChanged != null)
                OnPluginStatusChanged(this, args);
        }

        void settings_TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // storing settings for subscribers in the DHT, so they can load them there
            //if (e.PropertyName == "SendAliveMessageInterval")
            //{
            //    this.p2pControl.DHTstore(settings.TopicName + this.sDHTSettingsPostfix, 
            //        System.BitConverter.GetBytes(this.settings.SendAliveMessageInterval));
            //}
            // if TaskName has changed, clear the Lists, because the Subscribers must reconfirm registering
            if (e.PropertyName == "TopicName")
            {
                GuiLogMessage("Topic Name has changed, so all subscribers must reconfirm registering!",NotificationLevel.Warning);
                // stop active publisher and tell all subscribers that topic name isn't valid anymore
                this.p2pManager.Stop(PubSubMessageType.Unregister);
                // start publisher for the changed topic
                this.p2pManager.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval);
            }
            if (e.PropertyName == "BtnUnregister")
            {
                this.p2pManager.Stop(PubSubMessageType.Unregister);
                GuiLogMessage("Unregister button pressed, Publisher has stopped!", NotificationLevel.Info);
            }
            if (e.PropertyName == "BtnRegister")
            {
                this.p2pManager.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval);
                GuiLogMessage("Register button pressed, Publisher has been started!", NotificationLevel.Info);
            }
            if (e.PropertyName == "BtnSolutionFound")
            {
                this.p2pManager.Stop(PubSubMessageType.Solution);
                GuiLogMessage("TEST: Emulate Solution-Found-message",NotificationLevel.Info);
            }
        }

        public ISettings Settings
        {
            set { this.settings = (P2PManagerSettings)value; }
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
        // Pre-Execute Method is below this region

        // Execute-Method is below this region

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

            if (this.p2pManager != null && this.p2pManager.Started)
            {
                this.p2pManager.Stop(PubSubMessageType.Unregister);
                this.settings.MngStatusChanged(P2PManagerSettings.MngStatus.Neutral);
            }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        public void PreExecution()
        {
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PControl == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }

            if (this.p2pManager == null)
            {
                this.p2pManager = new P2PManagerBase(this.P2PControl);
                this.p2pManager.OnGuiMessage += new P2PPublisherBase.GuiMessage(p2pManager_OnGuiMessage);
                this.p2pManager.OnFinishedOnePattern += new P2PManagerBase.FinishedOnePattern(p2pManager_OnFinishedOnePattern);
                this.p2pManager.OnFinishedDistributingPatterns += new P2PManagerBase.FinishedDistributingPatterns(p2pManager_OnFinishedDistributingPatterns);
                //this.p2pManager.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval);
            }
        }

        void p2pManager_OnFinishedOnePattern(string wildCardKey, double firstCoeffResult, string firstKeyResult, string workerId)
        {
            if (this.Outputvalue != null)
                this.Outputvalue += "\n";
            this.Outputvalue += "Value: " + firstCoeffResult.ToString() + "\nKey: " + firstKeyResult + "\nFrom: " + workerId;
        }

        void p2pManager_OnFinishedDistributingPatterns(LinkedList<KeySearcher.KeySearcher.ValueKey> lstTopList)
        {
            this.settings.MngStatusChanged(P2PManagerSettings.MngStatus.Finished);

            this.Outputvalue = this.p2pManager.GetGlobalTopList(lstTopList);

            //this.Outputvalue = "All Patterns were bruteforced:";
        }

        void p2pManager_OnGuiMessage(string sData, NotificationLevel notificationLevel)
        {
            GuiLogMessage(sData, notificationLevel);
        }

        public void Execute()
        {
        }

        private void process(IControlEncryption iControlEncryption)
        {
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PControl == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }

            string pattern = iControlEncryption.getKeyPattern();
            GuiLogMessage("string pattern = Encrypt.GetKeyPattern() = '" + pattern + "'", NotificationLevel.Debug);
            KeyPattern kp = new KeyPattern(pattern);

            if (this.settings.Key != null)
            {
                if (!kp.testWildcardKey(this.settings.Key))
                {
                    GuiLogMessage("The input key pattern isn't valid! Key: '" + this.settings.Key + "'", NotificationLevel.Error);
                    return;
                }
                else
                {
                    kp.WildcardKey = this.settings.Key;
                    GuiLogMessage("Key Pattern was set out of the settings! Key: '" + kp.getKey() + "'", NotificationLevel.Debug);
                }
            }
            else //no key was set in settings, so choose a standard key
            {
                /*Begin Testspace*/
                int len = pattern.ToString().Length;

                if (len == 271) //AES
                    kp.WildcardKey = "30-30-30-30-30-30-30-30-30-30-30-30-30-**-**-**";
                else if (len == 135) //DES
                    kp.WildcardKey = "30-30-30-**-**-**-**-**";
                else
                    throw (new Exception("Encryption Type not supported"));
                GuiLogMessage("STANDARD Key Pattern was set! Key: '" + kp.getKey() + "'", NotificationLevel.Debug);
            }

            //this.p2pManager.StartManager(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval, kp);
            this.p2pManager.StartManager(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval, kp, this.settings.KeyPatternSize);

            this.settings.MngStatusChanged(P2PManagerSettings.MngStatus.Working);
            /*End Testspace*/
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
