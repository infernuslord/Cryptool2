/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Cryptool.P2P;
using Cryptool.PluginBase;

namespace Cryptool.CrypWin
{
    public partial class MainWindow
    {
        private bool _reconnect = false;
        private bool _reconnecting = false;
        private object _reconnectSyncObject = new object();

        public static readonly DependencyProperty P2PButtonVisibilityProperty =
      DependencyProperty.Register(
      "P2PButtonVisibility",
      typeof(Visibility),
      typeof(MainWindow),
      new FrameworkPropertyMetadata(Visibility.Collapsed, FrameworkPropertyMetadataOptions.AffectsRender, null));

        [TypeConverter(typeof(Visibility))]
        public Visibility P2PButtonVisibility
        {
            get
            {
                return (Visibility)GetValue(P2PButtonVisibilityProperty);
            }
            set
            {
                SetValue(P2PButtonVisibilityProperty, value);
            }
        }

        private bool p2PIconImageRotating;
        public bool P2PIconImageRotating
        {
            get { return p2PIconImageRotating; }
            set
            {
                p2PIconImageRotating = value;

                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Storyboard p2PBigIconImageRotateStoryboard = (Storyboard)FindResource("P2PBigIconImageRotateStoryboard");
                    Storyboard p2PSmallIconImageRotateStoryboard = (Storyboard)FindResource("P2PSmallIconImageRotateStoryboard");
                    if (p2PIconImageRotating)
                    {
                        p2PBigIconImageRotateStoryboard.Begin();
                        p2PSmallIconImageRotateStoryboard.Begin();
                    }
                    else
                    {
                        p2PBigIconImageRotateStoryboard.Stop();
                        p2PSmallIconImageRotateStoryboard.Stop();
                    }
                }, null);
            }
        }

        private bool p2PIconImageGray = false;
        public bool P2PIconImageGray
        {
            get { return p2PIconImageGray; }
            set
            {
                p2PIconImageGray = value;

                this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    if (p2PIconImageGray)
                    {
                        P2PIconImageBig.Source = (ImageSource)FindResource("p2pconnectIcon");
                        P2PIconImageSmall.Source = (ImageSource)FindResource("p2pconnectIcon");

                        try
                        {
                            notifyIcon.Icon = Properties.Resources.cryptool;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        P2PIconImageBig.Source = (ImageSource)FindResource("p2pdisconnectIcon");
                        P2PIconImageSmall.Source = (ImageSource)FindResource("p2pdisconnectIcon");

                        try
                        {
                            notifyIcon.Icon = Properties.Resources.cryptoolP2P;
                        }
                        catch (Exception)
                        {
                        }
                    }

                }, null);
            }

        }

        private void InitP2P()
        {
            var reconnectTimer = new System.Windows.Forms.Timer();
            reconnectTimer.Tick += delegate
            {
                lock (_reconnectSyncObject)
                {
                    if (_reconnect && !P2PManager.IsConnected &&
                        !P2PManager.ConnectionManager.IsConnecting)
                    {
                        _reconnect = false;
                        _reconnecting = true;
                        GuiLogMessage("Lost P2P Connection. Try reconnecting...",
                                      NotificationLevel.Error);

                        P2PManager.Connect();
                    }
                }
            };

            P2PManager.ConnectionManager.OnP2PConnectionStateChangeOccurred += delegate
            {
                lock (_reconnectSyncObject)
                {
                    if (_reconnecting)
                    {
                        _reconnecting = false;

                        if (P2PManager.IsConnected)
                            GuiLogMessage("Successfully reconnected!", NotificationLevel.Balloon);
                        else
                            _reconnect = true;   //try again..
                    }
                }
            };

            P2PManager.P2PBase.OnSystemJoined += delegate
            {
                P2PIconImageGray = false;
            };
            P2PManager.P2PBase.OnSystemLeft += delegate
            {
                P2PIconImageGray = true;

                if (P2PManager.ConnectionManager.Disconnected)
                    return;
                if (!P2PManager.ConnectionManager.IsConnecting)
                    _reconnect = true;
            };
            P2PManager.ConnectionManager.OnP2PTryConnectingStateChangeOccurred += delegate(object sender, bool newState)
            {
                P2PIconImageRotating = newState;
            };
            P2PIconImageGray = true;

            reconnectTimer.Interval = 1000;
            reconnectTimer.Start();

            P2PButtonVisibility = Visibility.Visible;
        }

        private void ValidateAndSetupPeer2Peer()
        {
            P2PManager.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
            P2PManager.IsAutoconnectConsoleOptionSet = IsCommandParameterGiven("-peer2peer") || IsCommandParameterGiven("-p2p");
            P2PManager.HandleConnectOnStartup();
        }
    }
}
