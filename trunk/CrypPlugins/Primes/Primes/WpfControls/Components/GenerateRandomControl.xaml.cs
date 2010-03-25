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
using Primes.Library;

namespace Primes.WpfControls.Components
{
  /// <summary>
  /// Interaction logic for GenerateRandomControl.xaml
  /// </summary>
  
  public partial class GenerateRandomControl : UserControl
  {
    public event GmpBigIntegerParameterDelegate OnRandomNumberGenerated;
    public GenerateRandomControl()
    {
      InitializeComponent();
    }


    private void FireOnRandomNumberGenerated(PrimesBigInteger value)
    {
      if (OnRandomNumberGenerated != null)
        OnRandomNumberGenerated(value);
    }

    private Primes.OnlineHelp.OnlineHelpActions m_HelpAction;
    public Primes.OnlineHelp.OnlineHelpActions HelpAction
    {
      get { return m_HelpAction; }
      set { 
        this.m_HelpAction = value; 

      }
    }
    private void ImageHelpClick(object sender, MouseButtonEventArgs e)
    {

      OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(m_HelpAction);
      e.Handled = true;
    }

    public string Title
    {
      get { if (miHeader != null && miHeader.Header != null) return miHeader.Header.ToString(); else return null; }
      set { if (miHeader != null)miHeader.Header = value; }
    }

    public bool ShowMultipleFactors
    {
      get { return miIntegerManyFactors.Visibility == Visibility.Visible; }
      set { if (value)miIntegerManyFactors.Visibility = Visibility.Visible; else miIntegerManyFactors.Visibility = Visibility.Collapsed; }
    }

    public bool ShowTwoBigFactors
    {
      get { return miTowBigFactors.Visibility == Visibility.Visible; }
      set { if (value)miTowBigFactors.Visibility = Visibility.Visible; else miTowBigFactors.Visibility = Visibility.Collapsed; }
    }
    private void miIntegerManyFactors_Click(object sender, RoutedEventArgs e)
    {
      PrimesBigInteger value = null;
      if (sender == miBigInteger)
      {
        value = PrimesBigInteger.Random(100);
        while (value.IsProbablePrime(20))
          value = PrimesBigInteger.Random(100);
        if (value.Mod(PrimesBigInteger.Two).Equals(PrimesBigInteger.Zero)) value = value.Add(PrimesBigInteger.One);
      }
      else if (sender == miIntegerManyFactors)
      {
        Random r = new Random();
        value = PrimesBigInteger.ValueOf(r.Next(999)).NextProbablePrime();
        for (int i = 0; i < 19; i++)
        {
          value = value.Multiply(PrimesBigInteger.ValueOf(r.Next(999)).NextProbablePrime());
        }
      }
      else if (sender == miTowBigFactors)
      {
        value = PrimesBigInteger.Random(15).NextProbablePrime().Multiply(PrimesBigInteger.Random(15).NextProbablePrime());
      }
      if (value != null)
        FireOnRandomNumberGenerated(value);
    }

  }
}
