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

namespace Cryptool.P2PEditor.GUI.Controls
{
    public partial class GetNewCertificate
    {
        public static string WorldName = ".*";

        public GetNewCertificate()
        {
            InitializeComponent();            
        }

        private void Request_Click(object sender, RoutedEventArgs e)
        {
            this.MessageLabel.Visibility = Visibility.Hidden;
            if (!Verification.IsValidAvatar(this.UsernameField.Text))
            {

                this.MessageLabel.Content = "Username is not valid.";
                this.MessageLabel.Visibility = Visibility.Visible;     
                this.UsernameField.Focus();
                this.UsernameField.SelectAll();
                return;
            }

            if (!Verification.IsValidEmailAddress(this.EmailField.Text))
            {
                this.MessageLabel.Content = "Email is not valid.";
                this.MessageLabel.Visibility = Visibility.Visible; 
                this.EmailField.Focus();
                this.EmailField.SelectAll();
                return;
            }
            
            if (this.PasswordField.Password != this.ConfirmField.Password)
            {
                this.MessageLabel.Content = "Passwords did not match.";
                this.MessageLabel.Visibility = Visibility.Visible;
                this.PasswordField.Password = "";
                this.ConfirmField.Password = "";
                this.PasswordField.Focus();
                return;
            }    

            if (!Verification.IsValidPassword(this.PasswordField.Password))
            {
                this.MessageLabel.Content = "Password is not valid.";
                this.MessageLabel.Visibility = Visibility.Visible;
                this.PasswordField.Password = "";
                this.ConfirmField.Password = "";
                this.PasswordField.Focus();
                return;
            }
            
            Requesting = true;
            Thread thread = new Thread(new ParameterizedThreadStart(RetrieveCertificate));
            CertificateRegistration certReg = new CertificateRegistration(this.UsernameField.Text,
                                                     this.EmailField.Text,
                                                     WorldName,
                                                     this.PasswordField.Password);
            thread.Start(certReg);
        }

        public void RetrieveCertificate(object o)
        {
            CertificateRegistration certReg = (CertificateRegistration)o;

            try
            {
                CertificateClient certificateClient = new CertificateClient();
                certificateClient.ProgramName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                certificateClient.ProgramVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

                certificateClient.CertificateAuthorizationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Certificate authorization required";
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                });

                certificateClient.EmailVerificationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Registration successful. To activate your account, you need to validate your email address.\n A verification code was sent per email.";
                        this.UsernameField.Text = "";
                        this.EmailField.Text = "";
                        this.PasswordField.Password = "";
                        this.ConfirmField.Password = "";
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
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);
                });

                certificateClient.NewProtocolVersion += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "New ProtocolVersion. Please update CrypTool 2.0";
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null); 
                });

                certificateClient.RegisterCertificate(certReg);

            }
            catch (NetworkException nex)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "There was a communication problem with the server: " + nex.Message + "\n" + "Please try again later";
                    this.MessageLabel.Visibility = Visibility.Visible;
                }, null);
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.MessageLabel.Content = "An exception occured: " + ex.Message;
                    this.MessageLabel.Visibility = Visibility.Visible;
                }, null);
            }
            finally
            {
                Requesting = false;                
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
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    case ErrorType.EmailAlreadyExists:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "The email already exists. Please choose another one.";
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    case ErrorType.WrongPassword:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "The username and email already exist but the entered password was wrong.";
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {                           ;
                            this.PasswordField.Password = "";
                            this.ConfirmField.Password = "";
                            this.PasswordField.Focus();
                        }, null);        
                        break;
                    default:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Invalid registration: " + args.Type;
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                }
            }
            catch (Exception) 
            { 
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
                args.Certificate.SaveCrtToAppData();
                args.Certificate.SavePkcs12ToAppData(args.Certificate.Password);                
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {                    
                    ((P2PEditorPresentation)((P2PEditor)GetValue(P2PEditorProperty)).Presentation).Connect.Username.Text = this.UsernameField.Text;
                    ((P2PEditorPresentation)((P2PEditor)GetValue(P2PEditorProperty)).Presentation).Connect.Password.Password = this.PasswordField.Password;
                    this.UsernameField.Text = "";
                    this.EmailField.Text = "";
                    this.PasswordField.Password = "";
                    this.ConfirmField.Password = "";
                    this.RequestPage.Visibility = System.Windows.Visibility.Hidden;
                    this.OKPage.Visibility = System.Windows.Visibility.Visible;
                }, null);                
            }
            catch (Exception ex)
            {
                 this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Could not save the received certificate to your AppData folder:\n\n" +
                                (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message);
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
                            this.RequestButton.IsEnabled = false;
                            storyboard.Begin();
                        }
                        else
                        {
                            this.RequestLabel.Visibility = System.Windows.Visibility.Hidden;
                            this.RequestButton.IsEnabled = true;
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
            this.OKPage.Visibility = Visibility.Hidden;
            this.P2PEditorPresentation.ShowConnectView();
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            this.P2PEditorPresentation.ShowActivateEmailView();
        }
    }
}