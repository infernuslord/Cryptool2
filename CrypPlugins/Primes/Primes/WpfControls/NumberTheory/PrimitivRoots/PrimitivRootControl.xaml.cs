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
using System.Threading;
using Primes.WpfControls.Components;
using Primes.Library;
using Primes.Bignum;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using System.Diagnostics;

namespace Primes.WpfControls.NumberTheory.PrimitivRoots
{
  /// <summary>
  /// Interaction logic for PrimitivRootControl.xaml
  /// </summary>
  public partial class PrimitivRootControl : UserControl, IPrimeMethodDivision
  {
    private Thread m_ThreadCalculatePrimitiveRoots;
    private static int[] mersenneseed = new int[] {3, 5, 7, 13};
    public PrimitivRootControl()
    {
      InitializeComponent();
      this.OnStart += new VoidDelegate(PrimitivRootControl_OnStart);
      this.OnStop += new VoidDelegate(PrimitivRootControl_OnStop);
      validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.Five,MAX);
      primes = new List<PrimesBigInteger>();
      log.OverrideText = true;
      int mersenneexp = mersenneseed[new Random().Next(mersenneseed.Length - 1)];
      tbInput.Text = PrimesBigInteger.Random(2).Add(PrimesBigInteger.Three).NextProbablePrime().ToString();
      tbInput.Text += ", 2^" + mersenneexp + "-1";
      PrimesBigInteger rangeval =PrimesBigInteger.Random(2).Add(PrimesBigInteger.Three);

      tbInput.Text += ", " + rangeval.ToString() + ";" + rangeval.Add(PrimesBigInteger.Ten).ToString();

      rndGenerate = new Random((int)(DateTime.Now.Ticks% int.MaxValue));
      m_JumpLockObject = new object();
    }


