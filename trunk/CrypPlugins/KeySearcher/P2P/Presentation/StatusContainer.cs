using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;

namespace KeySearcher.P2P.Presentation
{
    public class StatusContainer : INotifyPropertyChanged
    {
        private string currentOperation;
        public string CurrentOperation
        {
            get { return currentOperation; }
            set
            {
                currentOperation = "> " + value + " <";
                OnPropertyChanged("CurrentOperation");
            }
        }

        private BigInteger localFinishedChunks;
        public BigInteger LocalFinishedChunks
        {
            get { return localFinishedChunks; }
            set
            {
                localFinishedChunks = value;
                OnPropertyChanged("LocalFinishedChunks");
            }
        }

        private BigInteger currentChunk;
        public BigInteger CurrentChunk
        {
            get { return currentChunk; }
            set
            {
                currentChunk = value;
                OnPropertyChanged("CurrentChunk");
            }
        }

        private double progressOfCurrentChunk;
        public double ProgressOfCurrentChunk
        {
            get { return progressOfCurrentChunk; }
            set
            {
                progressOfCurrentChunk = value;
                OnPropertyChanged("ProgressOfCurrentChunk");
            }
        }

        private bool isCurrentProgressIndeterminate;
        public bool IsCurrentProgressIndeterminate
        {
            get { return isCurrentProgressIndeterminate; }
            set
            {
                isCurrentProgressIndeterminate = value;
                OnPropertyChanged("IsCurrentProgressIndeterminate");
            }
        }

        private BigInteger keysPerSecond;
        public BigInteger KeysPerSecond
        {
            get { return keysPerSecond; }
            set
            {
                keysPerSecond = value;
                OnPropertyChanged("KeysPerSecond");
            }
        }

        private DateTime startDate;
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                OnPropertyChanged("StartDate");
            }
        }

        private TimeSpan elapsedTime;
        public TimeSpan ElapsedTime
        {
            get { return elapsedTime; }
            set
            {
                elapsedTime = value;
                OnPropertyChanged("ElapsedTime");
            }
        }

        private TimeSpan remainingTime;
        public TimeSpan RemainingTime
        {
            get { return remainingTime; }
            set
            {
                remainingTime = value;
                OnPropertyChanged("RemainingTime");
            }
        }

        private TimeSpan remainingTimeTotal;
        public TimeSpan RemainingTimeTotal
        {
            get { return remainingTimeTotal; }
            set
            {
                remainingTimeTotal = value;
                OnPropertyChanged("RemainingTimeTotal");
            }
        }

        private BigInteger totalAmountOfParticipants;
        public BigInteger TotalAmountOfParticipants
        {
            get { return totalAmountOfParticipants; }
            set
            {
                totalAmountOfParticipants = value;
                OnPropertyChanged("TotalAmountOfParticipants");
            }
        }

        private string estimatedFinishDate;
        public string EstimatedFinishDate
        {
            get { return estimatedFinishDate; }
            set
            {
                estimatedFinishDate = value;
                OnPropertyChanged("EstimatedFinishDate");
            }
        }

        private BigInteger totalDhtRequests;
        public BigInteger TotalDhtRequests
        {
            get { return totalDhtRequests; }
            set
            {
                totalDhtRequests = value;
                OnPropertyChanged("TotalDhtRequests");
            }
        }

        private BigInteger requestsPerNode;
        public BigInteger RequestsPerNode
        {
            get { return requestsPerNode; }
            set
            {
                requestsPerNode = value;
                OnPropertyChanged("RequestsPerNode");
            }
        }

        private BigInteger retrieveRequests;
        public BigInteger RetrieveRequests
        {
            get { return retrieveRequests; }
            set
            {
                retrieveRequests = value;
                OnPropertyChanged("RetrieveRequests");
            }
        }

        private BigInteger removeRequests;
        public BigInteger RemoveRequests
        {
            get { return removeRequests; }
            set
            {
                removeRequests = value;
                OnPropertyChanged("RemoveRequests");
            }
        }

        private BigInteger storeRequests;
        public BigInteger StoreRequests
        {
            get { return storeRequests; }
            set
            {
                storeRequests = value;
                OnPropertyChanged("StoreRequests");
            }
        }

        private TimeSpan dhtOverheadInReadableTime;
        public TimeSpan DhtOverheadInReadableTime
        {
            get { return dhtOverheadInReadableTime; }
            set
            {
                dhtOverheadInReadableTime = value;
                OnPropertyChanged("DhtOverheadInReadableTime");
            }
        }

        private string dhtOverheadInPercent;
        public string DhtOverheadInPercent
        {
            get { return dhtOverheadInPercent; }
            set
            {
                dhtOverheadInPercent = value;
                OnPropertyChanged("DhtOverheadInPercent");
            }
        }

        public ObservableCollection<ResultEntry> TopList { get; set; }

        private bool isSearchingForReservedNodes;
        public bool IsSearchingForReservedNodes
        {
            get { return isSearchingForReservedNodes; }
            set
            {
                isSearchingForReservedNodes = value;
                OnPropertyChanged("IsSearchingForReservedNodes");
            }
        }

        private long storedBytes;
        public long StoredBytes
        {
            get { return storedBytes; }
            set
            {
                storedBytes = value;
                OnPropertyChanged("StoredBytes");
            }
        }

        private long retrievedBytes;
        public long RetrievedBytes
        {
            get { return retrievedBytes; }
            set
            {
                retrievedBytes = value;
                OnPropertyChanged("RetrievedBytes");
            }
        }

        private long totalBytes;
        public long TotalBytes
        {
            get { return totalBytes; }
            set
            {
                totalBytes = value;
                OnPropertyChanged("TotalBytes");
            }
        }

        private long sentBytesByLinkManager;
        public long SentBytesByLinkManager
        {
            get { return sentBytesByLinkManager; }
            set
            {
                sentBytesByLinkManager = value;
                OnPropertyChanged("SentBytesByLinkManager");
            }
        }

        private long receivedBytesByLinkManager;
        public long ReceivedBytesByLinkManager
        {
            get { return receivedBytesByLinkManager; }
            set
            {
                receivedBytesByLinkManager = value;
                OnPropertyChanged("ReceivedBytesByLinkManager");
            }
        }

        private long totalBytesByLinkManager;
        public long TotalBytesByLinkManager
        {
            get { return totalBytesByLinkManager; }
            set
            {
                totalBytesByLinkManager = value;
                OnPropertyChanged("TotalBytesByLinkManager");
            }
        }

        public StatusContainer()
        {
            EstimatedFinishDate = "-";
            DhtOverheadInPercent = "-";
            TopList = new ObservableCollection<ResultEntry>();
            TotalAmountOfParticipants = 1;
            CurrentOperation = "Idle";
        }

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
