using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.CLK
{
  public class CLKSettings : ISettings
  {
    # region private variables
    private bool hasChanges;
    private bool setClockToTrue;
    # endregion private variables

    #region ISettings Members

    public bool HasChanges
    {
      get { return hasChanges; }
      set { hasChanges = value; }
    }

    [ContextMenu("Set clock to true", " yes / no ", 0, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Set clock to true")]
    [TaskPaneAttribute("Set clock to true", " yes / no ", "", 0, true, DisplayLevel.Beginner, ControlType.CheckBox, null)]
    public bool SetClockToTrue
    {
        get { return this.setClockToTrue; }
        set
        {
            this.setClockToTrue = value;
            OnPropertyChanged("SetClockToTrue");
        }
     
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
