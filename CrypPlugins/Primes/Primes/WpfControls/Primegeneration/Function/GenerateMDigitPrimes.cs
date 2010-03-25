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
using Primes.Library;

namespace Primes.WpfControls.Primegeneration.Function
{
  public class GenerateMDigitPrimes : Primes.WpfControls.Components.IExpression
  {
    public static readonly string LEN = "Lenght";
    private IList<PrimesBigInteger> m_GeneratedPrimes;
    private PrimesBigInteger m_LastPrime;
    private PrimesBigInteger m_Length;
    public event VoidDelegate NonFurtherPrimeFound;
    public GenerateMDigitPrimes()
    {
      m_GeneratedPrimes = new List<PrimesBigInteger>();
    }
    #region IFunction Members

    public PrimesBigInteger Execute(PrimesBigInteger input)
    {
      m_LastPrime = GetStartBigInteger();
      int counter = 0;
      while (m_GeneratedPrimes.Contains(m_LastPrime))
      {
        m_LastPrime = m_LastPrime.NextProbablePrime();
        while (m_LastPrime.ToString().Length != this.m_Length.IntValue+1)
        {
          
          m_LastPrime = GetStartBigInteger();
          if (counter == 1000)
          {
            bool found = false;
            char[] sn = new char[this.m_Length.IntValue + 1];
            sn[0] = '1';
            for (int i = 1; i < this.m_Length.IntValue + 1; i++)
            {
              sn[i] = '0';
            }
            PrimesBigInteger x = new PrimesBigInteger(new string(sn)).NextProbablePrime();
            for (int i = 0; i < 100000; i++)
            {
              if (!m_GeneratedPrimes.Contains(x))
              {
                m_LastPrime = x;
                found = m_LastPrime.ToString().Length == this.m_Length.IntValue + 1;
                counter = 0;
                break;
              }
              x = x.NextProbablePrime();
            }
            if (NonFurtherPrimeFound != null&&!found) NonFurtherPrimeFound();
          }
          counter++;

        }
      }
      m_GeneratedPrimes.Add(m_LastPrime);
      return m_LastPrime;
    }
    private PrimesBigInteger GetStartBigInteger()
    {
      PrimesBigInteger _len = m_Length.Add(PrimesBigInteger.One);
      PrimesBigInteger result = PrimesBigInteger.Random(_len).NextProbablePrime();
      while (result.ToString().Length != this.m_Length.IntValue+1)
        result = PrimesBigInteger.Random(_len).NextProbablePrime();
      return result;
    }
    public void SetParameter(string name, PrimesBigInteger value)
    {
      if (name.Equals(LEN))
      {
        m_Length = value;
      }
      else
      {
        throw new ArgumentException("Invalid Name");
      }
    }

    public void Reset()
    {
      m_LastPrime = null;
    }

    #endregion
  }
}
