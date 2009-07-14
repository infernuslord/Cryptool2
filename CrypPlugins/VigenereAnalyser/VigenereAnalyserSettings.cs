using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.ComponentModel;


namespace Cryptool.VigenereAnalyser
{
    class VigenereAnalyserSettings : ISettings
    {
        #region ISettings Members
        private int elf = 0;
        private bool hasChanges = false;
        [ContextMenu("Letter Frequency", "Select the language to be analysed", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new String[] { "English", "German", "French", "Spanish", "Italian", "Portugeese" })]
        [TaskPane("Letter Frequency", "Select the language to be analysed", null, 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "English", "German", "French", "Spanish", "Italian", "Portugeese" })]
        public int ELF // Expected Letter Frequencies
        {
            get { return this.elf; }
            set
            {
                if (((int)value) != elf) hasChanges = true;
                this.elf = (int)value;
                OnPropertyChanged("ELF");
            }
        }
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; OnPropertyChanged("HasChanges"); }
        }
        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                if (value != text) hasChanges = true;
                text = value;
            }
        }
        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
