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
using WorkspaceManager.Properties;

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

       [TaskPane("GuiUpdateInterval", "The interval the gui should be updated in miliseconds.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String GuiUpdateInterval
        {
            get
            {
                return Settings.Default.GuiUpdateInterval;
            }
            set
            {
                Settings.Default.GuiUpdateInterval = value;
                Settings.Default.Save();
                OnPropertyChanged("GuiUpdateInterval");
            }
        }

        [TaskPane("SleepTime", "The time which the execution will sleep after executing a plugin.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String SleepTime
        {
            get
            {
                return Settings.Default.SleepTime;
            }
            set
            {
                Settings.Default.SleepTime = value;
                Settings.Default.Save();
                OnPropertyChanged("SleepTime");
            }
        }

        [TaskPane("Threads", "The amount of used threads for scheduling.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String Threads
        {
            get
            {
                return Settings.Default.Threads;
            }
            set
            {
                Settings.Default.Threads = value;
                Settings.Default.Save();
                OnPropertyChanged("Threads");
            }
        }

        [TaskPane("ThreadPriority", "Should the event handling be synchronous?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new String[] { "AboveNormal", "BelowNormal", "Highest", "Lowest", "Normal" })]
        public int ThreadPriority
        {
            get
            {
                return Settings.Default.ThreadPriority;
            }
            set
            {
                Settings.Default.ThreadPriority = value;
                Settings.Default.Save();
                OnPropertyChanged("ThreadPriority");
            }
        }  

        [TaskPane("BenchmarkPlugins", "Should the WorkspaceManager benchmark the amount of executed plugins per second?", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox)]
        public bool BenchmarkPlugins
        {
            get
            {
                return Settings.Default.BenchmarkPlugins;
            }
            set
            {
                Settings.Default.BenchmarkPlugins = value;
                Settings.Default.Save();
                OnPropertyChanged("BenchmarkPlugins");
            }
        }

        [TaskPane("SynchronousEvents", "Should the event handling be synchronous?", null, 1, false, DisplayLevel.Beginner, ControlType.CheckBox)]
        public bool SynchronousEvents
        {
            get
            {
                return Settings.Default.SynchronousEvents;
            }
            set
            {
                Settings.Default.SynchronousEvents = value;
                Settings.Default.Save();
                OnPropertyChanged("SynchronousEvents");
            }
        }

        [TaskPane("LogLevel", "Should the event handling be synchronous?", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "Debug", "Info", "Warning", "Error"})]
        public int LogLevel
        {
            get
            {
                return Settings.Default.LogLevel;
            }
            set
            {
                Settings.Default.LogLevel = value;
                Settings.Default.Save();
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
