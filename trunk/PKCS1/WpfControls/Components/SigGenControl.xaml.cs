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
//using Cryptool.PluginBase.Miscellaneous;
using PKCS1.Library;
using Org.BouncyCastle.Math;

namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaktionslogik für SigGenControl.xaml
    /// </summary>
    public partial class SigGenControl : UserControl
    {
        private RSASignature m_RSASignature;
        
        public SigGenControl()
        {
            InitializeComponent();
            // zeile muss weg; Signatur muss sich bei RSAKeyManager anmelden
            RSAKeyManager.Instance.RaiseKeyGeneratedEvent += handleCustomEvent; // bei KeyGen-Listener anmelden 
            this.handleCustomEvent(ParameterChangeType.RsaKey);
        }

        private void handleCustomEvent(ParameterChangeType type)
        {
            //this.tbResult.Text = String.Empty;
            this.tbResultEncrypted.Text = String.Empty;
            this.lblPublicKeyRes.Content = RSAKeyManager.Instance.PubExponent.ToString();
            this.lblRsaKeySizeRes.Content = RSAKeyManager.Instance.RsaKeySize.ToString();
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            this.m_RSASignature = (RSASignature) SignatureHandler.getInstance().getSignature();
            this.m_RSASignature.GenerateSignature();
            //this.tbResult.Text = this.m_RSASignature.GetSignatureDecToHexString();
            UserControlHelper.loadRtbColoredSig(this.rtbResult, this.m_RSASignature.GetSignatureDecToHexString());
            this.tbResultEncrypted.Text = this.m_RSASignature.GetSignatureToHexString();

            // nur temp
            SignatureHandler.getInstance().setSignature(this.m_RSASignature);
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
