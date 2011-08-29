using System.Windows;
using System.Windows.Controls;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Properties;

namespace Cryptool.P2P
{
    /// <summary>
    /// Interaction logic for P2PSettingsTab.xaml
    /// </summary>
    [Localization("Cryptool.P2P.Properties.Resources")]
    [SettingsTab("P2PSettings", "/MainSettings/", 0.8)]
    public partial class P2PSettingsTab : UserControl
    {
        public P2PSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            earthGrid.DataContext = this;

            P2P.P2PSettings.Default.PropertyChanged += delegate
                                                       {
                                                           P2P.P2PSettings.Default.Save();
                                                       };
        }
    }
}
