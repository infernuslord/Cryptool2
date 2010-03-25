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
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Concatenate
{
  public class ConcatenateSettings : ISettings
  {
    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

    public enum DataTypes
    {
      CryptoolStream, String, ByteArray, Boolean, Integer
    }

    private Concatenate MyConcatenate { get; set; }

    public ConcatenateSettings(Concatenate concatenate)
    {
      MyConcatenate = concatenate;
    }

    public bool CanChangeProperty { get; set; }

    private DataTypes currentDataType;
    public DataTypes CurrentDataType
    {
      get { return currentDataType; }
      set
      {
        if (currentDataType != value)
        {
          currentDataType = value;
          MyConcatenate.CreateInputOutput(MyConcatenate.CanSendPropertiesChangedEvent);

          // OnPropertyChanged("CurrentDataType");
        }
      }
    }

    [ContextMenu("Type", "Select DataType of plugin.", 0, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "CryptoolStream", "string", "byte[]", "boolean", "int" })]
    [TaskPane("Type", "Select DataType of plugin.", "", 0, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "CryptoolStream", "string", "byte[]", "boolean", "int" })]
    public int DataType
    {
      get { return (int)CurrentDataType; }
      set
      {
        if (CanChangeProperty)
        {
          if (value != (int)CurrentDataType) HasChanges = true;
          CurrentDataType = (DataTypes)value;
        }
        else EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs("Can't change type while plugin is connected.", null, NotificationLevel.Warning));
        OnPropertyChanged("DataType");
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

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
