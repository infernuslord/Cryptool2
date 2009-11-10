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
    public class P2PSubscriberSettings : ISettings
    {
        private bool hasChanges = false;
        private P2PSubscriber p2pSubscriber;

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

        public P2PSubscriberSettings (P2PSubscriber p2pSubscriber)
	    {
            this.p2pSubscriber = p2pSubscriber;
	    }

        private string sTaskName = "NewCompTask";
        [TaskPane("Task Name", "Choose the name of a published computational task", null, 0, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string TaskName
        {
            get { return this.sTaskName; }
            set
            {
                if (this.sTaskName != value && value != String.Empty && value != null)
                {
                    this.sTaskName = value;
                    HasChanges = true;
                    OnPropertyChanged(TaskName);
                }
            }
        }

        private int checkPublishersAvailability = 60;
        [TaskPane("Check Publisher Interval (in sec)","To check liveness or possibly changed publishing peer in intervals","Intervals",0,false,DisplayLevel.Beginner,ControlType.NumericUpDown, ValidationType.RangeInteger,20,int.MaxValue)]
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
        [TaskPane("Publisher Reply Timespan (in sec)", "When checking publishers availability, ping message is sent. The publisher must answer with a pong message in the timespan!", "Intervals", 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 10, 60)]
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
    }
}
