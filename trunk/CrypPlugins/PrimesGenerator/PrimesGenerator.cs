using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;
using System.Security.Cryptography;

namespace Cryptool.PrimesGenerator
{

  [PluginInfo("PrimesGenerator.Properties.Resources", "PluginCaption", "PluginTooltip", "PrimesGenerator/DetailedDescription/doc.xml", "PrimesGenerator/icon.png")]
  [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
  public class PrimesGenerator : ICrypComponent
  {
    public PrimesGenerator()
    {
      m_Settings = new PrimesGeneratorSettings();
      m_Settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Settings_PropertyChanged);
      m_Mode = 0;
      m_Input = 100;
    }


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


    private BigInteger m_OutputString;
    [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true, QuickWatchFormat.Text, null)]
    public BigInteger OutputString
    {
      get { return this.m_OutputString; }
      set
      {
        m_OutputString = value;
        FirePropertyChangedEvent("OutputString");
      }
    }
    private PrimesGeneratorSettings m_Settings = new PrimesGeneratorSettings();
    public ISettings Settings
    {
      get { return m_Settings; }
    }

    private BigInteger m_Input;
    private int m_Mode;
    private bool hasErrors;
    void m_Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      hasErrors = false;
      try
      {
        m_Input = BigInteger.Parse(m_Settings.Input);
        
        switch (e.PropertyName)
        {
          case PrimesGeneratorSettings.MODE:
          case PrimesGeneratorSettings.INPUT:
            m_Mode = m_Settings.Mode;
            switch (m_Mode)
            {
              case 0:
                if ( !(m_Input > 1 && m_Input <= 1024) )
                {
                    FireOnGuiLogNotificationOccuredEventError("Value for n has to be greater than 1 and less than or equal to 1024.");
                    hasErrors = true;
                } 
                break;
              case 1:
                if (m_Input <= 1)
                {
                    FireOnGuiLogNotificationOccuredEventError("Value for n has to be greater than 1");
                    hasErrors = true;
                }
                break;
            }
            break;
          default:
            break;
        }
      }
      catch
      {
        FireOnGuiLogNotificationOccuredEventError("Please enter an Integer value for n.");
        hasErrors = true;
      }
    }

    public System.Windows.Controls.UserControl Presentation
    {
      get { return null; }
    }

      public void PreExecution()
    {
      
    }

    public void Execute()
    {
        if (hasErrors) return; 
        
        ProgressChanged(0, 100);

        switch (m_Mode)
        {
            case 0:   // create prime with m_Input bits
                OutputString = BigIntegerHelper.RandomPrimeBits( (int)m_Input );
                break;
            case 1:   // create prime <= m_Input
                OutputString = BigIntegerHelper.RandomPrimeLimit( m_Input + 1 );
                break;
        }

        ProgressChanged(100, 100);
    }

    public void PostExecution()
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

    private void FirePropertyChangedEvent(string propertyName)
    {
      if (PropertyChanged != null) PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
    #endregion
  }
}
