﻿/*
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
    private EncodingTypes encoding = EncodingTypes.UTF8;
    private PresentationFormat presentation = PresentationFormat.Text;
    private int maxLength = 65536; //64kB
    private TextOutput myTextOutput;
    #endregion

    public enum EncodingTypes { Default = 0, Unicode = 1, UTF7 = 2, UTF8 = 3, UTF32 = 4, ASCII = 5, BigEndianUnicode = 6 };
    public enum PresentationFormat { Text, Hex, Base64, Decimal }

    public TextOutputSettings(TextOutput textOutput)
    {
      if (textOutput == null) throw new ArgumentException("textOutput");
      myTextOutput = textOutput;
    }

    public PresentationFormat Presentation
    {
      get { return this.presentation; }
      set
      {
        if (this.presentation != value)
        {
            this.presentation = value;
            OnPropertyChanged("Presentation");            
        }
      }
    }

    #region settings

    /// <summary>
    /// Encoding property used in the Settings pane. 
    /// </summary>
    [ContextMenu( "EncodingSettingCaption", "EncodingSettingTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
    [TaskPane( "EncodingSettingTPCaption", "EncodingSettingTPTooltip", null, 1, false, ControlType.RadioButton, new string[] { "EncodingSettingList1", "EncodingSettingList2", "EncodingSettingList3", "EncodingSettingList4", "EncodingSettingList5", "EncodingSettingList6", "EncodingSettingList7" })]
    public EncodingTypes Encoding
    {
      get
      {
        return this.encoding;
      }
      set
      {
        if (this.encoding != value)
        {
            this.encoding = value;
            OnPropertyChanged("Encoding");            
        }
      }
    }


    /// <summary>
    /// Gets or sets the presentation format setting.
    /// </summary>
    /// <value>The presentation format setting.</value>
    [ContextMenu( "PresentationFormatSettingCaption", "PresentationFormatSettingTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "PresentationFormatSettingList1", "PresentationFormatSettingList2", "PresentationFormatSettingList3", "PresentationFormatSettingList4" })]
    [TaskPane( "PresentationFormatSettingCaption", "PresentationFormatSettingTooltip", null, 2, false, ControlType.RadioButton, new string[] { "PresentationFormatSettingList1", "PresentationFormatSettingList2", "PresentationFormatSettingList3", "PresentationFormatSettingList4" })]
    public int PresentationFormatSetting
    {
      get
      {
        return (int)this.presentation;
      }
      set
      {
        if (this.presentation != (PresentationFormat)value)
        {
            this.presentation = (PresentationFormat)value;
            OnPropertyChanged("PresentationFormatSetting");            
        }
      }
    }


    /// <summary>
    /// Maximum size property used in the settings pane. 
    /// </summary>
    [TaskPane( "MaxLengthCaption", "MaxLengthTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 65536)]
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
    [ContextMenu( "AppendCaption", "AppendTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "AppendList1" })]
    [TaskPane("AppendCaption", "AppendTooltip", "AppendGroup", 0, false, ControlType.CheckBox, "", null)]
    public bool Append
    {
      get { return append; }
      set
      {
        if (value != append)
        {
          append = value;
          OnPropertyChanged("Append");
        }
      }
    }

    private int appendBreaks = 1;
    [TaskPane("AppendBreaksCaption", "AppendBreaksTooltip", "AppendGroup", 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
    public int AppendBreaks
    {
      get { return this.appendBreaks; }
      set
      {
        if (value != this.appendBreaks)
        {
          this.appendBreaks = value;
          OnPropertyChanged("AppendBreaks");
        }
      }
    }

    private bool booleanAsNumeric = false;
    [ContextMenu( "BooleanAsNumericCaption", "BooleanAsNumericTooltip", 5, ContextMenuControlType.CheckBox, null, new string[] { "BooleanAsNumericList1" })]
    [TaskPane( "BooleanAsNumericCaption", "BooleanAsNumericTooltip", null, 5, false, ControlType.CheckBox, "", null)]
    public bool BooleanAsNumeric
    {
        get { return booleanAsNumeric; }
        set
        {
            if (value != booleanAsNumeric)
            {
                booleanAsNumeric = value;
                OnPropertyChanged("BooleanAsNumeric");
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
