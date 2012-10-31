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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Primes.Bignum;
using Primes.WpfControls.Validation.Validator;
using System.Threading;
using Primes.WpfControls.Components;
using Primes.WpfControls.Validation;
using Primes.Library;

namespace Primes.WpfControls.Primetest.MillerRabin
{
  /// <summary>
  /// Interaction logic for MillerRabinControl.xaml
  /// </summary>
  public partial class MillerRabinControl : UserControl, IPrimeTest
  {

    #region Initialization

    public MillerRabinControl()
    {
      InitializeComponent();
      SetInputValidators();
      SetDefaults();
      iscBaseRandom.Execute += new ExecuteSingleDelegate(iscBaseRandom_Execute);
      iscRounds.Execute += new ExecuteSingleDelegate(iscRounds_Execute);
      ircSystematic.Execute += new ExecuteDelegate(ircSystematic_Execute);
      ircSystematic.IntervalDoesntSizeNeedToBeGreateThanZero = true;
    }

    void ircSystematic_Execute(PrimesBigInteger from, PrimesBigInteger to)
    {
      if (ForceGetInteger != null) ForceGetInteger(new ExecuteIntegerDelegate(Execute));
    }

    void iscRounds_Execute(PrimesBigInteger value)
    {
      if (ForceGetInteger != null) ForceGetInteger(new ExecuteIntegerDelegate(Execute));
    }

    void iscBaseRandom_Execute(PrimesBigInteger value)
    {
      if (ForceGetInteger != null) ForceGetInteger(new ExecuteIntegerDelegate(Execute));
    }

    private void SetInputValidators()
    {
      InputValidator<PrimesBigInteger> ivRndRounds = new InputValidator<PrimesBigInteger>();
      ivRndRounds.DefaultValue = "2";
      ivRndRounds.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.One);
      iscRounds.AddInputValidator(InputSingleControl.Free, ivRndRounds);

      InputValidator<PrimesBigInteger> ivRndBase = new InputValidator<PrimesBigInteger>();
      ivRndBase.DefaultValue = "3";
      ivRndBase.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Three);
      iscBaseRandom.AddInputValidator(InputSingleControl.Free, ivRndBase);

