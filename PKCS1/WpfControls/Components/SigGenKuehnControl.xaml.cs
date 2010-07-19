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
        private KuehnSignature m_signature = new KuehnSignature();

        public SigGenKuehnControl()
        {
            InitializeComponent();
            RSAKeyManager.Instance.RaiseKeyGeneratedEvent += handleCustomEvent; // listen
            this.handleCustomEvent(ParameterChangeType.RsaKey);
        }

        private void handleCustomEvent(ParameterChangeType type)
        {
            this.lblPublicKeyRes.Content = RSAKeyManager.Instance.PubExponent.ToString();
            this.lblRsaKeySizeRes.Content = RSAKeyManager.Instance.RsaKeySize.ToString();
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            this.m_signature = (KuehnSignature)SignatureHandler.getInstance().getKuehnSig();

            if (this.m_signature.GenerateSignature())
            {
                UserControlHelper.loadRtbColoredSig(this.rtbResult, this.m_signature.GetSignatureDecToHexString());
                this.tbResultEncrypted.Text = this.m_signature.GetSignatureToHexString();
                SignatureHandler.getInstance().setKuehnSig(this.m_signature);
            }
            else
            {
                this.tbError.Text = "Es ist ein Fehler aufgetreten. Signatur konnte nicht erstellt werden.";
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
    }
}
