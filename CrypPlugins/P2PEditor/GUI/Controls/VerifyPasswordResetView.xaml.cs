using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Cryptool.P2P.Types;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using PeersAtPlay.CertificateLibrary.Network;
using System.Threading;
using PeersAtPlay.CertificateLibrary.Util;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using PeersAtPlay.CertificateLibrary.Certificates;
using Cryptool.P2P;
using Cryptool.PluginBase.Attributes;
using System.Windows.Media;

namespace Cryptool.P2PEditor.GUI.Controls
{
    [Localization("Cryptool.P2PEditor.Properties.Resources")]
    public partial class VerifyPasswordResetView
    {
        public static string WorldName = ".*";

        public VerifyPasswordResetView()
        {
            InitializeComponent();            
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            this.MessageLabel.Visibility = Visibility.Hidden;
            this.MessageBox.Visibility = Visibility.Hidden;
            if (string.IsNullOrEmpty(this.ActivationCode.Text))
            {

                LogMessage(Properties.Resources.Activation_code_may_not_be_empty_);
                this.ActivationCode.Focus();
                return;
            }
          
            if (!Verification.IsValidPassword(this.PasswordField.Password))
            {
                LogMessage(Properties.Resources.Password_is_not_valid_);
                this.PasswordField.Password = "";
                this.PasswordField.Focus();
                return;
            }

            if (!Verification.IsValidPassword(this.ConfirmField.Password))
            {
                LogMessage(Properties.Resources.Password_is_not_valid_);
                this.ConfirmField.Password = "";
                this.ConfirmField.Focus();
                return;
            }

            if (this.PasswordField.Password != this.ConfirmField.Password)
            {
                LogMessage(Properties.Resources.Passwords_did_not_match_);
                this.PasswordField.Password = "";
                this.ConfirmField.Password = "";
                this.PasswordField.Focus();
                return;
            }    

            Requesting = true;
            Thread thread = new Thread(new ParameterizedThreadStart(VerifyPasswordReset));
            thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            PasswordResetVerification passwordResetVerification = new PasswordResetVerification(this.PasswordField.Password, this.ActivationCode.Text);
            thread.Start(passwordResetVerification);
        }

        public void VerifyPasswordReset(object o)
        {
            PasswordResetVerification passwordResetVerification = (PasswordResetVerification)o;

            try
            {
                CertificateClient certificateClient = new CertificateClient();

                //use a proxy server:
                if (P2PSettings.Default.UseProxy)
                {
                    certificateClient.ProxyAddress = P2PSettings.Default.ProxyServer;
                    certificateClient.ProxyPort = P2PSettings.Default.ProxyPort;
                    certificateClient.ProxyAuthName = P2PSettings.Default.ProxyUser;
                    certificateClient.ProxyAuthPassword = StringHelper.DecryptString(P2PSettings.Default.ProxyPassword);
                    certificateClient.UseProxy = true;
                    certificateClient.UseSystemWideProxy = P2PSettings.Default.UseSystemWideProxy;
                    certificateClient.SslCertificateRefused += new EventHandler<EventArgs>(delegate
                    {
                        LogMessage(Properties.Resources.SSLCertificate_revoked__Please_update_CrypTool_2_0_, true);
                    });
                    certificateClient.HttpTunnelEstablished += new EventHandler<ProxyEventArgs>(delegate
                    {
                       LogMessage(Properties.Resources.HttpTunnel_successfully_established_);
                    });
                    certificateClient.NoProxyConfigured += new EventHandler<EventArgs>(delegate
                    {
                        LogMessage(Properties.Resources.No_proxy_server_configured__Please_check_your_configuration_, true);
                    });
                    certificateClient.ProxyErrorOccured += ProxyErrorOccured;
                }

                Assembly asm = Assembly.GetEntryAssembly();
                certificateClient.ProgramName = asm.GetName().Name;
                certificateClient.ProgramVersion = AssemblyHelper.GetVersionString(asm);

                certificateClient.CertificateAuthorizationRequired += new EventHandler<EventArgs>(delegate
                {
                   LogMessage(Properties.Resources.Certificate_authorization_required);
                });

                certificateClient.CertificateReceived += CertificateReceived;
                certificateClient.InvalidEmailVerification += InvalidEmailVerification;
                certificateClient.InvalidPasswordResetVerification += InvalidPasswordResetVerification;

                certificateClient.ServerErrorOccurred += new EventHandler<ProcessingErrorEventArgs>(delegate
                {
                    LogMessage(Properties.Resources.Server_error_occurred__Please_try_again_later, true);
                });

                certificateClient.NewProtocolVersion += new EventHandler<EventArgs>(delegate
                {
                    LogMessage(Properties.Resources.New_ProtocolVersion__Please_update_CrypTool_2_0, true);
                });

                certificateClient.VerifyPasswordReset(passwordResetVerification);
            }
            catch (NetworkException nex)
            {
                LogMessage(String.Format(Properties.Resources.There_was_a_communication_problem_with_the_server, nex.Message), true);
            }
            catch (Exception ex)
            {
                LogMessage(String.Format(Properties.Resources.An_exception_occured___1, ex.Message), true);
            }
            finally
            {
                Requesting = false;                
            }
        }

