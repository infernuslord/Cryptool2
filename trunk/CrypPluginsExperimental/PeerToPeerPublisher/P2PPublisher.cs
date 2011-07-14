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
using Cryptool.Plugins.PeerToPeer.Internal;

namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// This PlugIn only works, when its connected with a P2P_Peer object.
    /// </summary>
    [Author("Christian Arnold", "arnold@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("Cryptool.Plugins.PeerToPeer.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "PeerToPeerPublisher/ct2_p2p_pub_medium.png")]
    [ComponentCategory(ComponentCategory.ToolsP2P)]
    public class P2PPublisher : ICrypComponent
    {
        private P2PPublisherSettings settings;
        private IP2PControl p2pControl;
        private P2PPublisherBase p2pPublisher;

        public P2PPublisher()
        {
            this.settings = new P2PPublisherSettings(this);
            this.settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            this.settings.TaskPaneAttributeChanged += new TaskPaneAttributeChangedHandler(settings_TaskPaneAttributeChanged);
        }

        #region SettingEvents

        private void settings_TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.p2pControl == null || this.p2pPublisher == null)
                return;

            // storing settings for subscribers in the DHT, so they can load them there
            if (e.PropertyName == "SendAliveMessageInterval")
            {
                this.p2pControl.DHTstore(settings.TopicName + "AliveMsg",
                    System.BitConverter.GetBytes(this.settings.SendAliveMessageInterval * 1000));
            }
            // if TaskName has changed, clear the Lists, because the Subscribers must reconfirm registering
            if (e.PropertyName == "TopicName")
            {
                GuiLogMessage("Topic Name has changed, so all subscribers must reconfirm registering!", NotificationLevel.Warning);
                // stop active publisher and tell all subscribers that topic name isn't valid anymore
                this.p2pPublisher.Stop(PubSubMessageType.Unregister);
                // start publisher for the changed topic
                this.p2pPublisher.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval);
            }
            if (e.PropertyName == "BtnUnregister")
            {
                if (this.p2pPublisher.Started)
                {
                    this.p2pPublisher.Stop(PubSubMessageType.Unregister);
                    GuiLogMessage("Unregister button pressed, Publisher has stopped!", NotificationLevel.Info);
                }
            }
            if (e.PropertyName == "BtnRegister")
            {
                if (!this.p2pPublisher.Started)
                {
                    this.p2pPublisher.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval);
                    GuiLogMessage("Register button pressed, Publisher has been started!", NotificationLevel.Info);
                }
            }
            if (e.PropertyName == "BtnSolutionFound")
            {
                if (this.p2pPublisher.Started)
                {
                    this.p2pPublisher.Stop(PubSubMessageType.Solution);
                    GuiLogMessage("TEST: Emulate Solution-Found-message", NotificationLevel.Info);
                }
            }
            if (e.PropertyName == "BtnSerDeser")
            {
                this.p2pPublisher.TestSerialization();
            }
        }

        #endregion

        #region In and Output

        /// <summary>
        /// Catches the completely configurated, initialized and joined P2P object from the P2PPeer-Slave-PlugIn.
        /// </summary>
        [PropertyInfo(Direction.ControlMaster, "P2PControlCaption", "P2PControlTooltip", "", true, false, QuickWatchFormat.Text, null)]
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
                    this.p2pControl = value;
                    this.p2pControl.OnStatusChanged += new IControlStatusChangedEventHandler(P2PControl_OnStatusChanged);
                    OnPropertyChanged("P2PControl");
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

        private string sInputvalue;
        [PropertyInfo(Direction.InputData, "InputvalueCaption", "InputvalueTooltip", "", true, false, QuickWatchFormat.Text, null)]
        public string Inputvalue
        {
            get
            {
                return this.sInputvalue;
            }
            set
            {
                this.sInputvalue = value;
                OnPropertyChanged("Inputvalue");
            }
        }

        #endregion

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region Standard PlugIn-Functionality

        public ISettings Settings
        {
            set { this.settings = (P2PPublisherSettings)value; }
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
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PControl == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }

            // when Workspace has stopped and has been started again
            if (this.p2pPublisher != null && !this.p2pPublisher.Started)
            {
                this.p2pPublisher.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval * 1000);
            }

            if (this.p2pPublisher == null)
            {
                this.p2pPublisher = new P2PPublisherBase(this.P2PControl);
                this.p2pPublisher.OnGuiMessage += new P2PPublisherBase.GuiMessage(p2pPublisher_OnGuiMessage);
                this.p2pPublisher.Start(this.settings.TopicName, (long)this.settings.SendAliveMessageInterval * 1000);
            }
        }

        public void Execute()
        {
            if (this.settings == null && this.settings.TopicName == null)
            {
                GuiLogMessage("There is no input and/or empty Settings. Storing isn't possible.", NotificationLevel.Error);
                return;
            }

            if (this.Inputvalue != null)
            {
                this.p2pPublisher.Publish(this.Inputvalue);
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
            if(this.p2pPublisher != null && this.p2pPublisher.Started)
                this.p2pPublisher.Stop(PubSubMessageType.Unregister);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            if (this.p2pPublisher != null && this.p2pPublisher.Started)
            {
                this.p2pPublisher.Stop(PubSubMessageType.Unregister);
            }
        }

        #endregion

        private void p2pPublisher_OnGuiMessage(string sData, NotificationLevel notificationLevel)
        {
            GuiLogMessage(sData, notificationLevel);
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
