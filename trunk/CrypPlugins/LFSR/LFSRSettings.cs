using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Security.Cryptography;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;

namespace Cryptool.LFSR
{
    public class LFSRSettings : ISettings
    {
        #region ISettings Members

        private bool hasChanges = false;
        private int rounds = 4; //how many bits will be generated

        //[ContextMenu("Rounds","How many bits shall be generated?",1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 5, 10}, "5 bits","10 bits")]
        [TaskPane("Rounds", "How many bits shall be generated?", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public int Rounds
        {
            get { return this.rounds; }
            set { 
                this.rounds = (int)value;
                OnPropertyChanged("Rounds");
                HasChanges = true;
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
