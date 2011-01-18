using System;
using System.Windows;
using Cryptool.P2P;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using Cryptool.P2P.Internal;
using PeersAtPlay.CertificateLibrary.Certificates;
using Cryptool.PluginBase;
using System.IO;

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

            P2PManager.ConnectionManager.OnP2PTryConnectingStateChangeOccurred += new ConnectionManager.P2PTryConnectingStateChangeEventHandler(delegate(object sender, bool newState)
            {
                if (newState)
                {
                    
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        ((P2PEditorPresentation)P2PEditor.Presentation).UpdateConnectionState();
                        Storyboard storyboard = (Storyboard)FindResource("AnimateBigWorldIcon");
                        storyboard.Begin();
                    }
                    , null);
                }
                else
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        ((P2PEditorPresentation)P2PEditor.Presentation).UpdateConnectionState();                    
                        Storyboard storyboard = (Storyboard)FindResource("AnimateBigWorldIcon");
                        storyboard.Stop(); 
                    }
                    , null);         
                }
            });

        }

        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CertificateServices.GetPeerCertificateByAvatar(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PeersAtPlay" + Path.DirectorySeparatorChar + "Certificates" + Path.DirectorySeparatorChar),
                    P2PSettings.Default.PeerName, P2PSettings.Default.Password) == null)
                {
                    System.Windows.MessageBox.Show("Cannot connect, account \"" + P2PSettings.Default.PeerName + "\" not found!", "Can not connect.");
                    return;
                }
            }
            catch (NoCertificateFoundException)
            {
                System.Windows.MessageBox.Show("Cannot connect, account \"" + P2PSettings.Default.PeerName + "\" not found!", "Can not connect.");
                return;
            }            
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot connect using account \"" + P2PSettings.Default.PeerName + "\": " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message), "Can not connect.");
                return;
            }

            if (!P2PManager.IsConnected)
                P2PManager.Connect();
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
            if (this.Visibility == System.Windows.Visibility.Visible)
            {
                this.Username.Text = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).PeerName;
                this.Worldname.Text = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).WorldName;
                this.Password.Password = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).Password;
                if (this.IsP2PConnecting)
                {
                    Storyboard storyboard = (Storyboard)FindResource("AnimateBigWorldIcon");
                    storyboard.Begin();
                }
                else
                {
                    Storyboard storyboard = (Storyboard)FindResource("AnimateBigWorldIcon");
                    storyboard.Stop();
                }
            }
        }
    }
}
