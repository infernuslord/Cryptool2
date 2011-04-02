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

namespace WorkspaceManager
{
    /// <summary>
    /// Interaction logic for WorkspaceManagerSettingsTab.xaml
    /// </summary>
    [Localization("WorkspaceManager.Properties.Resources")]
    [SettingsTab("WorkspaceManagerSettings", "/MainSettings/", 1.1)]
    public partial class WorkspaceManagerSettingsTab : UserControl
    {
        public WorkspaceManagerSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            int num = 0;
            bool success = int.TryParse(((TextBox)sender).Text, out num);
            if (success & num >= 0)
            {
                ((TextBox)sender).Text.Trim();
            }
            else
            {
                ((TextBox)sender).Text = "";
            }
        }
    }
}
