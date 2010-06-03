using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Cryptool.P2P;

namespace Cryptool.P2PEditor.Distributed
{
    [Serializable]
    public class DistributedJob : INotifyPropertyChanged
    {
        [NonSerialized] private String _localFilePath;

        public DistributedJob()
        {
            JobGuid = Guid.NewGuid();
            JobOwner = P2PSettings.Default.PeerName;
            JobDate = new DateTime();
        }

        public Guid JobGuid { get; private set; }

        public String JobName { get; set; }

        public String JobDescription { get; set; }

        public String JobOwner { get; set; }

        public DateTime JobDate { get; set; }

        public String FileName { get; private set; }

        public String LocalFilePath
        {
            get { return _localFilePath; }
            set
            {
                if (value == _localFilePath) return;
                _localFilePath = value;
                FileName = Path.GetFileName(_localFilePath);
                OnPropertyChanged("LocalFilePath");
            }
        }

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void ConvertRawWorkspaceToLocalFile(byte[] rawWorkspaceData)
        {
            string workspacePath = P2PSettings.Default.WorkspacePath;
            if (String.IsNullOrEmpty(workspacePath) || !Directory.Exists(workspacePath))
            {
                throw new ArgumentOutOfRangeException(workspacePath, "Configured local workspace directory is null, empty or does not exist.");
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
                throw new NotSupportedException("Workspace data could not be fetching using Peer-to-peer.");
            }

            File.WriteAllBytes(LocalFilePath, rawWorkspaceData);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((DistributedJob) obj);
        }

        public bool Equals(DistributedJob obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.JobGuid == JobGuid;
        }

        public override int GetHashCode()
        {
            return JobGuid.GetHashCode();
        }
    }
}