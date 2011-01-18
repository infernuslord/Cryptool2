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
using System.ComponentModel;

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
            this.MessageLabel.Visibility = Visibility.Hidden;
            try
            {
                if (CertificateServices.GetPeerCertificateByAvatar(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PeersAtPlay" + Path.DirectorySeparatorChar + "Certificates" + Path.DirectorySeparatorChar),
                    P2PSettings.Default.PeerName, P2PBase.DecryptString(P2PSettings.Default.Password)) == null)
                {
                    this.MessageLabel.Content = "Cannot connect, account \"" + P2PSettings.Default.PeerName + "\" not found!";
                    this.MessageLabel.Visibility = Visibility.Visible;
                    return;
                }
            }
            catch (NoCertificateFoundException)
            {
                this.MessageLabel.Content = "Cannot connect, account \"" + P2PSettings.Default.PeerName + "\" not found!";
                this.MessageLabel.Visibility = Visibility.Visible;
                return;
            }            
            catch (Exception ex)
            {
                this.MessageLabel.Content = "Cannot connect using account \"" + P2PSettings.Default.PeerName + "\": " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                this.MessageLabel.Visibility = Visibility.Visible;
                return;
            }

            if (!P2PManager.IsConnected)
                P2PManager.Connect();
        }

        private void HelpButtonClick(object sender, RoutedEventArgs e)
        {
            ((P2PEditorPresentation) P2PEditor.Presentation).ShowHelp();
        }
        
        private void GetACertificateLabel_Click(object sender, RoutedEventArgs e)
        {
            P2PEditorPresentation.ShowGetNewCertificateView();
        }

        private void ForgotPasswordLabel_Click(object sender, RoutedEventArgs e)
        {
           //todo
        }

        private void Username_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).PeerName = this.Username.Text;
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).Password = this.Password.Password;
        }

        private void P2PUserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {            
            this.Username.Text = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).PeerName;
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

        private void OnPropertyChanged_Settings(object sender, PropertyChangedEventArgs args)
        {            
            if(args.PropertyName.Equals("PeerName")){
                //this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                //{
                    if (this.Username.Text != ((P2PEditorSettings)sender).PeerName)
                    {

                        this.Username.Text = ((P2PEditorSettings)sender).PeerName;
                    }
                //},null);
            }
            if(args.PropertyName.Equals("Password")){
                //this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                //{
                    if (this.Password.Password != ((P2PEditorSettings)sender).Password)
                    {
                        this.Password.Password = ((P2PEditorSettings)sender).Password;
                    }
                //},null);
            }            
        }

        private void P2PUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).PropertyChanged += OnPropertyChanged_Settings;
        }
    }
}
