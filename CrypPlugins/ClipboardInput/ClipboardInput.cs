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
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using Cryptool.PluginBase.IO;

namespace ClipboardInput
{
  // Retrieves data from clipboard and passes it on as a stream
  [Author("Timm Korte", "cryptool@easycrypt.de", "Uni Bochum", "http://www.ruhr-uni-bochum.de")]
  [PluginInfo("ClipboardInput.Properties.Resources", true, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "ClipboardInput/icon.png")]
  public class ClipboardInput : IInput
  {

    #region Private Variables
    private ClipboardInputSettings settings;

    public ISettings Settings
    {
      get { return (ISettings)settings; }
      set { settings = (ClipboardInputSettings)value; }
    }

    #endregion

    public ClipboardInput()
    {
      this.settings = new ClipboardInputSettings();
    }


    private string data;
    // [QuickWatch(QuickWatchFormat.Text, null)]
    public string Data
    {
      get { return data; }
      set
      {
        data = value;
        // OnPropertyChanged("Data");
      }
    }

    private void GetClipboardData()
    {
      try
      {
        Data = Clipboard.GetText();
      }
      catch (Exception ex)
      {
        GuiLogMessage(ex.Message, NotificationLevel.Error);
      }
    }

    #region Interface
    [PropertyInfo(Direction.OutputData, "StreamOutputCaption", "StreamOutputTooltip", "", true, false, QuickWatchFormat.Text, null)]
    public ICryptoolStream StreamOutput
    {
      get
      {
        Progress(0.5, 1.0);
        Thread t = new Thread(new ThreadStart(GetClipboardData));
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();

        if (Data != null && Data != string.Empty)
        {
            ICryptoolStream cryptoolStream = null;
          switch (settings.Format)
          { //0="Text", 1="Hex", 2="Base64"
            case 1:
              GuiLogMessage("converting " + Data.Length + " bytes of clipboard data from hex...", NotificationLevel.Debug);
              cryptoolStream = Hex2Stream(Data);
              break;
            case 2:
              GuiLogMessage("converting " + Data.Length + " bytes of clipboard data from base64...", NotificationLevel.Debug);
              cryptoolStream = Base642Stream(Data);
              break;
            default:
              GuiLogMessage("converting " + Data.Length + " bytes of clipboard data from text...", NotificationLevel.Debug);
              cryptoolStream = Text2Stream(Data);
              break;
          }
          Progress(1.0, 1.0);
          return cryptoolStream;
        }
        else
        {
          GuiLogMessage("clipboard did not contain any text data", NotificationLevel.Error);
          return null;
        }
      }
      set
      {
      }
    }
    #endregion

    private ICryptoolStream Text2Stream(string data)
    {
        return new CStreamWriter(Encoding.Default.GetBytes(data));
    }

    private ICryptoolStream Base642Stream(string data)
    {
      byte[] temp = new byte[0];
      try
      {
        temp = Convert.FromBase64String(data);
      }
      catch (Exception exception)
      {
        GuiLogMessage(exception.Message, NotificationLevel.Error);
      }

        return new CStreamWriter(temp);
    }

    private ICryptoolStream Hex2Stream(string data)
    {
        return new CStreamWriter(ToByteArray(data));
    }

    public byte[] ToByteArray(String HexString)
    {
      int NumberChars = HexString.Length;
      byte[] bytes = new byte[NumberChars / 2];
      for (int i = 0; i < NumberChars; i += 2)
      {
        try
        {
          bytes[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
        }
        catch (Exception exception)
        {

          GuiLogMessage(exception.Message, NotificationLevel.Error);
          bytes[i / 2] = 0;
        }
      }
      return bytes;
    }

    #region IPlugin Member
    public event StatusChangedEventHandler OnPluginStatusChanged;
    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    public System.Windows.Controls.UserControl Presentation
    {
      get { return null; }
    }

    public System.Windows.Controls.UserControl QuickWatchPresentation
    {
      get { return null; }
    }

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
      Dispose();
    }

    public void PostExecution()
    {
      Dispose();
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string name)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(name));
      }
    }

    #endregion

    private void Progress(double value, double max)
    {
      if (OnPluginProgressChanged != null)
        OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
    }

    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      if (OnGuiLogNotificationOccured != null)
      {
        OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
      }
    }

    #region IPlugin Members


    public void Execute()
    {
      OnPropertyChanged("StreamOutput");
    }

    public void Pause()
    {

    }

    #endregion
  }
}
