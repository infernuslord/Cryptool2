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
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
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
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Hidden)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            }
        }

        private string sTopic = "NewTopic";
        [TaskPane( "TopicNameCaption", "TopicNameTooltip",null,0,false,ControlType.TextBox)]
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
        [TaskPane( "BtnUnregisterCaption", "BtnUnregisterTooltip", "Control region", 0, true, ControlType.Button)]
        public void BtnUnregister()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            OnPropertyChanged("BtnUnregister");
        }
        [TaskPane( "BtnRegisterCaption", "BtnRegisterTooltip", "Control region", 1, true, ControlType.Button)]
        public void BtnRegister()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Visible)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Visible)));
            OnPropertyChanged("BtnRegister");
        }
        [TaskPane( "BtnSolutionFoundCaption", "BtnSolutionFoundTooltip", "Control region", 2, true, ControlType.Button)]
        public void BtnSolutionFound()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            OnPropertyChanged("BtnSolutionFound");
        }

        [TaskPane( "BtnSerDeserCaption", "BtnSerDeserTooltip", "Serialization Test", 0, true, ControlType.Button)]
        public void BtnSerDeser()
        {
            OnPropertyChanged("BtnSerDeser");
        }
        /* FOR TESTING ISSUES */

        private int sendAliveMessageInterval = 60;
        [TaskPane( "SendAliveMessageIntervalCaption", "SendAliveMessageIntervalTooltip", "Subscriber Properties", 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 10, 3600)]
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
