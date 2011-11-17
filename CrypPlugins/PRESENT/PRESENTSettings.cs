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

namespace Cryptool.PRESENT
{
  public class PRESENTSettings : ISettings
  {
    private int action = 0; //0=encrypt, 1=decrypt
    private int mode = 0; //0="ECB", 1="CBC", 2="CFB", 3="OFB"
    private int padding = 0; //0="None", 1="Zeros", 2="PKCS7"

    [ContextMenu( "ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, new string[] { "ActionList1", "ActionList2" })]
    [TaskPane( "ActionCaption", "ActionTooltip", "", 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
    public int Action
    {
      get { return this.action; }
      set
      {
        if (((int)value) != action)
        {
            this.action = (int)value;
            OnPropertyChanged("Action");
        }
      }
    }

    [ContextMenu("ModeCaption", "ModeTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4" })]
    [TaskPane("ModeCaption", "ModeTooltip", "", 2, false, ControlType.ComboBox, new String[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4" })]
    public int Mode
    {
      get { return this.mode; }
      set
      {
        if (((int)value) != mode)
        {
            this.mode = (int)value;
            OnPropertyChanged("Mode");
        }
      }
    }

    [ContextMenu("PaddingCaption", "PaddingTooltip", 3, ContextMenuControlType.ComboBox, null, new string[] { "PaddingList1", "PaddingList2", "PaddingList3" })]
    [TaskPane("PaddingCaption", "PaddingTooltip", "", 3, false, ControlType.ComboBox, new String[] { "PaddingList1", "PaddingList2", "PaddingList3" })]
    public int Padding
    {
      get { return this.padding; }
      set
      {
        if (((int)value) != padding)
        {
            this.padding = (int)value;
            OnPropertyChanged("Padding");
        }
      }
    }

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

  }
}
