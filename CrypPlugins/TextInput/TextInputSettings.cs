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

namespace Cryptool.TextInput
{
  public class TextInputSettings : ISettings
  {    
    public delegate void TextInputLogMessage(string message, NotificationLevel loglevel);
    public event TextInputLogMessage OnLogMessage;

    private void LogMessage(string message, NotificationLevel logLevel)
    {
      if (OnLogMessage != null) OnLogMessage(message, logLevel);
    }

    private string text;
    public string Text 
    {
      get { return text; }
      set 
      {
        if (value != text)
        {
            text = value;
            OnPropertyChanged("Text");
        }
      }
    }

    #region settings

    private bool showChars = true;
    [ContextMenu("ShowCharsCaption", "ShowCharsTooltip", 1, ContextMenuControlType.CheckBox, null)]
    [TaskPane("ShowCharsCaption", "ShowCharsTooltip", "ShowCharsGroup", 1, true, ControlType.CheckBox, "", null)]
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
    [ContextMenu("ShowLinesCaption", "ShowLinesTooltip", 2, ContextMenuControlType.CheckBox, null)]
    [TaskPane("ShowLinesCaption", "ShowLinesTooltip", "ShowCharsGroup", 2, true, ControlType.CheckBox, "", null)]
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

    #endregion settings

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
      public void Initialize()
      {
          
      }

      public void OnPropertyChanged(string name)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(name));
      }
    }

    #endregion
  }
}
