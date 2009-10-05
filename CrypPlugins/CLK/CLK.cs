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
// for mouse click
using System.Windows.Input;
// for QuickwatchPresentaton
using System.Windows.Threading;


using System.Windows;
// for setting image uri programmatically
using System.Windows.Media.Imaging;
//using System.Windows.Data;
//using System.IO;
//using System.Runtime.CompilerServices;

namespace Cryptool.CLK
{
    [Author("Soeren Rinne", "soeren.rinne@cryptool.de", "Ruhr-Universitaet Bochum, Chair for System Security", "http://www.trust.rub.de/")]
  [PluginInfo(true, "CLK", "Simple clock for clock-based plugins.", null, "CLK/icon.png", "CLK/Images/true.png", "CLK/Images/false.png")]
  public class CLK : DependencyObject, IInput
  {
    # region private variables
    private CLKPresentation cLKPresentation;
    private bool output;
    private bool eventInput;
    private int timeout = 2000;
    private int rounds = 10;
    private Timer aTimer = new Timer();
    # endregion private variables

    public int myRounds;
    public DateTime startTime;

    public CLK()
    {
        settings = new CLKSettings();
        settings.PropertyChanged += settings_PropertyChanged;

        cLKPresentation = new CLKPresentation();
        Presentation = cLKPresentation;

        cLKPresentation.CLKButtonImage.MouseLeftButtonUp += cLKButton_MouseLeftButtonUp;
        cLKPresentation.CLKButtonImage.MouseLeftButtonDown += cLKButton_MouseLeftButtonUp;

        // set picture according to settings value
        /* BRINGT NIX - WARUM?
        if (settings.SetClockToTrue) StatusChanged((int)CLKImage.True);
        else StatusChanged((int)CLKImage.False);*/
    }

    void cLKButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        OnPropertyChanged("Output");
    }

    void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "SetClockToTrue")
      {
        output = settings.SetClockToTrue;
        if (output)
        {
            StatusChanged((int)CLKImage.True);
            cLKPresentation.setImageTrue();
        }
        else
        {
            StatusChanged((int)CLKImage.False);
            cLKPresentation.setImageFalse();
        }
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
       
    [PropertyInfo(Direction.OutputData, "Output", "Output.", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
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

    [PropertyInfo(Direction.InputData, "EventInput", "EventInput", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public bool EventInput
    {
        get { return eventInput; }
        set
        {
            eventInput = value;
            OnPropertyChanged("EventInput");
        }
    }


    # endregion public interface

    #region IPlugin Members
    public event StatusChangedEventHandler OnPluginStatusChanged;
    public event PluginProgressChangedEventHandler OnPluginProgressChanged;
    private void ProgressChanged(double value, double max)
    {
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
    }

    private CLKSettings settings;
    public ISettings Settings
    {
        get { return settings; }
    }

    public UserControl Presentation { get; private set; }

    public System.Windows.Controls.UserControl QuickWatchPresentation
    {
        get { return Presentation; }
    }

    public void PreExecution()
    {
        if (settings.SetClockToTrue)
        {
            StatusChanged((int)CLKImage.True);
            cLKPresentation.setImageTrue();
        }
        else
        {
            StatusChanged((int)CLKImage.False);
            cLKPresentation.setImageFalse();
        }

        myRounds = settings.Rounds;
        //GuiLogMessage("myRounds: " + myRounds.ToString(), NotificationLevel.Info);

        startTime = DateTime.Now;
    }

    public void Execute()
    {
        if (settings.UseEvent)
        {
            if (myRounds != 0)
            {
                OnPropertyChanged("Output");
                myRounds--;
                ProgressChanged(settings.Rounds - myRounds, settings.Rounds);
            }
            else
            {
                // stop counter
                DateTime stopTime = DateTime.Now;
                // compute overall time
                TimeSpan duration = stopTime - startTime;
                GuiLogMessage("Overall time used: " + duration, NotificationLevel.Debug);
            }
        }
        else
        {
            if (settings.CLKTimeout <= 499)
            {
                GuiLogMessage("Are you trying to generate bulk output? Please do not use CLK plugin for this purpose. Try setting the number of rounds in the corresponding plugin settings.", NotificationLevel.Warning);
            }
            else
            {
                process(settings.CLKTimeout);
                //change picture
                if (settings.SetClockToTrue) StatusChanged((int)CLKImage.True);
                else StatusChanged((int)CLKImage.False);
            }
        }
    }

    private void process(int timeout)
    {
        // check if rounds are more than zero
        if (myRounds != 0)
        {
            // first fire up an event, then get the timer to handle that for us
            OnPropertyChanged("Output");
            myRounds--;

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new ElapsedEventHandler(sendCLKSignal);

            // Set the Interval to 'timeout' seconds (in milliseconds).
            aTimer.Interval = timeout;
            aTimer.Enabled = true;

            // Keep the timer alive until the end of Main.
            //GC.KeepAlive(aTimer);
        }
    }

    private void sendCLKSignal(object sender, EventArgs e)
    {
        if (myRounds != 0)
        {
            OnPropertyChanged("Output");
            myRounds--;
            ProgressChanged(settings.Rounds - myRounds, settings.Rounds);
        }
        else
        {
            // disable timer
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
        // disable timer
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
