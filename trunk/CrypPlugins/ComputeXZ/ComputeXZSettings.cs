using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.ComputeXZ
{
    class ComputeXZSettings : ISettings
    {
        #region Private variables
        private bool hasChanges;
        private int savedstartX = 0;
        private Hashtable savedXZ = null;
        private string savedoutputfunction = "";
        private string savedmemoryupdatefunction = "";
        private int savedrunlength = 0;
        private bool isxzcomputed = false;
        string outputs;
        private OutputTypes outputtypes = OutputTypes.todisplay;
        public enum OutputTypes { todisplay = 0, plugininput = 1, both = 2 };
        #endregion

        #region ISettings Members
        public OutputTypes Output
        {
            get { return this.outputtypes; }
            set
            {
                if (this.outputtypes != value) hasChanges = true;
                this.outputtypes = value;
                OnPropertyChanged("OutputSetting");
            }
        }
        [TaskPane("OutputTypes of XZ", "Choose Outputtype of th sets XZ", null, 2, false, ControlType.RadioButton, new string[] { "Display in TextOutput ", "Input of other Plug-in", "both" })]
        public int OutputSetting
        {
            get
            {
                return (int)this.outputtypes;
            }
            set
            {
                if (this.outputtypes != (OutputTypes)value) HasChanges = true;
                this.outputtypes = (OutputTypes)value;
                OnPropertyChanged("OutputSetting");
            }
        }
        [TaskPane("Outputs Z", "express a sets of output Z to determine the set XZ to output", null, 3, false, ControlType.TextBox, ValidationType.RegEx, null)]
        public string SetOfOutputs
        {
            get { return this.outputs; }
            set
            {
                if (((string)value) != outputs) hasChanges = true;
                this.outputs = value;
                OnPropertyChanged("SetOfOutputs");
            }
        }
        public string Saveoutputfunction
        {
            get { return savedoutputfunction; }
            set
            {
                if (value != savedoutputfunction) hasChanges = true;
                savedoutputfunction = value;
            }
        }
        public string Savedmemoryupdatefunction
        {
            get { return savedmemoryupdatefunction; }
            set
            {
                if (value != savedmemoryupdatefunction) hasChanges = true;
                savedmemoryupdatefunction = value;
            }
        }
        public int Savedrunlength
        {
            get { return savedrunlength; }
            set
            {
                if (value != savedrunlength) hasChanges = true;
                savedrunlength = value;
            }
        }
        public int SavedstartX
        {
            get { return savedstartX; }
            set
            {
                if (value != savedstartX) hasChanges = true;
                savedstartX = value;
            }
        }
        public bool IsXZcomputed
        {
            get { return isxzcomputed; }
            set
            {
                if (value != isxzcomputed) hasChanges = true;
                isxzcomputed = value;
            }
        }
        public Hashtable SavedXZ
        {
            get { return savedXZ; }
            set
            {
                if (value != savedXZ) hasChanges = true;
                savedXZ = value;
            }
        }
        public delegate void ComputeXZLogMessage(string msg, NotificationLevel logLevel);
        public event ComputeXZLogMessage LogMessage;
        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }
        #endregion
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
