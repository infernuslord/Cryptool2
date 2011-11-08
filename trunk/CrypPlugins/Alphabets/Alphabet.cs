/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using System.Windows.Controls;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Alphabets
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni-Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Cryptool.Alphabets.Properties.Resources", true, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "Alphabets/icon.gif")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class Alphabet : ICrypComponent
    {
      private AlphabetPresentation alphabetPresentation;

      private AlphabetSettings settings = new AlphabetSettings();
      public ISettings Settings
      {
        get { return settings; }
        set { settings = (AlphabetSettings)value; }
      }
      

      public Alphabet()
      {          
          alphabetPresentation = new AlphabetPresentation(this);
          Presentation = this.alphabetPresentation;
          alphabetPresentation.OnGuiLogNotificationOccured += alphabetPresentation_OnGuiLogNotificationOccured;
          settings.PropertyChanged += settings_PropertyChanged;
      }

      void alphabetPresentation_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
      {
          GuiLogMessage(args.Message, args.NotificationLevel); 
      }

      void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
        if (e.PropertyName == "Alphabet")
        {
          OnPropertyChanged("AlphabetOutput");
        }
      }

      [PropertyInfo(Direction.OutputData, "AlphabetOutputCaption", "AlphabetOutputTooltip", "", false, false, QuickWatchFormat.Text, null)]
      public string AlphabetOutput
      {
          get { return settings.Alphabet; }
          set { } //readonly
      }


      public UserControl Presentation { get; private set; }

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
      }

      public void PostExecution()
      {
      }

      public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

      public void OnPropertyChanged(string name)
      {
          if (PropertyChanged != null)
          {
              PropertyChanged(this, new PropertyChangedEventArgs(name));
          }
      }

      #region IPlugin Members

#pragma warning disable 67
			public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
            public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
            public event PluginProgressChangedEventHandler OnPluginProgressChanged;
      
      private void GuiLogMessage(string message, NotificationLevel logLevel)
      {
        if (OnGuiLogNotificationOccured != null)
        {
          OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
        }
      }

        public void Execute()
      {
          OnPropertyChanged("AlphabetOutput");
          ShowProgress(100, 100);
      }

        #endregion

      #region Private
      private void ShowProgress(double value, double max)
      {
          EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
      }
      #endregion
    }
}
