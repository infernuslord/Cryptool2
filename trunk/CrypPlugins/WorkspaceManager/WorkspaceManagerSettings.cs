using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.IO;

namespace WorkspaceManager
{
    class WorkspaceManagerSettings : ISettings
    {
        #region ISettings Members
        private bool hasChanges = false;

        private WorkspaceManager WorkspaceManager { get; set; }

        public WorkspaceManagerSettings(WorkspaceManager manager)
        {
            this.Threads = "" + System.Environment.ProcessorCount;
            this.WorkspaceManager = manager;
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

        private String threads = "0";
        [TaskPane("Threads", "The amount of used threads for scheduling.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String Threads
        {
            get
            {
                return threads;
            }
            set
            {
                threads = value;
                OnPropertyChanged("Threads");
            }
        }

        private int threadPriority = 4;
        [TaskPane("ThreadPriority", "Should the event handling be synchronous?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new String[] { "AboveNormal", "BelowNormal", "Highest", "Lowest", "Normal" })]
        public int ThreadPriority
        {
            get
            {
                return threadPriority;
            }
            set
            {
                threadPriority = value;
                OnPropertyChanged("ThreadPriority");
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

        private int logLevel = 0;
        [TaskPane("LogLevel", "Should the event handling be synchronous?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Debug", "Info", "Warning", "Error"})]
        public int LogLevel
        {
            get
            {
                return logLevel;
            }
            set
            {
                logLevel = value;
                OnPropertyChanged("LogLevel");
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
