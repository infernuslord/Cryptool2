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
    internal class ConnectionWorker : WorkerBase
    {
        private readonly P2PBase p2PBase;
        private readonly ConnectionManager connectionManager;

        public ConnectionWorker(P2PBase p2PBase, ConnectionManager connectionManager)
        {
            this.p2PBase = p2PBase;
            this.connectionManager = connectionManager;
        }

        protected override void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            P2PManager.GuiLogMessage(
                p2PBase.IsConnected
                    ? "Connection to P2P network established."
                    : "Connection to P2P network terminated.", NotificationLevel.Info);
            connectionManager.IsConnecting = false;
            connectionManager.FireConnectionStatusChange();
        }

        protected override void PerformWork(object sender, DoWorkEventArgs e)
        {
            if (!p2PBase.IsConnected)
            {
                P2PManager.GuiLogMessage("Connecting to P2P network...", NotificationLevel.Info);

                // Validate certificats
                if (!PAPCertificate.CheckAndInstallPAPCertificates())
                {
                    P2PManager.GuiLogMessage("Certificates not validated, P2P might not be working!",
                                                      NotificationLevel.Error);
                    return;
                }

                p2PBase.Initialize();
                p2PBase.SynchStart();
            }
            else
            {
                P2PManager.GuiLogMessage("Disconnecting from P2P network...", NotificationLevel.Info);
                p2PBase.SynchStop();
            }
        }

        protected override void PrePerformWork()
        {
        }
    }
}