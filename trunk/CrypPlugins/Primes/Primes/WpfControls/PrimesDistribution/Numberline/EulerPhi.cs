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
using Primes.WpfControls.Components;
using System.Windows.Controls;
using System.Threading;
using Primes.Bignum;
using Primes.Library;
using System.Windows;
using System.Diagnostics;

namespace Primes.WpfControls.PrimesDistribution.Numberline
{
  public class EulerPhi : BaseNTFunction
  {
    object lockobj = new object();
    public EulerPhi(LogControl2 lc, TextBlock tb) : base(lc, tb) { }

    #region Calculating

    protected override void DoExecute()
    {
      FireOnStart();
      ControlHandler.SetPropertyValue(m_tbCalcInfo, "Visibility", Visibility.Visible);
      if (m_Value.IsPrime(20))
      {
        string info = 
          string.Format(
            Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_eulerphiisprime, 
            new object[] { m_Value.ToString("D"), m_Value.ToString("D"), m_Value.ToString("D"), m_Value.Subtract(PrimesBigInteger.One).ToString("D") });
        m_Log.Info(info);
        SetCalcInfo(info);
      }
      else
      {
        SetCalcInfo( Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_calculating );

        PrimesBigInteger d = PrimesBigInteger.One;
        PrimesBigInteger counter = PrimesBigInteger.Zero;
        while (d.CompareTo(m_Value) < 0)
        {
          if (PrimesBigInteger.GCD(d, m_Value).Equals(PrimesBigInteger.One))
          {
            m_Log.Info(d.ToString()+"   ");
            counter = counter.Add(PrimesBigInteger.One);
            //SetCalcInfo(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_eulerphifoundresult, new object[] { counter.ToString("D"), m_Value.ToString("D") }));
          }

          d = d.Add(PrimesBigInteger.One);
        }

        SetCalcInfo(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_eulerphifoundresult, new object[] { counter.ToString("D"), m_Value.ToString("D") }));

      }
      FireOnStop();

    }


    #endregion
  }
}
