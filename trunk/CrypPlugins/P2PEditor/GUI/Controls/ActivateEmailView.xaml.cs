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

namespace Cryptool.P2PEditor.GUI.Controls
{
    public partial class ActivateEmailView
    {
        public static string WorldName = ".*";

        public ActivateEmailView()
        {
            InitializeComponent();            
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            this.MessageLabel.Visibility = Visibility.Hidden;
            if (string.IsNullOrEmpty(this.ActivationCodeField.Text))
            {

                this.MessageLabel.Content = "Activation code may not be empty.";
                this.MessageLabel.Visibility = Visibility.Visible;
                this.ActivationCodeField.Focus();
                return;
            }
          
            if (!Verification.IsValidPassword(this.PasswordField.Password))
            {
                this.MessageLabel.Content = "Password is not valid.";
                this.MessageLabel.Visibility = Visibility.Visible;
                this.PasswordField.Password = "";
                this.PasswordField.Focus();
                return;
            }
            
            Requesting = true;
            Thread thread = new Thread(new ParameterizedThreadStart(ActivateEmail));
            EmailVerification emailVer = new EmailVerification(this.PasswordField.Password, this.ActivationCodeField.Text, false);
            thread.Start(emailVer);
        }

        public void ActivateEmail(object o)
        {
            EmailVerification emailVer = (EmailVerification)o;

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

                certificateClient.CertificateReceived += CertificateReceived;

                certificateClient.InvalidEmailVerification += InvalidEmailVerification;

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

                certificateClient.VerifyEmail(emailVer);
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
                            this.MessageLabel.Visibility = Visibility.Visible;
                        }, null);
                        break;
                    case ErrorType.WrongPassword:
                        this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            this.MessageLabel.Content = "The verification code is ok but the entered password was wrong.";
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

            String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PeersAtPlay" + Path.DirectorySeparatorChar + "Certificates" + Path.DirectorySeparatorChar);
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                this.MessageLabel.Content = "Cannot create default account data directory '" + path + "':\n" + ex.Message;
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
                            this.ActivateButton.IsEnabled = false;
                            storyboard.Begin();
                        }
                        else
                        {
                            this.RequestLabel.Visibility = System.Windows.Visibility.Hidden;
                            this.ActivateButton.IsEnabled = true;
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
            this.P2PEditorPresentation.ShowGetNewCertificateView();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.ActivatePage.Visibility = Visibility.Visible;
            this.OKPage.Visibility = Visibility.Hidden;
            this.P2PEditorPresentation.ShowConnectView();
        }
    }
}