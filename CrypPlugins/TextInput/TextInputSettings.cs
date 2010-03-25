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
    private EncodingTypes encoding = EncodingTypes.Default;
    private bool hasChanges = false;
    #endregion

    public enum EncodingTypes { Default = 0, Unicode = 1, UTF7 = 2, UTF8 = 3, UTF32 = 4, ASCII = 5, BigEndianUnicode = 6 };
    
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
        if (value != text) hasChanges = true;
        text = value; 
      }
    }

    /// <summary>
    /// Retrieves the current used encoding, or sets it.
    /// </summary>
    public EncodingTypes Encoding
    {
      get { return this.encoding; }
      set
      {
        if (this.encoding != value)
        {
          hasChanges = true;
          this.encoding = value;
          OnPropertyChanged("EncodingSetting");
        }
      }
    }

    /// <summary>
    /// Encoding property used in the Settings pane. 
    /// </summary>
    [ContextMenu("Stream encoding", "Choose the expected encoding of the byte array and stream.", 1, DisplayLevel.Experienced, ContextMenuControlType.ComboBox, null, new string[] { "Default system encoding", "Unicode", "UTF-7", "UTF-8", "UTF-32", "ASCII", "Big endian unicode" })]
    [TaskPane("Stream encoding", "Choose the expected encoding of the byte array and stream.", "", 1, false, DisplayLevel.Experienced, ControlType.RadioButton, new string[] { "Default system encoding", "Unicode", "UTF-7", "UTF-8", "UTF-32", "ASCII", "Big endian unicode" })]
    public int EncodingSetting
    {
      get
      {
        return (int)this.encoding;
      }
      set
      {
        if (this.Encoding != (EncodingTypes)value)
        {
          hasChanges = true;
          this.Encoding = (EncodingTypes)value;
          OnPropertyChanged("EncodingSetting");
          HasChanges = true;
        }
      }
    }
    
    public bool HasChanges
    {
      get { return hasChanges; }
      set 
      { 
        hasChanges = value;
        OnPropertyChanged("HasChanges");
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
