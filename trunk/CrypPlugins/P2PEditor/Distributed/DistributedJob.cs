using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Cryptool.P2P;

namespace Cryptool.P2PEditor.Distributed
{
    [Serializable]
    public class DistributedJob : INotifyPropertyChanged, ISerializable
    {
        [NonSerialized] private String _localFilePath;

        protected DistributedJob(SerializationInfo info, StreamingContext ctxt)
        {
            JobName = (string) info.GetValue("JobName", typeof (string));
            JobDescription = (string) info.GetValue("JobDescription", typeof (string));
            JobGuid = (Guid) info.GetValue("JobGuid", typeof (Guid));
            JobOwner = (string) info.GetValue("JobOwner", typeof (string));
            JobDate = (DateTime) info.GetValue("JobDate", typeof (DateTime));
            FileName = (string) info.GetValue("FileName", typeof (string));
        }

        public DistributedJob()
        {
            JobGuid = Guid.NewGuid();
        }

        public Guid JobGuid { get; private set; }

        public String JobName { get; set; }

        public String JobDescription { get; set; }

        public String JobOwner { get; set; }

        public DateTime JobDate { get; set; }

        private String FileName { get; set; }

        public String LocalFilePath
        {
            get { return _localFilePath; }
            set
            {
                if (value == _localFilePath) return;
                _localFilePath = value;
                OnPropertyChanged("LocalFilePath");
            }
        }

        public String JobLabel
        {
            get { return JobName; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("JobName", JobName);
            info.AddValue("JobDescription", JobDescription);
            info.AddValue("JobGuid", JobGuid);
            info.AddValue("JobOwner", P2PManager.Instance.P2PSettings.PeerName);
            info.AddValue("JobDate", DateTime.Now);
            info.AddValue("FileName", Path.GetFileName(LocalFilePath));
        }

        #endregion

        public void ConvertRawWorkspaceToLocalFile(byte[] rawWorkspaceData)
        {
            string workspacePath = P2PManager.Instance.P2PSettings.WorkspacePath;
            if (String.IsNullOrEmpty(workspacePath) || !Directory.Exists(workspacePath))
            {
                throw new ArgumentOutOfRangeException(workspacePath, "Workspace path is null, empty or does not exist.");
            }

            // Avoid overwriting previous versions of this workspace or workspaces with common names by adding an integer prefix
            LocalFilePath = Path.Combine(workspacePath, JobOwner + "_" + FileName);
            int counter = 0;
            while (File.Exists(LocalFilePath))
            {
                LocalFilePath = Path.Combine(workspacePath, counter++ + "_" + JobOwner + "_" + FileName);
            }

            if (rawWorkspaceData == null || rawWorkspaceData.Length == 0)
            {
                throw new ArgumentOutOfRangeException(LocalFilePath, "Workspace could not be fetching using Peer-to-peer.");
            }

            File.WriteAllBytes(LocalFilePath, rawWorkspaceData);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}