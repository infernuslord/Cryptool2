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
using Cryptool.PluginBase.Miscellaneous;
using System.Numerics;

namespace Factorizer
{
  [Author("Timo Eckhardt", "T-Eckhardt@gmx.de", "Uni Siegen", "http://www.uni-siegen.de")]
  [PluginInfo("Factorizer.Properties.Resources", "PluginCaption", "PluginTooltip", "Factorizer/DetailedDescription/doc.xml", "Factorizer/icon.png")]
  [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class Factorizer : ICrypComponent
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

    private void ProgressChanged(double value, double max)
    {
        EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
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

      public void PreExecution()
    {
    }

    public void Execute()
    {
        if (m_Settings.HasErrors)
        {
            foreach (string message in m_Settings.Errors)
                FireOnGuiLogNotificationOccuredEventError(message);

            return;
        }
     
        if (m_Input == 0)
        {
            FireOnGuiLogNotificationOccuredEventError("Input is zero");
            return;
        }

        if (m_Input.IsProbablePrime())
        {
            // Input = Factor
            // No remainder
            Factor = m_InputString;
            return;
        }

        BigInteger limit = BigIntegerHelper.Min(m_Settings.BruteForceLimit, m_Input.Sqrt());
        int progressdisplay = 0;

        for (BigInteger factor = 2; factor <= limit; factor = (factor + 1).NextProbablePrime())
        {
            if (++progressdisplay == 100)
            {
                progressdisplay = 0;
                ProgressChanged((int)((factor * 100) / limit), 100);
            }
            if (m_Input % factor == 0)
            {
                // Factor found, exit gracefully
                Factor = factor.ToString();
                Remainder = (m_Input / factor).ToString();
                return;
            }
        }

        FireOnGuiLogNotificationOccuredEvent(string.Format("Brute force limit of {0} reached, no factors found", limit), NotificationLevel.Warning);
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

    #region Properties
    private BigInteger m_Input;
    private string m_InputString;
    [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
    public string InputString
    {
      get { return m_InputString; }
      set {
        if (!string.IsNullOrEmpty(value))
        {
          m_InputString = value;
          try
          {
            m_Input = BigIntegerHelper.ParseExpression(m_InputString);
            if (m_Input <= 0 )
            {
              m_Input = 0;
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
          m_Input = 0;
        }
      }
    }
    private string m_Factor;

    [PropertyInfo(Direction.OutputData, "FactorCaption", "FactorTooltip", true)]
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

    [PropertyInfo(Direction.OutputData, "RemainderCaption", "RemainderTooltip", true)]
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
