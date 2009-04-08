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

namespace Cryptool.Plugins.CLK
{
  [Author("Soeren Rinne", "soeren.rinne@cryptool.org", "Uni Bochum", "http://www.rub.de")]
  [PluginInfo(true, "CLK", "Simple clock for clock-based plugins.", null, "CLK/icon.png")]
  public class CLK : IInput
  {
    # region private variables
    private bool output;
    # endregion private variables

    public CLK()
    {
        settings = new CLKSettings();
        settings.PropertyChanged += settings_PropertyChanged;
    }

    void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "SetClockToTrue")
      {
        //output = settings.SetClockToTrue;
      }
    }


    # region public interface
       
    [PropertyInfo(Direction.Output, "Output", "Output.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public bool Output
    {
        get { return output; }
        set
        {
            if (value != output)
            {
                output = value;
                OnPropertyChanged("Output");
            }
        }
    }


    # endregion public interface

    #region IPlugin Members
    public event StatusChangedEventHandler OnPluginStatusChanged;
    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    public event PluginProgressChangedEventHandler OnPluginProgressChanged;

    private CLKSettings settings;
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
        //output = settings.SetClockToTrue;
        OnPropertyChanged("Output");
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
      if (settings.SetClockToTrue)
        output = settings.SetClockToTrue;
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

    #endregion
  }
}
