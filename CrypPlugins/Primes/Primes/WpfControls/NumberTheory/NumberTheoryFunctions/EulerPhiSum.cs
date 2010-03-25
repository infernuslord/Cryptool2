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
using Primes.Library;
using System.Windows;

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
  public class EulerPhiSum:BaseNTFunction
  {
    public EulerPhiSum()
      : base()
    {
    }

    protected override void DoExecute()
    {
      FireOnStart();
      PrimesBigInteger from = m_From;
      while (from.CompareTo(m_To) <= 0)
      {
        PrimesBigInteger result = PrimesBigInteger.Zero;
        PrimesBigInteger k = PrimesBigInteger.One;
        while (k.CompareTo(from) <= 0)
        {
          if (from.Mod(k).Equals(PrimesBigInteger.Zero))
          {
            PrimesBigInteger phik = EulerPhi(k);
            result = result.Add(phik);
            FireOnMessage(this, from, result.ToString());
          }
          k = k.Add(PrimesBigInteger.One);
        }
        from = from.Add(PrimesBigInteger.One);
      }
      FireOnStop();

    }

    private PrimesBigInteger EulerPhi(PrimesBigInteger n)
    {
      if (n.Equals(PrimesBigInteger.One)) return PrimesBigInteger.One;
      PrimesBigInteger result = PrimesBigInteger.Zero;
      PrimesBigInteger k = PrimesBigInteger.One;
      while (k.CompareTo(n) <= 0)
      {
        if (PrimesBigInteger.GCD(k, n).Equals(PrimesBigInteger.One))
        {
          result = result.Add(PrimesBigInteger.One);
        }
        k = k.Add(PrimesBigInteger.One);
      }
      return result;
    }
    public override string Description
    {
      get
      {
        return m_ResourceManager.GetString(BaseNTFunction.eulerphisum);
      }
    }

  }
  
}
