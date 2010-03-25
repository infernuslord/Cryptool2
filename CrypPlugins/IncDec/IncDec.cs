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
using System.Windows.Controls;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace IncDec
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo(false, "IncDec", "Increment/decrement operation", null, "IncDec/increment.png", "IncDec/decrement.png")]
  public class IncDec : IThroughput
  {
    private IncDecSettings settings = new IncDecSettings();
    private int input;

    public IncDec()
    {
      settings.PropertyChanged += settings_PropertyChanged;
    }

    void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "ModeSelect")
      {
        switch (settings.CurrentMode)
        {
          case IncDecSettings.Operator.Increment:
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 0));
            break;
          case IncDecSettings.Operator.Decrement:
            EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, 1));
            break;
          default:
            break;
        }
      }
    }


    [PropertyInfo(Direction.InputData, "Input.", "Input to increment or decrement.", null, false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public int Input
    {
      get { return input; }
      set
      {
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(50, 100));
        input = value;
        int returnValue = 0;
        switch (settings.CurrentMode)
        {
          case IncDecSettings.Operator.Increment:
            returnValue = input + settings.Value;
            break;
          case IncDecSettings.Operator.Decrement:
            returnValue = input - settings.Value;
            break;
          default:
            break;
        }
        this.output = returnValue;

        OnPropertyChanged("Input");
        OnPropertyChanged("Output");
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(100, 100));
      }
    }

    private int output;
    [PropertyInfo(Direction.OutputData, "Output.", "Output.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public int Output
    {
      get { return output; }
      set { } // readonly
    }


    #region IPlugin Members

#pragma warning disable 67
		public event StatusChangedEventHandler OnPluginStatusChanged;
		public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
		public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

    public Cryptool.PluginBase.ISettings Settings
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
    }

    public void Execute()
    {
    }

    public void PostExecution()
    {
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
  }
}
