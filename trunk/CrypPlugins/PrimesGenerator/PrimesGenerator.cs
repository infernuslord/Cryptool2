using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Generator;
using System.Reflection;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using LibGmpWrapper;
using Cryptool.PluginBase.Tool;

namespace Cryptool.PrimesGenerator
{

  [PluginInfo(true, "PrimesGenerator", "PrimesGenerator", null, "PrimesGenerator/icon.png")]
  public class PrimesGenerator : IInput
  {
    private GmpBigInteger m_max = new GmpBigInteger("10000000000");
    public PrimesGenerator()
    {
      m_Settings = new PrimesGeneratorSettings();
      m_Settings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(m_Settings_PropertyChanged);
      m_Mode = 0;
      m_Input = GmpBigInteger.ValueOf(100);
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


    private string m_OutputString;
    [PropertyInfo(Direction.Output, "Text output", "A primenumber", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public string OutputString
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

    private GmpBigInteger m_Input;
    private int m_Mode;
    private bool hasErrors;
    void m_Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      hasErrors = false;
      try
      {
        m_Input = new GmpBigInteger(m_Settings.Input);
        switch (e.PropertyName)
        {
          case PrimesGeneratorSettings.MODE:
          case PrimesGeneratorSettings.INPUT:
            m_Mode = m_Settings.Mode;
            switch (m_Mode)
            {
              case 0:
                if (m_Input.CompareTo(GmpBigInteger.One) < 0 || m_Input.CompareTo(GmpBigInteger.ValueOf(400)) > 0)
                {
                  FireOnGuiLogNotificationOccuredEventError("Value for n has to be greater or equal than one an less or equal than 400.");
                  hasErrors = true;
                }
                break;
              case 1:
                if (m_Input.CompareTo(GmpBigInteger.Ten) < 0 || m_Input.CompareTo(m_max) > 0)
                {
                  FireOnGuiLogNotificationOccuredEventError("Please enter an Integer value for n.");
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

    public System.Windows.Controls.UserControl QuickWatchPresentation
    {
      get { return null; }
    }

    public void PreExecution()
    {
      
    }


    public void Execute()
    {
      if (!hasErrors)
      {
        switch (m_Mode)
        {
          case 0:
            OutputString = LibGmpWrapper.GmpBigInteger.Random(m_Input).NextProbablePrime().ToString();
            break;
          case 1:
            GmpBigInteger result = LibGmpWrapper.GmpBigInteger.RandomM(m_Input).NextProbablePrime();
            while (result.CompareTo(m_Input) > 0)
            {
              result = LibGmpWrapper.GmpBigInteger.RandomM(m_Input).NextProbablePrime();
            }
            OutputString = result.ToString();
            break;
        }
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
