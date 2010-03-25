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
using System.Windows;
using Primes.Bignum;

namespace Primes.WpfControls.Factorization.QS
{
  public class QSData:List<QuadraticPair>, IList<QuadraticPair>
  {

    public QSData()
    {
      m_IgnoreQuadrats = new List<PrimesBigInteger>();
    }
    #region Properties

    public IList<int> CalculateFactorBase()
    {
      List<int> result = new List<int>();
      PrimesBigInteger b = PrimesBigInteger.ValueOf(m_B);
      PrimesBigInteger c = PrimesBigInteger.One;
      while (c.CompareTo(b) <= 0)
      {
        c = c.NextProbablePrime();
        result.Add(c.IntValue);

      }
      return result;
    }
    public IList<QuadraticPair> BSmooth
    {
      get
      {
        List<QuadraticPair> result = new List<QuadraticPair>();
        foreach (QuadraticPair pair in this)
        {
          if (pair.IsBSmooth) result.Add(pair);
        }
        return result;
      }
    }

    private long m_B;

    public long B
    {
      get { return m_B; }
    }
    private long m_N;

    public long N
    {
      get { return m_N; }
      set { 
        m_N = value;
        double lnn = Math.Log(m_N);
        double lnlnn = Math.Log(lnn);
        double exp = Math.Sqrt(lnn * lnlnn);
        double eexp = Math.Exp(exp);
        m_B = (long)Math.Ceiling(Math.Sqrt(eexp));

        m_L = Math.Exp(Math.Sqrt(lnlnn * lnn));
      }
    }

    private double m_L;

    public short From
    {
      get { return (short)((m_L*(-1))+Math.Sqrt(m_N)); }
      //set { m_From = value; }
    }

    public short To
    {
      get { return (short)((m_L) + Math.Sqrt(m_N)); }
      //set { m_To = value; }
    }

    #endregion
    private IList<PrimesBigInteger> m_IgnoreQuadrats;

    public void AddIgnoreQuadrat(PrimesBigInteger value)
    {
      m_IgnoreQuadrats.Add(value);
    }

    public bool IsIgnored(PrimesBigInteger value)
    {
      return m_IgnoreQuadrats.Contains(value);
    }
    public void ClearAll()
    {
      base.Clear();
      N = 0;
    }

    public void Reset()
    {
      this.Clear();
    }
  }

  public class QuadraticPair
  {
    public QuadraticPair()
    {
      m_Exponents = new Dictionary<int, int>();
    }

    public QuadraticPair(long a, long b)
      : this()
    {
      this.A = a;
      this.B = b;
    }
    #region Properties
    private long m_A;

    public long A
    {
      get { return m_A; }
      set { m_A = value; }
    }
    private long m_B;

    public long B
    {
      get { return m_B; }
      set { m_B = Math.Abs(value); }
    }

    private bool m_IsBSmooth;

    public bool IsBSmooth
    {
      get { return m_IsBSmooth; }
      set { m_IsBSmooth = value; }
    }
    private QuadraticStatus m_QuadraticStatus;

    public QuadraticStatus QuadraticStatus
    {
      get { return m_QuadraticStatus; }
      set { m_QuadraticStatus = value; }
    }
    
    #endregion 

    private IDictionary<int, int> m_Exponents;
    public void AddExponent(int factor, int exp)
    {
      if (!m_Exponents.ContainsKey(factor)) m_Exponents.Add(factor, exp);
      else m_Exponents[factor] = exp;
    }
    internal void Reset()
    {
      this.m_Exponents.Clear();
      this.IsBSmooth = false;
    }
    
  }

  public enum QuadraticStatus { Nope, Part, Quadratic, Ignore  }
}
