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
  [PluginInfo("Cryptool.TextInput.Properties.Resources", "PluginCaption", "PluginTooltip", "TextInput/Documentation/doc.xml", "TextInput/icon.png")]
  [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
  public class TextInput : DependencyObject, ICrypComponent
  {
    private TextInputPresentation textInputPresentation;

    public TextInput()
    {
      settings = new TextInputSettings();
      settings.OnLogMessage += settings_OnLogMessage;

      textInputPresentation = new TextInputPresentation();
      Presentation = textInputPresentation;
    }

    void textBoxInputText_TextChanged(object sender, TextChangedEventArgs e)
    {
        this.NotifyUpdate();

        // No dispatcher necessary, handler is being called from GUI component
        settings.Text = textInputPresentation.textBoxInputText.Text;
        textInputPresentation.labelBytesCount.Content =
            string.Format(Properties.Resources.PresentationFmt, settings.Text.Length, Encoding.UTF8.GetBytes(settings.Text).Length);
    }

    public void NotifyUpdate()
    {
      OnPropertyChanged("TextOutput");
    }

    void settings_OnLogMessage(string message, NotificationLevel loglevel)
    {
      GuiLogMessage(message, loglevel);
    }

    private string GetInputString()
    {
        if (textInputPresentation.textBoxInputText.Dispatcher.CheckAccess())
        {
            return textInputPresentation.textBoxInputText.Text;
        }
        else
        {
            return (string) this.textInputPresentation.textBoxInputText.Dispatcher.Invoke(DispatcherPriority.Normal, (DispatcherOperationCallback) delegate
            {
                return textInputPresentation.textBoxInputText.Text;
            }, textInputPresentation);
        }
    }


    # region Properties

    [PropertyInfo(Direction.OutputData, "TextOutputCaption", "TextOutputTooltip", true)]
    public string TextOutput
    {
        get
        {
            return GetInputString();  
        }
        set { }
    }

    #endregion

    #region IPlugin Members

    public UserControl Presentation { get; private set; }

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

      if (string.IsNullOrEmpty(value))
        GuiLogMessage("No input value returning null.", NotificationLevel.Debug); 
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
