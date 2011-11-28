using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cryptool.PluginBase.Attributes;
using Cryptool.CrypWin.Properties;

namespace Cryptool.CrypWin.SettingsTabs
{
    /// <summary>
    /// Interaction logic for UpdaterSettingsTab.xaml
    /// </summary>
    [Localization("Cryptool.CrypWin.SettingsTabs.Resources.res")]
    [SettingsTab("UpdaterSettings", "/MainSettings/", 0.9)]
    public partial class UpdaterSettingsTab : UserControl
    {

        public UpdaterSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            minutesInput.Text = Settings.Default.CheckInterval+"";
            SetPeriodicallyDependencies();
        }

        private void checkPeriodically_Changed(object sender, RoutedEventArgs e)
        {
            SetPeriodicallyDependencies();
        }

        private void SetPeriodicallyDependencies()
        {
            if (checkPeriodically.IsChecked != null && checkStartup != null && minutesInput != null)
            {
                bool isChecked = (bool)checkPeriodically.IsChecked;
                minutesInput.IsEnabled = isChecked;
                checkStartup.IsEnabled = !isChecked;
                if (isChecked)
                    checkStartup.IsChecked = isChecked;
            }
        }

        private void minutesInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            int minutes = 0;
            bool isInteger = Int32.TryParse(minutesInput.Text, out minutes);

            if (isInteger && minutes > 0)
            {
                Settings.Default.CheckInterval = minutes;
                checkPeriodicallyWarning.Visibility = Visibility.Hidden;
            }
            else
                checkPeriodicallyWarning.Visibility = Visibility.Visible;
        }

    }

}
