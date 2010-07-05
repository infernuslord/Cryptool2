using System;
using System.IO;

namespace Cryptool.P2PEditor.Distributed
{
    static class DistributedJobSerializer
    {
        public static DistributedJob FromReader(BinaryReader reader)
        {
            var newJob = new DistributedJob();
            
            newJob.Guid = new Guid(reader.ReadBytes(16));
            newJob.Name = reader.ReadString();
            newJob.Description = reader.ReadString();
            newJob.Owner = reader.ReadString();
            newJob.CreateDate = DateTime.FromBinary(reader.ReadInt64());
            newJob.FileName = reader.ReadString();

            if (reader.ReadBoolean())
                newJob.StatusKey = reader.ReadString();

            return newJob;
        }

        public static DistributedJobStatus StatusFromReader(BinaryReader reader)
        {
            var newJobStatus = new DistributedJobStatus();

            newJobStatus.CurrentStatus = (DistributedJobStatus.Status) reader.ReadByte();
            newJobStatus.Participants = reader.ReadInt64();
            newJobStatus.Progress = reader.ReadDouble();
            newJobStatus.StartDate = DateTime.FromBinary(reader.ReadInt64());

            return newJobStatus;
        }

        public static void ToWriter(DistributedJob distributedJob, BinaryWriter writer)
        {
            writer.Write(distributedJob.Guid.ToByteArray());
            writer.Write(distributedJob.Name);
            writer.Write(distributedJob.Description);
            writer.Write(distributedJob.Owner);
            writer.Write(distributedJob.CreateDate.ToBinary());
            writer.Write(distributedJob.FileName);

            var hasStatusKey = !string.IsNullOrEmpty(distributedJob.StatusKey);
            writer.Write(hasStatusKey);
            if (hasStatusKey)
                writer.Write(distributedJob.StatusKey);
        }

        public static void ToWriter(DistributedJobStatus distributedJobStatus, BinaryWriter writer)
        {
            writer.Write((byte) distributedJobStatus.CurrentStatus);
            writer.Write(distributedJobStatus.Participants);
            writer.Write(distributedJobStatus.Progress);
            writer.Write(distributedJobStatus.StartDate.ToBinary());
        }
    }
}
