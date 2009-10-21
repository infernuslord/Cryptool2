using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace ClassLibrary1
{
    class TranspositionSettings : ISettings

    {
        private Boolean hasChanges = false;

        #region ISettings Member

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                this.hasChanges = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
