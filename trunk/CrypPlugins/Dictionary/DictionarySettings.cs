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
using System.Windows.Controls;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace Dictionary
{
  public class CryptoolDictionarySettings : ISettings
  {
    # region private_variables
    private int currentDictionary;
    private ObservableCollection<string> collection = new ObservableCollection<string>();
    # endregion private_variables

    public delegate void ExecuteCallback();

    public CryptoolDictionarySettings()
    {
    }

    [TaskPane( "DictionaryCaption", "DictionaryTooltip", "", 0, true, ControlType.DynamicComboBox, new string[] { "Collection" })]
    public int Dictionary
    {
      get { return currentDictionary; }
      set
      {
        if (value != currentDictionary)
        {
          HasChanges = true;
          this.currentDictionary = value;
          OnPropertyChanged("Dictionary");
        }
      }
    }

    // CrypWin requires this to be a collection of strings
    [DontSave]
    public ObservableCollection<string> Collection
    {
      get { return collection; }
      set
      {
        if (value != collection)
        {
          collection = value;
        }
        OnPropertyChanged("Collection");
      }
    }

    #region ISettings Members

    private bool hasChanges;
    public bool HasChanges
    {
      get { return hasChanges; }
      set
      {
        if (value != hasChanges)
        {
          hasChanges = value;
          OnPropertyChanged("HasChanges");
        }
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    #endregion
  
  }
}
