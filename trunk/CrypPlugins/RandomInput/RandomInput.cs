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

// Cryptool RandomInput Plugin
// Author: Timm Korte, cryptool@easycrypt.de

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Security.Cryptography;
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase.Miscellaneous;

namespace RandomInput
{
  [Author("Timm Korte", "cryptool@easycrypt.de", "Uni Bochum", "http://www.ruhr-uni-bochum.de")]
  [PluginInfo("RandomInput.Properties.Resources", true, "PluginCaption", "PluginTooltip", "RandomInput/DetailedDescription/doc.xml", "RandomInput/icon.png")]
  [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
  public class RandomInput : ContextBoundObject, ICrypComponent
  {
    private int maxByteArraySize = 10496000;
    private byte[] rndArray;
    private RandomInputSettings settings;
    private RNGCryptoServiceProvider sRandom = new RNGCryptoServiceProvider();

    public ISettings Settings
    {
      get { return settings; }
      set { settings = (RandomInputSettings)value; }
    }

    public RandomInput()
    {
      this.settings = new RandomInputSettings();
    }

    private void getRand()
    {
      try
      {
        if (settings.KeepRND == 0 || rndArray == null || (UInt64)rndArray.Length != settings.NumBytes)
        {
          Progress(50, 100);
          rndArray = new byte[Math.Min((UInt64)settings.NumBytes, (UInt64)maxByteArraySize)];
          sRandom.GetBytes(rndArray);

          GuiMessage("Generated " + rndArray.Length + " bytes of random data.", NotificationLevel.Info);
          Progress(100, 100);
        }

        // The PropertyChanged event is send always, because new chain run needs to 
        // get the output events from input plugins every time the execute method is
        // executed.
        OnPropertyChanged("ByteArrayOutput");
        OnPropertyChanged("StreamOutput");
      }
      catch
      {
        GuiMessage("Can't create random data. Incorrect length value set in the Counter field.", NotificationLevel.Error);
      }
    }

    [PropertyInfo(Direction.OutputData, "StreamOutputCaption", "StreamOutputTooltip", "", false, false, QuickWatchFormat.Hex, null)]
    public ICryptoolStream StreamOutput
    {
      get
      {
        Progress(0.5, 1.0);
        // getRand(); creates a loop if OnPropertyChanged is used inside the property. is now called in execute method
        if (rndArray == null) getRand();
        if (rndArray != null)
        {
            
          GuiMessage("Send " + rndArray.Length + " bytes of random data as stream.", NotificationLevel.Debug);
          Progress(1.0, 1.0);
          return new CStreamWriter(rndArray);
        }
        GuiMessage("No stream generated. Returning null.", NotificationLevel.Error);
        Progress(1.0, 1.0);
        return null;
      }
      set { } // readonly
    }

    [PropertyInfo(Direction.OutputData, "ByteArrayOutputCaption", "ByteArrayOutputTooltip", "", false, false, QuickWatchFormat.Hex, null)]
    public byte[] ByteArrayOutput
    {
      get
      {
        Progress(0.5, 1.0);
        // getRand();
        if (rndArray != null)
          GuiMessage("Send " + rndArray.Length + " bytes of random data as byte array.", NotificationLevel.Debug);
        else
          GuiMessage("No bytes generated. Returning null.", NotificationLevel.Error);

        Progress(1.0, 1.0);        
        return rndArray;
      }
      set { } // readonly
    }

    #region IInput Member

    #endregion

    #region IPlugin Member

    public UserControl PresentationControl { get; private set; }

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
      if (!(settings.KeepRND == 1)) rndArray = null;
      Dispose();
    }

    public void PostExecution()
    {
      Dispose();
    }

    public event StatusChangedEventHandler OnPluginStatusChanged;

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    public UserControl Presentation
    {
      get { return null; }
    }

      public void Execute()
    {
      getRand();
      Progress(1.0, 1.0);
    }

    public void Pause()
    {

    }

    #endregion


    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion

    private void GuiMessage(string message, NotificationLevel level)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, level));
    }

    private void Progress(double value, double max)
    {
      EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
    }
  }
}
