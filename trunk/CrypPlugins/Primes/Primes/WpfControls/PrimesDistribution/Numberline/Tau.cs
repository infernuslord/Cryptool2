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
using Primes.Bignum;
using Primes.Library;
using System.Windows;

namespace Primes.WpfControls.PrimesDistribution.Numberline
{
  public class Tau:BaseNTFunction
  {
    public Tau(LogControl2 lc, TextBlock tb):base(lc,tb)
    {
    }

    protected override void DoExecute()
    {
      FireOnStart();
      ControlHandler.SetPropertyValue(m_tbCalcInfo, "Visibility", Visibility.Visible);

      PrimesBigInteger d = PrimesBigInteger.One;
      PrimesBigInteger counter = PrimesBigInteger.Zero;
      SetCalcInfo(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_tauinfo, new object[] { counter.ToString("D"), m_Value.ToString("D") }));

      while (d.Multiply(PrimesBigInteger.Two).CompareTo(m_Value) <= 0)
      {
        if (m_Value.Mod(d).Equals(PrimesBigInteger.Zero))
        {
          m_Log.Info(d.ToString("D") + "   ");
          counter = counter.Add(PrimesBigInteger.One);
          SetCalcInfo(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_tauinfo, new object[] { counter.ToString("D"), m_Value.ToString("D") }));

        }
        d = d.Add(PrimesBigInteger.One);
      }
      m_Log.Info(m_Value.ToString("D")+"   ");
      counter = counter.Add(PrimesBigInteger.One);
      SetCalcInfo(string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_tauinfo, new object[] { counter.ToString("D"), m_Value.ToString("D") }));

      FireOnStop();

    }
  }
}
