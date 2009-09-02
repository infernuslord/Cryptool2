using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Security.Cryptography;

namespace BooleanFunctionParser
{
    class BooleanFunctionParserSettings : ISettings
    {
        #region Private variables

        private bool hasChanges;

        #endregion

        #region ISettings Members

        private int countOfInputs = 0;
        [TaskPane("Count of inputs", "How many inputs do you need?", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int CountOfInputs
        {
            get { return this.countOfInputs; }
            set
            {
                this.countOfInputs = value;
                OnPropertyChanged("CountOfInputs");
                HasChanges = true;
            }
        }

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }
        }

        public bool CanChangeProperty { get; set; }

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
