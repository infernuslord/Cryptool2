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
using PKCS1.Library;

namespace PKCS1.WpfVisualization.Navigation
{
    /// <summary>
    /// Interaktionslogik für Navigation.xaml
    /// </summary>
    public partial class Navigation : UserControl
    {
        public event Navigate OnNavigate;
        public Navigation()
        {
            InitializeComponent();
        }

        private void link_Click(object sender, RoutedEventArgs e)
        {
            if (null != OnNavigate)
            {
                NavigationCommandType commandtype = NavigationCommandType.None;

                if (sender == link_SignatureGenerate) commandtype = NavigationCommandType.SigGen;
                else if (sender == link_RsaKeyGenerate) commandtype = NavigationCommandType.RsaKeyGen;
                else if (sender == link_AttackBleichenbacher) commandtype = NavigationCommandType.SigGenFakeBleichenb;
                else if (sender == link_AttackShortKeysVariant) commandtype = NavigationCommandType.SigGenFakeShort;
                else if (sender == link_SignatureValidate) commandtype = NavigationCommandType.SigVal;
                else if (sender == link_Start) commandtype = NavigationCommandType.Start;

                OnNavigate(commandtype);
            }
        }
    }
}
