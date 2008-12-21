using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Collections.ObjectModel;

namespace SmartCard
{
  public class SmartCardSettings : ISettings
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


    private int cardReader = 0; // 0=default reader

    public SmartCardSettings()
    {
      collection.Add("Virtual Cardreader");
      collection.Add("Cardreader1");
      Collection = collection;
    }

    [TaskPane("SmartCard reader", "Select your reader.", "", 0, false, DisplayLevel.Beginner, ControlType.DynamicComboBox, new string[] { "Collection" })]
    public int CardReader
    {
      get { return this.cardReader; }
      set
      {
        if (((int)value) != cardReader) hasChanges = true;
        this.cardReader = (int)value;
        OnPropertyChanged("CardReader");
      }
    }

    [TaskPane("Search Card Readers", "Search for readers connected to system.", "", 1, false, DisplayLevel.Beginner, ControlType.Button)]
    public void SearchCardReaders()
    {
      GuiLogMessage("found N new readers", NotificationLevel.Info);
      collection.Clear();
      collection.Add("Cardreader2");
      Collection = collection;
      CardReader = 0;
    }

    public ObservableCollection<string> Collection
    {
      get { return collection; }
      set 
      {
        if (value != collection)
        {
          collection = value;
        }
        OnPropertyChanged("Collection");
      }
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
