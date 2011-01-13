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

namespace ClipboardInput {
	public class ClipboardInputSettings : ISettings {
		private int format = 0; //0="Text", 1="Hex", 2="Base64"

    [ContextMenu("Format", "Data format to convert from", 1, ContextMenuControlType.ComboBox, null, new string[] { "text", "hex", "base64" })]
    [TaskPane("Format", "Data format to convert from", "", 1, false, ControlType.ComboBox, new string[] { "text", "hex", "base64" })]
		public int Format {
			get { return this.format; }
			set 
      { 
        this.format = (int)value;
        OnPropertyChanged("Format");
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

    private bool hasChanges = false;
    public bool HasChanges
    {
      get { return hasChanges; }
      set { hasChanges = value; }
    }

    #endregion
  }
}
