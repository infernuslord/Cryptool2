using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Analysis;
using System.ComponentModel;

namespace FriedmanTest
{
    class FriedmanTestSettings : ISettings
    {
        #region ISettings Members

        public bool HasChanges
        {
            get
            {
                return false;
                //throw new NotImplementedException();
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
