using System;
using System.IO;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using Cryptool.P2PEditor.Distributed;

namespace Cryptool.P2PEditor.Helper
{
    public static class DistributedStatusUpdater
    {
        public static void UpdateStatus(string keyInDht, DistributedJobStatus.Status status, long participants, double progress, DateTime startDateUtc)
        {
            var oldStatusRequest = P2PManager.Retrieve(keyInDht);
            if (oldStatusRequest.Status == RequestResultType.Success)
            {
                var oldStatus =
                    DistributedJobSerializer.StatusFromReader(new BinaryReader(new MemoryStream(oldStatusRequest.Data)));
            }

            var distributedJobStatus = new DistributedJobStatus
                                           {
                                               CurrentStatus = status,
                                               Participants = participants,
                                               Progress = progress,
                                               StartDate = startDateUtc
                                           };

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            DistributedJobSerializer.ToWriter(distributedJobStatus, binaryWriter);

            P2PManager.Store(keyInDht, memoryStream.ToArray());
        }
    }
}
