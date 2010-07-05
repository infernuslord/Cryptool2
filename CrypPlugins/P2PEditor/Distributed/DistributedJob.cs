using System;
using System.ComponentModel;
using System.IO;
using Cryptool.P2P;

namespace Cryptool.P2PEditor.Distributed
{
    public class DistributedJob : INotifyPropertyChanged
    {
        public DistributedJob()
        {
            Guid = Guid.NewGuid();
            Owner = P2PSettings.Default.PeerName;
            CreateDate = DateTime.UtcNow;
        }

        public Guid Guid { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }

        public String Owner { get; set; }

        public DateTime CreateDate { get; set; }

        public String FileName { get; set; }

        private DistributedJobStatus status;
        public DistributedJobStatus Status
        {
            get { return status; }
            set
            {
                if (value == status) return;
                status = value;
                OnPropertyChanged("Status");
            }
        }

        private int downloads;
        public int Downloads
        {
            get { return downloads; }
            set
            {
                if (value == downloads) return;
                downloads = value;
                OnPropertyChanged("Downloads");
            }
        }

        private DateTime lastDownload;
        public DateTime LastDownload
        {
            get { return lastDownload; }
            set
            {
                if (value == lastDownload) return;
                lastDownload = value;
                OnPropertyChanged("LastDownload");
            }
        }

        private String localFilePath;
        public String LocalFilePath
        {
            get { return localFilePath; }
            set
            {
                if (value == localFilePath) return;
                localFilePath = value;
                FileName = Path.GetFileName(localFilePath);
                OnPropertyChanged("LocalFilePath");
            }
        }

        private String statusKey;
        public String StatusKey
        {
            get { return statusKey; }
            set
            {
                if (value == statusKey) return;
                statusKey = value;
                OnPropertyChanged("StatusKey");
            }
        }

        public void ConvertRawWorkspaceToLocalFile(byte[] rawWorkspaceData)
        {
            var workspacePath = P2PSettings.Default.WorkspacePath;
            if (String.IsNullOrEmpty(workspacePath) || !Directory.Exists(workspacePath))
            {
                throw new ArgumentOutOfRangeException(workspacePath, "Configured local workspace directory is null, empty or does not exist.");
            }

            // Avoid overwriting previous versions of this workspace or workspaces with common names by adding an integer prefix
            var originalFileName = FileName;
            LocalFilePath = Path.Combine(workspacePath, Owner + "_" + originalFileName);
            var counter = 0;
            while (File.Exists(LocalFilePath))
            {
                LocalFilePath = Path.Combine(workspacePath, counter++ + "_" + Owner + "_" + originalFileName);
            }

            if (rawWorkspaceData == null || rawWorkspaceData.Length == 0)
            {
                throw new NotSupportedException("Workspace data could not be fetched using Peer-to-Peer system.");
            }

            File.WriteAllBytes(LocalFilePath, rawWorkspaceData);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Equals and HashCode

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

            return obj.Guid == Guid;
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        #endregion
    }
}