using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.ComputeAnnihilators
{
    class ComputeAnnihilatorsSettings : ISettings
    {
        #region Private variables
        private Hashtable savedZfunctions = null;
        private string savedoutputfunction = "";
        private string savedmemoryupdatefunction = "";
        private int saveddegree = 0;
        private bool computeended = false;
        private int savedrunlength = 0;
        string outputset;
        private int degree;
        private bool hasChanges;
        private ActionTypes actiontypes = ActionTypes.Combiner;
        public enum ActionTypes { Combiner = 0, function = 1, setofSequence = 2 };
        private OutputTypes outputtypes = OutputTypes.todisplay;
        public enum OutputTypes { todisplay = 0, plugininput = 1, both = 2 };
        #endregion
        #region ISettings Members
        public ActionTypes Actiontypes
        {
            get { return this.actiontypes; }
            set
            {
                if (this.actiontypes != value) hasChanges = true;
                this.actiontypes = value;
                OnPropertyChanged("ActionSetting");
            }
        }
        [ContextMenu("Action", "Choose application.", 1, ContextMenuControlType.ComboBox, null, new string[] { "Combiner", "function", "setofSequence" })]
        [TaskPane("Action", "Choose application", null, 1, false, ControlType.RadioButton, new string[] { "Z-functions of combiner", "Annihilators of Boolean function", "Annihilators of sets of BitsSequences" })]
        public int ActionSetting
        {
            get
            {
                return (int)this.actiontypes;
            }
            set
            {
                if (this.actiontypes != (ActionTypes)value) HasChanges = true;
                this.actiontypes = (ActionTypes)value;
                OnPropertyChanged("ActionSetting");
            }
        }
        [TaskPane("Degree ", "most degree of the searched annihilator", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger,0, int.MaxValue)]
        public int Degree
        {
            get { return this.degree; }
            set
            {
                if (((int)value) != degree) hasChanges = true;
                this.degree = value;
                OnPropertyChanged("Degree");
            }
        }
        public OutputTypes Outputtypes
        {
            get { return this.outputtypes; }
            set
            {
                if (this.outputtypes != value) hasChanges = true;
                this.outputtypes = value;
                OnPropertyChanged("OutputSetting");
            }
        }
        [ContextMenu("Output Type", "display in Textoutput or delivre to plugin system of equation", 4, ContextMenuControlType.ComboBox, null, new string[] { "todisplay", "plugininput", "both" })]
        [TaskPane("Output Type", "display in Textoutput or delivre to plugin system of equation", "required only in Z-functions", 4, false, ControlType.RadioButton, new string[] { "Display in Textoutput ", "Input of other Plug-in", "both" })]
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
        [TaskPane("Outputs Z", "express a set of output Z to determine Z-function to output", "required only in Z-functions", 5, false, ControlType.TextBox, ValidationType.RegEx, "^(1|[\\*]|0)*")]
        public string OutputSet
        {
            get { return this.outputset; }
            set
            {
                if (((string)value) != outputset) hasChanges = true;
                this.outputset = value;
                OnPropertyChanged("OutputSet");
            }
        }
        public Hashtable SavedZfunctions
        {
            get { return savedZfunctions; }
            set
            {
                if (value != savedZfunctions) hasChanges = true;
                savedZfunctions = value;
            }
        }
        public string Savedoutputfunction
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
        public int Saveddegree
        {
            get { return saveddegree; }
            set
            {
                if (value != saveddegree) hasChanges = true;
                saveddegree = value;
            }
        }
        public bool ComputeEnded
        {
            get { return computeended; }
            set
            {
                if (value != computeended) hasChanges = true;
                computeended = value;
            }
        }
        public delegate void ComputeAnnihilatorsLogMessage(string msg, NotificationLevel logLevel);
        public event ComputeAnnihilatorsLogMessage LogMessage;
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
