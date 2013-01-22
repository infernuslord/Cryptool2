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
using System.Reflection;
using Cryptool.PluginBase.Miscellaneous;

namespace Primes.Options
{
    /// <summary>
    /// Interaction logic for OptionVersionInfo.xaml
    /// </summary>
    public partial class OptionVersionInfo : UserControl
    {
        public OptionVersionInfo()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Version version = AssemblyHelper.GetVersion(Assembly.GetAssembly(this.GetType()));
            string strVersion = String.Format("{0}.{1}.{2}", new Object[] { version.Major - 1, version.Minor, version.Revision });
            tbVersionInfo.Text = strVersion;
            tbBuildInfo.Text = version.Build.ToString();
        }
    }
}