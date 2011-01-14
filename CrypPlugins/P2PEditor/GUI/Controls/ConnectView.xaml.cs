using System;
using System.Windows;
using Cryptool.P2P;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.P2PEditor.GUI.Controls
{
    public partial class ConnectTab
    {
        public static readonly DependencyProperty IsP2PConnectingProperty =
            DependencyProperty.Register("IsP2PConnecting",
                                        typeof(
                                            Boolean),
                                        typeof(
                                            ConnectTab), new PropertyMetadata(false));

        public Boolean IsP2PConnecting
        {
            get { return (Boolean)GetValue(IsP2PConnectingProperty); }
            set { SetValue(IsP2PConnectingProperty, value); }
        }

        public ConnectTab()
        {
            InitializeComponent();            
        }

        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {            
            if (!P2PManager.IsConnected)
                P2PManager.Connect();

            ((P2PEditorPresentation) P2PEditor.Presentation).UpdateConnectionState();
            
            Storyboard storyboard = (Storyboard)FindResource("AnimateBigWorldIcon");                
            storyboard.Begin();                

        }

        private void HelpButtonClick(object sender, RoutedEventArgs e)
        {
            ((P2PEditorPresentation) P2PEditor.Presentation).ShowHelp();
        }

        private void GetACertificateButton_Click(object sender, RoutedEventArgs e)
        {
            P2PEditorPresentation.ShowGetNewCertificateView();
        }
        
        private void Username_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).PeerName = this.Username.Text;
        }

        private void Worldname_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).WorldName = this.Worldname.Text;
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).Password = this.Password.Password;
        }

        private void P2PUserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Username.Text = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).PeerName;
            this.Worldname.Text = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).WorldName;
            this.Password.Password = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).Password;
            
            Storyboard storyboard = (Storyboard)FindResource("AnimateBigWorldIcon");
            if (IsP2PConnecting)
            {
                storyboard.Begin();
            }
            else
            {
                storyboard.Stop();
            }
        }
    }
}
