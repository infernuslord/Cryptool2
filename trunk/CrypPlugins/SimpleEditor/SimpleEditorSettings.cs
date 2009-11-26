using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase;

namespace SimpleEditor
{
    public class SimpleEditorSettings : ISettings
    {
      #region INotifyPropertyChanged Members

      public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

      #endregion

      #region ISettings Members

      public bool HasChanges
      {
          get { return false; }
          set { }
      }

      #endregion
    }
}
