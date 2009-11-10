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
    public class P2PPublisherSettings : ISettings
    {
        private bool hasChanges = false;
        private P2PPublisher p2pPublisher;

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

        public P2PPublisherSettings(P2PPublisher p2pPublisher)
        {
            this.p2pPublisher = p2pPublisher;
        }

        private string sTaskName = "NewCompTask";
        [TaskPane("Task Name","Choose a name for the computational task",null,0,false,DisplayLevel.Beginner,ControlType.TextBox)]
        public string TaskName 
        {
            get { return this.sTaskName; }
            set
            {
                if (this.sTaskName != value && value != String.Empty && value != null)
                {
                    this.sTaskName = value;
                    HasChanges = true;
                    OnPropertyChanged("TaskName");
                }
            }
        }

        private int sendAliveMessageInterval = 20;
        [TaskPane("Alive Message Interval (in seconds)","In which interval do you wish to receive Alive-Messages from your Subscribers?"
            ,"Subscriber Properties",0,false,DisplayLevel.Beginner,ControlType.NumericUpDown, ValidationType.RangeInteger, 10, 3600)]
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
    }
}
