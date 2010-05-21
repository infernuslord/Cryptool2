/*
   Copyright 2010 Paul Lelgemann, University of Duisburg-Essen

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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cryptool.P2P.Helper;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase;
using DevComponents.WpfRibbon;

namespace Cryptool.P2P.Worker
{
    internal class ConnectionWorker : WorkerBase
    {
        private readonly P2PBase _p2PBase;
        private readonly ButtonDropDown _p2PButton;
        private readonly P2PSettings _p2PSettings;

        public ConnectionWorker(P2PBase p2PBase, P2PSettings p2PSettings, ButtonDropDown p2PButton)
        {
            _p2PBase = p2PBase;
            _p2PSettings = p2PSettings;
            _p2PButton = p2PButton;
        }

        protected override void PerformWork(object sender, DoWorkEventArgs e)
        {
            //p2pButton.IsEnabled = false;

            if (!_p2PBase.Started)
            {
                P2PManager.Instance.GuiLogMessage("Connecting to P2P network...", NotificationLevel.Info);

                // Validate certificats
                if (!PAPCertificate.CheckAndInstallPAPCertificates())
                {
                    P2PManager.Instance.GuiLogMessage("Certificates not validated, P2P might not be working!",
                                                      NotificationLevel.Error);
                    return;
                }

                _p2PBase.Initialize(_p2PSettings);
                _p2PBase.SynchStart();
            }
            else
            {
                P2PManager.Instance.GuiLogMessage("Disconnecting from P2P network...", NotificationLevel.Info);
                _p2PBase.SynchStop();
            }
        }

        protected override void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // Update image according to new state
            var newImage = new Image();

            if (P2PManager.Instance.P2PConnected())
            {
                P2PManager.Instance.GuiLogMessage("Connection to P2P network established.", NotificationLevel.Info);
                P2PManager.Instance.GuiLogMessage("P2P user info: " + P2PManager.Instance.UserInfo(),
                                                  NotificationLevel.Debug);
                newImage.Source = new BitmapImage(new Uri(P2PManager.P2PDisconnectImageUri, UriKind.RelativeOrAbsolute));
            }
            else
            {
                P2PManager.Instance.GuiLogMessage("Connection to P2P network terminated.", NotificationLevel.Info);
                newImage.Source = new BitmapImage(new Uri(P2PManager.P2PConnectImageUri, UriKind.RelativeOrAbsolute));
            }

            newImage.Stretch = Stretch.Uniform;
            newImage.Width = 40;
            newImage.Height = 40;
            _p2PButton.Image = newImage;

            _p2PButton.IsEnabled = true;
        }

        protected override void PrePerformWork()
        {
            _p2PButton.IsEnabled = false;
        }
    }
}