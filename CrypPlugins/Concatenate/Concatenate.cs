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
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using System.Runtime.CompilerServices;

namespace Concatenate
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo(false, "Concatenate", "Concatenates two input values. Fires output if second value arrives (The other value can be overwritten in the meantime).", null, "Concatenate/icon.png")]
  public class Concatenate : IThroughput
  {
    # region Fields
    private List<CryptoolStream> listCryptoolStreamsOut = new List<CryptoolStream>();
    private readonly string inputOne = "InputOne";
    private readonly string inputTwo = "InputTwo";
    private readonly string outputOne = "OutputOne";
    private ConcatenateSettings settings;
    # endregion Fields

    public event DynamicPropertiesChanged OnDynamicPropertiesChanged;

    public Concatenate()
    {
      settings = new ConcatenateSettings(this);
      settings.OnGuiLogNotificationOccured += settings_OnGuiLogNotificationOccured;
      CanChangeDynamicProperty = true;
      CreateInputOutput(false);
    }

    public void CreateInputOutput(bool announcePropertyChange)
    {
      DicDynamicProperties.Clear();
      AddInput(inputOne, "Input one");
      AddInput(inputTwo, "Input two");
      AddOutput(outputOne);
      if (announcePropertyChange) DynamicPropertiesChanged();
    }

    private void DynamicPropertiesChanged()
    {
      if (OnDynamicPropertiesChanged != null) OnDynamicPropertiesChanged(this);
    }

    void settings_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(args.Message, this, args.NotificationLevel));
    }

    # region DynamicProperties

    public Dictionary<string, DynamicProperty> dicDynamicProperties = new Dictionary<string, DynamicProperty>();

    [DynamicPropertyInfo("methodGetValue", "methodSetValue", "CanChangeDynamicProperty", "OnDynamicPropertiesChanged", "CanSendPropertiesChangedEvent")]
    public Dictionary<string, DynamicProperty> DicDynamicProperties
    {
      get { return dicDynamicProperties; }
      set { dicDynamicProperties = value; }
    }

    private bool canSendPropertiesChangedEvent = true;
    public bool CanSendPropertiesChangedEvent
    {
      get { return canSendPropertiesChangedEvent; }
      set { canSendPropertiesChangedEvent = value; }
    }

    public bool CanChangeDynamicProperty
    {
      get { return settings.CanChangeProperty; }
      set { settings.CanChangeProperty = value; }
    }

    private Type getCurrentType()
    {
      switch (settings.CurrentDataType)
      {
        case ConcatenateSettings.DataTypes.CryptoolStream:
          return typeof(CryptoolStream);
        case ConcatenateSettings.DataTypes.String:
          return typeof(string);
        case ConcatenateSettings.DataTypes.ByteArray:
          return typeof(byte[]);
        case ConcatenateSettings.DataTypes.Boolean:
          return typeof(bool);
        case ConcatenateSettings.DataTypes.Integer:
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

    private void AddInput(string name, string toolTip)
    {
      DicDynamicProperties.Add(name,
        new DynamicProperty(name, getCurrentType(),
          new PropertyInfoAttribute(Direction.InputData, name, toolTip, "", false, true, DisplayLevel.Beginner, getQuickWatchFormat(), null)));
    }

    private void AddOutput(string name)
    {
      DicDynamicProperties.Add(name,
        new DynamicProperty(name, getCurrentType(),
          new PropertyInfoAttribute(Direction.OutputData, name, "", "", false, false, DisplayLevel.Beginner, getQuickWatchFormat(), null)));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void methodSetValue(string propertyKey, object value)
    {
      try
      {
        if (DicDynamicProperties.ContainsKey(propertyKey))
        {
          DicDynamicProperties[propertyKey].Value = value;
          if (value is CryptoolStream) listCryptoolStreamsOut.Add((CryptoolStream)value);
          OnPropertyChanged(propertyKey);
        }
        // Fire output on value Pair. 
        if (DicDynamicProperties[inputOne].Value != null && DicDynamicProperties[inputTwo].Value != null)
        {
          object val1 = DicDynamicProperties[inputOne].Value;
          //DicDynamicProperties[inputOne].Value = null;
          object val2 = DicDynamicProperties[inputTwo].Value;
          //DicDynamicProperties[inputTwo].Value = null;

          switch (settings.CurrentDataType)
          {
            #region CryptoolStream
            case ConcatenateSettings.DataTypes.CryptoolStream:
              CryptoolStream stream1 = val1 as CryptoolStream;
              CryptoolStream stream2 = val2 as CryptoolStream;
              CryptoolStream returnStream = new CryptoolStream();
              returnStream.OpenWrite();
              byte[] byteValues = new byte[1024];
              int byteRead;
              int position = 0;
              long total = stream1.Length + stream2.Length;
              // first stream 
              while ((byteRead = stream1.Read(byteValues, 0, byteValues.Length)) != 0)
              {
                returnStream.Write(byteValues, 0, byteRead);
                if (OnPluginProgressChanged != null && stream1.Length > 0 &&
                    (int)(stream1.Position * 100 / total) > position)
                {
                  position = (int)(stream1.Position * 100 / total);
                  Progress(stream1.Position, total);
                }
              }
              // second stream
              while ((byteRead = stream2.Read(byteValues, 0, byteValues.Length)) != 0)
              {
                returnStream.Write(byteValues, 0, byteRead);
                if (OnPluginProgressChanged != null && stream2.Length > 0 &&
                    (int)((stream2.Position + stream1.Length) * 100 / total) > position)
                {
                  position = (int)((stream2.Position + stream1.Length) * 100 / total);
                  Progress((stream2.Position + stream1.Length), total);
                }
              }
              returnStream.Close();
              stream1.Position = 0;
              stream2.Position = 0;
              setOutputInternal(outputOne, returnStream);
              break;
            #endregion
            #region String
            case ConcatenateSettings.DataTypes.String:
              setOutputInternal(outputOne, (val1 as string) + (val2 as string));
              break;
            #endregion
            #region ByteArray
            case ConcatenateSettings.DataTypes.ByteArray:
              byte[] arrReturn = new byte[(val1 as byte[]).Length + (val2 as byte[]).Length];
              System.Buffer.BlockCopy((val1 as byte[]), 0, arrReturn, 0, (val1 as byte[]).Length);
              System.Buffer.BlockCopy((val2 as byte[]), 0, arrReturn, (val1 as byte[]).Length, (val2 as byte[]).Length);
              setOutputInternal(outputOne, arrReturn);
              break;
            #endregion
            #region Boolean
            case ConcatenateSettings.DataTypes.Boolean:
              Nullable<bool> boolValue = ((bool)val1) && ((bool)val2);
              setOutputInternal(outputOne, boolValue);
              break;
            #endregion
            #region Integer
            case ConcatenateSettings.DataTypes.Integer:
              int? intValue = ((int)val1) + ((int)val2);
              setOutputInternal(outputOne, intValue);
              break;
            #endregion
            default:
              break;
          }
          Progress(100, 100);
        }
      }
      catch (Exception exception)
      {
        GuiLogMessage(exception.Message, NotificationLevel.Error);
      }
    }

    /// <summary>
    /// Used to avoid loop in methodSetValue (setting values to null there destroys quickWatch updates)
    /// </summary>
    /// <param name="propertyKey">The property key.</param>
    /// <param name="value">The value.</param>
    private void setOutputInternal(string propertyKey, object value)
    {
      try
      {
        if (DicDynamicProperties.ContainsKey(propertyKey))
        {
          DicDynamicProperties[propertyKey].Value = value;
          if (value is CryptoolStream) 
            listCryptoolStreamsOut.Add((CryptoolStream)value);
          OnPropertyChanged(propertyKey);
        }
      }
      catch (Exception exception)
      {
        GuiLogMessage(exception.Message, NotificationLevel.Error);
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public object methodGetValue(string propertyKey)
    {
      try
      {
        if (DicDynamicProperties.ContainsKey(propertyKey))
        {
          if (DicDynamicProperties[propertyKey].Value is CryptoolStream)
          {
            CryptoolStream cryptoolStream = new CryptoolStream();
            cryptoolStream.OpenRead((DicDynamicProperties[propertyKey].Value as CryptoolStream).FileName);
            listCryptoolStreamsOut.Add(cryptoolStream);
            return cryptoolStream;
          }
          return DicDynamicProperties[propertyKey].Value;
        }
      }
      catch (Exception exception)
      {
        GuiLogMessage(exception.Message, NotificationLevel.Error);
      }
      return null;
    }

    # endregion DynamicProperties

    #region IPlugin Members

    public event StatusChangedEventHandler OnPluginStatusChanged;

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
    }

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;
    private void Progress(double value, double max)
    {
      EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
    }

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
      Dispose();
    }

    public void Execute()
    {
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
      // This values have to be set to null here because it might be CryptoolStreams. 
      // The files will be deleted before each run. So the quickWatch calls on PreExec
      // will produce an exception, because MethodGetValue tries to access the old files if not 
      // set to null here.
      DicDynamicProperties[inputOne].Value = null;
      DicDynamicProperties[inputTwo].Value = null;
      DicDynamicProperties[outputOne].Value = null;
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
