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
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.CompilerServices;

namespace Multiplexer
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo(false, "Multiplexer", "Choose one of the given inputs using the boolean-switch", "", "Multiplexer/icon.png")]
  public class Mux : IThroughput
  {
    #region Private variables
    private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();

    private readonly string inputOne = "InputOne";
    private readonly string inputTwo = "InputTwo";
    private readonly string outputOne = "OutputOne";

    private int inputs;
    private int outputs;
    private bool gotSwitchValue;
    private bool canSendPropertiesChangedEvent = true;
    #endregion Private variables

    public event DynamicPropertiesChanged OnDynamicPropertiesChanged;

    public Mux()
    {
      settings = new MuxSettings(this);      
      settings.OnGuiLogNotificationOccured += settings_OnGuiLogNotificationOccured;
      CanChangeDynamicProperty = true;
      
      // No dynProp event in constructor - editor will read the property initial without the event.
      // event can cause problems when using save files and is processed after 
      // connections have been restored. 
      CreateInputOutput(false); 
    }

    public void CreateInputOutput(bool announcePropertyChange)
    {
      DicDynamicProperties.Clear();
      dicInputBuffer.Clear();
      dicSwitchBuffer.Clear();
      AddInput(inputOne, "This value will be forwarded if switch is true(default).");
      AddInput(inputTwo, "This value will be forwarded if switch is false.");
      AddOutput(outputOne);
      if (announcePropertyChange) DynamicPropertiesChanged();
    }

    private void DynamicPropertiesChanged()
    {
      if (OnDynamicPropertiesChanged != null) OnDynamicPropertiesChanged(this);
    }

    private void settings_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
    {      
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(args.Message, this, args.NotificationLevel));
    }
    
    public Dictionary<string, DynamicProperty> dicDynamicProperties = new Dictionary<string, DynamicProperty>();

    [DynamicPropertyInfo("methodGetValue", "methodSetValue", "CanChangeDynamicProperty", "OnDynamicPropertiesChanged", "CanSendPropertiesChangedEvent")]
    public Dictionary<string, DynamicProperty> DicDynamicProperties
    {
      get { return dicDynamicProperties; }
      set { dicDynamicProperties = value; }
    }
    
    public bool CanChangeDynamicProperty
    {
      get { return settings.CanChangeProperty; }
      set { settings.CanChangeProperty = value; }
    }

    public bool CanSendPropertiesChangedEvent
    {
      get { return canSendPropertiesChangedEvent; }
      set { canSendPropertiesChangedEvent = value; }
    }


    private Type getCurrentType()
    {
      switch (settings.CurrentDataType)
      {
        case MuxSettings.DataTypes.CryptoolStream:
          return typeof(CryptoolStream);          
        case MuxSettings.DataTypes.String:
          return typeof(string);
        case MuxSettings.DataTypes.ByteArray:
          return typeof(byte[]);
        case MuxSettings.DataTypes.Boolean:
          return typeof(bool);
        case MuxSettings.DataTypes.Integer:
          return typeof(int);
        default:
          return null;          
      }
    }

    private QuickWatchFormat getQuickWatchFormat()
    {
      Type type = getCurrentType();
      if (type == typeof(CryptoolStream))
        return QuickWatchFormat.Hex;
      else if (type == typeof(string))
        return QuickWatchFormat.Text;
      else if (type == typeof(byte[]))
        return QuickWatchFormat.Hex;
      else if (type == typeof(bool))
        return QuickWatchFormat.Text;
      else if (type == typeof(int))
        return QuickWatchFormat.Text;
      else
        return QuickWatchFormat.None;
    }

    private object getCurrentValue(string name)
    {
      if (DicDynamicProperties.ContainsKey(name))
      {
        switch (settings.CurrentDataType)
        {
          case MuxSettings.DataTypes.CryptoolStream:
            CryptoolStream cryptoolStream = new CryptoolStream();
            listCryptoolStreamsOut.Add(cryptoolStream);
            cryptoolStream.OpenRead(((CryptoolStream)DicDynamicProperties[name].Value).FileName);
            return cryptoolStream;
          case MuxSettings.DataTypes.String:
            return DicDynamicProperties[name].Value;
          case MuxSettings.DataTypes.ByteArray:
            return DicDynamicProperties[name].Value;
          case MuxSettings.DataTypes.Boolean:
            return DicDynamicProperties[name].Value;
          case MuxSettings.DataTypes.Integer:
            return DicDynamicProperties[name].Value;
          default:
            return null;
        }
      }
      return null;
    }

    private void AddInput(string name, string toolTip)
    {
      inputs++;      
      if (name == null || name == string.Empty) name = "Input " + inputs;      
      DicDynamicProperties.Add(name, 
        new DynamicProperty(name, getCurrentType(),
          new PropertyInfoAttribute(Direction.InputData, name, toolTip, "", false, true, DisplayLevel.Beginner, getQuickWatchFormat(), null)));
      dicInputBuffer.Add(name, 0);
    }

    private void AddOutput(string name)
    {
      outputs++;      
      if (name == null || name == string.Empty) name = "Output " + outputs;
      
      DicDynamicProperties.Add(name,
        new DynamicProperty(name, getCurrentType(),
          new PropertyInfoAttribute(Direction.OutputData, name, "", "", false, false, DisplayLevel.Beginner, getQuickWatchFormat(), null)));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void methodSetValue(string propertyKey, object value)
    {
      if (DicDynamicProperties.ContainsKey(propertyKey)) DicDynamicProperties[propertyKey].Value = value;
      if (value is CryptoolStream) listCryptoolStreamsOut.Add((CryptoolStream)value);
      OnPropertyChanged(propertyKey);
      dicInputBuffer[propertyKey]++;
    }

    private Dictionary<string, int> dicInputBuffer = new Dictionary<string, int>();
    private Dictionary<bool, int> dicSwitchBuffer = new Dictionary<bool, int>();

    private List<string> listBuffer = new List<string>();

    [MethodImpl(MethodImplOptions.Synchronized)]
    public object methodGetValue(string propertyKey)
    {
      if (propertyKey == outputOne)
      {
        if (DicDynamicProperties[inputOne].Value != null && InputSwitch)
          return getCurrentValue(inputOne);

        else if (DicDynamicProperties[inputTwo].Value != null && !InputSwitch)
          return getCurrentValue(inputTwo);
      }
      else
        return getCurrentValue(propertyKey); // QuickWatchDataCall to Input values

      return null;
    }

    private bool inputSwitch;
    [PropertyInfo(Direction.InputData, "Input switch", "Selects the input.", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public bool InputSwitch    
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get { return inputSwitch; }
      [MethodImpl(MethodImplOptions.Synchronized)]
      set 
      {        
        inputSwitch = value;        
        OnPropertyChanged("InputSwitch");

        // only when buffer has been initialized correctly
        if (dicSwitchBuffer.Count == 2)
            dicSwitchBuffer[value]++;
      }
    }


    #region IPlugin Members

#pragma warning disable 67
		public event StatusChangedEventHandler OnPluginStatusChanged;
		public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
		public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

    private MuxSettings settings;
    public ISettings Settings
    {
      get { return settings; }
    }

    public UserControl Presentation
    {
      get { return null; }
    }

    public UserControl QuickWatchPresentation
    {
      get { return null; }
    }
    
    public void PreExecution()
    {            
      // clear Switch Buffer
      dicSwitchBuffer.Clear();
      dicSwitchBuffer.Add(false, 0);
      dicSwitchBuffer.Add(true, 0);

      // clear InputBuffer
      //for (int i = 0; i < dicInputBuffer.Count; i++)
      //{
      //  dicInputBuffer.Values.ToList()[i] = 0;
      //}
      foreach (string item in dicInputBuffer.Keys.ToList())
      {
        dicInputBuffer[item] = 0;
      }

      Dispose();

      // Set InputSwitch with default value => dicSwitchBuffer[default] will be increased, so 
      // the first input on default input will be forwared directly without waiting for a first
      // boolean input swtich.
      inputSwitch = false;

      if (settings.DefaultValue == 0)
        InputSwitch = true;
      else if (settings.DefaultValue == 1)
        InputSwitch = false;
      // else nothing to do -> no default value
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Execute()
    {
      if (InputSwitch && dicSwitchBuffer[true] > 0 && dicInputBuffer[inputOne] > 0)
      {        
        dicSwitchBuffer[true]--;
        dicInputBuffer[inputOne]--;        
        OnPropertyChanged(outputOne);
      }
      else if (!InputSwitch && dicSwitchBuffer[false] > 0 && dicInputBuffer[inputTwo] > 0)
      {
        dicSwitchBuffer[false]--;
        dicInputBuffer[inputTwo]--;
        OnPropertyChanged(outputOne);
      }
    }

    public void PostExecution()
    {
      Dispose();
    }

    public void Pause()
    {
      
    }

    public void Stop()
    {
      
    }

    public void Initialize()
    {
      
    }

    public void Dispose()
    {
      foreach (CryptoolStream cryptoolStream in listCryptoolStreamsOut)
      {
        cryptoolStream.Close();
      }
      listCryptoolStreamsOut.Clear();
      foreach (DynamicProperty item in DicDynamicProperties.Values)
      {
        item.Value = null;        
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

    private CryptoolStream getNewCryptoolStream(string Filename)
    {
      CryptoolStream cryptoolStream = new CryptoolStream();
      cryptoolStream.OpenRead(Filename);
      listCryptoolStreamsOut.Add(cryptoolStream);
      return cryptoolStream;
    }
  }
}
