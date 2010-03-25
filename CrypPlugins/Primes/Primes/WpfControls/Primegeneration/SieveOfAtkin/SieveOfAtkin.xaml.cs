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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Primes.Bignum;
using System.Threading;
using Primes.Library;

namespace Primes.WpfControls.Primegeneration.SieveOfAtkin
{
  /// <summary>
  /// Interaction logic for SieveOfAtkin.xaml
  /// </summary>
  public partial class SieveOfAtkin : UserControl
  {
    private Thread m_SieveThread;
    private PrimesBigInteger m_Value;
    public SieveOfAtkin()
    {
      InitializeComponent();
    }

    #region Thread
    public void startThread()
    {
      m_SieveThread = new Thread(new ThreadStart(Sieve));
      m_SieveThread.Start();
    }

    public void CancelSieve()
    {
      FireCancelEvent();
      CancelThread();
    }

    private void CancelThread()
    {
      if (m_SieveThread != null)
      {
        m_SieveThread.Abort();
        m_SieveThread = null;
      }
    }
    #endregion

    #region events
    public event VoidDelegate Start;
    public event VoidDelegate Stop;
    public event VoidDelegate Cancel;

    private void FireStartEvent()
    {
      if (Start != null) Start();
    }
    private void FireStopEvent()
    {
      if (Stop != null) Stop();
    }
    private void FireCancelEvent()
    {
      if (Cancel != null) Cancel();
    }
    #endregion
    public void Execute(PrimesBigInteger value)
    {
      m_Value = value;
      numbergrid.Limit = m_Value;
      startThread();
    }

    public void Sieve()
    {
      FireStartEvent();
      bool[] list = new bool[m_Value.LongValue+1];
      log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_initvsieve, m_Value.ToString("D")));
      IList<PrimesBigInteger> result = new List<PrimesBigInteger>();
      result.Add(PrimesBigInteger.Two);
      result.Add(PrimesBigInteger.Three);
      result.Add(PrimesBigInteger.Five);
      numbergrid.MarkNumber(PrimesBigInteger.Two, Brushes.LightBlue);
      numbergrid.MarkNumber(PrimesBigInteger.Three, Brushes.LightBlue);
      numbergrid.MarkNumber(PrimesBigInteger.Five, Brushes.LightBlue);


      log.Info(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_startsieve);
      for (int i = 1; i < list.Length; i++)
      {
        log.Info(string.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_actualnumber, i.ToString()));
        int mod = i % 60;
        if (i % 60 == 1 || i % 60 == 13 || i % 60 == 17 || i % 60 == 29 || i % 60 == 37
            || i % 60 == 41 || i % 60 == 49 || i % 60 == 53)
        {
          log.Info(String.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_firstifmatch, new object[] { i.ToString(), mod.ToString(), i.ToString() }));
          for (int j = 0; j < i; j++)
          {
            for (int k = 0; k < i; k++)
            {

              if (4 * j * j + k * k == i)
              {
                list[i] = !list[i];
                log.Info(String.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_firstsolutionfound, new object[] { j.ToString(), k.ToString(), i.ToString(), i.ToString(), (list[i]) ? "ist Primzahl" : "ist keine Primzahl" }));

                if (list[i])
                  numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), Brushes.LightBlue);
                else
                  numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), Brushes.Transparent);
              }
            }
          }
        }
        if (i % 60 == 7 || i % 60 == 19 || i % 60 == 31 || i % 60 == 43)
        {
          log.Info(String.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_secondifmatch, new object[] { i.ToString(), mod.ToString(), i.ToString() }));
          for (int j = 0; j < i; j++)
          {
            for (int k = 0; k < i; k++)
            {
              if (3 * j * j + k * k == i)
              {
                list[i] = !list[i];
                log.Info(String.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_secondsolutionfound, new object[] { j.ToString(), k.ToString(), i.ToString(), i.ToString(), (list[i]) ? "ist Primzahl" : "ist keine Primzahl" }));
                if (list[i])
                  numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), Brushes.LightBlue);
                else
                  numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), Brushes.Transparent);

              }
            }
          }
        }
        if (i % 60 == 11 || i % 60 == 23 || i % 60 == 47 || i % 60 == 59)
        {
          log.Info(String.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_thirdifmatch, new object[] { i.ToString(), mod.ToString(), i.ToString() }));
          for (int j = 0; j < i; j++)
          {
            for (int k = 0; k < i; k++)
            {
              if (3 * j * j - k * k == i && j > k)
              {
                list[i] = !list[i];
                log.Info(String.Format(Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_thirdsolutionfound, new object[] { j.ToString(), k.ToString(), i.ToString(), i.ToString(), (list[i]) ? Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_isprime : Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_isnotprime }));

                if (list[i])
                  numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), Brushes.LightBlue);
                else
                  numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), Brushes.Transparent);

              }
            }
          }
        }
        Thread.Sleep(10);
      }
      list[2] = true;
      list[3] = true;
      list[5] = true;

      for (int i = 7; i < list.Length; i++)
      {
        if (list[i])
        {
          numbergrid.MarkNumber(PrimesBigInteger.ValueOf(i), Brushes.LightBlue);
          log.Info(
            String.Format(
              Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_fourthsolutionfound,
              new object[] { i.ToString(), i.ToString()}));
          for (int j = i * i; j < list.Length; j += i)
          {
            list[j] = false;
            numbergrid.MarkNumber(PrimesBigInteger.ValueOf(j), Brushes.Transparent);
            log.Info(
              String.Format(
              Primes.Resources.lang.WpfControls.Generation.PrimesGeneration.soa_fithsolutionfound,
              new object[] { j.ToString(), i.ToString()}));

          }
        }
      }
      numbergrid.Sieved = list;
      //for (int i = 7; i < list.Length; i++)
      //{
      //  if (list[i])
      //  {
      //    numbergrid.RemoveMulipleOf(PrimesBigInteger.ValueOf(i));
      //  }
      //}
      FireStopEvent();
    }
  }
}
