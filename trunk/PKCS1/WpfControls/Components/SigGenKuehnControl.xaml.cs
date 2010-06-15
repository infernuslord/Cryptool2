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
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            this.m_signature = (KuehnSignature)SignatureHandler.getInstance().getKuehnSig();

            this.m_signature.GenerateSignature();
            UserControlHelper.loadRtbColoredSig(this.rtbResult, this.m_signature.GetSignatureDecToHexString());
            this.tbResultEncrypted.Text = this.m_signature.GetSignatureToHexString();

            SignatureHandler.getInstance().setKuehnSig(this.m_signature);

            Cursor = Cursors.Arrow;
        }

        private void tbResultEncrypted_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void rtbResult_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
