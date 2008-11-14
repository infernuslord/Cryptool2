using System;
using System.Collections.Generic;
using System.Text;

using LibGmpWrapper;

namespace Primes.WpfControls.Primetest.SieveOfEratosthenes
{
  public enum StepResult { SUCCESS, FAILED, END }
  public class Step
  {

    private GmpBigInteger m_Current;

    public GmpBigInteger Current
    {
      get { return m_Current; }
    }
    private GmpBigInteger m_Expected;

    public GmpBigInteger Expected
    {
      get { return m_Expected; }
    }

    private GmpBigInteger m_MaxValue;

    private Numbergrid.Numbergrid m_Numbergrid;

    public Step(Numbergrid.Numbergrid numbergrid, GmpBigInteger maxValue)
    {
      m_Expected = m_Current = GmpBigInteger.Two;
      m_Numbergrid = numbergrid;
      m_MaxValue = maxValue;
    }
    public StepResult DoStep(GmpBigInteger value)
    {

      if (m_Expected.CompareTo(value) == 0)
      {
        m_Numbergrid.RemoveMulipleOf(value);
        m_Expected = m_Expected.NextProbablePrime();
        m_Current = value;
        if (m_Current.Pow(2).CompareTo(m_MaxValue) >= 0)
          return StepResult.END;
        return StepResult.SUCCESS;
      }
      else
      {
        return StepResult.FAILED;
      }
    }
    public void Reset()
    {
      m_Expected = m_Current = GmpBigInteger.Two;
    }
  }
}
