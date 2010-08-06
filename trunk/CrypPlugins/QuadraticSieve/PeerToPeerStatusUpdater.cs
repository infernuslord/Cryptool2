using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.P2PEditor.Helper;
using Cryptool.P2PEditor.Distributed;
using Cryptool.P2P;

namespace Cryptool.Plugins.QuadraticSieve
{
    class PeerToPeerStatusUpdater
    {
        private PeerToPeer peerToPeer;

        public PeerToPeerStatusUpdater(PeerToPeer peerToPeer)
        {
            this.peerToPeer = peerToPeer;
        }

        public void UpdateStatus(double progress)
        {
            if (!P2PManager.IsConnected)
                return;

            DistributedJobStatus.Status status;
            if (progress == 1)
                status = DistributedJobStatus.Status.Finished;
            else
                status = DistributedJobStatus.Status.Active;

            DistributedStatusUpdater.UpdateStatus(peerToPeer.StatusKey(), status, peerToPeer.getActivePeers(), progress, new DateTime());
        }
    }
}
