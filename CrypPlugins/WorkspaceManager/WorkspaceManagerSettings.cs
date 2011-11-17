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

        private WorkspaceManagerClass WorkspaceManager { get; set; }

        public WorkspaceManagerSettings(WorkspaceManagerClass manager)
        {
            WorkspaceManager = manager;
        }

        public String GuiUpdateInterval
        {
            get
            {
                return Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_GuiUpdateInterval;
            }
            set
            {
                Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_GuiUpdateInterval = value;
                Cryptool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("GuiUpdateInterval");
            }
        }

        public String SleepTime
        {
            get
            {
                return Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SleepTime;
            }
            set
            {
                Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SleepTime = value;
                Cryptool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("SleepTime");
            }
        }        

        public bool BenchmarkPlugins
        {
            get
            {
                return Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_BenchmarkPlugins;
            }
            set
            {
                Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_BenchmarkPlugins = value;
                Cryptool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("BenchmarkPlugins");
            }
        }

        public bool SynchronousEvents
        {
            get
            {
                return Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SynchronousEvents;
            }
            set
            {
                Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_SynchronousEvents = value;
                Cryptool.PluginBase.Properties.Settings.Default.Save();
                OnPropertyChanged("SynchronousEvents");
            }
        }

        public int LogLevel
        {
            get
            {
                return Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_LogLevel;
            }
            set
            {
                Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_LogLevel = value;
                Cryptool.PluginBase.Properties.Settings.Default.Save();
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
