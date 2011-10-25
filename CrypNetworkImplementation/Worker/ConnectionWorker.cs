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
using Cryptool.P2PDLL.Internal;
using Cryptool.PluginBase;

namespace Cryptool.P2PDLL.Worker
{
    internal class ConnectionWorker : WorkerBase
    {
        private readonly P2PBase p2PBase;
        private readonly ConnectionManager connectionManager;
        private readonly bool connect;
        private static object syncObj = new object();

        public ConnectionWorker(P2PBase p2PBase, ConnectionManager connectionManager, bool connect)
        {
            this.p2PBase = p2PBase;
            this.connectionManager = connectionManager;
            this.connect = connect;
        }

        protected override void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        protected override void PerformWork(object sender, DoWorkEventArgs e)
        {
            // enforce strict serialization of connect/disconnects
            lock(syncObj)
            {
                if (connect)
                {
                    // clean up old one before
                    if(connectionManager.IsConnecting ||  P2PManager.IsConnected)
                    {
                        performDisconnect();
                    }
                    establishConnection();
                }
                else
                {
                    performDisconnect();
                }
                P2PManager.GuiLogMessage(
                    connect
                    ? "Connection to P2P network established."
                    : "Connection to P2P network terminated.", NotificationLevel.Info);
                connectionManager.IsConnecting = false;
                connectionManager.FireConnectionStatusChange();
            }
        }

        private void performDisconnect()
        {
            P2PManager.GuiLogMessage("Disconnecting from P2P network...", NotificationLevel.Info);
            p2PBase.SynchStop();
        }

        private void establishConnection()
        {
            P2PManager.GuiLogMessage("Connecting to P2P network...", NotificationLevel.Info);
            try
            {
                p2PBase.Initialize();
                p2PBase.SynchStart();
            }
            catch (InvalidOperationException ex)
            {
                P2PManager.GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        protected override void PrePerformWork()
        {
        }
    }
}