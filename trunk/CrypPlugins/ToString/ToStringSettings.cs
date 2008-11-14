using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.ToString
{
  public class ToStringSettings : ISettings
  {
    # region private variables
    private bool hasChanges;
    # endregion private variables

    #region ISettings Members

    public bool HasChanges
    {
      get { return hasChanges; }
      set { hasChanges = value; }
    }



    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
