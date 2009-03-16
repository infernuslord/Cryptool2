using System;
using System.ComponentModel;
using Cryptool.PluginBase;

namespace Cryptool.SkyTale
{
    internal enum Action
    {
        Encrypt,
        Decrypt
    }

    class SkyTaleSettings : ISettings
    {
        private Action action = Cryptool.SkyTale.Action.Encrypt;
        [TaskPane("Action", "Select the Algorithm action", null, 0, false, DisplayLevel.Beginner, ControlType.ComboBox, new [] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get { return (int)action; }
            set
            {
                try
                {
                    action = (Action)value;
                    OnPropertyChanged("Action");
                }
                catch (Exception)
                {
                    action = Cryptool.SkyTale.Action.Encrypt;
                }
            }
        }

        private int stickSize = 1;
        [TaskPane("Stick size", "This is the size of the used stick.", null, 1, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 100)]
        public int StickSize
        {
            get { return stickSize; }
            set
            {
                stickSize = value;
                OnPropertyChanged("StickSize");
            }
        }

        private bool hasChanges;
        public bool HasChanges
        {
            get { return hasChanges; }
            set
            {
                if (value != hasChanges)
                {
                    hasChanges = value;
                    OnPropertyChanged("HasChanges");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            if (name.ToLower() != "haschanges")
                HasChanges = true;
        }
    }
}
