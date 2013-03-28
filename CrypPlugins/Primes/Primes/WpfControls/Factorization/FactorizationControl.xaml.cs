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

using Primes.WpfControls.Validation;
using Primes.Library.FactorTree;
using Primes.WpfControls.Validation.Validator;
using Primes.Bignum;
using Primes.WpfControls.Components;
using Primes.Library;
using System.Threading;

namespace Primes.WpfControls.Factorization
{
    /// <summary>
    /// Interaction logic for FactorizationGraph.xaml
    /// </summary>
    public partial class FactorizationControl : UserControl, IPrimeMethodDivision
    {
        private PrimesBigInteger m_Integer;

        private IFactorizer _rho;
        private IFactorizer _bruteforce;
        private IFactorizer _qs;
        private TextBox m_TbFactorizationCopy;

        private string m_gbFactorizationInfo_BruteForce = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_result;
        private string m_gbFactorizationInfo_QS = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_result;
        private string m_factors_BruteForce = "";
        private string m_factors_QS = "";
        private string m_lblInput_BruteForce = "";
        private string m_lblInput_QS = "";
        

        public FactorizationControl()
        {
            InitializeComponent();

            _bruteforce = graph;
            _bruteforce.Start += OnFactorizationStart;
            _bruteforce.Stop += OnFactorizationStop;
            _bruteforce.FoundFactor += OnFoundFactor;
            _bruteforce.Cancel += new VoidDelegate(_bruteforce_Cancel);

            _rho = rhoctrl;
            _rho.Start += OnFactorizationStart;
            _rho.Stop += OnFactorizationStop;
            _rho.FoundFactor += OnFoundFactor;
            _rho.Cancel += new VoidDelegate(_rho_Cancel);

            _qs = qsctrl;
            _qs.Start += OnFactorizationStart;
            _qs.Stop += OnFactorizationStop;
            _qs.FoundFactor += OnFoundFactor;
            _qs.Cancel += new VoidDelegate(_qs_Cancel);

            _rho.ForceGetInteger += new CallbackDelegateGetInteger(_rho_ForceGetValue);
            _bruteforce.ForceGetInteger += new CallbackDelegateGetInteger(_rho_ForceGetValue);

            inputnumbermanager.Execute += new ExecuteSingleDelegate(InputSingleNumberControl_Execute);
            inputnumbermanager.Cancel += new VoidDelegate(InputSingleNumberControl_Cancel);
            inputnumbermanager.HelpActionGenerateRandomNumber = Primes.OnlineHelp.OnlineHelpActions.Factorization_Generate;
            SetInputValidators();
            m_TbFactorizationCopy = new TextBox();
            m_TbFactorizationCopy.IsReadOnly = true;
            m_TbFactorizationCopy.MouseLeave += new MouseEventHandler(m_TbFactorizationCopy_MouseLeave);
            m_TbFactorizationCopy.MouseMove += new MouseEventHandler(m_TbFactorizationCopy_MouseMove);
            ContextMenu tbFactorizationCopyContextMenu = new ContextMenu();
            MenuItem tbFactorizationCopyContextMenuCopy = new MenuItem();
            tbFactorizationCopyContextMenuCopy.Click += new RoutedEventHandler(tbFactorizationCopyContextMenuCopy_Click);
            tbFactorizationCopyContextMenuCopy.Header = Primes.Resources.lang.WpfControls.Factorization.Factorization.qs_copytoclipboard;
            tbFactorizationCopyContextMenu.Items.Add(tbFactorizationCopyContextMenuCopy);
            m_TbFactorizationCopy.ContextMenu = tbFactorizationCopyContextMenu;
            //inputnumbermanager.Execute += new ExecuteBigIntegerDelegate(InputSingleNumberControl_Execute);
            //inputnumbermanager.MinValue = "1";

            inputnumbermanager.FreeText = "100";
            inputnumbermanager.CalcFactorText = "2";
            inputnumbermanager.CalcBaseText = "13";
            inputnumbermanager.CalcExpText = "2";
            inputnumbermanager.CalcSumText = "7";
        }

