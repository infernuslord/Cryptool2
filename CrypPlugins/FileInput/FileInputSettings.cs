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

namespace FileInput
{
  public class FileInputSettings : ISettings
  {
    private string saveAndRestoreState;
    public string SaveAndRestoreState
    {
      get { return saveAndRestoreState; }
      set 
      {
        if (value != saveAndRestoreState) hasChanges = true;
        saveAndRestoreState = value;
        OnPropertyChanged("SaveAndRestoreState");
      }
    }

    private bool hasChanges;

      private string openFilename;
    [TaskPane( "OpenFilenameCaption", "OpenFilenameTooltip", null, 1, false, ControlType.OpenFileDialog, FileExtension="All Files (*.*)|*.*")]
    public string OpenFilename
    {
      get { return openFilename; }
      set
      {
        if (value != openFilename)
        {
          openFilename = value;
            OnPropertyChanged("OpenFilename");
        }
      }
    }

    [TaskPane( "CloseFileCaption", "CloseFileTooltip", null, 2, false, ControlType.Button)]
    public void CloseFile()
    {
      OpenFilename = null;
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
