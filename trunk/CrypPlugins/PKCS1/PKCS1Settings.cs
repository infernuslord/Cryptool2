using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace PKCS1
{
    class PKCS1Settings : ISettings
    {
        private bool hasChanges = false;

        #region ISettings Member

        bool ISettings.HasChanges
        {
            get
            {
                return this.hasChanges;
            }
            set
            {
                this.hasChanges = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        event System.ComponentModel.PropertyChangedEventHandler System.ComponentModel.INotifyPropertyChanged.PropertyChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        #endregion
    }
}
