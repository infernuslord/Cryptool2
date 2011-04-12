using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Wizard.Properties;

namespace Wizard
{
    class WizardSettings : ISettings
    {
        public WizardSettings()
        {
            Settings.Default.SettingChanging += new System.Configuration.SettingChangingEventHandler(Default_SettingChanging);
        }

        private void Default_SettingChanging(object sender, SettingChangingEventArgs e)
        {
            if (e.SettingName == "ShowOnStartup")
                OnPropertyChanged("ShowOnStartup");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool HasChanges
        { get; set; }

        [TaskPane("ShowOnStartupCaption", "ShowOnStartupTooltip", null, 1, true, ControlType.CheckBox)]
        public bool ShowOnStartup
        {
            get
            {
                return Settings.Default.ShowOnStartup;
            }
            set
            {
                Settings.Default.ShowOnStartup = value;
                Settings.Default.Save();
                HasChanges = true;
                OnPropertyChanged("ShowOnStartup");
            }
        }


        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

    }
}
