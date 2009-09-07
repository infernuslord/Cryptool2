using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.Substring
{
    class SubstringSettings : ISettings
    {
        bool hasChanges = false;

        #region taskPane

        private int integerLengthValue;
        [TaskPane("Length value", "Integer value for the length.", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int IntegerLengthValue
        {
            get { return this.integerLengthValue; }
            set
            {
                if (value != this.integerLengthValue)
                {
                    this.integerLengthValue = value;
                    OnPropertyChanged("IntegerLengthValue");
                    HasChanges = true;
                }
            }
        }

        private int integerStartValue;
        [TaskPane("Start value", "Integer value to start from.", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int IntegerStartValue
        {
            get { return this.integerStartValue; }
            set
            {
                if (value != this.integerStartValue)
                {
                    this.integerStartValue = value;
                    OnPropertyChanged("IntegerStartValue");
                    HasChanges = true;
                }
            }
        }

        #endregion taskPane



        #region ISettings Members

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {

        }

        #endregion
    }
}
