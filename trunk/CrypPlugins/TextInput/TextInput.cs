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
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.IO;
using Cryptool.TextInput.Helper;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Runtime.CompilerServices;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Remoting.Contexts;

namespace Cryptool.TextInput
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo("Cryptool.TextInput.Properties.Resources", true, "TextInputCaption", "TextInputTooltip", "Cryptool.TextInput.Documentation.doc.xml", "TextInput/icon.png")]
  public class TextInput : DependencyObject, IInput
  {
    private TextInputPresentation textInputPresentation;

    public TextInput()
    {
      settings = new TextInputSettings();
      settings.OnLogMessage += settings_OnLogMessage;
      settings.PropertyChanged += settings_PropertyChanged;

      textInputPresentation = new TextInputPresentation();
      Presentation = textInputPresentation;
    }

    void textBoxInputText_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.NotifyUpdate();

      int bytes = 0;

      switch (settings.InputFormatSetting)
      {
          case TextInputSettings.InputFormat.Text:
              bytes = Encoding.Default.GetBytes(textInputPresentation.textBoxInputText.Text.ToCharArray()).Length;
              break;
          case TextInputSettings.InputFormat.Hex:
              bytes = ConvertHexStringToByteArray(textInputPresentation.textBoxInputText.Text).Length;
              break;
          case TextInputSettings.InputFormat.Base64:
              try
              {
                  bytes = Convert.FromBase64String(textInputPresentation.textBoxInputText.Text).Length;
              }
              catch (FormatException)
              {
                  bytes = 0;
                  GuiLogMessage("Invalid Base64 format", NotificationLevel.Warning);
              }
              break;
          default:
              throw new ArgumentOutOfRangeException();
      }

        // No dispatcher necessary, handler is being called from GUI component
      textInputPresentation.labelBytesCount.Content = string.Format("{0:0,0}", bytes) + " Bytes";
      settings.Text = textInputPresentation.textBoxInputText.Text;
    }

    void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "EncodingSetting")
      {
        textInputPresentation.textBoxInputText.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
        {
          if (!string.IsNullOrEmpty(textInputPresentation.textBoxInputText.Text))
          {
            NotifyUpdate();
          }
        }, null);
      }
      else if (e.PropertyName == "InputFormatSetting")
      {
          textInputPresentation.textBoxInputText.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
          {
              if (!string.IsNullOrEmpty(textInputPresentation.textBoxInputText.Text))
              {
                  textBoxInputText_TextChanged(null, null);
                  NotifyUpdate();
              }
          }, null);
      }
    }

    public void NotifyUpdate()
    {
      OnPropertyChanged("TextOutput");
      OnPropertyChanged("StreamOutput");
      OnPropertyChanged("ByteArrayOutput");
      OnPropertyChanged("BoolArrayOutput");
    }

    void settings_OnLogMessage(string message, NotificationLevel loglevel)
    {
      GuiLogMessage(message, loglevel);
    }

    private string GetInputString()
    {
        return (string)this.textInputPresentation.textBoxInputText.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
        {
            return textInputPresentation.textBoxInputText.Text;
        }, textInputPresentation);
    }

      private byte[] ConvertHexStringToByteArray(string input)
      {
          if (String.IsNullOrEmpty(input))
              return new byte[0];

          StringBuilder cleanHexString = new StringBuilder();

          //cleanup the input
          foreach (char c in input)
          {
              if (Uri.IsHexDigit(c))
                  cleanHexString.Append(c);
          }

          int numberChars = cleanHexString.Length;

          if (numberChars < 2) // Need at least 2 chars to make one byte
              return new byte[0];

          byte[] bytes = new byte[numberChars / 2];

          for (int i = 0; i < numberChars-1; i += 2)
          {
              bytes[i / 2] = Convert.ToByte(cleanHexString.ToString().Substring(i, 2), 16);
          }
          return bytes;
      }

      private string ConvertBytesToString(byte[] data)
      {
          switch (settings.Encoding)
          {
              case TextInputSettings.EncodingTypes.Unicode:
                  return Encoding.Unicode.GetString(data);
                  break;
              case TextInputSettings.EncodingTypes.UTF7:
                  return Encoding.UTF7.GetString(data);
                  break;
              case TextInputSettings.EncodingTypes.UTF8:
                  return Encoding.UTF8.GetString(data);
                  break;
              case TextInputSettings.EncodingTypes.UTF32:
                  return Encoding.UTF32.GetString(data);
                  break;
              case TextInputSettings.EncodingTypes.ASCII:
                  return Encoding.ASCII.GetString(data);
                  break;
              case TextInputSettings.EncodingTypes.BigEndianUnicode:
                  return Encoding.BigEndianUnicode.GetString(data);
                  break;
              default:
                  return Encoding.Default.GetString(data);
          }
      }

      private byte[] ConvertStringToByteArray(string inputString)
      {
          if (String.IsNullOrEmpty(inputString))
          {
              return new byte[0];
          }

          // here conversion happens        
          switch (settings.Encoding)
          {
              case TextInputSettings.EncodingTypes.Default:
                  return Encoding.Default.GetBytes(inputString);
              case TextInputSettings.EncodingTypes.Unicode:
                  return Encoding.Unicode.GetBytes(inputString);
              case TextInputSettings.EncodingTypes.UTF7:
                  return Encoding.UTF7.GetBytes(inputString);
              case TextInputSettings.EncodingTypes.UTF8:
                  return Encoding.UTF8.GetBytes(inputString);
              case TextInputSettings.EncodingTypes.UTF32:
                  return Encoding.UTF32.GetBytes(inputString);
              case TextInputSettings.EncodingTypes.ASCII:
                  return Encoding.ASCII.GetBytes(inputString);
              case TextInputSettings.EncodingTypes.BigEndianUnicode:
                  return Encoding.BigEndianUnicode.GetBytes(inputString);
              default:
                  return Encoding.Default.GetBytes(inputString);
          }
      }

      private bool[] ConvertByteArrayToBoolArray(byte[] input)
      {
          bool[] output = new bool[input.Length];

          for (int i = 0; i < output.Length; i++)
          {
              output[i] = input[i] != 0x00;
          }

          return output;
      }

    # region Properties

    [PropertyInfo(Direction.OutputData, "Text", "Simple text to use as input for other plug-ins.", "", true, false, QuickWatchFormat.None, null)]
    public string TextOutput
    {
      get
      {
          string inputString = GetInputString();

          switch(settings.InputFormatSetting)
          {
              case TextInputSettings.InputFormat.Hex:
                  {
                      byte[] data = ConvertHexStringToByteArray(inputString);
                      return ConvertBytesToString(data);
                  }
              case TextInputSettings.InputFormat.Base64:
                  {
                      try
                      {
                          byte[] data = Convert.FromBase64String(inputString);
                          return ConvertBytesToString(data);
                      }
                      catch (FormatException e)
                      {
                          GuiLogMessage("Invalid Base64 format", NotificationLevel.Warning);
                          return string.Empty;
                      }
                  }
              default: // includes InputFormat.Text
                  return inputString;
          }
          
      }
      set { }
    }

    [PropertyInfo(Direction.OutputData, "Stream", "The text input converted to memory stream.", "", true, false, QuickWatchFormat.None, null)]
    public ICryptoolStream StreamOutput
    {
      get
      {
        byte[] arr = ByteArrayOutput;
        if (arr != null)
        {
            return new CStreamWriter(arr);
        }
        GuiLogMessage("Stream: No input provided. Returning null", NotificationLevel.Debug);
        // ShowProgress(100, 100);
        return null;
      }
      set { } // readonly
    }

    [PropertyInfo(Direction.OutputData, "ByteArray", "The hex values as byte array.", "", true, false, QuickWatchFormat.None, null)]
    public byte[] ByteArrayOutput
    {
      get
      {
          string inputString = GetInputString();

          switch (settings.InputFormatSetting)
          {
              case TextInputSettings.InputFormat.Hex:
                  return ConvertHexStringToByteArray(inputString);
              case TextInputSettings.InputFormat.Base64:
                  try
                  {
                      return Convert.FromBase64String(inputString);
                  } catch(FormatException e)
                  {
                      GuiLogMessage("Invalid Base64 format", NotificationLevel.Warning);
                      return new byte[0];
                  }
              default: // includes InputFormat.Text
                  return ConvertStringToByteArray(inputString);
          }
      }
      set { } // readonly
    }

    [PropertyInfo(Direction.OutputData, "BoolArray", "The text input converted to bool array ('0' char or 0x00 equals false, else true).", "", true, false, QuickWatchFormat.None, null)]
    public bool[] BoolArrayOutput
    {
        get
        {
            string inputString = GetInputString();

            switch(settings.InputFormatSetting)
            {
                case TextInputSettings.InputFormat.Hex:
                    {
                        byte[] data = ConvertHexStringToByteArray(inputString);
                        return ConvertByteArrayToBoolArray(data);
                    }
                case TextInputSettings.InputFormat.Base64:
                    {
                        try
                        {
                            byte[] data = Convert.FromBase64String(inputString);
                            return ConvertByteArrayToBoolArray(data);
                        }
                        catch (FormatException e)
                        {
                            GuiLogMessage("Invalid Base64 format", NotificationLevel.Warning);
                            return new bool[0];
                        }
                    }
                default: // includes InputFormat.Text
                    bool[] output = new bool[inputString.Length];
                    for(int i = 0; i < output.Length; i++)
                    {
                        output[i] = inputString[i] != '0';
                    }
                    return output;
            }
        }
        set { } // readonly
    }

    #endregion

    #region IPlugin Members

    public UserControl Presentation { get; private set; }

    public UserControl QuickWatchPresentation
    {
      get { return Presentation; }
    }

    public void Initialize()
    {
      if (textInputPresentation.textBoxInputText != null)
      {
          textInputPresentation.textBoxInputText.TextChanged -= textBoxInputText_TextChanged;
          textInputPresentation.textBoxInputText.TextChanged += textBoxInputText_TextChanged;

          textInputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
          {
              textInputPresentation.textBoxInputText.Text = settings.Text;
          }, null);
      }      
    }

    public void Dispose()
    {
      textInputPresentation.textBoxInputText.TextChanged -= textBoxInputText_TextChanged;
    }

    public void Execute()
    {      
      NotifyUpdate();
      ShowProgress(100, 100);
      string value = (string)this.textInputPresentation.textBoxInputText.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
      {
          return textInputPresentation.textBoxInputText.Text;
      }, textInputPresentation);

      if (value == null || value == string.Empty)
        GuiLogMessage("No input value returning null.", NotificationLevel.Debug); 
    }

    public void Stop()
    {
    }

    public void PreExecution()
    {
      // textInputPresentation.labelBytesCount.Content = "0 Bytes";
    }

    public void PostExecution()
    {
    }

    #endregion

    private void ShowProgress(double value, double max)
    {
      EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
    }

    #region IPlugin Members

#pragma warning disable 67
		public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      if (OnGuiLogNotificationOccured != null)
      {
        OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
      }
    }

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    private TextInputSettings settings;
    public ISettings Settings
    {
      get
      {
        return settings;
      }
    }

    public void Pause()
    {

    }
    #endregion

    #region INotifyPropertyChanged Members
    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }
    #endregion
  }
}
