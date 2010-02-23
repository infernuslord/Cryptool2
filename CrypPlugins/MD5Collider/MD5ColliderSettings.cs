using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.MD5Collider
{
    class MD5ColliderSettings : ISettings
    {
        #region ISettings Member

        public bool HasChanges { get; set; }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
