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

namespace PKCS1.WpfControls.SigGenFake
{
    /// <summary>
    /// Interaktionslogik für SigGenFakeShort.xaml
    /// </summary>
    public partial class SigGenFakeShortControl : UserControl, IPkcs1UserControl
    {
        private bool isKeyGen = false;
        private bool isDatablockGen = false;

        public SigGenFakeShortControl()
        {
            InitializeComponent();
            RSAKeyManager.Instance.RaiseKeyGeneratedEvent += handleKeyGenerated;
            DatablockControl3.RaiseDataBlockGenerated += handleKeyGenerated;

            if (RSAKeyManager.Instance.isKeyGenerated())
            {
                this.tabGenSignature.IsEnabled = true;
            }
            else
            {
                this.tabGenSignature.IsEnabled = false;
            }
        }

        private void handleKeyGenerated(ParameterChangeType type)
        {
            if (type == ParameterChangeType.RsaKey)
            {
                this.isKeyGen = true;
            }
            else if (type == ParameterChangeType.DataBlock)
            {
                this.isDatablockGen = true;
            }

            if (this.isKeyGen == true && this.isDatablockGen == true)
            {
                this.tabGenSignature.IsEnabled = true;
            }
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

        private void TabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabGenDatablock)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_Datablock_Tab);
            }
            else if (sender == tabGenSignature)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_Kuehn_Sig_Tab);
            }
        }
    }
}
