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
//timer
using System.Timers;

namespace Cryptool.Plugins.CLK
{
  [Author("Soeren Rinne", "soeren.rinne@cryptool.org", "Uni Bochum", "http://www.rub.de")]
  [PluginInfo(true, "CLK", "Simple clock for clock-based plugins.", null, "CLK/icon.png", "CLK/Images/true.png", "CLK/Images/false.png")]
  public class CLK : IInput
  {
    # region private variables
    private bool output;
    private int timeout = 2000;
    private int rounds = 10;
    private Timer aTimer = new Timer();
    # endregion private variables

    public int myRounds;

    public CLK()
    {
        settings = new CLKSettings();
        settings.PropertyChanged += settings_PropertyChanged;

        // set picture according to settings value
        /* BRINGT NIX - WARUM?
        if (settings.SetClockToTrue) StatusChanged((int)CLKImage.True);
        else StatusChanged((int)CLKImage.False);*/
    }

    void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "SetClockToTrue")
      {
        output = settings.SetClockToTrue;
        if (output) StatusChanged((int)CLKImage.True);
        else StatusChanged((int)CLKImage.False);
      }
      if (e.PropertyName == "CLKTimeout")
      {
          timeout = settings.CLKTimeout;
      }
      if (e.PropertyName == "Rounds")
      {
          rounds = settings.Rounds;
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
        if (settings.SetClockToTrue) StatusChanged((int)CLKImage.True);
        else StatusChanged((int)CLKImage.False);

        myRounds = settings.Rounds;
        GuiLogMessage("myRounds: " + myRounds.ToString(), NotificationLevel.Info);
    }

    public void Execute()
    {        
        process(settings.CLKTimeout);
        //change picture
        if (settings.SetClockToTrue) StatusChanged((int)CLKImage.True);
        else StatusChanged((int)CLKImage.False);
        
    }

    private void process(int timeout)
    {
        
        // Normally, the timer is declared at the class level, so
        // that it doesn't go out of scope when the method ends.
        // In this example, the timer is needed only while Main 
        // is executing. However, KeepAlive must be used at the
        // end of Main, to prevent the JIT compiler from allowing 
        // aggressive garbage collection to occur before Main 
        // ends.

        // Hook up the Elapsed event for the timer.
        aTimer.Elapsed += new ElapsedEventHandler(sendCLKSignal);

        // Set the Interval to 2 seconds (2000 milliseconds).
        aTimer.Interval = timeout;
        aTimer.Enabled = true;

        // Keep the timer alive until the end of Main.
        //GC.KeepAlive(aTimer);
    }

    private void sendCLKSignal(object sender, EventArgs e)
    {
        if (myRounds != 0)
        {
            OnPropertyChanged("Output");
            myRounds--;
        }
        else
        {
            aTimer.Enabled = false;
            // globally remove timer event
            aTimer.Elapsed -= new ElapsedEventHandler(sendCLKSignal);
        }

    }

    public void PostExecution()
    {
    }

    public void Pause()
    {
    }

    public void Stop()
    {
        aTimer.Enabled = false;
        // globally remove timer event
        aTimer.Elapsed -= new ElapsedEventHandler(sendCLKSignal);
    }

    public void Initialize()
    {
      output = settings.SetClockToTrue;
      if (output) StatusChanged((int)CLKImage.True);
      else StatusChanged((int)CLKImage.False);
      settings.CLKTimeout = timeout;
    }

    public void Dispose()
    {
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
        EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
    }

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    private void StatusChanged(int imageIndex)
    {
        EventsHelper.StatusChanged(OnPluginStatusChanged, this, new StatusEventArgs(StatusChangedMode.ImageUpdate, imageIndex));
    }

    #endregion
  }

  enum CLKImage
  {
      Default,
      True,
      False
  }
}
