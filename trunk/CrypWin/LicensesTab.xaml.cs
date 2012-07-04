using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
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
using Cryptool.P2P;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.CrypWin
{
    /// <summary>
    /// Interaction logic for SystemInfos.xaml
    /// </summary>
    [TabColor("White")]
    [Localization("Cryptool.CrypWin.Properties.Resources")]
    public partial class LicensesTab : UserControl
    {
        public LicensesTab()
        {
            InitializeComponent();
            Tag = FindResource("Icon");
            LicenseTextbox.Text += (Properties.Resources.CrypToolLicenses + ":\r\n \r\n");
            LicenseTextbox.Text += Properties.Resources.ApacheLicense2;
        }    
    }
}
