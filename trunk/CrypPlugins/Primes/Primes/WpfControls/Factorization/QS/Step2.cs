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

namespace Primes.WpfControls.Factorization.QS
{
  public class Step2 : BaseStep, IQSStep
  {

    private TextBlock m_lblInfo;
    public Step2(Grid grid, TextBlock lblInfo)
      : base(grid)
    {
      m_lblInfo = lblInfo;
    }

    #region IQSStep Members

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

      TextBlock tbB = new TextBlock();
      tbB.Text = "b";
      tbB.Margin = new Thickness(5);

      TextBlock tbFactors = new TextBlock();
      tbFactors.Text = "Faktorisierung";
      tbFactors.Margin = new Thickness(5);

      TextBlock tbIsBSmooth = new TextBlock();
      tbIsBSmooth.Text = "Ist B-Glatt";
      tbIsBSmooth.Margin = new Thickness(5);

      Grid.SetColumn(tbB, 0);
      Grid.SetRow(tbB, 0);
      Grid.Children.Add(tbB);

      Grid.SetColumn(tbFactors, 1);
      Grid.SetRow(tbFactors, 0);
      Grid.Children.Add(tbFactors);

      Grid.SetColumn(tbIsBSmooth, 2);
      Grid.SetRow(tbIsBSmooth, 0);
      Grid.Children.Add(tbIsBSmooth);
    }

    public override void PostStep()
    {
    }

    public override QSResult Execute(ref QSData data)
    {
      IList<int> m_Factors = data.CalculateFactorBase();
      ControlHandler.SetPropertyValue(
        m_lblInfo,
        "Text",
        string.Format(
          Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step2_B,
          new object[] { StringFormat.FormatDoubleToIntString(data.B), m_Factors.ToString() }));
      IList<long> list = new List<long>();
      int counter = 1;
      foreach (QuadraticPair pair in data)
      {
        ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);

        long b = pair.B;
        if (b < 0)
        {
          b *= -1;
          pair.AddExponent(-1, 1);
        }
        else
        {
          pair.AddExponent(-1, 2);
        }
        AddToGrid(Grid, b.ToString(), counter, 0, 0, 0);
        TextBlock tb = AddTextBlock(counter, 1);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < m_Factors.Count;i++ )
        {
          int f = m_Factors[i];
          int exp = 0;
          while (b % f == 0)
          {
            b = b / f;
            exp++;
          }
          if (exp > 0)
          {
            if (sb.Length>0) sb.Append(" * ");
            sb.Append(f.ToString());
            sb.Append("^");
            sb.Append(exp.ToString());

            ControlHandler.SetPropertyValue(tb, "Text", sb.ToString());
          }
          pair.AddExponent(f, exp);

        }
        if (b == 1)
        {
          pair.IsBSmooth = true;
          ControlHandler.ExecuteMethod(
            this,
            "AddToGrid",
            new object[] { Grid, Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step2_yes, counter, 2, 0, 0 });

        }
        else
        {
          ControlHandler.ExecuteMethod(
            this,
            "AddToGrid",
            new object[] { Grid, Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step2_no, counter, 2, 0, 0 });
        }
        counter++;
        Thread.Sleep(m_Delay);

      }
      return QSResult.Ok;
    }


    #endregion

  }
}