      InputValidator<PrimesBigInteger> ivSysBaseFrom = new InputValidator<PrimesBigInteger>();
      ivSysBaseFrom.DefaultValue = "2";
      ivSysBaseFrom.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Two);
      InputValidator<PrimesBigInteger> ivSysBaseTo = new InputValidator<PrimesBigInteger>();
      ivSysBaseTo.DefaultValue = "3";
      ivSysBaseTo.Validator = new BigIntegerMinValueValidator(null, PrimesBigInteger.Three);
      ircSystematic.AddInputValidator(InputRangeControl.FreeFrom, ivSysBaseFrom);
      ircSystematic.AddInputValidator(InputRangeControl.FreeTo, ivSysBaseTo);
    }

    private void SetDefaults()
    {
      iscBaseRandom.FreeText = "100";
      iscRounds.FreeText = "10";
      ircSystematic.SetText(InputRangeControl.FreeFrom, "2");
      ircSystematic.SetText(InputRangeControl.FreeTo, "100");
    }
    
    #endregion

    #region Properties
    private PrimesBigInteger m_Value;
    private PrimesBigInteger m_Rounds;
    private PrimesBigInteger m_RandomBaseTo;
    private PrimesBigInteger m_SystematicBaseFrom;
    private PrimesBigInteger m_SystematicBaseTo;

    private Thread m_TestThread;
    #endregion

    //private bool MillerRabin(PrimesBigInteger rounds, PrimesBigInteger n)
    //{
    //  PrimesBigInteger i = PrimesBigInteger.One;

    //  while (i.CompareTo(rounds) <= 0)
    //  {
    //    if (Witness(PrimesBigInteger.RandomM(n.Subtract(PrimesBigInteger.Two)).Add(PrimesBigInteger.Two), n))
    //      return false;
    //    i = i.Add(PrimesBigInteger.One);
    //  }
    //  return true;
    //}

    private bool Witness(PrimesBigInteger a)
    {
        // a mustn't be a multiple of n
        if (a.Mod(m_Value).CompareTo(PrimesBigInteger.Zero) == 0)
            return false;

      PrimesBigInteger n_1 = m_Value.Subtract(PrimesBigInteger.One);
      log.Info("n-1 = " + n_1.ToString());

      PrimesBigInteger u = m_Value.Subtract(PrimesBigInteger.One);

      int shl = 0;
      while (u.Mod(PrimesBigInteger.Two).CompareTo(PrimesBigInteger.Zero) == 0)
      {
        shl++;
        log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.mr_shiftright, new object[] { u.ToString(), u.ShiftRight(1).ToString() }));
        u = u.ShiftRight(1);
      }
      log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.mr_shiftedright, shl.ToString()));
      PrimesBigInteger former = a.ModPow(u, m_Value);
      log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.mr_calculating1, new object[] { a.ToString(), u.ToString(), m_Value.ToString(),former.ToString() }));
      PrimesBigInteger tmpsqrt = null;
      for (int i = 0; i < shl; i++)
      {
        PrimesBigInteger tmp = former.ModPow(PrimesBigInteger.Two, m_Value);
        log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.mr_calculating2, new object[] { former.ToString(), m_Value.ToString(), tmp.ToString() }));
        if (tmp.CompareTo(PrimesBigInteger.One) == 0 && !former.Equals(PrimesBigInteger.One)&& !former.Equals(n_1))
        {
          log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.mr_isnotprime1, new object[] { tmp.ToString(), former.ToString(), m_Value.ToString(), m_Value.ToString() }));
          return true;
        }
        tmpsqrt = former;
        former = tmp;
      }
      bool result = !former.Equals(PrimesBigInteger.One) ;
      if (result)
      {
        log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.mr_isnotprime2, new object[] { m_Value.ToString(), a.ToString(), n_1.ToString(), m_Value.ToString() }));
      }
      return result;
    }

    #region IPrimeTest Members

    public void Execute(PrimesBigInteger value)
    {
      CancelTestThread();
      log.Clear();
      m_Value = value;
      if (value.Mod(PrimesBigInteger.Two).Equals(PrimesBigInteger.Zero))
      {
        log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.mr_iseven, value.ToString("D")));
        FireEventCancelTest();
      }
      else
      {
        switch (KindOfTest)
        {
          case KOD.Random:
            ExecuteRandom();
            break;
          case KOD.Systematic:
            ExecuteSystematic();
            break;
        }
      }
    }

    private void ExecuteRandom()
    {
      m_Rounds = iscRounds.GetValue();
      m_RandomBaseTo = iscBaseRandom.GetValue();

      if (m_Rounds != null && m_RandomBaseTo != null)
      {
        m_TestThread = new Thread(new ThreadStart(new VoidDelegate(ExecuteRandomThread)));
        m_TestThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
        m_TestThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
        m_TestThread.Start();
      }
      else
      {
        FireEventCancelTest();
      }
    }

    private void ExecuteRandomThread()
    {
      FireEventExecuteTest();
      Random rnd = new Random(DateTime.Today.Millisecond);
      PrimesBigInteger i = PrimesBigInteger.One;
      while (i.CompareTo(m_Rounds) <= 0)
      {
        PrimesBigInteger k = PrimesBigInteger.RandomM(m_RandomBaseTo);
        k = k.Mod(m_RandomBaseTo.Add(PrimesBigInteger.One));
        k = PrimesBigInteger.Max(k, PrimesBigInteger.Two);
        if (ExecuteWitness(i, k)) break;
        i = i.Add(PrimesBigInteger.One);
      }
      FireEventCancelTest();
    }

    private bool ExecuteWitness(PrimesBigInteger round, PrimesBigInteger a)
    {
      log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.mr_round, new object[] { round.ToString(), a.ToString() }));
      bool result = Witness(a);
      if(!result)
        log.Info(string.Format(Primes.Resources.lang.WpfControls.Primetest.Primetest.mr_isprime, new object[] { round.ToString(), a.ToString(), m_Value.ToString("D") }));
      log.Info("");
      return result;
    }

    private void ExecuteSystematic()
    {
      if (ircSystematic.GetValue(ref m_SystematicBaseFrom, ref m_SystematicBaseTo))
      {
        m_TestThread = new Thread(new ThreadStart(new VoidDelegate(ExecuteSystematicThread)));
        m_TestThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
        m_TestThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
        m_TestThread.Start();
      }
      else
      {
        FireEventCancelTest();
      }
    }

    private void ExecuteSystematicThread()
    {
      FireEventExecuteTest();
      PrimesBigInteger i = PrimesBigInteger.One;
      while (m_SystematicBaseFrom.CompareTo(m_SystematicBaseTo) <= 0)
      {
        if (ExecuteWitness(i, m_SystematicBaseFrom)) break;
        m_SystematicBaseFrom = m_SystematicBaseFrom.Add(PrimesBigInteger.One);
        i = i.Add(PrimesBigInteger.One);
      }
      FireEventCancelTest();
    }

    public void CancelExecute()
    {
      CancelTestThread();
      FireEventCancelTest();
    }

    private void CancelTestThread()
    {
      if (m_TestThread != null)
      {
        m_TestThread.Abort();
        m_TestThread = null;
      }
    }

    public bool IsRunning()
    {
        return (m_TestThread != null) && m_TestThread.IsAlive;
    }

    public event VoidDelegate Start;

    public event VoidDelegate Stop;

    private void FireEventExecuteTest()
    {
      if (Start != null) Start();
    }
    private void FireEventCancelTest()
    {
      if (Stop != null) Stop();
    }

    #endregion

    #region IPrimeTest Members


    public Primes.WpfControls.Validation.IValidator<PrimesBigInteger> Validator
    {
      get { return new BigIntegerMinValueValidator(null, PrimesBigInteger.Three); }
    }

    #endregion
    private void rbClick(object sender, RoutedEventArgs e)
    {
      pnlRandom.Visibility = (rbRandom.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
      pnlSystematic.Visibility = (rbSystematic.IsChecked.Value) ? Visibility.Visible : Visibility.Collapsed;
    }
    private KOD KindOfTest
    {
      get
      {
        if (rbRandom.IsChecked.Value) return KOD.Random;
        else return KOD.Systematic;
      }
    }
    private enum KOD { Random, Systematic}

    #region IPrimeTest Members



    #endregion

    #region IPrimeVisualization Members


    public event VoidDelegate Cancel;
    private void FireCancelEvent()
    {
      if (Cancel != null) Cancel();
    }
    public event CallbackDelegateGetInteger ForceGetInteger;
    private void FireForceGetInteger()
    {
      ForceGetInteger(null);
    }
    public event CallbackDelegateGetInteger ForceGetIntegerInterval;
    private void FireForceGetIntegerInterval()
    {
      ForceGetIntegerInterval(null);
    }

    public void Execute(PrimesBigInteger from, PrimesBigInteger to)
    {
    }

    #endregion
  }
}
