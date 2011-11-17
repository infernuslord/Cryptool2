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
            Cryptool.PluginBase.Properties.Settings.Default.SettingChanging += new System.Configuration.SettingChangingEventHandler(Default_SettingChanging);
        }

        private void Default_SettingChanging(object sender, SettingChangingEventArgs e)
        {
            if (e.SettingName == "ShowOnStartup")
                OnPropertyChanged("ShowOnStartup");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [TaskPane("ShowOnStartupCaption", "ShowOnStartupTooltip", null, 1, true, ControlType.CheckBox)]
        public bool ShowOnStartup
        {
            get
            {
                return Cryptool.PluginBase.Properties.Settings.Default.Wizard_ShowOnStartup;
            }
            set
            {
                Cryptool.PluginBase.Properties.Settings.Default.Wizard_ShowOnStartup = value;
                Cryptool.PluginBase.Properties.Settings.Default.Save();
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
