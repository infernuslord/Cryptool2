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
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
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
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            }
	    }

        private string sTopic = "NewTopic";
        [TaskPane("TopicNameCaption", "TopicNameTooltip", null, 0, false, ControlType.TextBox)]
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
        [TaskPane("BtnUnregisterCaption", "BtnUnregisterTooltip", "Control region", 0, true, ControlType.Button)]
        public void BtnUnregister()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            OnPropertyChanged("BtnUnregister");
        }
        [TaskPane("BtnRegisterCaption", "BtnRegisterTooltip", "Control region", 1, true, ControlType.Button)]
        public void BtnRegister()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Visible)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Visible)));
            OnPropertyChanged("BtnRegister");
        }

        [TaskPane("BtnSolutionFoundCaption", "BtnSolutionFoundTooltip", "Control region", 2, true, ControlType.Button)]
        public void BtnSolutionFound()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            OnPropertyChanged("BtnSolutionFound");
        }
        /* FOR TESTING ISSUES */

        private int checkPublishersAvailability = 240;
        [TaskPane("CheckPublishersAvailabilityCaption", "CheckPublishersAvailabilityTooltip", "Intervals", 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 20, int.MaxValue)]
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
        [TaskPane("PublishersReplyTimespanCaption", "PublishersReplyTimespanTooltip", "Intervals", 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 120)]
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
