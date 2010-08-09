using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Primes.Bignum;
using System.Numerics;

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
    private BigInteger m_InputNumber;
    PrimesBigInteger m_Value = null;
    [PropertyInfo(Direction.InputData, "Text input", "Input a BigInteger", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text,null)]
    public BigInteger InputNumber
    {
      get { return this.m_InputNumber; }
      set
      {
        if (value != m_InputNumber)
        {
          try
          {
            this.m_InputNumber = value;
            m_Value = new PrimesBigInteger(m_InputNumber.ToString());
            FirePropertyChangeEvent("InputString");
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