        public void InvalidPasswordResetVerification(object sender, ProcessingErrorEventArgs args)
        {
            try
            {
                switch (args.Type)
                {
                    case ErrorType.AlreadyVerified:
                        LogMessage(Properties.Resources.Your_password_change_was_already_verified_, true);
                        break;

                    case ErrorType.CertificateNotYetAuthorized:
                        LogMessage(Properties.Resources.Your_account_is_not_yet_authorized_, true);
                        break;

                    case ErrorType.CertificateRevoked:
                        LogMessage(Properties.Resources.Your_account_is_revoked_, true);
                        break;

                    case ErrorType.NoCertificateFound:
                        LogMessage(Properties.Resources.Account_reset_data_not_found, true);
                        break;

                    case ErrorType.WrongCode:
                        LogMessage(Properties.Resources.Wrong_code, true);
                        break;

                    default:
                        LogMessage(String.Format(Properties.Resources.Invalid_passwort_reset_verification___0__, args.Message ?? args.Type.ToString()), true);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogMessage(String.Format(Properties.Resources.An_exception_occured___1, ex.Message), true);
                return;
            }
            finally
            {
                Requesting = false;
            } 
        }

        public void InvalidEmailVerification(object sender, ProcessingErrorEventArgs args)
        {
            try
            {
                switch (args.Type)
                {
                    case ErrorType.NoCertificateFound:
                        LogMessage(Properties.Resources.You_have_entered_a_wrong_verification_code_, true);                           
                        break;
                    case ErrorType.WrongPassword:
                        LogMessage(Properties.Resources.The_verification_code_is_ok_but_the_entered_password_was_wrong_, true);
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.PasswordField.Password = "";
                            this.PasswordField.Focus();
                        }, null);        
                        break;
                    default:
                        LogMessage(String.Format(Properties.Resources.nvalid_certificate_request___0_, args.Message ?? args.Type.ToString()), true);
                        break;
                }
            }
            catch (Exception ex) 
            {
                LogMessage(String.Format(Properties.Resources.An_exception_occured___1, ex.Message), true);                
                return;
            }
            finally
            {
                Requesting = false;
            } 
        }

        public void CertificateReceived(object sender, CertificateReceivedEventArgs args)
        {

            try
            {
                if (!Directory.Exists(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY))
                {
                    Directory.CreateDirectory(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY);
                    this.P2PEditor.GuiLogMessage(String.Format(Properties.Resources.Automatic_created_account_folder_,PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY), NotificationLevel.Info);
                }
            }
            catch (Exception ex)
            {
                LogMessage(String.Format(Properties.Resources.Cannot_create_default_account_data_directory_,PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY, ex.Message));
                return;
            }

            try
            {
                args.Certificate.SaveCrtToAppData();
                args.Certificate.SavePkcs12ToAppData(args.Certificate.Password);                
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {                    
                    this.PasswordField.Password = "";
                    this.ConfirmField.Password = "";
                    this.ActivationCode.Text = "";
                    this.ActivatePage.Visibility = System.Windows.Visibility.Hidden;
                    this.OKPage.Visibility = System.Windows.Visibility.Visible;
                }, null);                
            }
            catch (Exception ex)
            {
                LogMessage(String.Format(Properties.Resources.Could_not_save_the_received_certificate_to_your_AppData_folder_,
                    (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message)), true);
            }
            finally
            {
                Requesting = false;
            }
        }

        private bool requesting = false;
        public bool Requesting
        {
            get { return requesting; }
            set
            {
                requesting = value;
                try
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        Storyboard storyboard = (Storyboard)FindResource("AnimateWorldIcon");
                        if (requesting)
                        {
                            this.RequestLabel.Visibility = System.Windows.Visibility.Visible;
                            this.VerifyButton.IsEnabled = false;
                            storyboard.Begin();
                        }
                        else
                        {
                            this.RequestLabel.Visibility = System.Windows.Visibility.Hidden;
                            this.VerifyButton.IsEnabled = true;
                            storyboard.Stop();
                        }
                    }, null);
                }
                catch (Exception)
                {                    
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.P2PEditorPresentation.ShowForgotPasswordView();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.ActivatePage.Visibility = Visibility.Visible;
            this.OKPage.Visibility = Visibility.Hidden;
            this.P2PEditorPresentation.ShowConnectView();
        }

        private void ProxyErrorOccured(object sender, ProxyEventArgs args)
        {
            LogMessage(String.Format(Properties.Resources.Proxy_Error_occured_, args.StatusCode, args.Message), true);
        }

        /// <summary>
        /// Logs a message to the network editor gui
        /// </summary>
        /// <param name="message"></param>
        /// <param name="error"></param>
        private void LogMessage(string message, bool error = false)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (error)
                {
                    this.Erroricon.Visibility = Visibility.Visible;
                    this.MessageLabel.Foreground = Brushes.Red;
                }
                else
                {
                    this.Erroricon.Visibility = Visibility.Hidden;
                    this.MessageLabel.Foreground = Brushes.Black;
                }
                this.MessageLabel.Text = message;
                this.P2PEditor.GuiLogMessage(message, NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                this.MessageBox.Visibility = Visibility.Visible;
            }, null);
        }
    }
}