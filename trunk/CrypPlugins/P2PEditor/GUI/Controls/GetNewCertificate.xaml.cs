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
            if (!Verification.IsValidAvatar(this.UsernameField.Text))
            {
                System.Windows.MessageBox.Show("Username is not valid.", "Username is not valid");                
                this.UsernameField.Focus();
                this.UsernameField.SelectAll();
                return;
            }

            if (!Verification.IsValidEmailAddress(this.EmailField.Text))
            {
                System.Windows.MessageBox.Show("Email is not valid.", "Email is not valid");
                this.EmailField.Focus();
                this.EmailField.SelectAll();
                return;
            }
            
            if (this.PasswordField.Password != this.ConfirmField.Password)
            {
                System.Windows.MessageBox.Show("Passwords did not match", "Passwords did not match");
                this.PasswordField.Password = "";
                this.ConfirmField.Password = "";
                this.PasswordField.Focus();
                return;
            }    

            if (!Verification.IsValidPassword(this.PasswordField.Password))
            {
                System.Windows.MessageBox.Show("Password is not valid.", "Password is not valid");
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
                    System.Windows.MessageBox.Show("Certificate authorization denied", "Certificate authorization denied");
                });

                certificateClient.CertificateAuthorizationRequired += new EventHandler<EventArgs>(delegate
                {
                    System.Windows.MessageBox.Show("Certificate authorization required", "Certificate authorization required");
                });

                certificateClient.CertificateNotYetAuthorized += new EventHandler<EventArgs>(delegate
                {
                    System.Windows.MessageBox.Show("Certificate not yet authorized", "Certificate not yet authorized");
                });

                certificateClient.CertificateReceived += CertificateReceived;

                certificateClient.InvalidCertificateRequest += InvalidCertificateRequest;

                certificateClient.InvalidEmailCheck += new EventHandler<InvalidRequestEventArgs>(delegate
                {
                    System.Windows.MessageBox.Show("Invalid email address", "Invalid email address");
                });

                certificateClient.ServerErrorOccurred += new EventHandler<EventArgs>(delegate
                {
                    System.Windows.MessageBox.Show("Server error occurred. Please try again later", "Server error occurred");
                });

                certificateClient.NewProtocolVersion += new EventHandler<EventArgs>(delegate
                {
                    System.Windows.MessageBox.Show("The protocol of the certificate server is different from the clients one. Please update.", "Protocol mismatch");
                });

                certificateClient.RequestCertificate(username,
                                                     email,
                                                     WorldName,
                                                     password);
            }
            catch (NetworkException nex)
            {
                System.Windows.MessageBox.Show("There was a communication problem with the server: " + nex.Message + "\n" + "Please try again later"  , "Communication problem");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An exception occured: " + ex.Message, "Exception occured");
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
                        System.Windows.MessageBox.Show("The username already exists. Please chose another one.", "Username already exists");
                        break;
                    case RespondType.EmailAlreadyExists:
                        System.Windows.MessageBox.Show("The email already exists. Please chose another ones.", "Email already exists");
                        break;
                    case RespondType.AvatarEmailAlreadyExists:
                        System.Windows.MessageBox.Show("The username and email already exist but the entered password was wrong. Either enter a new username and email combination or enter the correct password to receive the certificate again", "Username and email already exist");
                        break;
                    default:
                        System.Windows.MessageBox.Show("Invalid certificate request: " + args.ErrorType, "Invalid certificate request");
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
                System.Windows.MessageBox.Show("Certificate received and stored.", "Certificate received and stored");
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.UsernameField.Text = "";
                    this.EmailField.Text = "";
                    this.PasswordField.Password = "";
                    this.ConfirmField.Password = "";
                }, null);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Could not save the received certificate to your AppData folder:\n\n" +
                       (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message),
                       "Error while saving the certificate");
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
    }
}