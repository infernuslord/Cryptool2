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

namespace NumberGenerator
{
  public class GeneratorSettings : ISettings
  {

    #region taskPane

    private int integerStartValue;
    [TaskPane("Start value", "Integer value to start from.", null, 0, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
    public int IntegerStartValue
    {
      get { return this.integerStartValue; }
      set
      {
        if (value != this.integerStartValue)
        {
          this.integerStartValue = value;
          OnPropertyChanged("IntegerStartValue");
          HasChanges = true;
        }
      }
    }

    #endregion taskPane

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
