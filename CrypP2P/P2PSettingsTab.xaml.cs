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

namespace Cryptool.P2P
{
    /// <summary>
    /// Interaction logic for P2PSettingsTab.xaml
    /// </summary>
    [Localization("Cryptool.P2P.Properties.Resources")]
    [SettingsTab("P2PSettings", "/MainSettings/")]
    public partial class P2PSettingsTab : UserControl
    {
        public P2PSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            P2PSettings.Default.PropertyChanged += delegate
                                                       {
                                                           P2PSettings.Default.Save();
                                                       };
        }
    }
}
