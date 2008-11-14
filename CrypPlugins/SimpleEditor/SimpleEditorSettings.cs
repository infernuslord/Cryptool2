using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cryptool.PluginBase;

namespace SimpleEditor
{
    public class SimpleEditorSettings : IEditorSettings
    {
      #region INotifyPropertyChanged Members

      public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

      #endregion
    }
}
