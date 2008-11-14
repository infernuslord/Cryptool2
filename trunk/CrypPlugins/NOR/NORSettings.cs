using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.NOR
{
  public class NORSettings : ISettings
  {
    # region private variables
    private bool hasChanges;
    private bool setInputOneToTrue;
    private bool setInputTwoToTrue;
    # endregion private variables

    #region ISettings Members

    public bool HasChanges
    {
      get { return hasChanges; }
      set { hasChanges = value; }
    }

    [ContextMenu("Set input one to to true", " yes / no ", 0, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Set input one to true")]
    [TaskPaneAttribute("Set input one to to true", " yes / no ", "", 0, false, DisplayLevel.Beginner, ControlType.CheckBox, null)]
    public bool SetInputOneToTrue
    {
      get { return setInputOneToTrue; }
      set 
      { 
        setInputOneToTrue = value;
        OnPropertyChanged("SetInputOneToTrue");
      }
    }

    [ContextMenu("Set input two to to true", " yes / no ", 1, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Set input two to true")]
    [TaskPaneAttribute("Set input two to to true", " yes / no ", "", 1, false, DisplayLevel.Beginner, ControlType.CheckBox, null)]
    public bool SetInputTwoToTrue
    {
      get { return setInputTwoToTrue; }
      set 
      { 
        setInputTwoToTrue = value;
        OnPropertyChanged("SetInputTwoToTrue");
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
