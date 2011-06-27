/*
   Copyright 2008 Timm Korte, University of Siegen

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
using System.IO;
using Cryptool.PluginBase;
using System.Security.Cryptography;
using System.ComponentModel;

namespace RandomInput
{
  public class RandomInputSettings : ISettings
  {
    private bool hasChanges = false;
    private int bytes = 1;
    private int keepRND = 0; //0 = each stream / array will be filled with fresh random numbers

    [TaskPane( "BytesCaption", "BytesTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10496000)]
    public int Bytes
    {
      get { return this.bytes; }
      set
      {
        this.bytes = value;
        OnPropertyChanged("Bytes");
      }
    }

    [ContextMenu( "KeepRNDCaption", "KeepRNDTooltip", 2, ContextMenuControlType.ComboBox, null, new String[] { "KeepRNDList1", "KeepRNDList2" })]
    [TaskPane( "KeepRNDCaption", "KeepRNDTooltip", null, 2, false, ControlType.ComboBox, new String[] { "KeepRNDList1", "KeepRNDList2" })]
    public int KeepRND
    {
      get { return this.keepRND; }
      set
      {
        this.keepRND = (int)value;
        OnPropertyChanged("KeepRND");
      }
    }

    public UInt64 NumBytes
    {
      get
      {
        return UInt64.Parse(this.Bytes.ToString());
      }
      set
      {
        this.Bytes = int.Parse(value.ToString());
        OnPropertyChanged("NumBytes");
      }
    }

    #region IInputSettings Member

    public string SaveAndRestoreState { get; set; }

    #endregion

    #region INotifyPropertyChanged Members

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(name));
      }
    }

    #endregion

    #region ISettings Members

    public bool HasChanges
    {
      get
      {
        return hasChanges;
      }
      set
      {
        if (value != HasChanges)
        {
          hasChanges = value;
          OnPropertyChanged("HasChanges");
        }
      }
    }

    #endregion
  }
}
