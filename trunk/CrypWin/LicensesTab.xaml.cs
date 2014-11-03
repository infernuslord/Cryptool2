using System.Windows.Controls;
using Cryptool.PluginBase.Attributes;

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
