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

namespace Contains
{
  public class ContainsSettings : ISettings
  {
    public enum SearchType
    {      
      Hashtable,
      AhoCorasick
    }

    private SearchType search = SearchType.Hashtable;
    public SearchType Search
    {
      get { return search; }
    }


    [ContextMenu("Search Type", "Select search method.", 1, ContextMenuControlType.ComboBox, null, new string[] { "Hashtable" })]
    [TaskPane("Search Type", "Select search method.", "", 1, false, ControlType.ComboBox, new string[] { "Hashtable" })]
    public int SearchSetting
    {
      get { return (int)search; }
      set
      {
        if (value != (int)search)
        {
          HasChanges = true;
          this.search = (SearchType)value;
          OnPropertyChanged("SearchSetting");
        }
      }
    }

    private int hits = 1;
    [TaskPane("Number of hits to find.", "Search-method needs to find n-hits to return true.", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
    public int Hits
    {
      get { return this.hits; }
      set
      {
        if (value != this.hits)
        {
          this.hits = value;
          OnPropertyChanged("Hits");
          HasChanges = true;
        }
      }
    }

    private string delimiter = " ";
    [TaskPaneAttribute("Delimiter Input String", "The delimiter for the input text to split up the words.", null, 3, false, ControlType.TextBox, ValidationType.RegEx, "^(.){0,1}$")] // [a-zA-Z]|[0-9]|\\s
    public string DelimiterInputString
    {
      get { return this.delimiterDictionary; }
      set 
      {
        if (value != delimiterDictionary)
        {
          delimiterDictionary = value;
          HasChanges = true;
        }
        OnPropertyChanged("DelimiterInputString");
      }
    }

    private string delimiterDictionary = " ";
    [TaskPaneAttribute("Delimiter Dictionary", "The delimiter for the dictionary to split up the words.", null, 4, false, ControlType.TextBox, ValidationType.RegEx, "^(.){0,1}$")] // [a-zA-Z]|[0-9]|\\s
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

    private bool toLower;
    [ContextMenu("All words to lower", "yes / no", 5, ContextMenuControlType.CheckBox, null, "All words to lower")]
    [TaskPaneAttribute("All words to lower", "yes / no.", "", 5, false, ControlType.CheckBox, null)]
    public bool ToLower
    {
      get { return toLower; }
      set
      {
        if (toLower != value)
        {
          HasChanges = true;
          toLower = value;
          OnPropertyChanged("ToLower");
        }
      }
    }

    private bool hitPercentFromInputString = false;
    [ContextMenu("Hits as percent value", "Hits value is interpreted as percent value based on input string.", 6, ContextMenuControlType.CheckBox, null, "Hits as percent value")]
    [TaskPaneAttribute("Hits as percent value", "Hits value is interpreted as percent value based on input string.", "", 6, false, ControlType.CheckBox, null)]
    public bool HitPercentFromInputString
    {
      get { return hitPercentFromInputString; }
      set
      {
        if (hitPercentFromInputString != value)
        {
          HasChanges = true;
          hitPercentFromInputString = value;
          OnPropertyChanged("HitPercentFromInputString");
        }
      }
    }

    private bool countWordsOnlyOnce = true;
    [ContextMenu("Count each word only once", "Count each word only once (even if the same word appears several times)", 7, ContextMenuControlType.CheckBox, null, "Count each word only once.")]
    [TaskPaneAttribute("Count each word only once", "Count each word only once (even if the same word appears several times)", "", 7, false, ControlType.CheckBox, null)]
    public bool CountWordsOnlyOnce
    {
      get { return countWordsOnlyOnce; }
      set
      {
        if (countWordsOnlyOnce != value)
        {
          HasChanges = true;
          countWordsOnlyOnce = value;
          if (countWordsOnlyOnce && search == SearchType.AhoCorasick) countWordsOnlyOnce = false;
          OnPropertyChanged("CountWordsOnlyOnce");
        }
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
