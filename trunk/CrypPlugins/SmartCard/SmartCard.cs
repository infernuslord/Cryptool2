using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using Cryptool.PluginBase.Cryptography;

namespace SmartCard
{
  [Author("Malte Gronau", null, "", "")]
  [PluginInfo(false, "SmartCard", "SmartCard operations.", "", "SmartCard/Images/SmartCard.png")]
  public class SmartCard : IThroughput
  {
    # region private variables
    private SmartCardSettings settings = new SmartCardSettings();
    private byte[] dataInput;
    private byte[] response = null;
    private byte[] statusWord = { 0x90, 0x00 };
    # endregion

    #region events
    public event StatusChangedEventHandler OnPluginStatusChanged;

    private void PluginStatusChanged(int imageNumber)
    {
      if (OnPluginStatusChanged != null)
      {
        OnPluginStatusChanged(this, new StatusEventArgs(imageNumber));
      }
    }

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
    }

    public event PluginProgressChangedEventHandler OnPluginProgressChanged;
    # endregion events

    # region constructor
    public SmartCard()
    {
      settings.OnGuiLogNotificationOccured += settings_OnGuiLogNotificationOccured;
    }

    void settings_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
    {
      GuiLogMessage(args.Message, args.NotificationLevel);
    }
    # endregion

    #region IO

    [PropertyInfo(Direction.Input, "Data Input", "The input of the card reader.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
    public byte[] DataInput
    {
      get { return dataInput; }
      set
      {
        dataInput = value;
        OnPropertyChanged("DataInput");
      }
    }

    [PropertyInfo(Direction.Output, "Response", "The response of the card reader.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
    public byte[] Response
    {
      get { return response; }
      set { OnPropertyChanged("Response"); }
    }

    [PropertyInfo(Direction.Output, "StatusWord", "The response SW of the card reader.", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Hex, null)]
    public byte[] StatusWord
    {
      get { return statusWord; }
      set { OnPropertyChanged("StatusWord"); }
    }
    #endregion

    # region IPlugin-Methods
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
      this.Response = null;
      this.StatusWord = null;
      GuiLogMessage("Executed.", NotificationLevel.Info);
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

    #endregion IPlugin-Methods

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
