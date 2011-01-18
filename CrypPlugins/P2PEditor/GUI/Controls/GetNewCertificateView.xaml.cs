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
            object[] array = new object[3];
            array[0] = this.UsernameField.Text;
            array[1] = this.EmailField.Text;
            array[2] = this.PasswordField.Password;
            thread.Start(array);
        }

        public void RetrieveCertificate(object o)
        {
            object[] array = (object[])o;
            string username = (string)array[0];
            string email = (string)array[1];
            string password = (string)array[2];

            try
            {
                CertificateClient certificateClient = new CertificateClient();
                certificateClient.CertificateAuthorizationDenied += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Certificate authorization denied";
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null);        
                });

                certificateClient.CertificateAuthorizationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Certificate authorization required";
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null); 
                });

                certificateClient.CertificateNotYetAuthorized += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Certificate not yet authorized";
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null); 
                });

                certificateClient.CertificateReceived += CertificateReceived;

                certificateClient.InvalidCertificateRequest += InvalidCertificateRequest;

                certificateClient.InvalidEmailCheck += new EventHandler<InvalidRequestEventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Invalid email address";
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null); 
                });

                certificateClient.ServerErrorOccurred += new EventHandler<EventArgs>(delegate
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

                certificateClient.RequestCertificate(username,
                                                     email,
                                                     WorldName,
                                                     password);
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

        public void InvalidCertificateRequest(object sender, InvalidRequestEventArgs args)
        {
            try
            {
                switch (args.ErrorType)
                {
                    case RespondType.AvatarAlreadyExists:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "The username already exists. Please chose another one.";
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    case RespondType.EmailAlreadyExists:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "The email already exists. Please chose another ones.";
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    case RespondType.AvatarEmailAlreadyExists:
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
                            this.MessageLabel.Content = "Invalid certificate request: " + args.ErrorType;
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
    }
}