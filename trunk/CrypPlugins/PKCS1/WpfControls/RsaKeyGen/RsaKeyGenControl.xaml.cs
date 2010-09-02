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
using PKCS1.WpfControls;
using PKCS1.WpfControls.Components;

namespace PKCS1.WpfControls.RsaKeyGen
{
    /// <summary>
    /// Interaktionslogik für RsaKeyGenControl.xaml
    /// </summary>
    public partial class RsaKeyGenControl : UserControl, IPkcs1UserControl
    {
        public RsaKeyGenControl()
        {
            InitializeComponent();
        }

        private void TabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabGenKey)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyGen_Tab);
            }
            else if (sender == tabInputKey)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyInput_Tab);
            }
        }

        #region IPkcs1UserControl Member

        void IPkcs1UserControl.Dispose()
        {
            //throw new NotImplementedException();
        }

        void IPkcs1UserControl.Init()
        {
            //throw new NotImplementedException();
        }

        void IPkcs1UserControl.SetTab(int i)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
