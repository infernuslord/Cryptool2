using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace WorkspaceManager
{
    class WorkspaceManagerSettings : ISettings
    {
        #region ISettings Members
        private bool hasChanges = false;

        public WorkspaceManagerSettings()
        {
            this.Schedulers = "" + System.Environment.ProcessorCount * 2;
        }

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        private String guiUpdateInterval = "100";
        [TaskPane("GuiUpdateInterval", "The interval the gui should be updated in miliseconds.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String GuiUpdateInterval
        {
            get
            {
                return guiUpdateInterval;
            }
            set
            {
                guiUpdateInterval = value;
                OnPropertyChanged("GuiUpdateInterval");
            }
        }

        private String sleepTime = "0";
        [TaskPane("SleepTime", "The time which the execution will sleep after executing a plugin.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String SleepTime
        {
            get
            {
                return sleepTime;
            }
            set
            {
                sleepTime = value;
                OnPropertyChanged("SleepTime");
            }
        }

        private String schedulers = "0";
        [TaskPane("Schedulers", "The amount of parallel gears4net schedulers.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String Schedulers
        {
            get
            {
                return schedulers;
            }
            set
            {
                schedulers = value;
                OnPropertyChanged("Schedulers");
            }
        }     

        private bool benchmarkPlugins = false;
        [TaskPane("BenchmarkPlugins", "Should the WorkspaceManager benchmark the amount of executed plugins per second?", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox)]
        public bool BenchmarkPlugins
        {
            get
            {
                return benchmarkPlugins;
            }
            set
            {
                benchmarkPlugins = value;
                OnPropertyChanged("BenchmarkPlugins");
            }
        }

        private bool synchronousEvents = false;
        [TaskPane("SynchronousEvents", "Should the event handling be synchronous?", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox)]
        public bool SynchronousEvents
        {
            get
            {
                return synchronousEvents;
            }
            set
            {
                synchronousEvents = value;
                OnPropertyChanged("SynchronousEvents");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
