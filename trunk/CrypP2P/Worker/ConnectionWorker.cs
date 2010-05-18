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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevComponents.WpfRibbon;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Cryptool.PluginBase;
using Cryptool.P2P.Internal;
using Cryptool.P2P.Helper;

namespace Cryptool.P2P.Worker
{
    class ConnectionWorker : WorkerBase
    {
        P2PBase p2pBase;
        P2PSettings p2pSettings;
        ButtonDropDown p2pButton;

        public ConnectionWorker(P2PBase p2pBase, P2PSettings p2pSettings, ButtonDropDown p2pButton)
        {
            this.p2pBase = p2pBase;
            this.p2pSettings = p2pSettings;
            this.p2pButton = p2pButton;
        }

        protected override void PerformWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            //p2pButton.IsEnabled = false;

            if (!p2pBase.Started)
            {
                P2PManager.Instance.GuiLogMessage("Connecting to P2P network...", NotificationLevel.Info);

                // Validate certificats
                if (!PAPCertificate.CheckAndInstallPAPCertificates())
                {
                    P2PManager.Instance.GuiLogMessage("Certificates not validated, P2P might not be working!", NotificationLevel.Error);
                    return;
                }

                p2pBase.Initialize(p2pSettings);
                p2pBase.SynchStart();
            }
            else
            {
                P2PManager.Instance.GuiLogMessage("Disconnecting from P2P network...", NotificationLevel.Info);
                p2pBase.SynchStop();
            }
        }

        protected override void WorkComplete(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // Update image according to new state
            Image newImage = new Image();

            if (P2PManager.Instance.P2PConnected())
            {
                P2PManager.Instance.GuiLogMessage("Connection to P2P network established.", NotificationLevel.Info);
                P2PManager.Instance.GuiLogMessage("P2P user info: " + P2PManager.Instance.UserInfo(), NotificationLevel.Debug);
                newImage.Source = new BitmapImage(new Uri(P2PManager.P2PDisconnectImageURI, UriKind.RelativeOrAbsolute));
            }
            else
            {
                P2PManager.Instance.GuiLogMessage("Connection to P2P network terminated.", NotificationLevel.Info);
                newImage.Source = new BitmapImage(new Uri(P2PManager.P2PConnectImageURI, UriKind.RelativeOrAbsolute));
            }

            newImage.Stretch = Stretch.Uniform;
            newImage.Width = 40;
            newImage.Height = 40;
            p2pButton.Image = newImage;

            p2pButton.IsEnabled = true;
        }

        protected override void PrePerformWork()
        {
            p2pButton.IsEnabled = false;
        }
    }
}
