using System;
using System.Numerics;
using System.Windows;
using Cryptool.P2P;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Collections.ObjectModel;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Storage;

namespace KeySearcher
{
    public class KeySearcherSettings : ISettings
    {
        private readonly KeySearcher keysearcher;
        private int coresUsed;
        private const string GroupPeerToPeer = "Peer-to-Peer network";

        public KeySearcherSettings(KeySearcher ks)
        {
            keysearcher = ks;
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
                CoresAvailable.Add((i + 1).ToString());
            CoresUsed = Environment.ProcessorCount - 1;

            chunkSize = 21;
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

        private bool verbosePeerToPeerDisplay;
        [TaskPane("Display verbose information", "Display verbose information about network requests in the quick watch.", GroupPeerToPeer, 2, true, DisplayLevel.Beginner,
            ControlType.CheckBox)]
        public bool VerbosePeerToPeerDisplay
        {
            get { return verbosePeerToPeerDisplay; }
            set
            {
                if (value != verbosePeerToPeerDisplay)
                {
                    verbosePeerToPeerDisplay = value;
                    OnPropertyChanged("VerbosePeerToPeerDisplay");
                    HasChanges = true;
                }
            }
        }

        private int chunkSize;
        [TaskPane("Chunk size", "Amount of keys, that will be calculated by one peer at a time. This value is the exponent of the power of two used for the chunk size.", GroupPeerToPeer, 3, false, DisplayLevel.Professional,
            ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 1000)]
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

        [TaskPane("Copy status key", "Copy status key to clipboard. The key can than be used to upload it together with the job using the P2PEditor.", GroupPeerToPeer, 4, true, DisplayLevel.Professional, ControlType.Button)]
        public void StatusKeyButton()
        {
            if (!keysearcher.IsKeySearcherRunning)
            {
                keysearcher.GuiLogMessage("KeySearcher must be running to copy the status key.", NotificationLevel.Error);
                return;
            }

            var generator = new StorageKeyGenerator(keysearcher, this);
            var statusKey = generator.GenerateStatusKey();

            Clipboard.SetDataObject(statusKey, true);
            keysearcher.GuiLogMessage("Status key '" + statusKey + "' has been copied to clipboard.",
                                      NotificationLevel.Info);
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
