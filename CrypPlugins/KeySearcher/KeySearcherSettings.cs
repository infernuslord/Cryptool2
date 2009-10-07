using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace KeySearcher
{
    public class KeySearcherSettings : ISettings
    {
        private KeySearcher keysearcher;

        public KeySearcherSettings(KeySearcher ks)
        {
            keysearcher = ks;
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
                if (!(keysearcher.Pattern != null && keysearcher.Pattern.testKey(value)))                
                    keysearcher.GuiLogMessage("Wrong key pattern!", NotificationLevel.Error);                
            }
        }

        [TaskPane("Reset", "Reset Key", null, 2, false, DisplayLevel.Beginner, ControlType.Button)]
        public void Reset()
        {
            Key = keysearcher.Pattern.giveWildcardKey();
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
