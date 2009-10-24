using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase;
using System.ComponentModel;

namespace Cryptool.Plugins.RegularExpressions
{
    class RegExMatchSettings : ISettings
    {
        private bool hasChanges = false;

        #region ISettings Member

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

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
