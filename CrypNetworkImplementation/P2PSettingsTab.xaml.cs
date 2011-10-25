using System.Windows;
using System.Windows.Controls;
using Cryptool.PluginBase.Attributes;
using Cryptool.P2P.Types;

namespace Cryptool.P2PDLL
{
    /// <summary>
    /// Interaction logic for P2PSettingsTab.xaml
    /// </summary>
    [Localization("Cryptool.P2PDLL.Properties.Resources")]
    [SettingsTab("NetworkSettings", "/MainSettings/", 0.8)]
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
            ProxyPasswordBox.Password = StringHelper.DecryptString(P2P.P2PSettings.Default.ProxyPassword);
        }

        private void ProxyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            P2P.P2PSettings.Default.ProxyPassword = StringHelper.EncryptString(ProxyPasswordBox.Password);
        }
    }
}
