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
    private bool hasChanges = false;
    private int action = 0; //0=encrypt, 1=decrypt
    private int mode = 0; //0="ECB", 1="CBC", 2="CFB", 3="OFB"
    private int padding = 0; //0="None", 1="Zeros", 2="PKCS7"

    [ContextMenu("Action", "Do you want the input data to be encrypted or decrypted?", 1, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, new string[] { "encrypt", "decrypt" })]
    [TaskPane("Action", "Do you want the input data to be encrypted or decrypted?", "", 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "encrypt", "decrypt" })]
    public int Action
    {
      get { return this.action; }
      set
      {
        if (((int)value) != action) hasChanges = true;
        this.action = (int)value;
        OnPropertyChanged("Action");
      }
    }

    [ContextMenu("Chaining Mode", "Select the block cipher mode of operation.", 2, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "ECB", "CBC", "CFB", "OFB" })]
    [TaskPane("Chaining Mode", "Select the block cipher mode of operation.", "", 2, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "ECB", "CBC", "CFB", "OFB" })]
    public int Mode
    {
      get { return this.mode; }
      set
      {
        if (((int)value) != mode) hasChanges = true;
        this.mode = (int)value;
        OnPropertyChanged("Mode");
      }
    }

    [ContextMenu("Padding Mode", "Select a mode to fill partial data blocks.", 3, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "Zeros", "None", "PKCS7" })]
    [TaskPane("Padding Mode", "Select a mode to fill partial data blocks.", "", 3, false, DisplayLevel.Experienced, ControlType.ComboBox, new String[] { "Zeros", "None", "PKCS7" })]
    public int Padding
    {
      get { return this.padding; }
      set
      {
        if (((int)value) != padding) hasChanges = true;
        this.padding = (int)value;
        OnPropertyChanged("Padding");
      }
    }

    public bool HasChanges
    {
      get { return hasChanges; }
      set { hasChanges = value; }
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
