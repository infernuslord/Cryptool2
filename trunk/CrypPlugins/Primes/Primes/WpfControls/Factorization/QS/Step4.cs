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
using Primes.Library;
using System.Windows.Controls;
using System.Windows;
using Primes.Bignum;
using System.Threading;

namespace Primes.WpfControls.Factorization.QS
{
  public class Step4:BaseStep, IQSStep
  {

    private TextBlock m_lblInfo;
    public Step4(Grid grid, TextBlock lblInfo)
      : base(grid)
    {
      m_lblInfo = lblInfo;
    }

    #region IQSStep Members

    public override QSResult Execute(ref QSData data)
    {
      QSResult result = QSResult.Ok;
      PrimesBigInteger _n = PrimesBigInteger.ValueOf(data.N);
      int counter = 0;
      IList<QuadraticPair> pairs = data.BSmooth;
      PrimesBigInteger productAQuad = null;
      PrimesBigInteger productA = null;
      PrimesBigInteger productB = null;

      foreach (QuadraticPair pair in pairs)
      {
        if (pair.QuadraticStatus == QuadraticStatus.Quadratic)
        {
          productAQuad = PrimesBigInteger.ValueOf(pair.A).Pow(2);
          productA = PrimesBigInteger.ValueOf(pair.A);
          productB = PrimesBigInteger.ValueOf(pair.B);
          break;
        }
      }
      if (productAQuad == null && productA == null && productB == null)
      {
        foreach (QuadraticPair pair in pairs)
        {
          if (pair.QuadraticStatus == QuadraticStatus.Part)
          {
            if (productAQuad == null) productAQuad = PrimesBigInteger.ValueOf(pair.A).Pow(2).Mod(_n);
            else productAQuad = productAQuad.Multiply(PrimesBigInteger.ValueOf(pair.A).Pow(2)).Mod(_n);

            if (productA == null) productA = PrimesBigInteger.ValueOf(pair.A).Mod(_n);
            else productA = productA.Multiply(PrimesBigInteger.ValueOf(pair.A)).Mod(_n);

            if (productB == null) productB = PrimesBigInteger.ValueOf(pair.B).Mod(_n);
            else productB = productB.Multiply(PrimesBigInteger.ValueOf(pair.B)).Mod(_n);
          }
        }
      }
      result = (productA != null && productB != null)?QSResult.Ok:QSResult.Failed;
      if (result == QSResult.Ok)
      {
        StringBuilder sbInfo = new StringBuilder();
        sbInfo.Append(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_abcalculated);
        sbInfo.Append(productA.ToString("D"));
        sbInfo.Append(" b = ");
        sbInfo.Append(productB.SquareRoot().ToString("D"));
        sbInfo.Append(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_checkcong);
        sbInfo.Append("a² ≡ b² (mod n), a !≡ b (mod n)");
        ControlHandler.SetPropertyValue(m_lblInfo, "Text", sbInfo.ToString());

        result = (ModuloTest(productA.Mod(_n), productB.SquareRoot().Mod(_n), PrimesBigInteger.ValueOf(data.N)))?QSResult.Ok:QSResult.Failed;
        if (result == QSResult.Ok)
        {
          if (productAQuad != null && productA != null && productB != null)
          {
            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
            ControlHandler.ExecuteMethod(
              this,
              "AddToGrid",
              new object[] { Grid, BuildQuadMod(productA,productB.SquareRoot(),_n), counter, 0, 0, 0 });
            counter++;
            ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
            ControlHandler.ExecuteMethod(
              this,
              "AddToGrid",
              new object[] { Grid, BuildMod(productA,productB.SquareRoot(),_n), counter, 0, 0, 0 });
            PrimesBigInteger factor1 = PrimesBigInteger.GCD(productA.Add(productB.SquareRoot().Mod(_n)).Abs().Mod(_n), _n);
            PrimesBigInteger factor2 = PrimesBigInteger.GCD(productA.Subtract(productB.SquareRoot().Mod(_n)).Abs().Mod(_n), _n);

            if (factor1.Equals(PrimesBigInteger.One) || factor2.Equals(PrimesBigInteger.One))
            {
              data.AddIgnoreQuadrat(productB);
              result = QSResult.Failed;
              counter++;
              ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
              ControlHandler.ExecuteMethod(
                this,
                "AddToGrid",
                new object[] { 
                  Grid, 
                  string.Format("Die Faktorisierung mit den Werten a = {0} und b = {1} war nicht erfolgreich. Mit Klick auf Neustart starten Sie Faktorisierung neu. Die ungültigen Werte werden ab sofort ignoriert.",new object[]{ productA.ToString("D"), productB.SquareRoot().ToString("D")}), counter, 0, 0, 0 });

            }
            else
            {

              counter++;
              ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
              StringBuilder sbfactor1 = new StringBuilder();
              sbfactor1.Append(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_firstfactor);
              sbfactor1.Append(" = GCD(");
              sbfactor1.Append(productA.ToString("D"));
              sbfactor1.Append(" + ");
              sbfactor1.Append(productB.SquareRoot().ToString("D"));
              sbfactor1.Append(", ");
              sbfactor1.Append(_n.ToString("D"));
              sbfactor1.Append(") = ");
              sbfactor1.Append(factor1.ToString("D"));

              ControlHandler.ExecuteMethod(
                this,
                "AddToGrid",
                new object[] { Grid, sbfactor1.ToString(), counter, 0, 0, 0 });

              counter++;
              ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
              StringBuilder sbfactor2 = new StringBuilder();
              sbfactor2.Append(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_secondfactor);
              sbfactor2.Append(" = GCD(");
              sbfactor2.Append(productA.ToString("D"));
              sbfactor2.Append(" - ");
              sbfactor2.Append(productB.SquareRoot().ToString("D"));
              sbfactor2.Append(", ");
              sbfactor2.Append(_n.ToString("D"));
              sbfactor2.Append(") = ");
              sbfactor2.Append(factor2.ToString("D"));

              ControlHandler.ExecuteMethod(
                this,
                "AddToGrid",
                new object[] { Grid, sbfactor2.ToString(), counter, 0, 0, 0 });
              PrimesBigInteger notPrime = null;
              if (!factor1.IsPrime(10)) notPrime = factor1;
              else if (!factor2.IsPrime(10)) notPrime = factor2;

              if (factor1.IsPrime(10)) FireFoundFactorEvent(factor1);
              if (factor2.IsPrime(10)) FireFoundFactorEvent(factor2);

              if(notPrime!=null)
              {
                counter++;
                ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
                ControlHandler.ExecuteMethod(
                  this,
                  "AddToGrid",
                  new object[] { Grid, string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_refactorize, new object[] { notPrime.ToString("D"), notPrime.ToString("D") }), counter, 0, 0, 0 });
                result = QSResult.Restart;
                data.N = notPrime.LongValue;
              }
            }


          }
        }
        else
        {
          data.AddIgnoreQuadrat(productB);
          ControlHandler.AddRowDefintion(Grid, 1, GridUnitType.Auto);
          ControlHandler.ExecuteMethod(
            this,
            "AddToGrid",
            new object[] { Grid, Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_step4_notprofed, counter, 0, 0, 0 });
          result = QSResult.Failed;

        }
      }

