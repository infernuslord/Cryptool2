using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace KeySearcher
{
    public class KeySearcherSettings : ISettings
    {
        private KeySearcher keysearcher;
        private int coresUsed;
        private const string GroupPeerToPeer = "Peer-to-Peer network";

        public KeySearcherSettings(KeySearcher ks)
        {
            keysearcher = ks;
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i + 1).ToString());
            CoresUsed = Environment.ProcessorCount - 1;

            distributedJobIdentifier = Guid.NewGuid().ToString();
        }

        private string key;
        [TaskPane("Key", "Key pattern used to bruteforce", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public String Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
                OnPropertyChanged("Key");
                if (!(keysearcher.Pattern != null && keysearcher.Pattern.testWildcardKey(value)))
                    keysearcher.GuiLogMessage("Wrong key pattern!", NotificationLevel.Error);
                HasChanges = true;
            }
        }

        [TaskPane("Reset", "Reset Key", null, 2, false, DisplayLevel.Beginner, ControlType.Button)]
        public void Reset()
        {
            Key = keysearcher.Pattern.giveInputPattern();
        }
        
        [TaskPane("CoresUsed", "Choose how many cores should be used", null, 3, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int CoresUsed
        {
            get { return this.coresUsed; }
            set
            {
                if (value != this.coresUsed)
                {
                    this.coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                    HasChanges = true;
                }
            }
        }

        private bool usePeerToPeer;
        [TaskPane("Use Peer-to-Peer network", "Distributes the operation on available peers by using the built-in peer-to-peer network.", GroupPeerToPeer, 0, false, DisplayLevel.Beginner,
            ControlType.CheckBox)]
        public bool UsePeerToPeer
        {
            get { return usePeerToPeer; }
            set
            {
                if (value != usePeerToPeer)
                {
                    usePeerToPeer = value;
                    OnPropertyChanged("UsePeerToPeer");
                    HasChanges = true;
                }
            }
        }

        private bool autoconnectPeerToPeer;
        [TaskPane("Autoconnect network", "Establish a connection to the network if the workspace is started without the background connection being active.", GroupPeerToPeer, 1, false, DisplayLevel.Beginner,
            ControlType.CheckBox)]
        public bool AutoconnectPeerToPeer
        {
            get { return autoconnectPeerToPeer; }
            set
            {
                if (value != autoconnectPeerToPeer)
                {
                    autoconnectPeerToPeer = value;
                    OnPropertyChanged("AutoconnectPeerToPeer");
                    HasChanges = true;
                }
            }
        }

        private string distributedJobIdentifier;
        [TaskPane("Job identifier", "Arbitrary description, that allows other peers to join this calculation.", GroupPeerToPeer, 2, false, DisplayLevel.Professional,
            ControlType.TextBox)]
        public string DistributedJobIdentifier
        {
            get { return distributedJobIdentifier; }
            set
            {
                if (value != distributedJobIdentifier)
                {
                    distributedJobIdentifier = value;
                    OnPropertyChanged("DistributedJobIdentifier");
                    HasChanges = true;
                }
            }
        }

        private int chunkSize;
        [TaskPane("Chunk size", "Amount of keys (x 10.000), that will be calculated by one peer at a time.", GroupPeerToPeer, 3, false, DisplayLevel.Professional,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 250, int.MaxValue)]
        public int ChunkSize
        {
            get { return chunkSize; }
            set
            {
                if (value != chunkSize)
                {
                    chunkSize = value;
                    OnPropertyChanged("ChunkSize");
                    HasChanges = true;
                }
            }
        }

        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();
        public ObservableCollection<string> CoresAvailable
        {
            get { return coresAvailable; }
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                }
                OnPropertyChanged("CoresAvailable");
            }
        }

        #region ISettings Members

        private bool hasChanges;

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
                OnPropertyChanged("HasChanges");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion
    }
}
