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

namespace KeySearcher
{
    /// <summary>
    /// Interaction logic for KeysearcherSettingsTab.xaml
    /// </summary>
    [Localization("KeySearcher.Properties.Resources")]
    [SettingsTab("KeysearcherSettings", "/PluginSettings/")]
    public partial class KeysearcherSettingsTab : UserControl
    {
        public KeysearcherSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
        }
    }
}
