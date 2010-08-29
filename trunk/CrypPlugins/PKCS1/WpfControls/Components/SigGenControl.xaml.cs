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
    /// Interaktionslogik für SigGenControl.xaml
    /// </summary>
    public partial class SigGenControl : UserControl
    {
        private RsaSig m_RSASignature;
        
        public SigGenControl()
        {
            InitializeComponent();
            // zeile muss weg; Signatur muss sich bei RsaKey anmelden
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleCustomEvent; // bei KeyGen-Listener anmelden 
            this.handleCustomEvent(ParameterChangeType.RsaKey);
        }

        private void handleCustomEvent(ParameterChangeType type)
        {
            this.tbResultEncrypted.Text = String.Empty;
            this.lblPublicKeyRes.Content = RsaKey.Instance.PubExponent.ToString();
            this.lblRsaKeySizeRes.Content = RsaKey.Instance.RsaKeySize.ToString();
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            this.m_RSASignature = (RsaSig) SignatureHandler.getInstance().getSignature();
            this.m_RSASignature.GenerateSignature();
            UserControlHelper.loadRtbColoredSig(this.rtbResult, this.m_RSASignature.GetSignatureDecToHexString());
            this.tbResultEncrypted.Text = this.m_RSASignature.GetSignatureToHexString();

            // nur temp
            //SignatureHandler.getInstance().setSignature(this.m_RSASignature);
        }

        private void tbResultEncrypted_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblEncryptedSignatureLength.Content = "(" + Common.length +": " + this.tbResultEncrypted.Text.Length * 4 + " " + Common.bit + ")";
        }

        private void rtbResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblSignatureLength.Content = "(" + Common.length + ": " + UserControlHelper.GetRtbTextLength(this.rtbResult) * 4 + " " + Common.bit + ")";
        }
    }
}
