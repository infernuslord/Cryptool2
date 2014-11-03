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
    }
}
