/* Copyright 2010 Team CrypTool (Christian Arnold), Uni Duisburg-Essen

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
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using System.Windows;
using Cryptool.Plugins.PeerToPeer;

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PJobAdminSettings : ISettings
    {
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        private bool hasChanges = false;

        #region ISettings Members

        public bool HasChanges
        {
            get
            {
                return this.hasChanges;
            }
            set
            {
                this.hasChanges = true;
            }
        }

        #endregion

        public P2PJobAdminSettings ()
	    {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            }
	    }

        private string sTopic = "NewTopic";
        [TaskPane("Topic Name", "Choose a topic name with which this subscriber shall be registered.", null, 0, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string TopicName
        {
            get { return this.sTopic; }
            set
            {
                if (this.sTopic != value && value != String.Empty && value != null)
                {
                    this.sTopic = value;
                    HasChanges = true;
                    OnPropertyChanged(TopicName);
                }
            }
        }

        /* FOR TESTING ISSUES */
        [TaskPane("Unregister", "Click here to Unregister the publisher from all registered subscribers!", "Control region", 0, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnUnregister()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            OnPropertyChanged("BtnUnregister");
        }
        [TaskPane("Register", "Click here to Register the publisher pro-active with all formely registered subscribers!", "Control region", 1, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnRegister()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Visible)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Visible)));
            OnPropertyChanged("BtnRegister");
        }
        /* FOR TESTING ISSUES */

        private int checkPublishersAvailability = 240;
        [TaskPane("Check Publisher Interval (in sec)", "To check liveness or possibly changed publishing peer in intervals", "Intervals", 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 20, int.MaxValue)]
        public int CheckPublishersAvailability
        {
            get
            {
                // multiplied with thousand because the interval is denoted in milliseconds!
                return this.checkPublishersAvailability;
            }
            set
            {
                if (value != this.checkPublishersAvailability)
                {
                    this.checkPublishersAvailability = value;
                    OnPropertyChanged("CheckPublishersAvailability");
                }
            }
        }

        private int publishersReplyTimespan = 10;
        [TaskPane("Publisher Reply Timespan (in sec)", "When checking publishers availability, ping message is sent. The publisher must answer with a pong message in the timespan!", "Intervals", 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 60)]
        public int PublishersReplyTimespan
        {
            get
            {
                // multiplied with thousand because the interval is denoted in milliseconds!
                return this.publishersReplyTimespan;
            }
            set
            {
                if (value != this.publishersReplyTimespan)
                {
                    this.publishersReplyTimespan = value;
                    OnPropertyChanged("PublishersReplyTimespan");
                }
            }
        }
        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion

        #region PlugIn-Icon status Stuff

        // Index depends on icon-position in P2PWorker-Class properties
        public enum WorkerStatus
        {
            Neutral = 0,
            Working = 1,
            Finished = 2
            //Error = 3
        }

        /// <summary>
        /// Changes icon of P2PWorker and visibility of the control buttons in settings
        /// </summary>
        /// <param name="peerStat"></param>
        public void WorkerStatusChanged(WorkerStatus wkrStat)
        {
            ChangePluginIcon(wkrStat);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(WorkerStatus wkrStatus)
        {
            if (OnPluginStatusChanged != null)
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, (int)wkrStatus));
        }

        #endregion
    }
}