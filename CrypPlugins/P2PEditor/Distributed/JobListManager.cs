using System;
using System.Collections.Generic;
using System.IO;
using Cryptool.P2P;
using Cryptool.P2P.Internal;
using Cryptool.PluginBase;

namespace Cryptool.P2PEditor.Distributed
{
    public class JobListManager
    {
        private const string JoblistKey = "Cryptool.P2PEditor.JobList";
        private const string WorkspaceKeyPrefix = "Workspace";
        private readonly P2PEditor p2PEditor;

        public JobListManager(P2PEditor p2PEditor)
        {
            this.p2PEditor = p2PEditor;
        }

        public ICollection<DistributedJob> JobList()
        {
            p2PEditor.GuiLogMessage("Fetching DHT job list...", NotificationLevel.Debug);

            if (!P2PManager.IsConnected)
            {
                p2PEditor.GuiLogMessage("P2P not connected, cannot fetch job list.", NotificationLevel.Error);
                return new List<DistributedJob>();
            }

            var serialisedJobList = P2PManager.Retrieve(JoblistKey).Data;
            if (serialisedJobList == null)
            {
                // no job list in DHT, create empty list
                p2PEditor.GuiLogMessage("No list in DHT, creating empty list.", NotificationLevel.Debug);
                return new List<DistributedJob>();
            }

            return ByteArrayToJobList(serialisedJobList);
        }

        public void AddDistributedJob(DistributedJob distributedJob)
        {
            p2PEditor.GuiLogMessage("Distributing new job...", NotificationLevel.Debug);

            if (!P2PManager.IsConnected)
            {
                p2PEditor.GuiLogMessage("P2P not connected, cannot distribute job.", NotificationLevel.Error);
                return;
            }

            var currentJobList = JobList();
            currentJobList.Add(distributedJob);

            var serializedJobList = JobListToByteArray(currentJobList);
            P2PManager.Store(JoblistKey, serializedJobList);

            var workspaceData = File.ReadAllBytes(distributedJob.LocalFilePath);
            p2PEditor.GuiLogMessage(
                "Workspace size: " + workspaceData.Length + ", storing at " + GenerateWorkspaceKey(distributedJob),
                NotificationLevel.Debug);
            P2PManager.Store(GenerateWorkspaceKey(distributedJob), workspaceData);

            p2PEditor.GuiLogMessage("Distributed job " + distributedJob.Name, NotificationLevel.Info);
        }

        public void DeleteDistributedJob(DistributedJob distributedJobToDelete)
        {
            p2PEditor.GuiLogMessage("Deleting job...", NotificationLevel.Debug);

            if (!P2PManager.IsConnected)
            {
                p2PEditor.GuiLogMessage("P2P not connected, cannot distribute job.", NotificationLevel.Error);
                return;
            }

            var currentJobList = JobList();
            currentJobList.Remove(distributedJobToDelete);

            var serializedJobList = JobListToByteArray(currentJobList);
            P2PManager.Store(JoblistKey, serializedJobList);

            // Retrieve job first to satify versioned DHT
            P2PManager.Retrieve(GenerateWorkspaceKey(distributedJobToDelete));
            P2PManager.Remove(GenerateWorkspaceKey(distributedJobToDelete));

            p2PEditor.GuiLogMessage("Deleted distributed job " + distributedJobToDelete.Name, NotificationLevel.Info);
        }

        public void CompleteDistributedJob(DistributedJob distributedJob)
        {
            distributedJob.ConvertRawWorkspaceToLocalFile(P2PManager.Retrieve(GenerateWorkspaceKey(distributedJob)).Data);
        }

        public void RetrieveDownloadCount(DistributedJob distributedJob)
        {
            try
            {
                var result = P2PManager.Retrieve(GenerateDownloadCounterKey(distributedJob));

                if (result.Status == RequestResultType.KeyNotFound)
                {
                    distributedJob.Downloads = 0;
                    return;
                }

                if (result.Data != null)
                {
                    var binaryReader = new BinaryReader(new MemoryStream(result.Data));
                    distributedJob.Downloads = binaryReader.ReadInt32();
                    distributedJob.LastDownload = DateTime.FromBinary(binaryReader.ReadInt64());
                }
            }
            catch (Exception)
            {
            }
        }

        public void RetrieveCurrentStatus(DistributedJob distributedJob)
        {
            if (string.IsNullOrEmpty(distributedJob.StatusKey)) return;

            var result = P2PManager.Retrieve(distributedJob.StatusKey);
            if (result.Status != RequestResultType.Success) return;

            var status = DistributedJobSerializer.StatusFromReader(new BinaryReader(new MemoryStream(result.Data)));
            distributedJob.Status = status;
        }

        public void IncreaseDownloadCount(DistributedJob distributedJob)
        {
            RetrieveDownloadCount(distributedJob);
            distributedJob.Downloads++;

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(distributedJob.Downloads);
            binaryWriter.Write(DateTime.UtcNow.ToBinary());

            P2PManager.Store(GenerateDownloadCounterKey(distributedJob), memoryStream.ToArray());
        }

        private static string GenerateWorkspaceKey(DistributedJob distributedJob)
        {
            return string.Format("{0}.{1}.{2}", JoblistKey, WorkspaceKeyPrefix, distributedJob.Guid);
        }

        private static string GenerateDownloadCounterKey(DistributedJob distributedJob)
        {
            return string.Format("{0}.{1}.{2}.{3}", JoblistKey, WorkspaceKeyPrefix, distributedJob.Guid, "downloads");
        }

        private static byte[] JobListToByteArray(ICollection<DistributedJob> distributedJobList)
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(distributedJobList.Count);

            foreach (var distributedJob in distributedJobList)
            {
                DistributedJobSerializer.ToWriter(distributedJob, binaryWriter);
            }

            return memoryStream.ToArray();
        }

        private ICollection<DistributedJob> ByteArrayToJobList(byte[] rawData)
        {
            var distributedJobList = new List<DistributedJob>();
            var binaryReader = new BinaryReader(new MemoryStream(rawData));

            var numberOfJobs = binaryReader.ReadInt32();
            for (var i = 0; i < numberOfJobs; i++)
            {
                distributedJobList.Add(DistributedJobSerializer.FromReader(binaryReader));
            }

            return distributedJobList;
        }
    }
}