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

namespace Multiplexer
{
  public class MuxSettings : ISettings
  {
    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

    public enum DataTypes
    {
      ICryptoolStream, String, ByteArray, Boolean, Integer
    }

    private Mux MyMux { get; set; }

    public MuxSettings(Mux mux)
    {
      MyMux = mux;
    }

    #region ISettings Members

    private bool hasChanges;
    public bool HasChanges
    {
      get { return hasChanges; }
      set 
      { 
        hasChanges = value;
        OnPropertyChanged("HasChanges");
      }
    }

    private int defaultValue;
    [ContextMenu("Default value", "Select the default start value", 0, ContextMenuControlType.ComboBox, null, "True", "False")]
    [TaskPane("Default value", "Select the default start value", null, 0, false, ControlType.ComboBox, new string[] { "True", "False" })]
    public int DefaultValue
    {
      get { return this.defaultValue; }
      set
      {
        if (value != defaultValue)
        {
          defaultValue = value;
          HasChanges = true;
          OnPropertyChanged("DefaultValue");
        }
      }
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

          // Changes must be applied synchronously, because onLoad of save file 
          // the Properties have to be set correctly BEFORE init of restore connections starts.
          //
          // The flag CanSendPropertiesChangedEvent will be set to false while loading a save file 
          // right after creating plugin instance. Next this Property will be set by the editor-loading-method. 
          // Here we set the new type without sending an event, because the event could be processed after
          // the connections have been restored. That would result in an unuseable workspace or throw an exception
          // while executing the init method.
          MyMux.CreateInputOutput(MyMux.CanSendPropertiesChangedEvent);
          // OnPropertyChanged("CurrentDataType");
        }
      }
    }
    
    [TaskPane("Type", "Select DataType of plugin.", "", 2, false, ControlType.ComboBox, new string[] { "ICryptoolStream", "string", "byte[]", "boolean", "int" } )]
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

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
