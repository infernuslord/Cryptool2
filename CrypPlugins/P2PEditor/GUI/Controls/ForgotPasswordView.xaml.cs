using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Cryptool.P2PEditor.Distributed;
using Cryptool.P2PEditor.Worker;
using Cryptool.PluginBase;
using PeersAtPlay.CertificateLibrary.Network;
using System.Threading;
using PeersAtPlay.CertificateLibrary.Util;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using PeersAtPlay.CertificateLibrary.Certificates;

namespace Cryptool.P2PEditor.GUI.Controls
{
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
            if (!Verification.IsValidAvatar(this.UsernameField.Text))
            {
                this.MessageLabel.Content = "Username is not valid.";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;     
                this.UsernameField.Focus();
                this.UsernameField.SelectAll();
                return;
            }

            
            Requesting = true;
            Thread thread = new Thread(new ParameterizedThreadStart(ResetPassword));
            PasswordReset passwordReset = new PasswordReset(this.UsernameField.Text, null);
            thread.Start(passwordReset);
        }

        public void ResetPassword(object o)
        {
            PasswordReset passwordReset = (PasswordReset)o;

            try
            {
                CertificateClient certificateClient = new CertificateClient();
                certificateClient.ProgramName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                certificateClient.ProgramVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
                certificateClient.InvalidPasswordReset += InvalidPasswordReset;
                certificateClient.PasswordResetVerificationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Please check your email account for a password reset verification code.";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                        this.UsernameField.Text = "";
                    }, null);
                });
                certificateClient.CertificateAuthorizationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Certificate authorization required";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                });

                certificateClient.EmailVerificationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Registration successful. To activate your account, you need to validate your email address.\n A verification code was sent per email.";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.UsernameField.Text = "";                     
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                });

                certificateClient.CertificateReceived += CertificateReceived;

                certificateClient.InvalidCertificateRegistration += InvalidCertificateRegistration;

                certificateClient.ServerErrorOccurred += new EventHandler<ProcessingErrorEventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Server error occurred. Please try again later";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                });

                certificateClient.NewProtocolVersion += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "New ProtocolVersion. Please update CrypTool 2.0";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null); 
                });

                certificateClient.ResetPassword(passwordReset);

            }
            catch (NetworkException nex)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "There was a communication problem with the server: " + nex.Message + "\n" + "Please try again later";
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                }, null);
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "An exception occured: " + ex.Message;
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                }, null);
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
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Your certificate was not authorized,\nso a reset is not possible at this moment";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                    break;
                case ErrorType.CertificateRevoked:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Your certificate was revoked.\nYou can not reset it";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                    break;
                case ErrorType.DeserializationFailed:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Deserialization of communication packet on the server side failed.\nPlease try again.";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                    break;
                case ErrorType.NoCertificateFound:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "The username does not exist";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                    break;
                case ErrorType.SmtpServerDown:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Our email server is currently offline.\nPlease try again later.";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                    break;
                default:
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "We had an error: " + args.Message;
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
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
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "The username already exists. Please choose another one.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    case ErrorType.EmailAlreadyExists:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "The email already exists. Please choose another one.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    case ErrorType.WrongPassword:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "The username and email already exist but the entered password was wrong.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    default:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Invalid registration: " + args.Type;
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                }
            }
            catch (Exception ex) 
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "Exception occured: " + ex.Message;
                    this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                    this.MessageLabel.Visibility = Visibility.Visible;
                }, null);
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
                 this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Could not save the received certificate to your AppData folder:\n\n" +
                                (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message);
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
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
    }
}