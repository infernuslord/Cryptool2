using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
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
      //m_Settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Settings_PropertyChanged);
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
    [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true)]
    public BigInteger OutputString
    {
      get { return this.m_OutputString; }
      set
      {
        m_OutputString = value;
        FirePropertyChangedEvent("OutputString");
      }
    }

    [PropertyInfo(Direction.InputData, "nCaption", "nTooltip")]
    public BigInteger n
    {
        get;
        set;
    }

    private PrimesGeneratorSettings m_Settings = new PrimesGeneratorSettings();
    public ISettings Settings
    {
      get { return m_Settings; }
    }

    bool checkParameters()
    {
        try
        {
            if (n == 0) n = BigInteger.Parse(m_Settings.Input);

            switch ( m_Settings.Mode )
            {
                case 0:
                case 1:
                    if (!(n > 1 && n <= 1024))
                    {
                        FireOnGuiLogNotificationOccuredEventError("Value for n has to be greater than 1 and less than or equal to 1024.");
                        return false;
                    }
                    break;
                case 2:
                    if (n <= 1)
                    {
                        FireOnGuiLogNotificationOccuredEventError("Value for n has to be greater than 1");
                        return false;
                    }
                    break;
            }
        }
        catch
        {
            FireOnGuiLogNotificationOccuredEventError("Please enter an Integer value for n.");
            return false;
        }

        return true;
    }

    public System.Windows.Controls.UserControl Presentation
    {
      get { return null; }
    }

    public void PreExecution()
    {
        n = 0;
    }

    public void Execute()
    {
        if (!checkParameters()) return;

        ProgressChanged(0, 100);

        switch (m_Settings.Mode)
        {
            case 0:   // create prime with m_Input bits
                OutputString = BigIntegerHelper.RandomPrimeBits((int)n);
                break;
            case 1:   // create prime with m_Input bits, MSB set
                OutputString = BigIntegerHelper.RandomPrimeMSBSet((int)n);
                break;
            case 2:   // create prime <= m_Input
                OutputString = BigIntegerHelper.RandomPrimeLimit( n + 1 );
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
