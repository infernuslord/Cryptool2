using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cryptool.P2P;
using Cryptool.PluginBase;

namespace Cryptool.P2PEditor.Distributed
{
    public class JobListManager
    {
        private const string JoblistKey = "Cryptool.P2PEditor.JobList";
        private const string WorkspaceKeyPrefix = "Workspace";
        private readonly P2PEditor _p2PEditor;

        public JobListManager(P2PEditor p2PEditor)
        {
            _p2PEditor = p2PEditor;
        }

        public List<DistributedJob> JobList()
        {
            _p2PEditor.GuiLogMessage("Fetching DHT job list...", NotificationLevel.Debug);

            if (!P2PManager.IsConnected)
            {
                _p2PEditor.GuiLogMessage("P2P not connected, cannot fetch job list.", NotificationLevel.Error);
                return new List<DistributedJob>();
            }

            byte[] serialisedJobList = P2PManager.Retrieve(JoblistKey);

            if (serialisedJobList == null)
            {
                // no job list in DHT, create empty list
                _p2PEditor.GuiLogMessage("No list in DHT, creating empty list.", NotificationLevel.Debug);
                return new List<DistributedJob>();
            }

            var memoryStream = new MemoryStream(serialisedJobList);

            var bformatter = new BinaryFormatter();
            bformatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            return (List<DistributedJob>) bformatter.Deserialize(memoryStream);
        }

        public void AddDistributedJob(DistributedJob distributedJob)
        {
            _p2PEditor.GuiLogMessage("Distributing new job...", NotificationLevel.Debug);

            if (!P2PManager.IsConnected)
            {
                _p2PEditor.GuiLogMessage("P2P not connected, cannot distribute job.", NotificationLevel.Error);
                return;
            }

            var currentJobList = JobList();
            currentJobList.Add(distributedJob);

            var memoryStream = new MemoryStream();
            var bformatter = new BinaryFormatter();
            bformatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

            bformatter.Serialize(memoryStream, currentJobList);
            P2PManager.Store(JoblistKey, memoryStream.ToArray());

            var workspaceData = File.ReadAllBytes(distributedJob.LocalFilePath);
            _p2PEditor.GuiLogMessage(
                "Workspace size: " + workspaceData.Length + ", storing at " + GenerateWorkspaceKey(distributedJob),
                NotificationLevel.Debug);
            P2PManager.Store(GenerateWorkspaceKey(distributedJob), workspaceData);

            _p2PEditor.GuiLogMessage("Distributed job " + distributedJob.JobName, NotificationLevel.Info);
        }

        public void DeleteDistributedJob(DistributedJob distributedJobToDelete)
        {
            _p2PEditor.GuiLogMessage("Deleting job...", NotificationLevel.Debug);

            if (!P2PManager.IsConnected)
            {
                _p2PEditor.GuiLogMessage("P2P not connected, cannot distribute job.", NotificationLevel.Error);
                return;
            }

            var currentJobList = JobList();
            currentJobList.Remove(distributedJobToDelete);

            var memoryStream = new MemoryStream();
            var bformatter = new BinaryFormatter();
            bformatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

            bformatter.Serialize(memoryStream, currentJobList);
            P2PManager.Store(JoblistKey, memoryStream.ToArray());

            // Retrieve job first to satify versioned DHT
            P2PManager.Retrieve(GenerateWorkspaceKey(distributedJobToDelete));
            P2PManager.Remove(GenerateWorkspaceKey(distributedJobToDelete));

            _p2PEditor.GuiLogMessage("Deleted distributed job " + distributedJobToDelete.JobName, NotificationLevel.Info);
        }

        public void CompleteDistributedJob(DistributedJob distributedJob)
        {
            distributedJob.ConvertRawWorkspaceToLocalFile(P2PManager.Retrieve(GenerateWorkspaceKey(distributedJob)));
        }

        private static string GenerateWorkspaceKey(DistributedJob distributedJob)
        {
            return string.Format("{0}.{1}.{2}", JoblistKey, WorkspaceKeyPrefix, distributedJob.JobGuid);
        }
    }
}