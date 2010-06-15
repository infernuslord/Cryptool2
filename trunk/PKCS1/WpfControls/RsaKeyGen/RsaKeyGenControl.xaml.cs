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
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using PKCS1.Library;


namespace PKCS1.WpfControls.RsaKeyGen
{
    /// <summary>
    /// Interaktionslogik für RsaKeyGenControl.xaml
    /// </summary>
    public partial class RsaKeyGenControl : UserControl, IPkcs1UserControl
    {
        //private AsymmetricCipherKeyPair keyPair = null;

        public RsaKeyGenControl()
        {
            InitializeComponent();
            Init();
        }

        #region IPkcs1UserControl Member

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init()
        {
            this.tbInputPubKey.Text = RSAKeyManager.getInstance().PubExponent.ToString();
            this.tbInputMod.Text = RSAKeyManager.getInstance().RsaKeySize.ToString();
        }

        public void SetTab(int i)
        {
            //throw new NotImplementedException();
        }

        #endregion

        private void btnGenRsaKey_Click(object sender, RoutedEventArgs e)
        {
            //RSAKeyManager.getInstance().RsaKeySize = Convert.ToInt32(this.tbInputMod.Text);
            //RSAKeyManager.getInstance().PubExponent = Convert.ToInt32(this.tbInputPubKey.Text);
            Cursor = Cursors.Wait;
            RSAKeyManager.getInstance().RsaKeySize = Convert.ToInt32(this.tbInputMod.Text);
            RSAKeyManager.getInstance().genRsaKeyPair(25);
            Cursor = Cursors.Arrow;

            if( RSAKeyManager.getInstance().isKeyGenerated() )
            {
                this.tbResultModulus.Text = RSAKeyManager.getInstance().getModulusToBigInt().ToString(16);
                this.tbResultPrivKey.Text = RSAKeyManager.getInstance().getPrivKeyToBigInt().ToString(16);
            }
        }

        private void tbResultPrivKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblPrivKeyLength.Content = "(Länge: " + this.tbResultPrivKey.Text.Length * 4 + " bit)";
        }

        private void tbResultModulus_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblModulusLength.Content = "(Länge: " + this.tbResultModulus.Text.Length * 4 + " bit)";
        }

        private void tbInputPubKey_TextChanged(object sender, TextChangedEventArgs e)
        {           
            RSAKeyManager.getInstance().PubExponent = Convert.ToInt32(this.tbInputPubKey.Text);
        }

        private void tbInputMod_TextChanged(object sender, TextChangedEventArgs e)
        {
            //RSAKeyManager.getInstance().RsaKeySize = Convert.ToInt32(this.tbInputMod.Text);
        }

        private void btn_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == btnHelpPubKey)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyGen_PubExponent);
            }
            else if (sender == btnHelpBitSizeModulus)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyGen_ModulusSize);
            }
            e.Handled = true;
        }
    }
}