        private void SetInputValidators()
        {
            InputValidator<PrimesBigInteger> ivExp = new InputValidator<PrimesBigInteger>();
            ivExp.Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.One, PrimesBigInteger.OneHundred);
            inputnumbermanager.AddInputValidator(InputSingleControl.CalcExp, ivExp);
        }

        void _rho_ForceGetValue(ExecuteIntegerDelegate del)
        {
            PrimesBigInteger value = inputnumbermanager.GetValue();
            if (value != null && del != null) del(value);
        }

        private void Factorize(PrimesBigInteger value)
        {
            ClearInfoPanel();
            string inputvalue = m_Integer.ToString();
            if (lblInput.ToolTip == null)
            {
                lblInput.ToolTip = new ToolTip();
            }
            (lblInput.ToolTip as ToolTip).Content = StringFormat.FormatString(inputvalue, 80);

            if (inputvalue.Length > 7) inputvalue = inputvalue.Substring(0, 6) + "...";
            System.Windows.Documents.Underline ul = new Underline(); 

            if (CurrentFactorizer == _bruteforce)
            {
                m_lblInput_BruteForce = inputvalue;
            }
            else
            {
                m_lblInput_QS = inputvalue;
            }

            UpdateMessages();

            CurrentFactorizer.Execute(value);
        }

        public void OnFactorizationStart()
        {
            if (CurrentFactorizer == _bruteforce)
            {
                m_gbFactorizationInfo_BruteForce = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultrunning;
            }
            else
            {
                m_gbFactorizationInfo_QS = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultrunning;
            }

            UpdateMessages();

            inputnumbermanager.LockControls();
        }

        public void OnFactorizationStop()
        {
            if (m_Integer != null)
            {
                if (CurrentFactorizer == _bruteforce)
                {
                    m_gbFactorizationInfo_BruteForce = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultfinishedtime, TimeString(_bruteforce.Needs));
                }
                else
                {
                    m_gbFactorizationInfo_QS = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultfinished;
                }

                UpdateMessages();
            }

