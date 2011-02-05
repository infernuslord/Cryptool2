﻿using System;
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
using Cryptool.P2P;
using Cryptool.P2P.Internal;

namespace Cryptool.P2PEditor.GUI.Controls
{
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
            if (string.IsNullOrEmpty(this.ActivationCode.Text))
            {

                this.MessageLabel.Content = "Activation code may not be empty.";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                this.ActivationCode.Focus();
                return;
            }
          
            if (!Verification.IsValidPassword(this.PasswordField.Password))
            {
                this.MessageLabel.Content = "Password is not valid.";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                this.PasswordField.Password = "";
                this.PasswordField.Focus();
                return;
            }

            if (!Verification.IsValidPassword(this.ConfirmField.Password))
            {
                this.MessageLabel.Content = "Confirm is not valid.";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                this.ConfirmField.Password = "";
                this.ConfirmField.Focus();
                return;
            }

            if (this.PasswordField.Password != this.ConfirmField.Password)
            {
                this.MessageLabel.Content = "Passwords did not match.";
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                this.MessageLabel.Visibility = Visibility.Visible;
                this.PasswordField.Password = "";
                this.ConfirmField.Password = "";
                this.PasswordField.Focus();
                return;
            }    

            Requesting = true;
            Thread thread = new Thread(new ParameterizedThreadStart(VerifyPasswordReset));
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

                certificateClient.ProgramName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                certificateClient.ProgramVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

                certificateClient.CertificateAuthorizationRequired += new EventHandler<EventArgs>(delegate
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        this.MessageLabel.Content = "Certificate authorization required";
                        this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                        this.MessageLabel.Visibility = Visibility.Visible;
                    }, null); 
                });

                certificateClient.CertificateReceived += CertificateReceived;
                certificateClient.InvalidEmailVerification += InvalidEmailVerification;
                certificateClient.InvalidPasswordResetVerification += InvalidPasswordResetVerification;

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

                certificateClient.VerifyPasswordReset(passwordResetVerification);
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

        public void InvalidPasswordResetVerification(object sender, ProcessingErrorEventArgs args)
        {
            try
            {
                switch (args.Type)
                {
                    case ErrorType.AlreadyVerified:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Your password change was already verified.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;

                    case ErrorType.CertificateNotYetAuthorized:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Your account is not yet authorized.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;

                    case ErrorType.CertificateRevoked:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Your account is revoked.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;

                    case ErrorType.NoCertificateFound:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Account reset data not found";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;

                    case ErrorType.WrongCode:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Wrong code";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;

                    default:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Invalid passwort reset verification: " + args.Message;
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

        public void InvalidEmailVerification(object sender, ProcessingErrorEventArgs args)
        {
            try
            {
                switch (args.Type)
                {
                    case ErrorType.NoCertificateFound:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "You have entered a wrong verification code.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    case ErrorType.WrongPassword:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "The verification code is ok but the entered password was wrong.";
                            this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Info);
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {                           ;
                            this.PasswordField.Password = "";
                            this.PasswordField.Focus();
                        }, null);        
                        break;
                    default:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "Invalid certificate request: " + args.Type;
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
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.MessageLabel.Content = "Proxy Error (" + args.StatusCode + ") occured:" + args.Message;
                this.P2PEditor.GuiLogMessage(this.MessageLabel.Content.ToString(), NotificationLevel.Error);
                this.MessageLabel.Visibility = Visibility.Visible;
            }, null);
        }
    }
}