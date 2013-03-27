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
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using Primes.WpfControls.Components;
using Primes.Library;

namespace Primes.WpfControls.Primetest
{
    /// <summary>
    /// Interaction logic for PrimetestControl.xaml
    /// </summary>
    public partial class PrimetestControl : UserControl, IPrimeMethodDivision
    {
        public PrimetestControl(Navigate nav = null)
        {
            InitializeComponent();

            sieveoferatosthenes.Start += new VoidDelegate(sieveoferatosthenes_Execute);
            sieveoferatosthenes.Stop += new VoidDelegate(sieveoferatosthenes_Cancel);
            sieveoferatosthenes.ForceGetInteger += new CallbackDelegateGetInteger(sieveoferatosthenes_ForceGetInteger);

            fermat.ForceGetInteger += new CallbackDelegateGetInteger(sieveoferatosthenes_ForceGetInteger);
            fermat.ExecuteTest += new VoidDelegate(fermat_Start);
            fermat.CancelTest += new VoidDelegate(fermat_Stop);
            //fermat.ForceGetValue += new Primes.Library.CallBackDelegate(PrimeTestForceGetValue);

            millerrabin.Start += new VoidDelegate(millerrabin_ExecuteTest);
            millerrabin.Stop += new VoidDelegate(millerrabin_CancelTest);
            millerrabin.ForceGetInteger += new CallbackDelegateGetInteger(sieveoferatosthenes_ForceGetInteger);

            soa.Start += new VoidDelegate(millerrabin_ExecuteTest);
            soa.Stop += new VoidDelegate(millerrabin_CancelTest);

            iscNumber.SetText(InputSingleControl.Free, "100");
            iscNumber.SetText(InputSingleControl.CalcFactor, "1");
            iscNumber.SetText(InputSingleControl.CalcBase, ((new Random().Next() % 5) + 1).ToString());
            iscNumber.SetText(InputSingleControl.CalcExp, ((new Random().Next() % 5) + 1).ToString());
            iscNumber.SetText(InputSingleControl.CalcSum, (new Random().Next() % 11).ToString());

            myNavigate = nav;
        }

        void sieveoferatosthenes_ForceGetInteger(ExecuteIntegerDelegate ExecuteDelegate)
        {
            PrimesBigInteger value = iscNumber.GetValue();
            if (value != null && ExecuteDelegate != null)
            {
                ExecuteDelegate(value);
            }
        }

        void generateNumberControl_OnRandomNumberGenerated(PrimesBigInteger value)
        {
            iscNumber.FreeText = value.ToString();
            iscNumber_Execute(value);
        }

        void millerrabin_CancelTest()
        {
            iscNumber.UnLockControls();
        }

        void millerrabin_ExecuteTest()
        {
            iscNumber.LockControls();
        }

        #region fermat

        void fermat_Stop()
        {
            iscNumber.UnLockControls();
        }

        void fermat_Start()
        {
            iscNumber.LockControls();
        }

        #endregion

        #region Erathostenes

        void sieveoferatosthenes_Cancel()
        {
            iscNumber.UnLockControls();
        }

        void sieveoferatosthenes_Execute()
        {
            iscNumber.LockControls();
        }

        #endregion

        #region IPrimeUserControl Members

        public void Dispose()
        {
            if (sieveoferatosthenes != null)
            {
                sieveoferatosthenes.CleanUp();
            }
            //if (fermat != null)
            //{
            //  fermat.CleanUp();
            //}
        }

        #endregion

        private void iscNumber_Execute(PrimesBigInteger value)
        {
            CurrentControl.Execute(value);
        }

        private void iscNumber_Cancel()
        {
            CurrentControl.CancelExecute();
            SetLocks();
        }

        private IPrimeTest CurrentControl
        {
            get
            {
                if (tbctrl.SelectedItem == tabItemSieveOfEratosthenes) return sieveoferatosthenes;
                if (tbctrl.SelectedItem == tabItemTestOfFermat) return fermat;
                if (tbctrl.SelectedItem == tabItemMillerRabin) return millerrabin;
                if (tbctrl.SelectedItem == tabItemSoa) return soa;

                return null;
            }
        }

        private NavigationCommandType CurrentNavigationCommand
        {
            get
            {
                if (tbctrl.SelectedItem == tabItemSieveOfEratosthenes) return NavigationCommandType.Primetest_Sieve;
                if (tbctrl.SelectedItem == tabItemMillerRabin) return NavigationCommandType.Primetest_Miller;
                if (tbctrl.SelectedItem == tabItemSoa) return NavigationCommandType.SieveOfAtkin;

                return NavigationCommandType.Primetest_Sieve;
            }
        }

        public Navigate myNavigate = null;

        private void tbctrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            iscNumber.SetValueValidator(InputSingleControl.Value, CurrentControl.Validator);
            if (myNavigate != null) myNavigate(CurrentNavigationCommand);
            iscNumber.GetValue();

            SetLocks();
        }

        private void SetLocks()
        {
            PrimesBigInteger value = iscNumber.GetValue();
            if (value != null)
            {
                if (CurrentControl.IsRunning())
                    iscNumber.LockControls();
                else
                    iscNumber.UnLockControls();
            }
        }

        #region IPrimeUserControl Members

        public void SetTab(int i)
        {
            if (i >= 0 && i < tbctrl.Items.Count)
            {
                tbctrl.SelectedIndex = i;
                tbctrl_SelectionChanged(null, null);
                //ResourceDictionary rd = Application.LoadComponent(new Uri("Primes;component/WpfControls/Resources/Brushes.xaml", UriKind.Relative)) as ResourceDictionary;
                //(tbctrl.Items[i] as TabItem).Background = rd["HorizontalLightBrush"] as Brush;
            }
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

        private void TabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabItemSieveOfEratosthenes)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Primetest_SieveOfEratosthenes);
            }
            else if (sender == tabItemMillerRabin)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Primetest_MillerRabin);
            }
            else if (sender == tabItemSoa)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Generation_SieveOfAtkin);
            }
        }
    }
}
