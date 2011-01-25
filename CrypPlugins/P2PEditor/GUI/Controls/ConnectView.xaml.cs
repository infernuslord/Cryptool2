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
using PeersAtPlay.CertificateLibrary.Network;

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

        private bool HaveCertificate { get; set; }
        private bool EmailVerificationRequired { get; set; }
        private bool WrongPassword { get; set; }

        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            this.MessageLabel.Visibility = Visibility.Hidden;
            HaveCertificate = true;
            EmailVerificationRequired = false;
            WrongPassword = false;

            try
            {
                if (!Directory.Exists(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY))
                {
                    Directory.CreateDirectory(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY);
                    this.P2PEditor.GuiLogMessage("Automatic created account folder: " + PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY, NotificationLevel.Info);
                }
            }
            catch (Exception ex)
            {
                this.MessageLabel.Content = "Cannot create default account data directory '" + PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY + "':\n" + ex.Message;
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                this.MessageLabel.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                if (CertificateServices.GetPeerCertificateByAvatar(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY,
                    P2PSettings.Default.PeerName, P2PBase.DecryptString(P2PSettings.Default.Password)) == null)
                {                    
                    HaveCertificate = false;                
                }
            }
            catch (NoCertificateFoundException)
            {
                HaveCertificate = false;
            }            
            catch (Exception ex)
            {
                this.MessageLabel.Content = "Cannot connect using account \"" + P2PSettings.Default.PeerName + "\": " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                this.MessageLabel.Visibility = Visibility.Visible;
                return;
            }
            if(!HaveCertificate){
                try
                {
                    //we did not find a fitting certificate, so we just try to download one:
                    CertificateClient certificateClient = new CertificateClient();
                    certificateClient.TimeOut = 5;
                    certificateClient.ProgramName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                    certificateClient.ProgramVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
                    certificateClient.InvalidCertificateRequest += InvalidCertificateRequest;
                    certificateClient.CertificateReceived += CertificateReceived;
                    certificateClient.RequestCertificate(new CertificateRequest(P2PSettings.Default.PeerName, null, P2PBase.DecryptString(P2PSettings.Default.Password)));
                }
                catch (Exception ex)
                {
                    this.MessageLabel.Content = "Error while autodownloading your account data: " + ex.Message;
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                    return;
                }
            }

            //user entered the wrong password and the cert could not be download
            if (WrongPassword)
            {
                this.MessageLabel.Content = "Your password was wrong. We could not autodownload your account data.";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                return;
            }

            //we used login data, but our email was not authorized
            if (EmailVerificationRequired)
            {
                this.MessageLabel.Content = "The email address was not verified.\nPlease check your email account for an activation code we just sent to you and activate your account.";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                return;
            }

            //if we are here we did not find a fitting certificate in users appdate and could not download a certificate
            if (!HaveCertificate)
            {
               
                this.MessageLabel.Content = "Cannot connect, account \"" + P2PSettings.Default.PeerName + "\" not found!";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                return;
            }

            if (!P2PManager.IsConnected)
                P2PManager.Connect();
        }

        public void InvalidCertificateRequest(object sender, ProcessingErrorEventArgs args)
        {            
            switch (args.Type)
            {
                case ErrorType.EmailNotYetVerified:
                    EmailVerificationRequired = true;
                    break;
                case ErrorType.WrongPassword:
                    WrongPassword = true;
                    break;
            }
        }

        public void CertificateReceived(object sender, CertificateReceivedEventArgs args)
        {
            try
            {
                try
                {
                    if (!Directory.Exists(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY))
                    {
                        Directory.CreateDirectory(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY);
                    }
                }
                catch (Exception ex)
                {
                    this.MessageLabel.Content = "Cannot create default account data directory '" + PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY + "':\n" + ex.Message;
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                    return;
                }

                args.Certificate.SaveCrtToAppData();
                args.Certificate.SavePkcs12ToAppData(args.Certificate.Password);
                HaveCertificate = true;
                this.P2PEditor.GuiLogMessage("Autodownloaded user account data for user '" + args.Certificate.Avatar +"'", NotificationLevel.Info);
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "Could not save the received certificate to your AppData folder:\n\n" +
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message);
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                }, null);
            }
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
            P2PEditorPresentation.ShowForgotPasswordView();
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
