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
            WorkspaceManager = manager;
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
