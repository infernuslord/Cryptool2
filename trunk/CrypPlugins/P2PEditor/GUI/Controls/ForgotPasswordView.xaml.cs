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
    public partial class ForgotPasswordView
    {
        public static string WorldName = ".*";

        public ForgotPasswordView()
        {
            InitializeComponent();            
        }

        private void Request_Click(object sender, RoutedEventArgs e)
        {
            this.MessageLabel.Visibility = Visibility.Hidden;
            this.MessageBox.Visibility = Visibility.Hidden;
            if (!Verification.IsValidAvatar(this.UsernameField.Text))
            {
                this.LogMessage(Properties.Resources.Username_is_not_valid_);
                this.UsernameField.Focus();
                this.UsernameField.SelectAll();
                return;
            }

            
            Requesting = true;
            Thread thread = new Thread(new ParameterizedThreadStart(ResetPassword));
            thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            PasswordReset passwordReset = new PasswordReset(this.UsernameField.Text, null);
            thread.Start(passwordReset);
        }

        public void ResetPassword(object o)
        {
            PasswordReset passwordReset = (PasswordReset)o;

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
                       this.LogMessage(Properties.Resources.SSLCertificate_revoked__Please_update_CrypTool_2_0_,true);
                    });
                    certificateClient.HttpTunnelEstablished += new EventHandler<ProxyEventArgs>(delegate
                    {
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.P2PEditor.GuiLogMessage(Properties.Resources.HttpTunnel_successfully_established_, NotificationLevel.Debug);
                        }, null);
                    });
                    certificateClient.NoProxyConfigured += new EventHandler<EventArgs>(delegate
                        {
                            this.LogMessage(Properties.Resources.No_proxy_server_configured__Please_check_your_configuration_, true);
                        });
                    certificateClient.ProxyErrorOccured += ProxyErrorOccured;
                }

                Assembly asm = Assembly.GetEntryAssembly();
                certificateClient.ProgramName = asm.GetName().Name;
                certificateClient.ProgramVersion = AssemblyHelper.GetVersionString(asm);
                certificateClient.InvalidPasswordReset += InvalidPasswordReset;
                certificateClient.PasswordResetVerificationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.LogMessage(Properties.Resources.Please_check_your_email_account_for_a_password_reset_verification_code_);
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.UsernameField.Text = "";
                    }, null);
                });
                certificateClient.CertificateAuthorizationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.LogMessage(Properties.Resources.Certificate_authorization_required);
                });
                certificateClient.EmailVerificationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.LogMessage(Properties.Resources.Registration_successful_To_activate_your_account_you_need_);                     
                });
                certificateClient.CertificateReceived += CertificateReceived;
                certificateClient.InvalidCertificateRegistration += InvalidCertificateRegistration;
                certificateClient.ServerErrorOccurred += new EventHandler<ProcessingErrorEventArgs>(delegate
                {
                    this.LogMessage(Properties.Resources.Server_error_occurred__Please_try_again_later, true);
                });

                certificateClient.NewProtocolVersion += new EventHandler<EventArgs>(delegate
                {
                    this.LogMessage(Properties.Resources.New_ProtocolVersion__Please_update_CrypTool_2_0, true);
                });

                certificateClient.ResetPassword(passwordReset);

            }
            catch (NetworkException nex)
            {
                this.LogMessage(String.Format(Properties.Resources.There_was_a_communication_problem_with_the_server, nex.Message), true);
            }
            catch (Exception ex)
            {
                this.LogMessage(String.Format(Properties.Resources.An_exception_occured___1, ex.Message), true);
            }
            finally
            {
                Requesting = false;                
            }
        }

        public void InvalidPasswordReset(object sender, ProcessingErrorEventArgs args)
        {
            switch (args.Type)
            {
                case ErrorType.CertificateNotYetAuthorized:
                    this.LogMessage(Properties.Resources.Your_certificate_was_not_authorized__so_a_reset_is_not_possible_at_this_moment);
                    break;
                case ErrorType.CertificateRevoked:
                    this.LogMessage(Properties.Resources.Your_certificate_was_revoked_You_can_not_reset_it);
                    break;
                case ErrorType.DeserializationFailed:
                    this.LogMessage(Properties.Resources.Deserialization_of_communication_packet_on_the_server_side_failed__Please_try_again_, true);
                    break;
                case ErrorType.NoCertificateFound:
                    this.LogMessage(Properties.Resources.The_username_does_not_exist_);
                    break;
                case ErrorType.SmtpServerDown:
                    this.LogMessage(Properties.Resources.Our_email_server_is_currently_offline__Please_try_again_later_);
                    break;
                default:
                    this.LogMessage(String.Format(Properties.Resources.We_had_an_error___0_, args.Message ?? args.Type.ToString()), true);
                    break;
            }
        }

        public void InvalidCertificateRegistration(object sender, ProcessingErrorEventArgs args)
        {
            try
            {
                switch (args.Type)
                {
                    case ErrorType.AvatarAlreadyExists:
                        this.LogMessage(Properties.Resources.The_username_already_exists__Please_choose_another_one_, true);
                        break;
                    case ErrorType.EmailAlreadyExists:
                        this.LogMessage(Properties.Resources.The_email_already_exists__Please_choose_another_one_, true);
                        break;
                    case ErrorType.WrongPassword:
                        this.LogMessage(Properties.Resources.The_username_and_email_already_exist_but_the_entered_password_was_wrong_, true);
                        break;
                    default:
                        this.LogMessage(String.Format(Properties.Resources.InvalidCertificateRegistration_Invalid_registration___1, args.Message ?? args.Type.ToString()), true);
                        break;
                }
            }
            catch (Exception ex) 
            {
                this.LogMessage(String.Format(Properties.Resources.An_exception_occured___1, ex.Message), true);
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
                try
                {
                    if (!Directory.Exists(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY))
                    {
                        Directory.CreateDirectory(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY);
                        this.P2PEditor.GuiLogMessage(String.Format(Properties.Resources.Automatic_created_account_folder_, PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY), NotificationLevel.Info);
                    }
                }
                catch (Exception ex)
                {
                    this.LogMessage(String.Format(Properties.Resources.Cannot_create_default_account_data_directory_, PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY, ex.Message), true);
                    return;
                }

                args.Certificate.SaveCrtToAppData();
                args.Certificate.SavePkcs12ToAppData(args.Certificate.Password);                
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {                    
                    ((P2PEditorPresentation)((P2PEditor)GetValue(P2PEditorProperty)).Presentation).Connect.Username.Text = this.UsernameField.Text;
                    this.UsernameField.Text = "";                 
                    this.RequestPage.Visibility = System.Windows.Visibility.Hidden;
                }, null);                
            }
            catch (Exception ex)
            {
                this.LogMessage(String.Format(Properties.Resources.Could_not_save_the_received_certificate_to_your_AppData_folder_,
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
                            storyboard.Begin();
                        }
                        else
                        {
                            this.RequestLabel.Visibility = System.Windows.Visibility.Hidden;
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
            this.P2PEditorPresentation.ShowConnectView();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.RequestPage.Visibility = Visibility.Visible;
            this.P2PEditorPresentation.ShowConnectView();
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            this.P2PEditorPresentation.ShowVerifyPasswordResetView();
        }

        private void ProxyErrorOccured(object sender, ProxyEventArgs args)
        {
            this.LogMessage(String.Format(Properties.Resources.Proxy_Error_occured_, args.Message), true);
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