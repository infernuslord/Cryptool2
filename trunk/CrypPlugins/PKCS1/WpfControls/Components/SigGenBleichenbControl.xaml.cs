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
using PKCS1.Resources.lang.Gui;

namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaktionslogik für SigGenBleichenbControl.xaml
    /// </summary>
    public partial class SigGenBleichenbControl : UserControl
    {
        private BleichenbacherSig m_BleichSignature = new BleichenbacherSig();

        public SigGenBleichenbControl()
        {
            InitializeComponent();
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleCustomEvent; // listen
            this.handleCustomEvent(ParameterChangeType.RsaKey);
            this.loadComboDataBlocPos(24);
        }

        private void handleCustomEvent(ParameterChangeType type)
        {            
            this.lblPublicKeyRes.Content = RsaKey.Instance.PubExponent.ToString();
            this.lblRsaKeySizeRes.Content = RsaKey.Instance.RsaKeySize.ToString();            
            this.loadComboDataBlocPos(24);
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            this.m_BleichSignature = (BleichenbacherSig) SignatureHandler.getInstance().getBleichenbSig();
            this.m_BleichSignature.DataBlockStartPos = (int)this.cbPosDataBlock.SelectedValue;
            this.m_BleichSignature.ChangeSign = this.tbChangeSign.Text;

            this.m_BleichSignature.GenerateSignature();
            UserControlHelper.loadRtbColoredSig(this.rtbResult, this.m_BleichSignature.GetSignatureDecToHexString());
            this.tbResultEncrypted.Text = this.m_BleichSignature.GetSignatureToHexString();

            SignatureHandler.getInstance().setBleichenBSig(this.m_BleichSignature);
            Cursor = Cursors.Arrow;
        }

        private void loadComboDataBlocPos(int start)
        {
            this.cbPosDataBlock.Items.Clear();

            int lengthDatablock = Datablock.getInstance().HashFunctionIdent.DERIdent.Length * 4 + Datablock.getInstance().HashFunctionIdent.digestLength + 8;
            int end = RsaKey.Instance.RsaKeySize - lengthDatablock - start;

            for( int i=start; i<= end; i+=8)
            {
                this.cbPosDataBlock.Items.Add(i);

            }
            this.cbPosDataBlock.SelectedIndex = (end-start)/10;
        }

        private void rtbResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblSignatureLength.Content = "(" + Common.length  +": " + UserControlHelper.GetRtbTextLength(this.rtbResult) * 4 + " " + Common.bit + ")";
        }

        private void tbResultEncrypted_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblEncryptedSignatureLength.Content = "(" + Common.length + ": " + this.tbResultEncrypted.Text.Length * 4 + " " + Common.bit + ")";
        }

        private void btn_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == btnHelpChangeSign)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_Bleichenb_ChangeSign);
            }
            else if (sender == btnHelpBitPos)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_Bleichenb_BitPos);
            }
            e.Handled = true;
        }
    }
}
