using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
using Cryptool.CrypWin.Properties;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.CrypWin.SettingsTabs
{
    /// <summary>
    /// Interaction logic for PluginsSettingsTab.xaml
    /// </summary>
    [Localization("Cryptool.CrypWin.SettingsTabs.Resources.res")]
    [SettingsTab("PluginSettings", "/", 0.5)]
    public partial class PluginsSettingsTab : UserControl
    {
        public static readonly DependencyProperty IsVisibleProperty = 
            DependencyProperty.Register("IsVisible", typeof(Boolean), typeof(PluginsSettingsTab), new PropertyMetadata(false));

        public Boolean IsVisible
        {
            get { return (Boolean)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public PluginsSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            if (AssemblyHelper.BuildType == Ct2BuildType.Developer)
            {
                IsVisible = true;
            }

            PluginListBox.DataContext = PluginList.AllPlugins;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var assemblyToPluginMap = new Dictionary<string, List<PluginInformation>>();
            foreach (var plugin in PluginList.AllPlugins)
            {
                if (assemblyToPluginMap.ContainsKey(plugin.Assemblyname))
                    assemblyToPluginMap[plugin.Assemblyname].Add(plugin);
                else
                    assemblyToPluginMap.Add(plugin.Assemblyname, new List<PluginInformation>() {plugin});
            }

            Settings.Default.DisabledPlugins = new ArrayList();
            foreach (var plugin in PluginList.AllPlugins)
            {
                if (plugin.Disabled)
                {
                    bool canBeDisabled = assemblyToPluginMap[plugin.Assemblyname].All(brotherPlugin => brotherPlugin.Disabled);
                    if (canBeDisabled)
                        Settings.Default.DisabledPlugins.Add(plugin);
                }
            }

            Settings.Default.Save();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var plugin in PluginList.AllPlugins)
            {
                plugin.Disabled = true;
            }
            PluginListBox.Items.Refresh();
            CheckBox_Checked(null, null);
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var plugin in PluginList.AllPlugins)
            {
                plugin.Disabled = false;
            }
            PluginListBox.Items.Refresh();
            CheckBox_Checked(null, null);
        }

        private void Invert_Click(object sender, RoutedEventArgs e)
        {
            foreach (var plugin in PluginList.AllPlugins)
            {
                plugin.Disabled = !plugin.Disabled;
            }
            PluginListBox.Items.Refresh();
            CheckBox_Checked(null, null);
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class TrueToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be of Visibility");

            if ((bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
