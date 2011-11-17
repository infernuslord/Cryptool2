using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace Cryptool.PrimesGenerator
{
  public class PrimesGeneratorSettings: ISettings
  {
    public const string MODE = "Mode";
    public const string INPUT = "Input";

    #region Properties

    private int m_SelectedMode;
    [PropertySaveOrder(1)]
    [ContextMenu( "ModeCaption", "ModeTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "ModeList1", "ModeList2" })]
    [TaskPane( "ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2" })]
    public int Mode
    {
      get { return this.m_SelectedMode; }
      set
      {
        if (value != m_SelectedMode)
        {
            this.m_SelectedMode = value;
            FirePropertyChangedEvent(MODE);   
        }
      }
    }

    private string m_Input;
    [PropertySaveOrder(2)]
    [TaskPane( "InputCaption", "InputTooltip", null, 2, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]+$")]
    public string Input
    {
      get { if(string.IsNullOrEmpty(this.m_Input)) return "100"; else return m_Input; }
      set
      {
        if (value != m_Input)
        {
            this.m_Input = value;
            FirePropertyChangedEvent(INPUT);   
        }
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    private void FirePropertyChangedEvent(string propertyName)
    {
      if (PropertyChanged != null) PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
    #endregion
  }
}
