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
using System.Text;
using Primes.Bignum;
using Primes.WpfControls.Components;
using System.Windows.Controls;
using System.Diagnostics;
using Primes.Library;
using System.Windows;

namespace Primes.WpfControls.PrimesDistribution.Numberline
{
  public class Rho:BaseNTFunction
  {
    private int m_Row;
    private StringBuilder m_SbSum;
    public Rho(LogControl lc, TextBlock tb)
      : base(lc, tb)
    {
      m_Log.Columns = 1;
      m_Log.OverrideText = true;

    }

    protected override void DoExecute()
    {
      FireOnStart();
      ControlHandler.SetPropertyValue(m_tbCalcInfo, "Visibility", Visibility.Visible);

      m_SbSum = new StringBuilder();
      m_Row = m_Log.NewLine();
      PrimesBigInteger sum = PrimesBigInteger.Zero;
      PrimesBigInteger d = PrimesBigInteger.One;
      while (d.Multiply(PrimesBigInteger.Two).CompareTo(m_Value) <= 0)
      {        
        if (m_Value.Mod(d).Equals(PrimesBigInteger.Zero))
        {
          if (m_SbSum.Length > 0)
          {
            m_SbSum.Append(" + ");
          }
          m_SbSum.Append(d.ToString("D"));

          sum = sum.Add(d);
          m_Log.Info(m_SbSum.ToString(), 0, m_Row);
        }
        d = d.Add(PrimesBigInteger.One);
      }
      m_SbSum.Append(" + ");
      m_SbSum.Append(m_Value.ToString("D"));

      sum = sum.Add(m_Value);
      m_Log.Info(m_SbSum.ToString(), 0, m_Row);
      SetCalcInfo(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_rhoinfo, m_Value.ToString("D"), sum.ToString("D")));

      FireOnStop();

    }

    public override void Stop()
    {
      //Debug.WriteLine("Rho Stop " + m_Value.ToString());
      //if (m_SbSum != null)
      //{
      //  string s = m_SbSum.ToString().Trim();
      //  if (!string.IsNullOrEmpty(s))
      //  {
      //    if (s.EndsWith("+"))
      //    {
      //      s = s.Substring(0, s.Length - 2);
      //      m_Log.Info(s.ToString(), 0, m_Row);
      //    }
      //  }
      //}
      base.Stop();
    }
  }
}
