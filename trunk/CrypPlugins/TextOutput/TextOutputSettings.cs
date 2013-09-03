/*
   Copyright 2008 Thomas Schmid, University of Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace TextOutput
{
  public class TextOutputSettings : ISettings
  {
    #region Private variables

    const int maxmaxLength = int.MaxValue;
    private int maxLength = 65536; //64kB
    private TextOutput myTextOutput;

    #endregion

    public TextOutputSettings(TextOutput textOutput)
    {
      if (textOutput == null) throw new ArgumentException("textOutput");
      myTextOutput = textOutput;
    }

    #region settings

    /// <summary>
    /// Maximum size property used in the settings pane.
    /// </summary>
    [TaskPane( "MaxLengthCaption", "MaxLengthTooltip", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, maxmaxLength)]
    public int MaxLength
    {
      get
      {
        return maxLength;
      }
      set
      {
          if (value != maxLength)
          {
              maxLength = value;
              OnPropertyChanged("MaxLength");
          }
      }
    }

    private bool append = false;
    [ContextMenu( "AppendCaption", "AppendTooltip", 1, ContextMenuControlType.CheckBox, null)]
    [TaskPane("AppendCaption", "AppendTooltip", "", 1, false, ControlType.CheckBox, "", null)]
    public bool Append
    {
      get { return append; }
      set
      {
        if (value != append)
        {
          append = value;
          //OnPropertyChanged("Append");
        }
      }
    }

    private int appendBreaks = 1;
    [TaskPane("AppendBreaksCaption", "AppendBreaksTooltip", "", 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
    public int AppendBreaks
    {
      get { return this.appendBreaks; }
      set
      {
        if (value != this.appendBreaks)
        {
          this.appendBreaks = value;
          //OnPropertyChanged("AppendBreaks");
        }
      }
    }

    private bool showChars = true;
    [ContextMenu("ShowCharsCaption", "ShowCharsTooltip", 3, ContextMenuControlType.CheckBox, null)]
    [TaskPane("ShowCharsCaption", "ShowCharsTooltip", "ShowCharsGroup", 3, true, ControlType.CheckBox, "", null)]
    public bool ShowChars
    {
        get { return showChars; }
        set
        {
            if (value != showChars)
            {
                showChars = value;
                OnPropertyChanged("ShowChars");
            }
        }
    }

    private bool showLines = true;
    [ContextMenu("ShowLinesCaption", "ShowLinesTooltip", 4, ContextMenuControlType.CheckBox, null)]
    [TaskPane("ShowLinesCaption", "ShowLinesTooltip", "ShowCharsGroup", 4, true, ControlType.CheckBox, "", null)]
    public bool ShowLines
    {
        get { return showLines; }
        set
        {
            if (value != showLines)
            {
                showLines = value;
                OnPropertyChanged("ShowLines");
            }
        }
    }

    # endregion settings

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string name)
    {
      if (PropertyChanged != null)
      {
        EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
      }
    }

    #endregion
  }
}
