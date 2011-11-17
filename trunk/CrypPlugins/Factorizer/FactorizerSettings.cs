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
using Cryptool.PluginBase;

namespace Factorizer
{
  public class FactorizerSettings:ISettings
  {

    private const int BRUTEFORCEMIN = 100;
    private const int BRUTEFORCEMAX = 10000000;
    private const string ERROR_BFL = "ERROR_BFL";

    public FactorizerSettings()
    {
      m_Errors = new Dictionary<string, string>();
      BruteForceLimit = 100000;
    }

    #region Settings

    private IDictionary<string, string> m_Errors;
    public bool HasErrors
    {
      get 
      {
        return m_BruteForceLimit < 0;
      }
    }
    private long m_BruteForceLimit;

    [TaskPane("BruteForceLimitCaption", "BruteForceLimitTooltip", "BruteForceLimitGroup", 0, false, ControlType.TextBox, ValidationType.RangeInteger, 100, 1000000)]
    public long BruteForceLimit
    {
      get { return m_BruteForceLimit; }
      set {
        if (value >= BRUTEFORCEMIN && value <= BRUTEFORCEMAX)
        {
          m_BruteForceLimit = value;
          AddError(ERROR_BFL, null);
          FirePropertyChangedEvent("BruteForceLimit");
        }
        else
        {
          AddError(ERROR_BFL, "BruteForceLimit has to be greater or equal than "+BRUTEFORCEMIN+" and less or equal than "+BRUTEFORCEMAX+".");
          m_BruteForceLimit = -1;
        }
        //HasChanges = true;
      }
    }


    private void AddError(string key, string message)
    {
      if (string.IsNullOrEmpty(message))
      {
        if (m_Errors.ContainsKey(key))
          m_Errors.Remove(key);
      }
      else
      {
        if (!m_Errors.ContainsKey(key))
          m_Errors.Add(key, message);
        else
          m_Errors[key] = message;
      }
    }

    public ICollection<string> Errors
    {
      get
      {
        return m_Errors.Values;
      }

    }
    #endregion
    #region ISettings Members

    private bool m_HasChanges;

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
