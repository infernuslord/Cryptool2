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
    /// Interaktionslogik für SigGenKuehnControl.xaml
    /// </summary>
    public partial class SigGenKuehnControl : UserControl
    {
        private KuehnSig m_signature = (KuehnSig)SignatureHandler.getInstance().getKuehnSig();
        public KuehnSig Signature
        {
            get { return this.m_signature; }
            set { this.m_signature = (KuehnSig)value; }
        }

        public SigGenKuehnControl()
        {
            InitializeComponent();
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleCustomEvent; // listen
            this.handleCustomEvent(ParameterChangeType.RsaKey);
        }

        private void handleCustomEvent(ParameterChangeType type)
        {
            this.lblPublicKeyRes.Content = RsaKey.Instance.PubExponent.ToString();
            this.lblRsaKeySizeRes.Content = RsaKey.Instance.RsaKeySize.ToString();
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            //this.Signature = (KuehnSig)SignatureHandler.getInstance().getKuehnSig();

            if (this.Signature.GenerateSignature())
            {
                UserControlHelper.loadRtbColoredSig(this.rtbResult, this.Signature.GetSignatureDecToHexString());
                this.tbResultEncrypted.Text = this.Signature.GetSignatureToHexString();
                SignatureHandler.getInstance().setKuehnSig(this.Signature);
            }
            else
            {
                this.tbError.Text = "Signatur konnte nicht erstellt werden. Es ist das Limit an Iterationen erreicht worden.";
            }

            Cursor = Cursors.Arrow;
        }

        private void tbResultEncrypted_TextChanged(object sender, TextChangedEventArgs e)
        {            
            this.lblEncryptedSignatureLength.Content = "(Länge: " + this.tbResultEncrypted.Text.Length * 4 + " bit)";
        }

        private void rtbResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblSignatureLength.Content = "(Länge: " + UserControlHelper.GetRtbTextLength(this.rtbResult) * 4 + " bit)";
        }

        private void btn_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == btnHelpIterations)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_Kuehn_Iterations);
            }
            e.Handled = true;
        }
    }
}
