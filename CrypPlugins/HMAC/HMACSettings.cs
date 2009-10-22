using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.HMAC
{
    class HMACSettings : ISettings
    {
        public enum HashFunction { MD5, RIPEMD160, SHA1, SHA256, SHA384, SHA512 };

        private HashFunction selectedHashFunction = HashFunction.MD5;

        [ContextMenu("Hash Function", "Select the hash function to use for the message digest", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "MD5", "RIPEMD160", "SHA1", "SHA256", "SHA384", "SHA512" })]
        [TaskPane("Hash Function", "Select the hash function to use for the message digest", "", 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "MD5", "RIPEMD160", "SHA1", "SHA256", "SHA384", "SHA512" })]
        public int SelectedHashFunction
        {
            get { return (int)this.selectedHashFunction; }
            set
            {
                this.selectedHashFunction = (HashFunction)value;
                OnPropertyChanged("SelectedHashFunction");
            }
        }

        #region INotifyPropertyChanged Members

#pragma warning disable 67
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
#pragma warning restore

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        
        #endregion

        #region ISettings Members

        public bool HasChanges { get; set; }

        #endregion
    }
}
