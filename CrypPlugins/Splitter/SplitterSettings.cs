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

namespace Splitter
{
  public class SplitterSettings : ISettings
  {

    private int fireOnValue;
    [ContextMenu("Fire on true/false", "Fire next word on true/flase input", 0, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, "True", "False")]
    [TaskPane("Fire on true/false", "Fire next word on true/flase input", null, 0, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "True", "False" })]
    public int FireOnValue
    {
      get { return this.fireOnValue; }
      set
      {
        if (value != fireOnValue)
        {
          fireOnValue = value;
          HasChanges = true;
          OnPropertyChanged("FireOnValue");
        }
      }
    }

    private string delimiterDictionary = " ";
    [TaskPaneAttribute("Delimiter Dictionary", "The delimiter for the dictionary to split up the words.", null, 1, false, DisplayLevel.Beginner, ControlType.TextBox, ValidationType.RegEx, "^(.){0,1}$")]
    public string DelimiterDictionary
    {
      get { return this.delimiterDictionary; }
      set
      {
        if (value != delimiterDictionary)
        {
          delimiterDictionary = value;
          HasChanges = true;
        }
        OnPropertyChanged("DelimiterDictionary");
      }
    }

    #region ISettings Members

    private bool hasChanges;

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
