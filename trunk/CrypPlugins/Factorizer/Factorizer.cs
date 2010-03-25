/*
   Copyright 2008 Timo Eckhardt, University of Siegen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase.Tool;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Primes.Bignum;

namespace Factorizer
{
  [Author("Timo Eckhardt", "T-Eckhardt@gmx.de", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo(false, "Factorizer", "Factorizer", null, "Factorizer/icon.png")]
  public class Factorizer : IThroughput
  {
    public Factorizer()
    {
      m_Settings = new FactorizerSettings();
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

    private FactorizerSettings m_Settings;
    public Cryptool.PluginBase.ISettings Settings
    {
      get { return m_Settings; }
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
      if (m_Settings.HasErrors)
      {
        foreach (string message in m_Settings.Errors)
        {
          FireOnGuiLogNotificationOccuredEventError(message);
        }
      }
      else
      {
        if (m_Input != null)
        {
          if (m_Input.IsProbablePrime(10))
          {
            Factor = m_Input.ToString();
            Remainder = PrimesBigInteger.One.ToString();
          }
          else
          {
            PrimesBigInteger i = PrimesBigInteger.Two;
            while (i.Multiply(i).CompareTo(m_Input) <= 0)
            {
              if (m_Input.Mod(i).CompareTo(PrimesBigInteger.Zero) == 0)
              {
                Factor = i.ToString();
                Remainder = m_Input.Divide(i).ToString();
                return;
              }
              i = i.NextProbablePrime();
            }
          }
        }
        else
        {
          FireOnGuiLogNotificationOccuredEventError("No input given");
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

    #region Properties
    private PrimesBigInteger m_Input = null;
    private string m_InputString;
    [PropertyInfo(Direction.InputData, "String input", "A string that represents a natural number", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public string InputString
    {
      get { return m_InputString; }
      set {
        if (!string.IsNullOrEmpty(value))
        {
          m_InputString = value;
          try
          {
            m_Input = new PrimesBigInteger(m_InputString);
            if (m_Input.CompareTo(PrimesBigInteger.Zero) <= 0)
            {
              m_Input = null;
              throw new Exception();
            }

          }
          catch 
          {
            FireOnGuiLogNotificationOccuredEventError("Input has to be a natural number.");
          }
          FirePropertyChangedEvent("InputString");
        }
        else
        {
          FireOnGuiLogNotificationOccuredEventError("Input has to be a natural number.");
          m_Input = null;
        }
      }
    }
    private string m_Factor;

    [PropertyInfo(Direction.OutputData, "A prime factor", "A string that represents a factor that is a prime. ", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public string Factor
    {
      get { return m_Factor; }
      set {
        if (!string.IsNullOrEmpty(value))
        {
          m_Factor = value;
          FirePropertyChangedEvent("Factor");
        }
      }
    }
    private string m_Remainder;

    [PropertyInfo(Direction.OutputData, "Remainder", "Remainder", "", true, false, DisplayLevel.Beginner, QuickWatchFormat.Text, null)]
    public string Remainder
    {
      get { return m_Remainder; }
      set {
        if (!string.IsNullOrEmpty(value))
        {
          m_Remainder = value;
          FirePropertyChangedEvent("Remainder");
        }
      }
    }
    #endregion
  }
}
