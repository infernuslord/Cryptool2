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
    public class P2PManagerSettings : ISettings
    {
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        private bool hasChanges = false;
        private P2PManager p2pManager;
        //private KeySearcher keysearcher;

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

        public P2PManagerSettings(P2PManager p2pManager)
        {
            this.p2pManager = p2pManager;
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Hidden)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            }
            ChangePluginIcon(MngStatus.Neutral);
        }

        private string sTopic = "NewTopic";
        [TaskPane("Topic Name","Choose a topic name with which all subscribers can register.",null,0,false,DisplayLevel.Beginner,ControlType.TextBox)]
        public string TopicName 
        {
            get { return this.sTopic; }
            set
            {
                if (this.sTopic != value && value != String.Empty && value != null)
                {
                    this.sTopic = value;
                    HasChanges = true;
                    OnPropertyChanged("TopicName");
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
        [TaskPane("Solution found", "TESTING: Emulate solution-found-case!", "Control region", 2, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnSolutionFound()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            OnPropertyChanged("BtnSolutionFound");
        }
        /* FOR TESTING ISSUES */

        private int sendAliveMessageInterval = 20;
        [TaskPane("Alive Message Interval (in seconds)","In which interval do you wish to receive Alive-Messages from your Subscribers?"
            ,"Subscriber Properties",1,false,DisplayLevel.Beginner,ControlType.NumericUpDown, ValidationType.RangeInteger, 10, 3600)]
        public int SendAliveMessageInterval 
        {
            get 
            { 
                return this.sendAliveMessageInterval; 
            }
            set
            {
                if (value != this.sendAliveMessageInterval)
                {
                    this.sendAliveMessageInterval = value;
                    OnPropertyChanged("SendAliveMessageInterval");
                }
            }
        }

        private string key;
        [TaskPane("Key", "Key pattern used to bruteforce", null, 2, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
                OnPropertyChanged("Key");

                //if (!(keysearcher.Pattern != null && keysearcher.Pattern.testKey(value)))
                //    keysearcher.GuiLogMessage("Wrong key pattern!", NotificationLevel.Error);
            }
        }

        private int keyPatternSize = 10; // in hundred-thousand
        [TaskPane("KeyPatternSize", "Choose the Size of the specific sub-KeyPattern (in hundred-thousand steps)"
            , null, 3, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 90)]
        public int KeyPatternSize
        {
            get
            {
                return this.keyPatternSize;
            }
            set
            {
                if (value != this.keyPatternSize)
                {
                    this.keyPatternSize = value;
                    OnPropertyChanged("KeyPatternSize");
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

        // Index depends on icon-position in P2PManager-Class properties
        public enum MngStatus
        {
            Neutral = 0,
            Working = 1,
            Finished = 2
            //Error = 3
        }

        /// <summary>
        /// Changes icon of P2PManager and visibility of the control buttons in settings
        /// </summary>
        /// <param name="peerStat"></param>
        public void MngStatusChanged(MngStatus mngStat)
        {
            ChangePluginIcon(mngStat);
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(MngStatus mngStatus)
        {
            if (OnPluginStatusChanged != null)
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, (int)mngStatus));
        }

        #endregion
    }
}
