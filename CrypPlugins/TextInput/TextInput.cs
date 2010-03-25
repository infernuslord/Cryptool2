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
  [PluginInfo(true, "TextInput", "TextInput", "detailed description", "TextInput/icon.png")]
  public class TextInput : DependencyObject, IInput
  {
    private TextInputPresentation textInputPresentation;
    private List<CryptoolStream> listCryptoolStreams = new List<CryptoolStream>();

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

      // No dispatcher necessary, handler is being called from GUI component
      textInputPresentation.labelBytesCount.Content = string.Format("{0:0,0}", Encoding.Default.GetBytes(textInputPresentation.textBoxInputText.Text.ToCharArray()).Length) + " Bytes";
      settings.Text = textInputPresentation.textBoxInputText.Text;
    }

    void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "EncodingSetting")
      {
        textInputPresentation.textBoxInputText.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
        {
          if (textInputPresentation.textBoxInputText.Text != null && textInputPresentation.textBoxInputText.Text != string.Empty)
          {
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

    # region Properties

    [PropertyInfo(Direction.OutputData, "Text", "Simple text to use as input for other plug-ins.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
    public string TextOutput
    {
      //[MethodImpl(MethodImplOptions.Synchronized)]
      get
      {        
        // GuiLogMessage("Got request for text...", NotificationLevel.Debug);
        string ret = (string)this.textInputPresentation.textBoxInputText.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
        {          
          return textInputPresentation.textBoxInputText.Text;
        }, textInputPresentation);
        
        if (ret == null || ret == string.Empty)
        {
          GuiLogMessage("Text: No text provided. Returning null", NotificationLevel.Debug);
          return null;
        }
        return ret;
      }
      set { }
    }

    [PropertyInfo(Direction.OutputData, "Stream", "The text input converted to memory stream.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
    public CryptoolStream StreamOutput
    {
      //[MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        byte[] arr = GetByteArray(false);
        if (arr != null)
        {
          CryptoolStream cryptoolStream = new CryptoolStream();
          listCryptoolStreams.Add(cryptoolStream);

          cryptoolStream.OpenRead(arr);
          // ShowProgress(100, 100);
          // GuiLogMessage("Got request for Stream. CryptoolStream created: " + cryptoolStream.FileName, NotificationLevel.Debug);
          return cryptoolStream;
        }
        GuiLogMessage("Stream: No input provided. Returning null", NotificationLevel.Debug);
        // ShowProgress(100, 100);
        return null;
      }
      set { } // readonly
    }

    private byte[] byteArrayOutput;
    [PropertyInfo(Direction.OutputData, "ByteArray", "The hex values as byte array.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
    public byte[] ByteArrayOutput
    {
      //[MethodImpl(MethodImplOptions.Synchronized)]
      get
      {
        // GuiLogMessage("Got request for ByteArray...", NotificationLevel.Debug);
        byteArrayOutput = GetByteArray(true);
        if (byteArrayOutput == null)
        {
          GuiLogMessage("ByteArray: No input provided. Returning null", NotificationLevel.Debug);
          return null;
        }
        return byteArrayOutput;
      }
      set { } // readonly
    }

    public byte[] GetByteArray(bool showMessage)
    {
        string data = (string)this.textInputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
        {
            return textInputPresentation.textBoxInputText.Text;
        }, null);

        if ((data != null) && (data.Length != 0))
        {
            // here conversion happens        
            switch (settings.Encoding)
            {
                case TextInputSettings.EncodingTypes.Default:
                    byteArrayOutput = Encoding.Default.GetBytes(data.ToCharArray());
                    break;
                case TextInputSettings.EncodingTypes.Unicode:
                    byteArrayOutput = Encoding.Unicode.GetBytes(data.ToCharArray());
                    break;
                case TextInputSettings.EncodingTypes.UTF7:
                    byteArrayOutput = Encoding.UTF7.GetBytes(data.ToCharArray());
                    break;
                case TextInputSettings.EncodingTypes.UTF8:
                    byteArrayOutput = Encoding.UTF8.GetBytes(data.ToCharArray());
                    break;
                case TextInputSettings.EncodingTypes.UTF32:
                    byteArrayOutput = Encoding.UTF32.GetBytes(data.ToCharArray());
                    break;
                case TextInputSettings.EncodingTypes.ASCII:
                    byteArrayOutput = Encoding.ASCII.GetBytes(data.ToCharArray());
                    break;
                case TextInputSettings.EncodingTypes.BigEndianUnicode:
                    byteArrayOutput = Encoding.BigEndianUnicode.GetBytes(data.ToCharArray());
                    break;
                default:
                    byteArrayOutput = Encoding.Default.GetBytes(data.ToCharArray());
                    break;
            }
            return byteArrayOutput;
        }

        return null;
    }

    private bool[] boolArrayOutput;
    [PropertyInfo(Direction.OutputData, "BoolArray", "The text input converted to bool array ('0' char equals false, else true).", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.None, null)]
    public bool[] BoolArrayOutput
    {
        //[MethodImpl(MethodImplOptions.Synchronized)]
        get
        {
            // GuiLogMessage("Got request for BoolArray...", NotificationLevel.Debug);
            boolArrayOutput = GetBoolArray(true);
            if (boolArrayOutput == null)
            {
                GuiLogMessage("BoolArray: No input provided. Returning null", NotificationLevel.Debug);
                return null;
            }
            return boolArrayOutput;
        }
        set { } // readonly
    }

    public bool[] GetBoolArray(bool showMessage)
    {
        string data = (string)this.textInputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback)delegate
        {
            return textInputPresentation.textBoxInputText.Text;
        }, null);

        if ((data != null) && (data.Length != 0))
        {
            // convert data into char array
            char[] dataCharArray = data.ToCharArray();

            boolArrayOutput = new bool[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
               boolArrayOutput[i] = (Convert.ToInt64(dataCharArray[i]) == 48) ? false : true;
               //if (Convert.ToInt64(dataCharArray[i]) == 48) boolArrayOutput[i] = false; else boolArrayOutput[i] = true;
            }

            return boolArrayOutput;
        }

        return null;
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
          textInputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
          {
              textInputPresentation.textBoxInputText.Text = settings.Text;
          }, null);
      }

      textInputPresentation.textBoxInputText.TextChanged += textBoxInputText_TextChanged;
    }

    public void Dispose()
    {
      foreach (CryptoolStream stream in listCryptoolStreams)
      {
        stream.Close();
      }
      listCryptoolStreams.Clear();

      byteArrayOutput = null;

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
      byteArrayOutput = null;
      // textInputPresentation.labelBytesCount.Content = "0 Bytes";
    }

    public void PostExecution()
    {
      Dispose();
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
