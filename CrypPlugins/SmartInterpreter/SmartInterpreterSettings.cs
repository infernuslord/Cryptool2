using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Collections.ObjectModel;

namespace SmartInterpreter
{
  public class SmartInterpreterSettings : ISettings
  {
    private ObservableCollection<string> collection = new ObservableCollection<string>();

    #region ISettings Members

    private bool hasChanges = false;
    public bool HasChanges
    {
      get { return hasChanges; }
      set { hasChanges = value; }
    }

    #endregion

    public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    private void GuiLogMessage(string message, NotificationLevel logLevel)
    {
      EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, null, new GuiLogEventArgs(message, null, logLevel));
    }


    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
    }

    #endregion
  }
}
