using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase;
using LibGmpWrapper;
using Cryptool.PluginBase.IO;

namespace PrimeTest
{
  [PluginInfo(false, "Primetest", "Primetest", null, "PrimeTest/icon.png")] 
 
  public class PrimeTest:IThroughput
  {
    #region IPlugin Members

    public event Cryptool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;
    private void FireOnPluginStatusChangedEvent()
    {
      if (OnPluginStatusChanged != null) OnPluginStatusChanged(this, new StatusEventArgs(0));
    }

    public event Cryptool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
    private void FireOnGuiLogNotificationOccuredEvent(string message, NotificationLevel lvl)
    {
      if (OnGuiLogNotificationOccured != null) OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, lvl));
    }
    private void FireOnGuiLogNotificationOccuredEventError(string message)
    {
      FireOnGuiLogNotificationOccuredEvent(message, NotificationLevel.Error);
    }

    public event Cryptool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;
    private void FireOnPluginProgressChangedEvent(string message, NotificationLevel lvl)
    {
      if (OnPluginProgressChanged != null) OnPluginProgressChanged(this, new PluginProgressEventArgs(0, 0));
    }


    public Cryptool.PluginBase.ISettings Settings
    {
      get { return new PrimeTestSettings(); }
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
      if (m_Value != null)
        Output = m_Value.IsProbablePrime(10);
      else
        Output = false;
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

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    private void FirePropertyChangeEvent(string propertyName)
    {
      if (PropertyChanged != null) PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Properties
    private string m_InputString;
    GmpBigInteger m_Value = null;
    [PropertyInfo(Direction.InputData, "Text input", "Input a string that represent a natural number", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text,null)]
    public string InputString
    {
      get { return this.m_InputString; }
      set
      {
        if (value != m_InputString)
        {
          try
          {
            if (!string.IsNullOrEmpty(value))
            {
              this.m_InputString = value;
              m_Value = new GmpBigInteger(m_InputString.Trim());
              FirePropertyChangeEvent("InputString");
            }
            else 
            {
              throw new Exception();
            }

          }
          catch 
          {
            FireOnGuiLogNotificationOccuredEventError("Damn");
          }
        }
      }
    }

    private bool m_Output;
    // [QuickWatch(QuickWatchFormat.Text, DisplayLevel.Beginner, null)]
    [PropertyInfo(Direction.OutputData, "Boolean output", "True if input is a prime number, otherwise false", "", false, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public bool Output
    {
      get { return this.m_Output; }
      set
      {
        m_Output = value;
        FirePropertyChangeEvent("Output");
      }
    }
    #endregion
  }
}
