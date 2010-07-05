using Cryptool.P2PEditor.Distributed;
using Cryptool.P2PEditor.Helper;
using KeySearcher.P2P.Presentation;

namespace KeySearcher.P2P.Storage
{
    class StatusUpdater
    {
        private readonly StatusContainer status;
        private readonly string statusKey;

        public StatusUpdater(StatusContainer status, string statusKey)
        {
            this.status = status;
            this.statusKey = statusKey;
        }

        public void SendUpdate()
        {
            var currentStatus = DistributedJobStatus.Status.Active;
            if (status.GlobalProgress == 1)
                currentStatus = DistributedJobStatus.Status.Finished;

            SendUpdate(currentStatus);
        }

        public void SendUpdate(DistributedJobStatus.Status currentStatus)
        {
            var globalProgress = status.GlobalProgress;
            if (currentStatus == DistributedJobStatus.Status.Finished) globalProgress = 1;

            DistributedStatusUpdater.UpdateStatus(statusKey, currentStatus, (long) status.TotalAmountOfParticipants,
                                                  globalProgress, status.StartDate.ToUniversalTime());
        }
    }
}