      return result;
    }
    private string BuildQuadMod(PrimesBigInteger producta, PrimesBigInteger productb, PrimesBigInteger mod)
    {
      StringBuilder sbQuadMod = new StringBuilder();
      sbQuadMod.Append(producta.ToString("D"));
      sbQuadMod.Append("² ≡ ");
      sbQuadMod.Append(productb.ToString("D"));
      sbQuadMod.Append("² (mod ");
      sbQuadMod.Append(mod.ToString("D"));
      sbQuadMod.Append(")");
      return sbQuadMod.ToString();
    }

    private string BuildMod(PrimesBigInteger producta, PrimesBigInteger productb, PrimesBigInteger mod)
    {
      StringBuilder sbQuadMod = new StringBuilder();
      sbQuadMod.Append(producta.ToString("D"));
      sbQuadMod.Append(" ≢  ");
      sbQuadMod.Append(productb.ToString("D"));
      sbQuadMod.Append(" (mod ");
      sbQuadMod.Append(mod.ToString("D"));
      sbQuadMod.Append(")");
      return sbQuadMod.ToString();
    }

    public override void PreStep()
    {
      ControlHandler.ExecuteMethod(this, "_PreStep");
    }

    public void _PreStep()
    {
      Grid.RowDefinitions.Clear();
      Grid.Children.Clear();
      m_lblInfo.Text = "";
    }

    public override void PostStep()
    {
    }

    #endregion
  }
}
