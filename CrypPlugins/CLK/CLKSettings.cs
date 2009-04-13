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
    private int clkTimeout = 2000;
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

    /*[ContextMenu("Set clock to...", " true / false ", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "true", "false"})]
    [TaskPane("Set clock to...", " true / false ", null, 2, false, DisplayLevel.Beginner, ControlType.RadioButton, new string[] { "true", "false" })]
    public bool SetClockToTrue
    {
        get
        {
            return (this.setClockToTrue == "true");
        }
        set
        {
            //if (this.setClockToTrue != setClockToTrue) HasChanges = true;
            this.setClockToTrue = value;
            OnPropertyChanged("SetClockToTrue");
        }
    }*/

    //[ContextMenu("Set clock to true", " yes / no ", 0, DisplayLevel.Beginner, ContextMenuControlType.CheckBox, null, "Set clock to true")]
    [TaskPaneAttribute("Set CLK timeout in milliseconds", "How long should it last until next CLK event?", "", 1, false, DisplayLevel.Beginner, ControlType.TextBox, null)]
    public int CLKTimeout
    {
        get { return this.clkTimeout; }
        set
        {
            this.clkTimeout = value;
            OnPropertyChanged("CLKTimeout");
        }

    }

    private int rounds = 10; //how many bits will be generated
    //[ContextMenu("Rounds", "How many bits shall be generated?", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 10, 50, 100 }, "10 bits", "50 bits", "100 bits")]
    //[TaskPane("Rounds", "How many bits shall be generated?", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox)]
    [TaskPane("Rounds", "How many clock cycles shall be generated?", null, 2, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
    public int Rounds
    {
        get { return this.rounds; }
        set
        {
            this.rounds = value;
            OnPropertyChanged("Rounds");
            HasChanges = true;
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
