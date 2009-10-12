using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.Plugins.ArrayIndexer
{
    class ArrayIndexerSettings : ISettings
    {
        bool hasChanges = false;

        #region taskPane
        private int arrayIndex;
        [TaskPane("Index of the Array", "Indexes of an array begin always with 0. Example: If you have an array of the length 8, you can index the values 0 to 7", null, 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int ArrayIndex
        {
            get
            {
                return this.arrayIndex;
            }
            set
            {
                if (value != this.arrayIndex)
                {
                    this.arrayIndex = value;
                    OnPropertyChanged("ArrayIndex");
                    HasChanges = true;
                }
            }
        }

        #endregion
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion
    }
}