    private void btnPrimitivRootInput_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.PrimitivRoot_Input);
    }
    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      StopThread();

    }
    private void ClearLog()
    {
      log.Clear();
      log.Columns = 1;
    }

    private void btnExecute_Click(object sender, RoutedEventArgs e)
    {
      Execute(true);
    }

    private void Execute(bool doExecute)
    {
      
      ClearLog();
      string _input = tbInput.Text;
      if (!string.IsNullOrEmpty(_input))
      {
        string[] input = _input.Trim().Split(',');
        if (input != null && input.Length > 0)
        {
          primes.Clear();
          foreach (string s in input)
          {
            if (!string.IsNullOrEmpty(s))
            {
              string[] _inputrange = s.Split(';');
              if (_inputrange.Length == 1)
              {
                PrimesBigInteger ipt = null;
                validator.Value = s;
                Primes.WpfControls.Validation.ValidationResult res = validator.Validate(ref ipt);
                if (res == Primes.WpfControls.Validation.ValidationResult.OK)
                {
                  if (ipt.IsPrime(10))
                  {
                    primes.Add(ipt);
                  }
                  else
                  {
                    log.Info(string.Format(Primes.Resources.lang.Numbertheory.Numbertheory.proot_noprime, s));
                    log.Info(" ");
                  }
                }
                else
                {
                  log.Info(string.Format(Primes.Resources.lang.Numbertheory.Numbertheory.proot_novalidnumber, new object[] { s, MAX.ToString() }));
                  log.Info(" ");
                }
              }
              else
              {
                if (string.IsNullOrEmpty(_inputrange[0]))
                {
                  log.Info(string.Format(Primes.Resources.lang.Numbertheory.Numbertheory.proot_rangedown, s));
                  log.Info(" ");
                }
                else if (string.IsNullOrEmpty(_inputrange[1]))
                {
                  log.Info(string.Format(Primes.Resources.lang.Numbertheory.Numbertheory.proot_rangeupper, s));
                  log.Info(" ");
                }
                else
                {
                  PrimesBigInteger i1 = IsGmpBigInteger(_inputrange[0]);
                  PrimesBigInteger i2 = IsGmpBigInteger(_inputrange[1]);
                  if (i1 != null && i2 != null)
                  {
                    if (i1.CompareTo(i2) >= 0)
                    {
                      log.Info(string.Format(Primes.Resources.lang.Numbertheory.Numbertheory.proot_wronginterval, s));
                      log.Info(" ");
                    }
                    else
                    {
                      if (i1.IsPrime(10)) primes.Add(i1);
                      while (i1.CompareTo(i2) <= 0)
                      {
                        i1 = i1.NextProbablePrime();
                        if (i1.CompareTo(i2) <= 0)
                          primes.Add(i1);

                      }

                    }
                  }
                }
              }
            }
          }
        }
        if (primes.Count > 0 && doExecute)
        {
          StartThread();
        }
      }
      else
      {
        Info(Primes.Resources.lang.Numbertheory.Numbertheory.proot_insert);
      }
    }

    private PrimesBigInteger IsGmpBigInteger(string s)
    {
      PrimesBigInteger ipt = null;
      validator.Value = s;
      Primes.WpfControls.Validation.ValidationResult res = validator.Validate(ref ipt);
      if (res != Primes.WpfControls.Validation.ValidationResult.OK)
      {
        log.Info(s + " ist keine Zahl zwischen 5 und " + MAX.ToString() + ". Sie wird daher nicht weiter ausgewertet.");
        log.Info(" ");
      }
      return ipt;
    }
    private string[] GetInput()
    {
      string _input = tbInput.Text;
      if (!string.IsNullOrEmpty(_input))
      {
        return _input.Split(',');
      }
      return null;
    }
    #region Constants
    private static readonly PrimesBigInteger MAX = PrimesBigInteger.ValueOf(1000000);
    #endregion
    #region Properites
    private IList<PrimesBigInteger> primes;
    private IValidator<PrimesBigInteger> validator;
    private Random rndGenerate; 

    #endregion
    #region events
    private event VoidDelegate OnStart;
    private event VoidDelegate OnStop;
    private void FireOnStop()
    {
      if (OnStop != null) OnStop();
    }
    private void FireOnStart()
    {
      if (OnStart != null) OnStart();
    }
    #endregion
    #region CalculatePrimitiveRoots

    void PrimitivRootControl_OnStop()
    {
      ControlHandler.SetPropertyValue(log, "Title", Primes.Resources.lang.Numbertheory.Numbertheory.proot_result);
      ControlHandler.SetButtonEnabled(btnExecute, true);
      ControlHandler.SetButtonEnabled(btnCancel, false);
      ControlHandler.SetPropertyValue(btnJump, "Visibility", Visibility.Hidden);
      ControlHandler.SetPropertyValue(tbInput, "IsEnabled", true);
    }

    void PrimitivRootControl_OnStart()
    {
      ControlHandler.SetPropertyValue(log, "Title", Primes.Resources.lang.Numbertheory.Numbertheory.proot_progress);
      ControlHandler.SetButtonEnabled(btnExecute, false);
      ControlHandler.SetButtonEnabled(btnCancel, true);
      ControlHandler.SetPropertyValue(btnJump, "Visibility", Visibility.Visible);
      ControlHandler.SetPropertyValue(tbInput, "IsEnabled", false);
    }

    private void StartThread()
    {
      m_ThreadCalculatePrimitiveRoots = new Thread(new ThreadStart(DoCalculatePrimitiveRoots));
      m_ThreadCalculatePrimitiveRoots.Start();

    }
    private void StopThread()
    {
      FireOnStop();
      CancelThread();
    }

    private void CancelThread()
    {
      if (m_ThreadCalculatePrimitiveRoots != null)
      {
        m_ThreadCalculatePrimitiveRoots.Abort();
        m_ThreadCalculatePrimitiveRoots = null;
      }
    }

    private void DoCalculatePrimitiveRoots()
    {
      DateTime start = DateTime.Now;
      FireOnStart();
      foreach (PrimesBigInteger prime in primes)
      {
        int row1 = log.NewLine();
        int row2 = log.NewLine();

        StringBuilder sbResult = new StringBuilder();
        PrimesBigInteger primeroot = PrimesBigInteger.One;
        while(primeroot.CompareTo(prime)<0)
        {
            if (IsPrimitiveRoot(primeroot, prime))
            {
              break;
              
            }
          primeroot = primeroot.Add(PrimesBigInteger.One);
        }
        PrimesBigInteger i = PrimesBigInteger.One;
        PrimesBigInteger primeMinus1 = prime.Subtract(PrimesBigInteger.One);
        PrimesBigInteger counter = PrimesBigInteger.Zero;
        while (i.CompareTo(primeMinus1) <= 0)
        {
          //lock (m_JumpLockObject)
          //{
          //  if (m_Jump)
          //  {
          //    m_Jump = false;
          //    break;
          //  }
          //}
          if (PrimesBigInteger.GCD(i, primeMinus1).Equals(PrimesBigInteger.One))
          {
            lock (m_JumpLockObject)
            {
              if (m_Jump)
              {
                log.Info(string.Format(Primes.Resources.lang.Numbertheory.Numbertheory.proot_skipcalc,new object[]{counter.ToString(),prime.ToString()}), 0, row1);
                m_Jump = false;
                break;
              }
            }
            PrimesBigInteger proot = primeroot.ModPow(i, prime);
            counter = counter.Add(PrimesBigInteger.One);
            log.Info(string.Format(Primes.Resources.lang.Numbertheory.Numbertheory.proot_skipcalc,new object[]{counter.ToString(), prime.ToString()}), 0, row1);
            sbResult.Append(proot.ToString());
            sbResult.Append(" ");
            log.Info(sbResult.ToString(), 0, row2);
          }
          i = i.Add(PrimesBigInteger.One);
        }
        int r3 = log.NewLine();
        log.Info(" ");
      }

      TimeSpan diff = DateTime.Now - start;
      StopThread();
    }

    private bool IsPrimitiveRoot(PrimesBigInteger root, PrimesBigInteger prime)
    {
      bool result = true;      
      if(PrimesBigInteger.GCD(root,prime).Equals(PrimesBigInteger.One)){
          PrimesBigInteger k = PrimesBigInteger.One;
          while (k.CompareTo(prime.Subtract(PrimesBigInteger.One)) < 0)
          {            
            if(root.ModPow(k,prime).Subtract(PrimesBigInteger.One).Equals(PrimesBigInteger.Zero)){
              result = false;
              break;
            }
            k = k.Add(PrimesBigInteger.One);
          }
      }
      else
      {
        result = false;
      }
      return result;
    }
    #endregion

    #region InfoError

    private void Info(string message)
    {
      if (!string.IsNullOrEmpty(message))
      {
        tbInput.Background = Brushes.LightBlue;
        lblInfo.Text = message;
        lblInfo.Foreground = Brushes.Blue;
      }
    }
    private void HideInfo()
    {
      tbInput.Background = Brushes.White;
      lblInfo.Text = "";
      lblInfo.Foreground = Brushes.Blue;
    }

    #endregion

    #region IPrimeUserControl Members

    public void Dispose()
    {
      StopThread();
    }

    #endregion

    private void tbInput_KeyDown(object sender, KeyEventArgs e)
    {

    }

    private void tbInput_KeyUp(object sender, KeyEventArgs e)
    {
      HideInfo();
      if (!string.IsNullOrEmpty(tbInput.Text))
      {
        Execute(e.Key == Key.Enter);
      }
      else
      {
        Info(Primes.Resources.lang.Numbertheory.Numbertheory.proot_insert);

      }
    }

    private void miHeader_Click(object sender, RoutedEventArgs e)
    {
      int rndNumber = rndGenerate.Next(950);
      PrimesBigInteger prime = PrimesBigInteger.ValueOf(rndNumber).NextProbablePrime();
      if (!string.IsNullOrEmpty(tbInput.Text))
        tbInput.Text += ", ";
      else
      {
        HideInfo();

      }
      tbInput.Text += prime.ToString();
      
    }

    private object m_JumpLockObject;
    private bool m_Jump;
    private void btnJump_Click(object sender, RoutedEventArgs e)
    {
      lock(m_JumpLockObject)
      {
        m_Jump = true;
      }
    }



    #region IPrimeUserControl Members


    public void SetTab(int i)
    {
      
    }

    #endregion


    #region IPrimeUserControl Members


    public void Init()
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
