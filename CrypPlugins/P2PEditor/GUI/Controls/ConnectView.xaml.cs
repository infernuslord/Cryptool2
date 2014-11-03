using System;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using Cryptool.PluginBase.Miscellaneous;
using PeersAtPlay.CertificateLibrary.Certificates;
using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using PeersAtPlay.CertificateLibrary.Network;
using Cryptool.PluginBase.Attributes;
using System.Windows.Media;

namespace Cryptool.P2PEditor.GUI.Controls
{
    [Localization("Cryptool.P2PEditor.Properties.Resources")]
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
            try
            {
                if (P2PEditor == null)
                {
                    return;
                }
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
                if (P2PEditor.Presentation != null)
                {
                    ((P2PEditorPresentation)P2PEditor.Presentation).UpdateConnectionState();
                }
            }
            catch (Exception ex)
            {
                if(P2PEditor != null)
                {
                    P2PEditor.GuiLogMessage(string.Format("Exception during RaiseP2PConnectingEvent: {0}", ex.Message), NotificationLevel.Error);
                }
            }
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

            /*
            P2PManager.ConnectionManager.OnP2PTryConnectingStateChangeOccurred += delegate(object sender, bool newState)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    RaiseP2PConnectingEvent(newState);
                }, null);
            };
            */

            //Create Cert directory if it does not exist
            try
            {
                if (!Directory.Exists(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY))
                {
                    Directory.CreateDirectory(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY);
                    this.LogMessage(String.Format(Properties.Resources.Automatic_created_account_folder_, PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY));

                }
            }
            catch (Exception ex)
            {
                this.LogMessage(String.Format(Properties.Resources.Cannot_create_default_account_data_directory_, PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY, ex.Message));
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    RaiseP2PConnectingEvent(false);
                    IsP2PConnecting = false;
                }, null);
            }

            //Get all avatar names and put them into our UsernamesListBox
            //so the user can drop down it and select an username
            try
            {
                foreach (var cert in CertificateServices.GetX509Certificates(PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY))
                {
                    try
                    {
                        this.UsernamesListBox.Items.Add(CertificateServices.GetAvatarName(cert));
                    }
                    catch (Exception)
                    { 
                    }
                }
            }
            catch (Exception)
            {                
            }
        }

        private bool HaveCertificate { get; set; }
        private bool EmailVerificationRequired { get; set; }
        private bool WrongPassword { get; set; }

        private void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(DoConnect));
            thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            thread.Start();
        }

        private void DoConnect()
        {
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
                default:
                    this.LogMessage(String.Format(Properties.Resources.InvalidCertificateRegistration_Invalid_registration___1, args.Message ?? args.Type.ToString()), true);
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
                    this.LogMessage(String.Format(Properties.Resources.Cannot_create_default_account_data_directory_, PeerCertificate.DEFAULT_USER_CERTIFICATE_DIRECTORY, ex.Message), true);
                    return;
                }

                args.Certificate.SaveCrtToAppData();
                args.Certificate.SavePkcs12ToAppData(args.Certificate.Password);
                HaveCertificate = true;
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    this.P2PEditor.GuiLogMessage(String.Format(Properties.Resources.Autodownloaded_user_account_data_for_user_, args.Certificate.Avatar),NotificationLevel.Info);
                }, null);
            }
            catch (Exception ex)
            {
                this.LogMessage(String.Format(Properties.Resources.Could_not_save_the_received_certificate_to_your_AppData_folder_,
                        (ex.GetBaseException() != null && ex.GetBaseException().Message != null ? ex.GetBaseException().Message : ex.Message)));
            }
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
            
            this.PopupUsernames.IsOpen = false;
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
        }

        private void RememberPasswordCheckbox_Click(object sender, RoutedEventArgs e)
        {
        }

        private void P2PUserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {            
            this.RaiseP2PConnectingEvent(IsP2PConnecting);
        }

        private void OnPropertyChanged_Settings(object sender, PropertyChangedEventArgs args)
        {            
          
        }

        private void P2PUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ((P2PEditorSettings)((P2PEditor)GetValue(P2PEditorProperty)).Settings).PropertyChanged += OnPropertyChanged_Settings;
        }

        private void ProxyErrorOccured(object sender, ProxyEventArgs args)
        {
            this.LogMessage(String.Format(Properties.Resources.Proxy_Error_occured_, args.StatusCode, args.Message), true);
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

        private void UsernameButton_Click(object sender, RoutedEventArgs e)
        {
            this.PopupUsernames.IsOpen = !this.PopupUsernames.IsOpen;
        }

        private void UsernamesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.Username.Text = (string)this.UsernamesListBox.SelectedItem;
            this.Password.Password = "";
        }

        private void PopupUsernames_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.PopupUsernames.IsOpen = false;
        }
    }
}
