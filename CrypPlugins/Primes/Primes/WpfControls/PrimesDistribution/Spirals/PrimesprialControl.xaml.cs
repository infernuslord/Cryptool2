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
using Primes.WpfControls.Components;
using Primes.WpfControls.Validation.Validator;

namespace Primes.WpfControls.PrimesDistribution.Spirals
{
  /// <summary>
  /// Interaction logic for PrimesprialControl.xaml
  /// </summary>
  public partial class PrimesprialControl : UserControl, IPrimeMethodDivision
  {

    public PrimesprialControl()
    {
      InitializeComponent();
      spiral.StartDrawing += new VoidDelegate(spiral_StartDrawing);
      spiral.StopDrawing += new VoidDelegate(spiral_StopDrawing);
      irc.Execute += new Primes.WpfControls.Components.ExecuteDelegate(irc_Execute);
      irc.RangeValueValidator = new BigIntegerMinValueMaxValueValidator(null,PrimesBigInteger.Ten,PrimesBigInteger.ValueOf(100000));
      irc.RangeValueValidator.Message = "Der Abstand zwischen \"von\" und \"bis\" muss mindestens {0} und darf höchstens {1} betragen.";
      irc.SetText(InputRangeControl.FreeFrom, "1");
      irc.SetText(InputRangeControl.FreeTo, "2000");
    }


    void spiral_StopDrawing()
    {
      ControlHandler.SetPropertyValue(lblInfo, "Text", "");
      ControlHandler.SetButtonEnabled(btnExecute, true);
      ControlHandler.SetButtonEnabled(btnCancel, false);
      irc.UnLockControls();
    }

    void spiral_StartDrawing()
    {
      ControlHandler.SetPropertyValue(lblInfo,"Text","Spirale wird berechnet");
      ControlHandler.SetButtonEnabled(btnExecute, false);
      ControlHandler.SetButtonEnabled(btnCancel, true);
      irc.LockControls();
    }

    private void btnExecute_Click(object sender, RoutedEventArgs e)
    {

      PrimesBigInteger from = null;
      PrimesBigInteger to = null;

      if (irc.GetValue(ref from, ref to))
      {
        irc_Execute(from, to);
      }
    }
    
    void irc_Execute(PrimesBigInteger from, PrimesBigInteger to)
    {
      CurrentSpiral.Clear();
      CurrentSpiral.Draw(from, to);
      
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      CurrentSpiral.Cancel();
    }


    public void Dispose()
    {
      CurrentSpiral.Cancel();
    }

    private IPrimeSpiral CurrentSpiral
    {
      get { return spiral; }
    }

    #region IPrimeUserControl Members


    public void SetTab(int i)
    {
      
    }

    #endregion

    #region IPrimeUserControl Members


    public event VoidDelegate Execute;

    public event VoidDelegate Stop;


    private void FireExecuteEvent()
    {
      if (Execute != null) Execute();
    }

    private void FireStopEvent()
    {
      if (Stop != null) Stop();
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
