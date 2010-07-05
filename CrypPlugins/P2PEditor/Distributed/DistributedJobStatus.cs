using System;
using System.ComponentModel;

namespace Cryptool.P2PEditor.Distributed
{
    public class DistributedJobStatus : INotifyPropertyChanged
    {
        public enum Status : byte
        {
            New = 0,
            Active = 1,
            Finished = 2
        };

        public Status CurrentStatus { get; set; }

        private long participants;
        public long Participants
        {
            get { return participants; }
            set
            {
                if (value == participants) return;
                participants = value;
                OnPropertyChanged("Participants");
            }
        }

        public double Progress { get; set; }

        public DateTime StartDate { get; set; }

        public DistributedJobStatus()
        {
            Participants = 0;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
