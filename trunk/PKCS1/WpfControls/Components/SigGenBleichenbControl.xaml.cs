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

namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaktionslogik für SigGenBleichenbControl.xaml
    /// </summary>
    public partial class SigGenBleichenbControl : UserControl
    {
        private BleichenbacherSignature m_BleichSignature = new BleichenbacherSignature();
        //private int DataBlockPos = 0;

        public SigGenBleichenbControl()
        {
            InitializeComponent();
            RSAKeyManager.getInstance().RaiseKeyGeneratedEvent += handleCustomEvent; // listen
            this.handleCustomEvent(ParameterChangeType.RsaKey);
            this.loadComboDataBlocPos(24);
        }

        private void handleCustomEvent(ParameterChangeType type)
        {            

            this.lblPublicKeyRes.Content = RSAKeyManager.getInstance().PubExponent.ToString();
            this.lblRsaKeySizeRes.Content = RSAKeyManager.getInstance().RsaKeySize.ToString();            
            this.loadComboDataBlocPos(24);
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            this.m_BleichSignature = (BleichenbacherSignature) SignatureHandler.getInstance().getBleichenbSig();
            this.m_BleichSignature.DataBlockStartPos = (int)this.cbPosDataBlock.SelectedValue;

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
            int end = RSAKeyManager.getInstance().RsaKeySize - lengthDatablock - start;

            for( int i=start; i<= end; i+=8)
            {
                this.cbPosDataBlock.Items.Add(i);

            }
            this.cbPosDataBlock.SelectedIndex = (end-start)/10;
        }

        private void rtbResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblSignatureLength.Content = "(Länge: " + UserControlHelper.GetRtbTextLength(this.rtbResult) * 4 + " bit)";
        }

        private void tbResultEncrypted_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblEncryptedSignatureLength.Content = "(Länge: " + this.tbResultEncrypted.Text.Length * 4 + " bit)";
        }

        private void cbPosDataBlock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
