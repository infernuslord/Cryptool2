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
using System.Windows.Controls;
using Primes.Library;
using System.Windows;
using System.Threading;
using Primes.Bignum;

namespace Primes.WpfControls.Factorization.QS
{
  public class Step3 : BaseStep, IQSStep
  {
    private TextBlock m_lblInfo;
    public Step3(Grid grid, TextBlock lblInfo)
      : base(grid)
    {
      m_lblInfo = lblInfo;
    }

    #region IQSStep Members



    public override QSResult Execute(ref QSData data)
    {
      int counter = 1;
      IList<QuadraticPair> pairs = data.BSmooth;
      bool foundquadratic = false;
      foreach (QuadraticPair pair in pairs)
      {
        ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);

        StringBuilder msg = new StringBuilder();
        msg.Append(pair.B);
        if (data.IsIgnored(PrimesBigInteger.ValueOf(pair.B)))
          pair.QuadraticStatus = QuadraticStatus.Ignore;
        if (pair.QuadraticStatus == QuadraticStatus.Ignore)
        {
          msg.Append(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_ignored);
        }
        else
        {
          double sqrt = Math.Sqrt(pair.B);
          if (Math.Floor(sqrt) == sqrt)
          {
            foundquadratic = true;
            pair.QuadraticStatus = QuadraticStatus.Quadratic;
            msg.Append(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_issquare);
          }
          else
          {
            msg.Append(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_isnotsquare);
          }
        }
        ControlHandler.ExecuteMethod(
          this,
          "AddToGrid",
          new object[] { Grid, msg.ToString(), counter, 0, 0, 0 });
        counter++;
        Thread.Sleep(100);

      }
      if (!foundquadratic)
      {
        bool foundParts = false;
        double pslen = Math.Pow(2, pairs.Count);
        for (int i = 1; i < pslen; i++)
        {
          MyInteger mi = new MyInteger(i);
          int bc = mi.BitCount;
          if (bc > 1)
          {
            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);

            TextBlock tb = AddTextBlock(counter, 0);
            StringBuilder msg = new StringBuilder();
            int[] indices = new int[bc];
            int lastindex = 0;
            double erg = 0;
            for (int j = 0; j < pairs.Count; j++)
            {
              if (mi.TestBit(j))
              {
                if (msg.Length > 0) msg.Append(" * ");
                msg.Append(pairs[j].B.ToString("n"));
                indices[lastindex] = j + 1;
                lastindex++;
                if (erg == 0) erg = pairs[j].B;
                else erg *= pairs[j].B;
                ControlHandler.SetPropertyValue(tb, "Text", Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombi+ msg.ToString());
              }
            }
            if (erg != 0)
            {
              if (data.IsIgnored(PrimesBigInteger.ValueOf((long)erg)))
              {
                ControlHandler.SetPropertyValue(tb, "Text", string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombiignore, new object[] { msg.ToString(), erg.ToString("N") }));
              }
              else
              {
                double sqrt = Math.Sqrt(erg);
                if (Math.Floor(sqrt) == sqrt)
                {
                  ControlHandler.SetPropertyValue(tb, "Text", string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombiisquare, new object[] { msg.ToString(), erg.ToString("N"), erg.ToString("N"), sqrt.ToString("N") }));
                  foundParts = true; ;
                  foreach (int j in indices)
                  {
                    if (j > 0) pairs[j - 1].QuadraticStatus = QuadraticStatus.Part;
                  }
                }
                else
                {
                  ControlHandler.SetPropertyValue(tb, "Text", string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step3_testcombiisnotsquare, new object[] { msg.ToString(), erg.ToString("N") }));
                }
              }
            }
            counter++;
          }
          Thread.Sleep(m_Delay);
          if (foundParts) break;
        }
      }
      return QSResult.Ok;
    }


    public override void PreStep()
    {
      ControlHandler.ExecuteMethod(this, "_PreStep");

    }

    public void _PreStep()
    {
      Grid.RowDefinitions.Clear();
      Grid.Children.Clear();
      RowDefinition rd = new RowDefinition();
      rd.Height = new GridLength(1, GridUnitType.Auto);
      Grid.RowDefinitions.Add(rd);

      TextBlock tbA = new TextBlock();
      tbA.Text = "";
      tbA.Margin = new Thickness(5);


      Grid.SetColumn(tbA, 0);
      Grid.SetRow(tbA, 0);
      Grid.Children.Add(tbA);
    }

    public override void PostStep()
    {
    }
    #endregion
  }
}
