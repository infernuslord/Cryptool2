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
using PKCS1.WpfControls.Components;
using PKCS1.WpfControls;
using PKCS1.Library;
//using PKCS1.Library;
//using Cryptool.PluginBase.Miscellaneous;

namespace PKCS1.WpfControls.SigGen
{
    /// <summary>
    /// Interaktionslogik für SigGenPkcs1.xaml
    /// </summary>
    public partial class SigGenPkcs1Control : UserControl, IPkcs1UserControl
    {
        private bool isKeyGen = false;
        private bool isDatablockGen = false;

        public SigGenPkcs1Control()
        {
            InitializeComponent();
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleKeyGenerated;
            Datablockcontrol.RaiseDataBlockGenerated += handleKeyGenerated;

            if (RsaKey.Instance.isKeyGenerated())
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

        #region EventHanlder

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_PKCS1_Sig_Tab);
            }
        }
    }
}
