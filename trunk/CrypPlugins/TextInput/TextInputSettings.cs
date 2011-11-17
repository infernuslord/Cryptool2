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
    #region Private variables
    private EncodingTypes encoding = EncodingTypes.UTF8;
    private InputFormat inputFormat = InputFormat.Text;
    #endregion

    public enum EncodingTypes { Default = 0, Unicode = 1, UTF7 = 2, UTF8 = 3, UTF32 = 4, ASCII = 5, BigEndianUnicode = 6 };
    public enum InputFormat { Text, Hex, Base64 }
    
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
        }
      }
    }

    /// <summary>
    /// Encoding property used in the Settings pane. 
    /// </summary>
    [ContextMenu( "EncodingSettingCaption", "EncodingSettingTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
    [TaskPane( "EncodingSettingCaption", "EncodingSettingTooltip", "", 1, false, ControlType.RadioButton, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
    public EncodingTypes Encoding
    {
      get
      {
        return this.encoding;
      }
      set
      {
        if (this.Encoding != value)
        {
          this.Encoding = value;
          OnPropertyChanged("Encoding");
        }
      }
    }

    /// <summary>
    /// Gets or sets the presentation format setting.
    /// </summary>
    /// <value>The presentation format setting.</value>
    [ContextMenu( "InputFormatSettingCaption", "InputFormatSettingTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "InputFormatSettingList1", "InputFormatSettingList2", "InputFormatSettingList3" })]
    [TaskPane( "InputFormatSettingCaption", "InputFormatSettingTooltip", null, 2, false, ControlType.RadioButton, new string[] { "InputFormatSettingList1", "InputFormatSettingList2", "InputFormatSettingList3" })]
    public InputFormat InputFormatSetting
    {
        get
        {
            return this.inputFormat;
        }
        set
        {
            if (this.inputFormat != value)
            {
                this.inputFormat = value;
                OnPropertyChanged("InputFormatSetting");   
            }
        }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

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
