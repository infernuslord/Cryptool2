using System;
using System.Reflection;
using System.Windows;
using Cryptool.P2P;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase.Miscellaneous;
using PeersAtPlay.CertificateLibrary.Certificates;
using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using PeersAtPlay.CertificateLibrary.Network;

namespace Cryptool.P2PEditor.GUI.Controls
{
    public partial class ConnectTab
    {

        public static readonly RoutedEvent P2PConnectingTrueEvent = EventManager.RegisterRoutedEvent("P2PConnectingTrue", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConnectTab));

        public event RoutedEventHandler P2PConnectingTrue
        {
            add { AddHandler(P2PConnectingTrueEvent, value); }
            remove { RemoveHandler(P2PConnectingTrueEvent, value); }
        }

        public static readonly RoutedEvent P2PConnectingFalseEvent = EventManager.RegisterRoutedEvent("P2PConnectingFalse", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ConnectTab));

        public event RoutedEventHandler P2PConnectingFalse
        {
            add { AddHandler(P2PConnectingFalseEvent, value); }
            remove { RemoveHandler(P2PConnectingFalseEvent, value); }
        }

        public void RaiseP2PConnectingEvent(bool b)
        {
            if (b)
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(ConnectTab.P2PConnectingTrueEvent);
                RaiseEvent(newEventArgs);                
            }
            else 
            {
                RoutedEventArgs newEventArgs = new RoutedEventArgs(ConnectTab.P2PConnectingFalseEvent);
                RaiseEvent(newEventArgs);
            }
            ((P2PEditorPresentation)P2PEditor.Presentation).UpdateConnectionState();
        }

        public static readonly DependencyProperty IsP2PConnectingProperty =
            DependencyProperty.Register("IsP2PConnecting",
                                        typeof(
                                            Boolean),
                                        typeof(
                                            ConnectTab), new PropertyMetadata(false, new PropertyChangedCallback(OnIsP2PConnectingPropertyPropertyChanged)));

        public Boolean IsP2PConnecting
        {
            get { return (Boolean)GetValue(IsP2PConnectingProperty); }
            set { SetValue(IsP2PConnectingProperty, value); }
        }

        private static void OnIsP2PConnectingPropertyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }



        public ConnectTab()
        {
            InitializeComponent();

            P2PManager.ConnectionManager.OnP2PTryConnectingStateChangeOccurred += new ConnectionManager.P2PTryConnectingStateChangeEventHandler(delegate(object sender, bool newState)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    RaiseP2PConnectingEvent(newState);
                }, null);
            });            
        }

        private bool HaveCertificate { get; set; }
        private bool EmailVerificationRequired { get; set; }
        private bool WrongPassword { get; set; }

        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(DoConnect));
            thread.Start();
        }

        private void DoConnect()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                RaiseP2PConnectingEvent(true);
                IsP2PConnecting = true;                
            }, null);            
            HaveCertificate = true;
            EmailVerificationRequired = false;
            WrongPassword = false;

            string password = null;
            if (P2PSettings.Default.RememberPassword)
            {
                password = P2PBase.DecryptString(P2PSettings.Default.Password);
            }
            else
            {
                password = P2PBase.DecryptString(P2PBase.Password);
            }

            try
            {
                if (!Directory.Exists(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY))
                {
                    Directory.CreateDirectory(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY);
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.P2PEditor.GuiLogMessage("Automatic created account folder: " + PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY, NotificationLevel.Info);
                    }, null);
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "Cannot create default account data directory '" + PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY + "':\n" + ex.Message;
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                    RaiseP2PConnectingEvent(false);
                    IsP2PConnecting = false;
                }, null);                
                return;
            }

            try
            {

                if (CertificateServices.GetPeerCertificateByAvatar(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY,
                    P2PSettings.Default.PeerName, password) == null)
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
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "Cannot connect using account \"" + P2PSettings.Default.PeerName + "\": " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                    RaiseP2PConnectingEvent(false);
                    IsP2PConnecting = false;
                }, null);
                return;
            }
            if (!HaveCertificate)
            {
                try
                {
                    //we did not find a fitting certificate, so we just try to download one:
                    CertificateClient certificateClient = new CertificateClient();
                    certificateClient.ServerErrorOccurred += InvalidCertificateRequest;

                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "No account data found for \"" + P2PSettings.Default.PeerName + "\".\nTry to download from server...";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);

                    //use a proxy server:
                    if (P2PSettings.Default.UseProxy)
                    {
                        certificateClient.ProxyAddress = P2PSettings.Default.ProxyServer;
                        certificateClient.ProxyPort = P2PSettings.Default.ProxyPort;
                        certificateClient.ProxyAuthName = P2PSettings.Default.ProxyUser;
                        certificateClient.ProxyAuthPassword = P2PBase.DecryptString(P2PSettings.Default.ProxyPassword);
                        certificateClient.UseProxy = true;
                        certificateClient.UseSystemWideProxy = P2PSettings.Default.UseSystemWideProxy;
                        certificateClient.SslCertificateRefused += new EventHandler<EventArgs>(delegate
                        {
                            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                this.MessageLabel.Content = "SSLCertificate revoked. Please update CrypTool 2.0.";
                                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                                this.MessageLabel.Visibility = Visibility.Visible;
                            }, null);
                        });
                        certificateClient.HttpTunnelEstablished += new EventHandler<ProxyEventArgs>(delegate
                        {
                            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                this.P2PEditor.GuiLogMessage("HttpTunnel successfully established", NotificationLevel.Debug);
                            }, null);
                        });
                        certificateClient.NoProxyConfigured += new EventHandler<EventArgs>(delegate
                        {
                            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                this.MessageLabel.Content = "No proxy server configured. Please check your configuration.";
                                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                                this.MessageLabel.Visibility = Visibility.Visible;
                            }, null);
                        });
                        certificateClient.ProxyErrorOccured += ProxyErrorOccured;
                    }

                    certificateClient.TimeOut = 30;
                    Assembly asm = Assembly.GetEntryAssembly();
                    certificateClient.ProgramName = asm.GetName().Name;
                    certificateClient.ProgramVersion = AssemblyHelper.GetVersionString(asm);
                    certificateClient.InvalidCertificateRequest += InvalidCertificateRequest;
                    certificateClient.CertificateReceived += CertificateReceived;

                    certificateClient.RequestCertificate(new CertificateRequest(P2PSettings.Default.PeerName, null, password));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Error while autodownloading your account data: " + ex.Message;
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                        this.MessageLabel.Visibility = Visibility.Visible;
                        RaiseP2PConnectingEvent(false);
                        IsP2PConnecting = false;
                    }, null);
                    return;
                }
            }

            //user entered the wrong password and the cert could not be download
            if (WrongPassword)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "Your password was wrong. We could not autodownload your account data.";
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                    this.MessageLabel.Visibility = Visibility.Visible;
                    RaiseP2PConnectingEvent(false);
                    IsP2PConnecting = false;
                }, null);                
                return;
            }

            //we used login data, but our email was not authorized
            if (EmailVerificationRequired)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "The email address was not verified.\nPlease check your email account for an activation code we just sent to you and activate your account.";
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                    this.MessageLabel.Visibility = Visibility.Visible;
                    RaiseP2PConnectingEvent(false);
                    IsP2PConnecting = false;
                }, null);                
                return;
            }

            //if we are here we did not find a fitting certificate in users appdata and could not download a certificate
            if (!HaveCertificate)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "Cannot connect, account \"" + P2PSettings.Default.PeerName + "\" not found!";
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                    this.MessageLabel.Visibility = Visibility.Visible;
                    RaiseP2PConnectingEvent(false);
                    IsP2PConnecting = false;
                }, null);                
                return;
            }

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.MessageLabel.Visibility = Visibility.Hidden;
            }, null);   

            if (!P2PManager.IsConnected)
                P2PManager.Connect();                        
        }

        private void InvalidCertificateRequest(object sender, ProcessingErrorEventArgs args)
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

        private void CertificateReceived(object sender, CertificateReceivedEventArgs args)
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
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Cannot create default account data directory '" + PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY + "':\n" + ex.Message;
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                    return;
                }

                args.Certificate.SaveCrtToAppData();
                args.Certificate.SavePkcs12ToAppData(args.Certificate.Password);
                HaveCertificate = true;
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.P2PEditor.GuiLogMessage("Autodownloaded user account data for user '" + args.Certificate.Avatar + "'", NotificationLevel.Info);
                }, null);
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

        private void RememberPasswordCheckbox_Click(object sender, RoutedEventArgs e)
        {
            ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).RememberPassword = (bool)this.RememberPasswordCheckbox.IsChecked;
        }

        private void P2PUserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {            
            this.Username.Text = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).PeerName;
            this.Password.Password = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).Password;
            this.RememberPasswordCheckbox.IsChecked = ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).RememberPassword;
            this.RaiseP2PConnectingEvent(IsP2PConnecting);
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
            if (args.PropertyName.Equals("RememberPassword"))
            {
                //this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                //{
                if (this.RememberPasswordCheckbox.IsChecked != ((P2PEditorSettings)sender).RememberPassword)
                {
                    this.RememberPasswordCheckbox.IsChecked = ((P2PEditorSettings)sender).RememberPassword;
                }
                //},null);
            }
        }

        private void P2PUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).PropertyChanged += OnPropertyChanged_Settings;
        }

        private void ProxyErrorOccured(object sender, ProxyEventArgs args)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.MessageLabel.Content = "Proxy Error (" + args.StatusCode + ") occured:" + args.Message;
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                this.MessageLabel.Visibility = Visibility.Visible;
            }, null);
        }
    }
}