            inputnumbermanager.UnLockControls();
        }

        public void OnFactorizationCancel()
        {
            CurrentFactorizer.CancelFactorization();

            if (CurrentFactorizer == _bruteforce)
            {
                m_gbFactorizationInfo_BruteForce = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultabortedtime, TimeString(_bruteforce.Needs));
            }
            else
            {
                m_gbFactorizationInfo_QS = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultaborted;
            }

            UpdateMessages();
        }

        #region IPrimeUserControl Members

        public void Dispose()
        {
            if (_bruteforce.isRunning)
            {
                _bruteforce.CancelExecute();
                _bruteforce.CancelFactorization();
            }
            if (_rho.isRunning)
            {
                _rho.CancelExecute();
                _rho.CancelFactorization();
            }
            if (_qs.isRunning)
            {
                _qs.CancelExecute();
                _qs.CancelFactorization();
            }
        }

        #endregion

        public void OnFoundFactor(object o)
        {
            if (o is GmpFactorTree)
            {
                m_factors_BruteForce = OnFoundFactor_FactorTree(o as GmpFactorTree);
            }
            else if (o is IEnumerator<KeyValuePair<PrimesBigInteger, PrimesBigInteger>>)
            {
                m_factors_QS = OnFoundFactor_Enumerator(o as IEnumerator<KeyValuePair<PrimesBigInteger, PrimesBigInteger>>);
            }

            UpdateMessages();
        }

        public string OnFoundFactor_FactorTree(GmpFactorTree ft)
        {
            StringBuilder sbFactors = new StringBuilder();

            if (ft != null)
            {
                sbFactors.Append(" = ");

                foreach (string factor in ft.Factors)
                {
                    sbFactors.Append(factor.ToString());
                    PrimesBigInteger factorcount = ft.GetFactorCount(factor);
                    if (factorcount>1)
                        sbFactors.AppendFormat("^{0}", factorcount.ToString());
                    sbFactors.Append(" * ");
                }

                if (ft.Remainder != null)
                    sbFactors.Append(ft.Remainder.ToString());
                else
                    sbFactors = sbFactors.Remove(sbFactors.Length - 2, 2);
            }

            return sbFactors.ToString();
        }

        public string OnFoundFactor_Enumerator(IEnumerator<KeyValuePair<PrimesBigInteger, PrimesBigInteger>> _enum)
        {
            StringBuilder sbFactors = new StringBuilder();

            sbFactors.Append(" = ");

            while (_enum.MoveNext())
            {
                KeyValuePair<PrimesBigInteger, PrimesBigInteger> current = _enum.Current;
                sbFactors.Append(current.Key.ToString());
                PrimesBigInteger factorcount = current.Value;
                if (factorcount > 1)
                    sbFactors.AppendFormat("^{0}", factorcount.ToString());
                sbFactors.Append(" * ");
            }

            sbFactors = sbFactors.Remove(sbFactors.Length - 2, 2);

            return sbFactors.ToString();
        }

        private void InputSingleNumberControl_Execute(PrimesBigInteger integer)
        {
            m_Integer = integer;
            Factorize(integer);
        }
        
        private void InputSingleNumberControl_Cancel()
        {
            OnFactorizationCancel();
        }

        private string TimeString(TimeSpan t)
        {
            StringBuilder result = new StringBuilder();

            if (t.Hours > 0)
                result.AppendFormat("{0} {1}, ", t.Hours, (t.Hours == 1) ? Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timehour : Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timehours);

            if (t.Hours > 0 || t.Minutes > 0)
                result.AppendFormat("{0} {1}, ", t.Minutes, (t.Minutes == 1) ? Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timeminute : Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timeminutes);

            result.AppendFormat("{0}.{1:D3} {2}", t.Seconds, t.Milliseconds, Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_timeseconds);

            return result.ToString();
        }

        void _bruteforce_Cancel()
        {
            m_gbFactorizationInfo_BruteForce = string.Format(Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultabortedtime, TimeString(_bruteforce.Needs));
        }

        void _qs_Cancel()
        {
            m_gbFactorizationInfo_QS = Primes.Resources.lang.WpfControls.Factorization.Factorization.fac_resultaborted;
        }

        void _rho_Cancel()
        {
        }

        private void lblInput_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (lblInput.ContextMenu != null)
                lblInput.ContextMenu.IsOpen = true;
        }

        private void MenuItemCopyInputClick(object sender, RoutedEventArgs e)
        {
            if (m_Integer != null)
                Clipboard.SetText(m_Integer.ToString(), TextDataFormat.Text);
        }

        private void lblInputMouseMove(object sender, MouseEventArgs e)
        {
        }

        private void lblInputMouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void UpdateMessages()
        {
            switch (KindOfFactorization)
            {
                case KOF.BruteForce:
                    ControlHandler.SetElementContent(lblFactors, m_factors_BruteForce);
                    ControlHandler.SetPropertyValue(gbFactorizationInfo, "Header", m_gbFactorizationInfo_BruteForce);
                    ControlHandler.SetElementContent(lblInput, m_lblInput_BruteForce);
                    break;

                case KOF.QS:
                    ControlHandler.SetElementContent(lblFactors, m_factors_QS);
                    ControlHandler.SetPropertyValue(gbFactorizationInfo, "Header", m_gbFactorizationInfo_QS);
                    ControlHandler.SetElementContent(lblInput, m_lblInput_QS);
                    break;
            }
        }

        private KOF KindOfFactorization
        {
            get
            {
                object selecteditem = ControlHandler.GetPropertyValue(tbctrl, "SelectedItem");
                if (selecteditem == tabItemBruteForce)
                    return KOF.BruteForce;
                else if (selecteditem == tabItemRho)
                    return KOF.Rho;
                else if (selecteditem == tabItemQS)
                    return KOF.QS;
                return KOF.None;
            }
        }

        private enum KOF { None, BruteForce, Rho, QS }
        
        private void tbctrl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (KindOfFactorization)
            {
                case KOF.BruteForce:
                    inputnumbermanager.SetValueValidator(InputSingleControl.Value, _bruteforce.Validator);
                    break;
                case KOF.QS:
                    inputnumbermanager.SetValueValidator(InputSingleControl.Value, _qs.Validator);
                    break;
            }

            UpdateMessages();
            
            inputnumbermanager.GetValue();

            if (CurrentFactorizer.isRunning)
            {
                inputnumbermanager.LockControls();
                inputnumbermanager.CancelButtonIsEnabled = true;
                inputnumbermanager.ExecuteButtonIsEnabled = false;
            }
            else if (inputnumbermanager.GetValue() == null)
            {
                inputnumbermanager.UnLockControls();
                inputnumbermanager.CancelButtonIsEnabled = false;
                inputnumbermanager.ExecuteButtonIsEnabled = false;
            }
            else
            {
                inputnumbermanager.UnLockControls();
                inputnumbermanager.CancelButtonIsEnabled = false;
                inputnumbermanager.ExecuteButtonIsEnabled = true;
            }
        }

        private IFactorizer CurrentFactorizer
        {
            get
            {
                switch (KindOfFactorization)
                {
                    case KOF.BruteForce: return _bruteforce;
                    case KOF.Rho: return _rho;
                    case KOF.QS: return _qs;
                    default: return null;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(lblFactors.Content.ToString());
            }
            catch (Exception ex)
            {
            }
        }

        void tbFactorizationCopyContextMenuCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(m_TbFactorizationCopy.Text))
            {
                Clipboard.SetText(m_TbFactorizationCopy.Text);
            }
        }

        private void DockPanelFactors_MouseEnter(object sender, MouseEventArgs e)
        {
            if (lblFactors.Content != null && m_Integer != null)
            {
                m_TbFactorizationCopy.Text = m_Integer.ToString() + lblFactors.Content.ToString();
                pnInfo.Children.Clear();
                pnInfo.Children.Add(m_TbFactorizationCopy);
            }
        }

        void m_TbFactorizationCopy_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetInfoPanel();
        }

        void m_TbFactorizationCopy_MouseMove(object sender, MouseEventArgs e)
        {
            m_TbFactorizationCopy.SelectAll();
        }

        private void pnInfo_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetInfoPanel();
        }

        private void ResetInfoPanel()
        {
            if (!m_TbFactorizationCopy.ContextMenu.IsOpen)
            {
                pnInfo.Children.Clear();
                pnInfo.Children.Add(dpInfo);
            }
        }

        private void ClearInfoPanel()
        {
            ResetInfoPanel();
            lblInput.Content = "";
            lblFactors.Content = "";
        }

        #region IPrimeUserControl Members

        public void SetTab(int i)
        {
            if (i >= 0 && i < tbctrl.Items.Count)
                tbctrl.SelectedIndex = i;

            tbctrl_SelectionChanged(null, null);
        }

        #endregion

        #region IPrimeUserControl Members

        public event VoidDelegate Execute;

        public void FireExecuteEvent()
        {
            if (Execute != null) Execute();
        }

        public event VoidDelegate Stop;

        public void FireStopEvent()
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

        private void HelpTabItem_HelpButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == tabItemBruteForce)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Factorization_BruteForce);
            }
            else if (sender == tabItemRho)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Factorization_Rho);
            }
            else if (sender == tabItemQS)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Factorization_QS);
            }
        }
    }
}
