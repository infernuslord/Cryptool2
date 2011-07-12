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
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;
using System.Diagnostics;

namespace TextOutput
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo("TextOutput.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "TextOutput/icon.png")]
  [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
  public class TextOutput : DependencyObject, ICrypComponent
  {
    private readonly string inputOne = "InputOne";

    #region Private variables
    /// <summary>
    /// This dic is used to store error messages while properties are set in PlayMode. The messages
    /// will be send in the execute method. 
    /// The editor flushes plugin color markers before calling the execute method. 
    /// So this messages would would still appear in LogWindow, but the color marker of the 
    /// plugin(red/yellow) would be lost if sending the messages right on property set.
    /// </summary>
    private Dictionary<string, NotificationLevel> dicWarningsAndErros = new Dictionary<string, NotificationLevel>();
    private bool canSendPropertiesChangedEvent = true;
    private int inputs = 0;

    private string _currentValue;
    public string CurrentValue
    {
        get
        {
            return _currentValue;
        }
        private set
        {
            _currentValue = value;
            OnPropertyChanged("CurrentValue");
        }
    }
    #endregion

    #region events
    public event DynamicPropertiesChanged OnDynamicPropertiesChanged;
    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    public event PropertyChangedEventHandler PropertyChanged;
    #pragma warning disable 67
    public event StatusChangedEventHandler OnPluginStatusChanged;
    #pragma warning restore
    #endregion events

    # region constructor
    public TextOutput()
    {
      Presentation = new TextOutputPresentation();
      QuickWatchPresentation = new TextOutputPresentation();
      settings = new TextOutputSettings(this);
      settings.OnGuiLogNotificationOccured += settings_OnGuiLogNotificationOccured;
      settings.PropertyChanged += settings_PropertyChanged;
      CanChangeDynamicProperty = true;
      // No dynProp event in constructor - editor will read the property initial without the event.
      // event can cause problems when using save files and is processed after 
      // connections have been restored. 
      CreateInputOutput(false);
    }

    # endregion

    # region Properties

    TextOutputSettings settings;
    public ISettings Settings
    {
      get { return settings; }
      set { settings = (TextOutputSettings)value; }
    }

    public void CreateInputOutput(bool announcePropertyChange)
    {
      DicDynamicProperties.Clear();
      AddInput(inputOne, "Input value");
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

    void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "PresentationFormatSetting" && CurrentValue != null)
      {
        setText(CurrentValue);
      }
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
        case TextOutputSettings.DynamicDataTypes.CStream:
          return typeof(ICryptoolStream);
        case TextOutputSettings.DynamicDataTypes.String:
          return typeof(string);
        case TextOutputSettings.DynamicDataTypes.ByteArray:
          return typeof(byte[]);
        case TextOutputSettings.DynamicDataTypes.Boolean:
          return typeof(bool);
        case TextOutputSettings.DynamicDataTypes.Integer:
          return typeof(int);
        case TextOutputSettings.DynamicDataTypes.Double:
          return typeof(double);
        case TextOutputSettings.DynamicDataTypes.Object:
          return typeof(object);
        default:
          return null;
      }
    }

    private object getCurrentValue(string name)
    {
      if (DicDynamicProperties.ContainsKey(name))
      {
            return DicDynamicProperties[name].Value;
            }
            else
        {
              return null;
        }
      }

    private void AddInput(string name, string toolTip)
    {
      inputs++;
      if (name == null || name == string.Empty) name = "Input " + inputs;
      DicDynamicProperties.Add(name,
        new DynamicProperty(name, getCurrentType(),
          new PropertyInfoAttribute(Direction.InputData, name, toolTip, "", false, true, QuickWatchFormat.None, null))
      );
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void methodSetValue(string propertyKey, object value)
    {
      try
      {
        if (DicDynamicProperties.ContainsKey(propertyKey))
        {
          DicDynamicProperties[propertyKey].Value = value;
        }

        if (value == null)
        {
            CurrentValue = string.Empty;
            OnPropertyChanged(propertyKey);
        }
        else
        {
            

            // check type explicitly, if connector type is set to anything else than object
            if (getCurrentType() != typeof(object) && !getCurrentType().IsAssignableFrom(value.GetType()))
            {
                GuiLogMessage(String.Format("Input data type does not match setting. Expected: {0}, Found: {1}", getCurrentType(), value.GetType()),
                    NotificationLevel.Error);
                return;
            }

            if (value is bool)
            {
                if (settings.BooleanAsNumeric)
                {
                    CurrentValue = Convert.ToInt32(value).ToString();
                }
                else
                {
                    CurrentValue = ((bool)value).ToString();
                }
            }
            else if (value is ICryptoolStream)
            {
                using (CStreamReader reader = ((ICryptoolStream)value).CreateReader())
                {
                    reader.WaitEof(); // does not support chunked streaming

                    // GuiLogMessage("Stream: Filling TextBoxes now...", NotificationLevel.Debug);
                    if (reader.Length > settings.MaxLength)
                        AddMessage("WARNING - Stream is too large (" + (reader.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
                    byte[] byteValues = new byte[Math.Min(settings.MaxLength, reader.Length)];
                    int bytesRead;
                    reader.Seek(0, SeekOrigin.Begin);
                    bytesRead = reader.ReadFully(byteValues, 0, byteValues.Length);
                    CurrentValue = GetStringForSelectedEncoding(byteValues);   
                }
            }
            else if (value is byte[])
            {
                byte[] byteArray = value as byte[];
                // GuiLogMessage("Byte array: Filling textbox now...", NotificationLevel.Debug);
                if (byteArray.Length > settings.MaxLength)
                {
                    AddMessage("WARNING - byte array is too large (" + (byteArray.Length / 1024).ToString("0.00") + " kB), output will be truncated to " + (settings.MaxLength / 1024).ToString("0.00") + "kB", NotificationLevel.Warning);
                }

                long size = byteArray.Length;
                if (size > settings.MaxLength)
                {
                    size = settings.MaxLength;
                }
                byte[] sizedArray = new byte[size];
                for (int i = 0; i < size; i++)
                {
                    sizedArray[i] = byteArray[i];
                }
                CurrentValue = GetStringForSelectedEncoding(sizedArray);
            }
            else if (value is Array)
            {
                Array array = (Array) value;
                StringBuilder sb = new StringBuilder();

                foreach(object obj in array)
                {
                    sb.AppendLine(obj == null ? "null" : obj.ToString());
                }
                CurrentValue = sb.ToString();
            }
            else
            {
                CurrentValue = value.ToString();
            }
        }

        if (CurrentValue != null)
        {
            if (CurrentValue.Length > settings.MaxLength)
            {
                CurrentValue = CurrentValue.Substring(0, settings.MaxLength);
            }

            setText(CurrentValue);
            OnPropertyChanged(propertyKey);
        }
      }
      catch (Exception ex)
      {
        GuiLogMessage(ex.Message, NotificationLevel.Error);
      }
    }

    private void setText(string fillValue)
    {
      int bytes = 0;
      if (fillValue != null)
      {
        bytes = Encoding.Default.GetBytes(fillValue.ToCharArray()).Length;

        // Presentation format conversion
        switch (settings.Presentation)
        {
          case TextOutputSettings.PresentationFormat.Text:
            // nothin to do here)
            break;
          case TextOutputSettings.PresentationFormat.Hex:
            byte[] byteValues = Encoding.Default.GetBytes(fillValue.ToCharArray());
            fillValue = BitConverter.ToString(byteValues, 0, byteValues.Length).Replace("-", "");
            break;
          case TextOutputSettings.PresentationFormat.Base64:
            fillValue = Convert.ToBase64String(Encoding.Default.GetBytes(fillValue.ToCharArray()));
            break;
          case TextOutputSettings.PresentationFormat.Decimal:
            byte[] decValues = Encoding.Default.GetBytes(fillValue.ToCharArray());
            StringBuilder sb = new StringBuilder();
            if (decValues.Length > 0)
            {
              sb.Append(decValues[0]);
              for (int i = 1; i < decValues.Length; i++)
              {
                sb.Append(" ");
                sb.Append(decValues[i]);
              }
            }
            fillValue = sb.ToString();
            break;
          default:
            break;
        }


        Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
        {
          if (settings.Append)
          {
            if (textOutputPresentation.textBox.Text.Length > settings.MaxLength)
            {
              GuiLogMessage("Text exceeds size limit. Deleting text...", NotificationLevel.Debug);
              textOutputPresentation.textBox.Text = string.Empty;
              textOutputPresentation.textBox.Tag = 0;
              textOutputQuickWatchPresentation.textBox.Text = string.Empty;
              textOutputQuickWatchPresentation.textBox.Tag = 0;
            }

            // append line breaks only if not first line
            if (!string.IsNullOrEmpty(textOutputPresentation.textBox.Text))
            {
              for (int i = 0; i < settings.AppendBreaks; i++)
              {
                if (settings.Presentation == TextOutputSettings.PresentationFormat.Text)
                {
                    int newlineSize = Encoding.Default.GetBytes("\n".ToCharArray()).Length;
                    textOutputPresentation.textBox.Tag = (int)textOutputPresentation.textBox.Tag + newlineSize;
                    textOutputQuickWatchPresentation.textBox.Tag = (int)textOutputQuickWatchPresentation.textBox.Tag + newlineSize;
                }
                textOutputPresentation.textBox.AppendText("\n");
                textOutputQuickWatchPresentation.textBox.AppendText("\n");
              }
            }
            textOutputPresentation.textBox.AppendText(fillValue);
            textOutputPresentation.textBox.Tag = (int)textOutputPresentation.textBox.Tag + bytes;
            textOutputQuickWatchPresentation.textBox.AppendText(fillValue);
            textOutputQuickWatchPresentation.textBox.Tag = (int)textOutputQuickWatchPresentation.textBox.Tag + bytes;

            textOutputPresentation.textBox.ScrollToEnd();
            textOutputQuickWatchPresentation.textBox.ScrollToEnd();
          }
          else
          {
            textOutputPresentation.textBox.Text = fillValue;
            textOutputPresentation.textBox.Tag = bytes;
            textOutputQuickWatchPresentation.textBox.Text = fillValue;
            textOutputQuickWatchPresentation.textBox.Tag = bytes;
          }
          if (settings.BooleanAsNumeric)
          {
              textOutputPresentation.labelBytes.Content = string.Format("{0:0,0}", Encoding.Default.GetBytes(textOutputPresentation.textBox.Text.ToCharArray()).Length) + " Bits";
              textOutputQuickWatchPresentation.labelBytes.Content = string.Format("{0:0,0}", Encoding.Default.GetBytes(textOutputPresentation.textBox.Text.ToCharArray()).Length) + " Bits";
          }
          else
          {
              textOutputPresentation.labelBytes.Content = string.Format("{0:0,0}", (int)textOutputQuickWatchPresentation.textBox.Tag) + " Bytes";
              textOutputQuickWatchPresentation.labelBytes.Content = string.Format("{0:0,0}", (int)textOutputQuickWatchPresentation.textBox.Tag) + " Bytes";
          }
        }, fillValue);
      }
    }

    private List<string> listBuffer = new List<string>();

    [MethodImpl(MethodImplOptions.Synchronized)]
    public object methodGetValue(string propertyKey)
    {
      return getCurrentValue(propertyKey); // QuickWatchDataCall to Input values
    }

    #endregion

    # region methods
    private string GetStringForSelectedEncoding(byte[] arrByte)
    {
      if (arrByte != null)
      {
        GuiLogMessage("Converting from \"" + settings.Encoding.ToString() + "\"...", NotificationLevel.Debug);
        string returnValue;

        // here conversion happens
        switch (settings.Encoding)
        {
          case TextOutputSettings.EncodingTypes.Default:
            returnValue = Encoding.Default.GetString(arrByte, 0, arrByte.Length);
            break;
          case TextOutputSettings.EncodingTypes.Unicode:
            returnValue = Encoding.Unicode.GetString(arrByte, 0, arrByte.Length);
            break;
          case TextOutputSettings.EncodingTypes.UTF7:
            returnValue = Encoding.UTF7.GetString(arrByte, 0, arrByte.Length);
            break;
          case TextOutputSettings.EncodingTypes.UTF8:
            returnValue = Encoding.UTF8.GetString(arrByte, 0, arrByte.Length);
            break;
          case TextOutputSettings.EncodingTypes.UTF32:
            returnValue = Encoding.UTF32.GetString(arrByte, 0, arrByte.Length);
            break;
          case TextOutputSettings.EncodingTypes.ASCII:
            returnValue = Encoding.ASCII.GetString(arrByte, 0, arrByte.Length);
            break;
          case TextOutputSettings.EncodingTypes.BigEndianUnicode:
            returnValue = Encoding.BigEndianUnicode.GetString(arrByte, 0, arrByte.Length);
            break;
          default:
            returnValue = Encoding.Default.GetString(arrByte, 0, arrByte.Length);
            break;
        }
        return returnValue;
      }
      return null;
    }

    private void Progress(double value, double max)
    {
      EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
    }
    # endregion

    #region INotifyPropertyChanged Members
    
    public void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion

    #region IPlugin Members

    private TextOutputPresentation textOutputPresentation
    {
      get { return Presentation as TextOutputPresentation; }
    }

    private TextOutputPresentation textOutputQuickWatchPresentation
    {
      get { return QuickWatchPresentation as TextOutputPresentation; }
    }

    public UserControl Presentation { get; private set; }

    public UserControl QuickWatchPresentation { get; private set; }

    public void Initialize()
    {

    }

    public void Dispose()
    {
      }

    public void Stop()
    {

    }

    public void PreExecution()
    {
      if (settings.FlushOnPreExecution)
      {
        textOutputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
        {
          textOutputPresentation.textBox.Tag = 0;
          textOutputPresentation.textBox.Text = null;
          textOutputQuickWatchPresentation.textBox.Tag = 0;
          textOutputQuickWatchPresentation.textBox.Text = null;
        }, null);
      }
        }

    public void PostExecution()
    {
    }

    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
    }

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    public void Execute()
    {
      Progress(100, 100);
      foreach (KeyValuePair<string, NotificationLevel> kvp in dicWarningsAndErros)
      {
        GuiLogMessage(kvp.Key, kvp.Value);
      }
      dicWarningsAndErros.Clear();
    }

    public void Pause()
    {

    }

    private void AddMessage(string message, NotificationLevel level)
    {
      if (!dicWarningsAndErros.ContainsKey(message))
        dicWarningsAndErros.Add(message, level);
    }

    #endregion
  }
}
