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
using PKCS1.OnlineHelp;
//using PKCS1.WpfControls;

namespace PKCS1.WpfControls.Start
{
    /// <summary>
    /// Interaktionslogik für StartControl.xaml
    /// </summary>
    public partial class StartControl : UserControl, IPkcs1UserControl
    {
        //public event Navigate OnStartpageLinkClick;
        System.Windows.Forms.WebBrowser b;

        public StartControl()
        {
            InitializeComponent();
            b = new System.Windows.Forms.WebBrowser();
            b.Dock = System.Windows.Forms.DockStyle.Fill;
            windowsFormsHost1.Child = b;
            b.DocumentText = OnlineHelpAccess.HelpResourceManager.GetString("Start");
        }

        #region IPkcs1UserControl Member

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init()
        {
            //throw new NotImplementedException();
        }

        public void SetTab(int i)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
