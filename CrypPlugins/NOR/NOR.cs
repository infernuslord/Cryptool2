using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.CompilerServices;

namespace Cryptool.Plugins.NOR
{
  [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo(false, "NOR", "Simple OR-Gate with default values.", null, "NOR/icon.png")]
  public class NOR : IThroughput
  {
    # region private variables
    private NORSettings settings = new NORSettings();
    private bool inputOne = false;
    private bool inputTwo = false;
    # endregion private variables

    public NOR()
    {
      settings.PropertyChanged += settings_PropertyChanged;
    }

    void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "SetInputOneToTrue")
      {
        inputOne = settings.SetInputOneToTrue;
      }
      if (e.PropertyName == "SetInputTwoToTrue")
      {
        inputTwo = settings.SetInputTwoToTrue;
      }
    }


    # region public interface
       
    [PropertyInfo(Direction.InputData, "Input one", "Input one.", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public bool InputOne
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get { return inputOne; }
      [MethodImpl(MethodImplOptions.Synchronized)]
      set
      {
        if (value != inputOne)
        {
          inputOne = value;
          OnPropertyChanged("InputOne");
        }
        OnPropertyChanged("Output");
      }
    }
    
    [PropertyInfo(Direction.InputData, "Input two", "Input two.", "", false, true, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public bool InputTwo
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get { return inputTwo; }
      [MethodImpl(MethodImplOptions.Synchronized)]
      set
      {
        if (value != inputTwo)
        {
          inputTwo = value;
          OnPropertyChanged("InputTwo");
        }
        OnPropertyChanged("Output");
      }
    }

    [PropertyInfo(Direction.OutputData, "Output", "Output.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public bool Output
    {
      [MethodImpl(MethodImplOptions.Synchronized)]
      get { return !(inputOne || inputTwo); }
      [MethodImpl(MethodImplOptions.Synchronized)]
      set { } // readonly
    }


    # endregion public interface

    #region IPlugin Members

#pragma warning disable 67
			public event StatusChangedEventHandler OnPluginStatusChanged;
			public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
			public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore
		
		public ISettings Settings
    {
      get { return settings; }
    }

    public System.Windows.Controls.UserControl Presentation
    {
      get { return null; }
    }

    public System.Windows.Controls.UserControl QuickWatchPresentation
    {
      get { return null; }
    }

    public void PreExecution()
    {
    }

    public void Execute()
    {
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
      if (settings.SetInputOneToTrue)
        inputOne = true;
      if (settings.SetInputTwoToTrue)
        inputTwo = true;
    }

    public void Dispose()
    {
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
