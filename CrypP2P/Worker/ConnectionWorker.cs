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

using System.ComponentModel;
using Cryptool.P2P.Helper;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase;

namespace Cryptool.P2P.Worker
{
    public class ConnectionWorker : WorkerBase
    {
        private readonly P2PBase _p2PBase;
        private readonly P2PSettings _p2PSettings;

        public ConnectionWorker(P2PBase p2PBase, P2PSettings p2PSettings)
        {
            _p2PBase = p2PBase;
            _p2PSettings = p2PSettings;
        }

        protected override void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            P2PManager.Instance.GuiLogMessage(
                P2PManager.Instance.P2PConnected()
                    ? "Connection to P2P network established."
                    : "Connection to P2P network terminated.", NotificationLevel.Info);
        }

        protected override void PerformWork(object sender, DoWorkEventArgs e)
        {
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

        protected override void PrePerformWork()
        {
        }
    }
}