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
using Cryptool.PluginBase.Generator;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using System.Runtime.Remoting.Contexts;
using Cryptool.PluginBase.Miscellaneous;

namespace NumberGenerator
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo("NumberGenerator.Properties.Resources", true, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "NumberGenerator/icon.png")]
  public class Generator : IInput
  {
    
    public Generator()
    {
      settings = new GeneratorSettings();
      settings.PropertyChanged += settings_PropertyChanged;
    }

    void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IntegerStartValue")
      {
        IntegerValue = settings.IntegerStartValue;
      }
    }

    #region properties

    private int integerValue;
    [PropertyInfo(Direction.OutputData, "IntegerValueCaption", "IntegerValueTooltip", "", false, false, QuickWatchFormat.Text, null)]
    public int IntegerValue
    {
      get { return integerValue; }
      set 
      {
        if (value != integerValue)
        {
          integerValue = value;
          OnPropertyChanged("IntegerValue");
        }
      }
    }
    #endregion properties

    #region IPlugin Members
    public event StatusChangedEventHandler OnPluginStatusChanged;
    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    private GeneratorSettings settings;
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
    }

    public void Execute()
    {
      EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(50, 100));
      OnPropertyChanged("IntegerValue");
      EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(100, 100));
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

    #endregion

    # region helperMethods
    public void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
    }
    #endregion
  }
}
