using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;

namespace KeySearcher
{
  public class KeySearcherSettings : ISettings
  {
    #region ISettings Members

    private bool hasChanges;

    public bool HasChanges
    {
      get
      {
        return hasChanges;
      }
      set
      {
        hasChanges = value;
        NotifyPropertyChanged("HasChanges");
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged(string property)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(property));
    }

    #endregion
  }
}
