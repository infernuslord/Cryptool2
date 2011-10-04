using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using Primes.Bignum;
using System.Numerics;

namespace PrimeTest
{
  [PluginInfo("PrimeTest.Properties.Resources", false, "PluginCaption", "PluginTooltip", "PrimeTest/DetailedDescription/doc.xml", "PrimeTest/icon.png")]
  [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
  public class PrimeTest : ICrypComponent
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

    private void ProgressChanged(double value, double max)
    {
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
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
        ProgressChanged(0, 100);

        if (m_Value != null)
            Output = m_Value.IsProbablePrime(10);
        else
            Output = false;

        ProgressChanged(100, 100);
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
    [PropertyInfo(Direction.InputData, "InputNumberCaption", "InputNumberTooltip", "", true, false, QuickWatchFormat.Text,null)]
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
    // [QuickWatch(QuickWatchFormat.Text, null)]
    [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip", "", false, false, QuickWatchFormat.Text, null)]
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
