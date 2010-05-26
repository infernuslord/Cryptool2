using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cryptool.P2P;
using Cryptool.PluginBase;

namespace Cryptool.P2PEditor.Distributed
{
    public static class JobListManager
    {
        private const string JoblistKey = "Cryptool.P2PEditor.JobList";
        private const string WorkspaceKeyPrefix = "Workspace";

        public static List<DistributedJob> JobList()
        {
            P2PEditor.Instance.GuiLogMessage("Fetching DHT job list...", NotificationLevel.Debug);

            if (!P2PManager.Instance.P2PConnected())
            {
                P2PEditor.Instance.GuiLogMessage("P2P not connected, cannot fetch job list.", NotificationLevel.Error);
                return new List<DistributedJob>();
            }

            byte[] serialisedJobList = P2PManager.Retrieve(JoblistKey);

            if (serialisedJobList == null)
            {
                // no job list in DHT, create empty list
                P2PEditor.Instance.GuiLogMessage("No list in DHT, creating empty list.", NotificationLevel.Debug);
                return new List<DistributedJob>();
            }

            var memoryStream = new MemoryStream(serialisedJobList);

            var bformatter = new BinaryFormatter();
            return (List<DistributedJob>) bformatter.Deserialize(memoryStream);
        }

        public static void AddDistributedJob(DistributedJob distributedJob)
        {
            P2PEditor.Instance.GuiLogMessage("Distributing new job...", NotificationLevel.Debug);

            if (!P2PManager.Instance.P2PConnected())
            {
                P2PEditor.Instance.GuiLogMessage("P2P not connected, cannot distribute job.", NotificationLevel.Error);
                return;
            }

            var currentJobList = JobList();
            currentJobList.Add(distributedJob);

            var memoryStream = new MemoryStream();
            var bformatter = new BinaryFormatter();

            bformatter.Serialize(memoryStream, currentJobList);
            P2PManager.Store(JoblistKey, memoryStream.ToArray());

            var workspaceData = File.ReadAllBytes(distributedJob.LocalFilePath);
            var workspaceKey = JoblistKey + WorkspaceKeyPrefix + distributedJob.JobGuid;
            P2PEditor.Instance.GuiLogMessage(
                "Workspace size: " + workspaceData.Length + ", storing at " + workspaceKey, NotificationLevel.Debug);
            //P2PManager.Store(workspaceKey, workspaceData);

            P2PEditor.Instance.GuiLogMessage("Distributed job " + distributedJob.JobLabel, NotificationLevel.Info);
        }
    }
}