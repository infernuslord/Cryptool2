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
    [PluginInfo("Cryptool.Plugins.PeerToPeer.Properties.Resources", true, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "PeerToPeerSubscriber/ct2_p2p_sub_medium.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class P2PSubscriber : ICrypComponent
    {
        private P2PSubscriberSettings settings;
        private IP2PControl p2pControl;
        private P2PSubscriberBase p2pSubscriber;

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

        private string sOutputvalue;
        [PropertyInfo(Direction.OutputData, "OutputvalueCaption", "OutputvalueTooltip", "", true, false, QuickWatchFormat.Text, null)]
        public string Outputvalue
        {
            get
            {
                return this.sOutputvalue;
            }
            set
            {
                if (value != this.sOutputvalue)
                {
                    this.sOutputvalue = value;
                    OnPropertyChanged("Outputvalue");
                }
            }
        }

        #endregion

        private void P2PControl_OnStatusChanged(IControl sender, bool readyForExecution)
        {
            //throw new NotImplementedException();
        }

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region Standard functionality

        public P2PSubscriber()
        {
            this.settings = new P2PSubscriberSettings(this);
            this.settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            this.settings.TaskPaneAttributeChanged += new TaskPaneAttributeChangedHandler(settings_TaskPaneAttributeChanged);
        }

        void settings_TaskPaneAttributeChanged(ISettings settings, TaskPaneAttributeChangedEventArgs args)
        {
            // throw new NotImplementedException();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BtnUnregister")
            {
                if (this.p2pSubscriber != null)
                {
                    this.p2pSubscriber.Stop(PubSubMessageType.Unregister);
                    GuiLogMessage("Subscriber unregistered from Publisher!", NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Publisher isn't initialized, so this action isn't possible.", NotificationLevel.Info);
                }
            }
            if (e.PropertyName == "BtnRegister")
            {
                StartSubscriber();
                GuiLogMessage("Subscriber registers with Publisher!", NotificationLevel.Info);
            }
            if (e.PropertyName == "BtnSolutionFound")
            {
                if (this.p2pSubscriber != null)
                {
                    this.p2pSubscriber.SolutionFound(new byte[]{0});
                    GuiLogMessage("Solution found message sent to Publisher.", NotificationLevel.Info);
                }
                else
                {
                    GuiLogMessage("Publisher isn't initialized, so this action isn't possible.", NotificationLevel.Info);
                }
            }
        }

        public ISettings Settings
        {
            set { this.settings = (P2PSubscriberSettings)value; }
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

        //execution method is below this region

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
            //throw new NotImplementedException();
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            if(this.p2pSubscriber != null)
                this.p2pSubscriber.Stop(PubSubMessageType.Stop);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
        #endregion

        public void Execute()
        {
            // if no P2P Slave PlugIn is connected with this PlugIn --> No execution!
            if (P2PControl == null)
            {
                GuiLogMessage("No P2P_Peer connected with this PlugIn!", NotificationLevel.Error);
                return;
            }
            if (this.settings.TopicName != null)
            {
                StartSubscriber();
            }
            else
            {
                GuiLogMessage("The settings are empty. Operation isn't possible.", NotificationLevel.Error);
            }
        }

        private void StartSubscriber()
        {
            if (this.p2pSubscriber == null)
            {
                this.p2pSubscriber = new P2PSubscriberBase(this.P2PControl);
                this.p2pSubscriber.OnGuiMessage += new P2PSubscriberBase.GuiMessage(p2pSubscriber_OnGuiMessage);
                this.p2pSubscriber.OnTextArrivedFromPublisher += new P2PSubscriberBase.TextArrivedFromPublisher(p2pSubscriber_OnTextArrivedFromPublisher);
            }
            this.p2pSubscriber.Start(this.settings.TopicName, (long)(this.settings.CheckPublishersAvailability * 1000),
                    (long)(this.settings.PublishersReplyTimespan * 1000));
        }

        void p2pSubscriber_OnTextArrivedFromPublisher(byte[] data, PeerId pid)
        {
            this.Outputvalue = UTF8Encoding.UTF8.GetString(data);
        }

        void p2pSubscriber_OnGuiMessage(string sData, NotificationLevel notificationLevel)
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