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

namespace Primes.WpfControls.NumberTheory.NumberTheoryFunctions
{
  public class Rho:BaseNTFunction
  {
    public Rho() : base()
    {
    }

    protected override void DoExecute()
    {
      FireOnStart();

      for (PrimesBigInteger from = m_From; from.CompareTo(m_To) <= 0; from = from + 1)
      {
          PrimesBigInteger sum = 0;

          for (PrimesBigInteger d = 1; d * 2 <= from; d = d + 1)
          {
              if (from.Mod(d).Equals(PrimesBigInteger.Zero))
              {
                  sum = sum + d;
                  FireOnMessage(this, from, sum.ToString());
              }
          }

          sum = sum + from;
          FireOnMessage(this, from, sum.ToString());
      }

      FireOnStop();
    }

    public override string Description
    {
      get
      {
        return m_ResourceManager.GetString(BaseNTFunction.rho);
      }
    }
  }
}
