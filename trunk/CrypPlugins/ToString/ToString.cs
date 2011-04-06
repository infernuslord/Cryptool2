using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using System.Windows.Controls;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.ToString
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo("Cryptool.Plugins.ToString.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PluginDescriptionURL", "ToString/icon.png")]
  public class ToString : IThroughput
  {
    # region private variables
      private object input;
    private ToStringSettings settings = new ToStringSettings();
    # endregion private variables

    # region public interface
    [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", "", true, false, QuickWatchFormat.Text, null)]
    public object Input
    {
      get { return input; }
      set
      {
        input = value;
        OnPropertyChanged("Input");
        OnPropertyChanged("Output");
      }
    }

    [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", "", true, false, QuickWatchFormat.Text, null)]
    public string Output
    {
      get 
      {
        if (input != null)
          return input.ToString();
        else
          return null;
      }
      set { } 
    }
    # endregion public interface

    #region IPlugin Members

#pragma warning disable 67
		public event StatusChangedEventHandler OnPluginStatusChanged;
		public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
#pragma warning restore
    
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
        OnProgressChanged(1, 1);
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

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    private void OnProgressChanged(double value, double max)
    {
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
    }

    #endregion
  }
}
