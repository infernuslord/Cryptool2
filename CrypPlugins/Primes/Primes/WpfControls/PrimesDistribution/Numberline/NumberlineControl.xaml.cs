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
using Primes.WpfControls.Components;
using Primes.Bignum;
using Primes.Library;
using System.Threading;
using System.Diagnostics;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;
using Primes.Library.Function;

namespace Primes.WpfControls.PrimesDistribution.Numberline
{
	/// <summary>
	/// Interaction logic for NumberlineControl.xaml
	/// </summary>
	public partial class NumberlineControl : UserControl, IPrimeDistribution
	{
        private const int PADDINGLEFT = 10;
        private const int PADDINGRIGHT = 10;

		private static readonly PrimesBigInteger MAX = PrimesBigInteger.ValueOf(1000000000);
		private Thread m_ScrollThread;
		private IList<NumberButton> m_Buttons;
		private IDictionary<PrimesBigInteger, NumberButton> m_ButtonsDict;
		private IList<NumberButton> markedNumbers;
		private Thread m_FactorizeThread;
		private Thread m_GoldbachThread;
		private Thread m_CountPrimesThread;

		private INTFunction m_EulerPhi;
		private INTFunction m_Tau;
		private INTFunction m_Rho;
		private INTFunction m_DivSum;
		private double m_UnitSize;
		private double m_ButtonScale;
		private PrimesBigInteger m_Start;
		private PrimesBigInteger m_ActualNumber;
		private bool m_Initialized;

		public NumberlineControl()
		{
			InitializeComponent();
			m_Buttons = new List<NumberButton>();
			markedNumbers = new List<NumberButton>();
			m_ButtonsDict = new Dictionary<PrimesBigInteger, NumberButton>();
			m_ButtonScale = 45.0;
			m_Start = PrimesBigInteger.Two;
			iscTo.Execute += new ExecuteSingleDelegate(iscTo_Execute);
			iscTo.KeyDown += new ExecuteSingleDelegate(iscTo_Execute);
			iscFrom.Execute += new ExecuteSingleDelegate(iscFrom_Execute);
			iscFrom.KeyDown += new ExecuteSingleDelegate(iscFrom_Execute);
			iscFrom.OnInfoError += new MessageDelegate(iscFrom_OnInfoError);
			iscFrom.NoMargin = true;
			iscTo.NoMargin = true;

			iscFrom.SetBorderBrush(Brushes.Blue);
			iscTo.SetBorderBrush(Brushes.Violet);

			iscTo.OnInfoError += new MessageDelegate(iscFrom_OnInfoError);
			iscTo.KeyDownNoValidation += new MessageDelegate(iscTo_KeyDownNoValidation);
			iscFrom.KeyDownNoValidation += new MessageDelegate(iscTo_KeyDownNoValidation);
			IValidator<PrimesBigInteger> validatefrom = new BigIntegerMinValueValidator(null, PrimesBigInteger.Two);
			InputValidator<PrimesBigInteger> inputvalidatefrom = new InputValidator<PrimesBigInteger>();
			inputvalidatefrom.DefaultValue = "2";
			inputvalidatefrom.Validator = validatefrom;
			iscFrom.AddInputValidator(InputSingleControl.Free, inputvalidatefrom);
			SetInputValidator();
			this.FactorizationDone += new VoidDelegate(NumberlineControl_FactorizationDone);
			this.GoldbachDone += new VoidDelegate(NumberlineControl_GoldbachDone);

			m_EulerPhi = new EulerPhi(logEulerPhi, lblCalcEulerPhiInfo);
			m_EulerPhi.OnStop += new VoidDelegate(m_EulerPhi_OnStop);

			m_Tau = new Tau(logTau, lblCalcTauInfo);
			m_Tau.OnStop += new VoidDelegate(m_Tau_OnStop);

			m_Rho = new Rho(logRho, lblCalcRhoInfo);
			m_Rho.OnStop += new VoidDelegate(m_Rho_OnStop);

			m_DivSum = new EulerPhiSum(logDivSum, lblCalcDividerSum);
			m_DivSum.OnStop += new VoidDelegate(m_DivSum_OnStop);
		}

		void iscFrom_OnInfoError(string message)
		{
			tbInfoError.Text = message;
		}

		private void SetInputValidator()
		{
			IValidator<PrimesBigInteger> validateto = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.ValueOf((int)m_ButtonScale + 1), MAX);
			InputValidator<PrimesBigInteger> inputvalidateto = new InputValidator<PrimesBigInteger>();
			inputvalidateto.DefaultValue = ((int)m_ButtonScale + 1).ToString();
			inputvalidateto.Validator = validateto;
			iscTo.AddInputValidator(InputSingleControl.Free, inputvalidateto);
		}

#region IPrimeUserControl Members

		public void Close()
		{
			CancelThreads();
		}

		private void CancelThreads()
		{
			CancelScrollThread();
			CancelFactorization();
			CancelGoldbach();
			CancelCountPrimes();
			//CancelEulerPhi();
			m_EulerPhi.Stop();
			m_Tau.Stop();
			m_Rho.Stop();
			m_DivSum.Stop();
			ControlHandler.SetButtonEnabled(btnCancelAll, false);
		}
#endregion


		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
		}

#region Properties

		private PrimesBigInteger ButtonScale
		{
			get
			{
				return PrimesBigInteger.ValueOf((int)m_ButtonScale);
			}
		}

		private PrimesBigInteger ButtonScaleMinusOne
		{
			get
			{
				return PrimesBigInteger.ValueOf((int)m_ButtonScale - 1);
			}
		}

#endregion

#region Drawing

		private void DrawButtons()
		{
			double x1 = 0;
			double x2 = 8;

			if (m_Buttons.Count > 0)
			{
				x1 = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", new object[] { m_Buttons[0] });
				x2 = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", new object[] { m_Buttons[1] });
			}

			UIElementCollection children = ControlHandler.GetPropertyValue(PaintArea, "Children") as UIElementCollection;
			ControlHandler.ExecuteMethod(children, "Clear");
			m_Buttons.Clear();
			m_ButtonsDict.Clear();

            double width = (double)ControlHandler.GetPropertyValue(LineArea, "ActualWidth");
            double len = width - (PADDINGLEFT + PADDINGRIGHT);
            if (len < 0) len = 100;
            m_UnitSize = len / (m_ButtonScale - 1);

            ControlHandler.ExecuteMethod(LineArea, "SetLeft", new object[] {numberline, PADDINGLEFT});
            ControlHandler.SetPropertyValue(numberline, "Width", len);

            double y = (double)ControlHandler.ExecuteMethod(LineArea, "GetTop", new object[] { numberline }) + numberline.Height / 2;

            int buttonsize = Math.Max(3, Math.Min(20, (int)(len / m_ButtonScale)));

			for (int i = 0; i < m_ButtonScale; i++)
			{
                double x = i * m_UnitSize + PADDINGLEFT;
                DrawNumberButton(m_Start.Add(PrimesBigInteger.ValueOf(i)), x, y, buttonsize, buttonsize);
			}

			if (m_Buttons != null && m_Buttons.Count > 0)
			{
				NumberButton nb = m_Buttons[m_Buttons.Count - 1];
				Canvas.SetLeft(pToNumber, Canvas.GetLeft(nb) - 6);
				lblCountPoints.Text = m_Buttons.Count.ToString();
				SetEdgeButtonColor();
			}
		}

		//private bool CanAdd
		//{
		//  get
		//  {
		//    double x1 = 0;
		//    double x2 = 12;
		//    if (m_Buttons != null)
		//    {
		//      if (m_Buttons.Count > 0)
		//      {
		//        x1 = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", new object[] { m_Buttons[0] });
		//        x2 = (double)ControlHandler.ExecuteMethod(PaintArea, "GetLeft", new object[] { m_Buttons[1] });
		//      }
		//    }
		//    return x1 < x2 - 12;
		//  }
		//}

		private void DrawNumberButton(PrimesBigInteger value, double x, double y, double width, double height)
		{
			NumberButton nb = ControlHandler.CreateObject(typeof(NumberButton)) as NumberButton;
			nb.MouseEnter += new MouseEventHandler(nb_MouseMove);
			//nb.MouseLeave += new MouseEventHandler(nb_MouseLeave);
			nb.Cursor = Cursors.Hand;
			ControlHandler.SetPropertyValue(nb, "NumberButtonStyle", NumberButtonStyle.Ellipse.ToString());
			ControlHandler.SetPropertyValue(nb, "BINumber", value);
			ControlHandler.SetPropertyValue(nb, "Width", width);
			ControlHandler.SetPropertyValue(nb, "Height", height);
			ControlHandler.SetPropertyValue(nb, "BorderBrush", Brushes.Black);
			SetButtonColor(nb);

			ControlHandler.ExecuteMethod(PaintArea, "SetTop", new object[] { nb, y - height/2 });
			ControlHandler.ExecuteMethod(PaintArea, "SetLeft", new object[] { nb, x - width/2 });
			ControlHandler.AddChild(nb, PaintArea);
			m_Buttons.Add(nb);
			m_ButtonsDict.Add(value, nb);
		}

		private void SetButtonColor(NumberButton btn)
		{
			PrimesBigInteger number = ControlHandler.GetPropertyValue(btn, "BINumber") as PrimesBigInteger;

			if (number.IsProbablePrime(10))
				ControlHandler.SetPropertyValue(btn, "Background", Brushes.LightBlue);
			else
				ControlHandler.SetPropertyValue(btn, "Background", Brushes.Black);
		}

		private void SetEdgeButtonColor()
		{
			if (m_Buttons != null && m_Buttons.Count > 0)
			{
				NumberButton nbFirst = m_Buttons[0];
				NumberButton nbLast = m_Buttons[m_Buttons.Count - 1];
				ControlHandler.SetPropertyValue(nbFirst, "Background", Brushes.Blue);
				ControlHandler.SetPropertyValue(nbLast, "Background", Brushes.Violet);
			}
		}

#endregion

#region scrolling

		private void ButtonScrollLeftClick(object sender, MouseButtonEventArgs e)
		{
			foreach (NumberButton btn in PaintArea.Children)
			{
				if (btn.BINumber.CompareTo(PrimesBigInteger.Two) <= 0) break;
				btn.BINumber = btn.BINumber.Subtract(PrimesBigInteger.One);
				SetButtonColor(btn);
			}
		}

		private void ScrollRight(object obj)
		{
			if (obj.GetType() == typeof(PrimesBigInteger))
			{
				while (true)
				{
					DoAtomicScroll(obj as PrimesBigInteger);
				}
			}
			SetEdgeButtonColor();
		}

		private void DoAtomicScroll(PrimesBigInteger amount)
		{
			UIElementCollection children = ControlHandler.GetPropertyValue(PaintArea, "Children") as UIElementCollection;
			PrimesBigInteger max = null;
			int counter = 0;
			m_ButtonsDict.Clear();
			PrimesBigInteger _max = m_Start.Add(amount).Add(PrimesBigInteger.ValueOf(m_Buttons.Count - 1));
			if (amount.CompareTo(PrimesBigInteger.Zero) >= 0)
			{
				if (_max.CompareTo(MAX) > 0)
				{
					amount = amount.Subtract(_max.Subtract(MAX));
					_max = MAX;
				}
				//PrimesBigInteger _amount = MAX.Subtract(_max);
				//if(_amount.CompareTo(PrimesBigInteger.Zero)>0)
				//{
				//  amount = PrimesBigInteger.Min(_amount, amount);
				//}
			}

			foreach (NumberButton btn in m_Buttons)
			{
				PrimesBigInteger number = ControlHandler.GetPropertyValue(btn, "BINumber") as PrimesBigInteger;
				if (number.Add(amount).Subtract(PrimesBigInteger.ValueOf(counter)).CompareTo(PrimesBigInteger.Two) >= 0 && _max.CompareTo(MAX) <= 0)
				{
					number = number.Add(amount);
					ControlHandler.SetPropertyValue(btn, "BINumber", number);
					max = number;
				}
				if (!m_ButtonsDict.ContainsKey(number))
				{
					m_ButtonsDict.Add(number, btn);
				}
				SetButtonColor(btn);
				counter++;
			}

			if (max != null)
			{
				m_Start = max.Subtract(PrimesBigInteger.ValueOf((int)m_ButtonScale - 1));
				SetFromTo();
				SetCountPrimes();

			}
		}

		private void SetFromTo()
		{
			ControlHandler.SetPropertyValue(iscFrom, "FreeText", m_Start.ToString());
			ControlHandler.SetPropertyValue(iscTo, "FreeText", m_Start.Add(ButtonScaleMinusOne).ToString());
			ControlHandler.SetPropertyValue(lblInfoCountPrimesInterval, "Text", string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_numberofprimeinterval, m_Start.ToString(), m_Start.Add(ButtonScaleMinusOne).ToString()));
			SetEdgeButtonColor();
		}

		private void CancelScrollThread()
		{
			if (m_ScrollThread != null)
			{
				bool error = false;
				PrimesBigInteger last = null;
				foreach (NumberButton btn in m_Buttons)
				{
					if (last == null)
						last = btn.BINumber;
					else
					{
						if (last.CompareTo(btn.BINumber) >= 0)
						{
							error = true;
							break;
						}
						else
						{
							last = btn.BINumber;
						}
					}
				}
				PrimesBigInteger cmp = m_Buttons[0].BINumber.Subtract(m_Buttons[m_Buttons.Count - 1].BINumber);
				if (cmp.CompareTo(PrimesBigInteger.Zero) < 0) cmp = cmp.Multiply(PrimesBigInteger.NegativeOne);
				cmp = cmp.Add(PrimesBigInteger.One);
				error = cmp.CompareTo(PrimesBigInteger.ValueOf((int)m_ButtonScale)) != 0;
				if (error)
				{
					PrimesBigInteger value = m_Buttons[0].BINumber;
					for (int i = 0; i < m_Buttons.Count; i++)
					{
						m_Buttons[i].BINumber = value.Add(PrimesBigInteger.ValueOf(i));
					}
					DoAtomicScroll(PrimesBigInteger.One);

				}
				m_Start = m_Buttons[0].BINumber;
				SetFromTo();
				MarkNumberWithOutThreads(m_Start);
				m_ScrollThread.Abort();
				m_ScrollThread = null;
			}
		}
        
        private void btnScroll_MouseClick(object sender, RoutedEventArgs e)
        {
            int amount = 1;
            if (sender == btnScrollRight_Fast || sender == btnScrollLeft_Fast) amount = 10;
            if (sender == btnScrollLeft || sender == btnScrollLeft_Fast) amount *= -1;
            PrimesBigInteger _amount = PrimesBigInteger.ValueOf(amount);
            DoAtomicScroll(_amount);
        }

		private void btnScroll_MouseEnter(object sender, MouseEventArgs e)
		{
			int amount = 1;
			if (sender == btnScrollRight_Fast || sender == btnScrollLeft_Fast) amount = 10;
			if (sender == btnScrollLeft || sender == btnScrollLeft_Fast) amount *= -1;
			PrimesBigInteger _amount = PrimesBigInteger.ValueOf(amount);
			if (amount < 0)
			{
				while (m_Start.Add(_amount).CompareTo(PrimesBigInteger.Two) < 0)
				{
					_amount = _amount.Add(PrimesBigInteger.One);
				}
				amount = _amount.IntValue;
			}
			StartScrollThread(PrimesBigInteger.ValueOf(amount));
		}

		private void StartScrollThread(PrimesBigInteger value)
		{
			//DoAtomicScroll(PrimesBigInteger.ValueOf(1));
			m_ScrollThread = new Thread(new ParameterizedThreadStart(new ObjectParameterDelegate(ScrollRight)));
			m_ScrollThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			m_ScrollThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			m_ScrollThread.Start(value);
		}

		private void btnScrollRight_MouseLeave(object sender, MouseEventArgs e)
		{
			CancelScrollThread();
		}

		void iscFrom_Execute(PrimesBigInteger value)
		{
			EnableInput();
			iscTo_Execute(value.Add(ButtonScaleMinusOne));
			MarkNumberWithOutThreads(m_Start);
		}

		void iscTo_Execute(PrimesBigInteger value)
		{
			if (value.CompareTo(PrimesBigInteger.Two.Add(ButtonScaleMinusOne)) >= 0)
			{
				iscFrom.ResetMessages();
				iscTo.ResetMessages();
				EnableInput();
				PrimesBigInteger diff = value.Subtract(m_Start.Add(ButtonScaleMinusOne));
				DoAtomicScroll(diff);
				MarkNumberWithOutThreads(value);
			}
		}

		void iscTo_KeyDownNoValidation(string message)
		{
			DisableInput();
		}

		private void DisableInput()
		{
			pnlContent.IsEnabled = false;
			pnlContent.Opacity = 0.5;
			pnlScrollButtons.IsEnabled = false;
			pnlScrollButtons.Opacity = 0.5;
		}

		private void EnableInput()
		{
			pnlContent.IsEnabled = true;
			pnlContent.Opacity = 1.0;
			pnlScrollButtons.IsEnabled = true;
			pnlScrollButtons.Opacity = 1.0;
			tbInfoError.Text = "";
		}

#endregion

#region Scaling
		private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (m_Initialized && e.NewValue != m_ButtonScale)
				StartScaleThread((int)e.NewValue);
		}

		private void StartScaleThread(int value)
		{
			ScaleNumberline(value);

			SetCountPrimes();
			if (m_ActualNumber != null )
			{
				SetPointerActualNumber(m_ActualNumber);
			}
			//Thread t = new Thread(new ParameterizedThreadStart(new ObjectParameterDelegate(ScaleNumberline)));
			//t.Start(value);
		}

		object scalelockobject = new object();

		private void ScaleNumberline(object value)
		{
			lock (scalelockobject)
			{
				if (value != null && value.GetType() == typeof(int))
				{
					m_ButtonScale = (double)(int)value;
					DrawButtons();
					SetFromTo();
					SetInputValidator();
				}
			}
		}

		private void btnZoomOut_Click(object sender, RoutedEventArgs e)
		{
			if ((m_ButtonScale - 1) >= 10)
				StartScaleThread((int)(m_ButtonScale - 1));
			slider.Value = m_ButtonScale;
		}

		private void btnZoomIn_Click(object sender, RoutedEventArgs e)
		{
			if ((m_ButtonScale + 1) <= slider.Maximum)
				StartScaleThread((int)(m_ButtonScale + 1));
			slider.Value = m_ButtonScale;
		}

#endregion

		void nb_MouseMove(object sender, MouseEventArgs e)
		{
			if (sender.GetType() == typeof(NumberButton))
			{
				PrimesBigInteger value = (sender as NumberButton).BINumber;
				MarkNumber(value);
			}
		}

		void nb_MouseLeave(object sender, MouseEventArgs e)
		{
			if(sender!=null && sender.GetType()==typeof(NumberButton))
				SetButtonColor(sender as NumberButton);
			UnmarkAllNumbers();
			//CancelThreads();
		}

#region Show Number Information

		private void MarkNumber(PrimesBigInteger value)
		{
			UnmarkAllNumbers();
			MarkNumberWithOutThreads(value);
			Factorize(value);
			CalculateGoldbach(value);
			CountPrimes(value);
			m_EulerPhi.Start(value);
			m_Tau.Start(value);
			m_Rho.Start(value);
			m_DivSum.Start(value);
			ControlHandler.SetButtonEnabled(btnCancelAll, true);
		}

		private void MarkNumberWithOutThreads(PrimesBigInteger value)
		{
			NumberButton sender = null;
			try { sender = m_ButtonsDict[value]; }
			catch { }
			if(sender!=null)
				ControlHandler.SetPropertyValue((sender as NumberButton), "Background", Brushes.Yellow);
			m_ActualNumber = value;
			HideInfoPanels();
			SetPointerActualNumber(value);
			SetInfoActualNumber(value);
			SetNeighborPrimes(value);
			SetTwinPrimes(value);
			SetNeighborTwinPrimes(value);
			SetSextupletPrimes(value);
			SetQuadrupletPrimes(value);
			SetEdgeButtonColor();
		}

		private void SetPointerActualNumber(PrimesBigInteger value)
		{
			if (m_ButtonsDict.ContainsKey(value))
			{
				NumberButton btn = m_ButtonsDict[value];
				double left = Canvas.GetLeft(btn) + btn.Width/2 - 5;
				Canvas.SetLeft(pActualNumber, left);
			}
		}

		private void HideInfoPanels()
		{
			//lblCalcGoldbachInfo.Text = "";
			pnlQuadrupletPrimes.Visibility = Visibility.Collapsed;
			lblTwinPrimes.Visibility = Visibility.Collapsed;
			pnlSixTupletPrimes.Visibility = Visibility.Collapsed;
			lblCalcDividerSum.Visibility = Visibility.Collapsed;
			lblCalcRhoInfo.Visibility = Visibility.Collapsed;
			lblCalcTauInfo.Visibility = Visibility.Collapsed;
			lblCalcEulerPhiInfo.Visibility = Visibility.Collapsed;
		}

		private void SetInfoActualNumber(PrimesBigInteger value)
		{
			setActualNumberText(value.ToString("D"));
			string info = string.Empty;
			if (value.IsProbablePrime(10))
			{
				lblActualNumber.Foreground = Brushes.Red;
				info = Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_isprime;
			}
			else
			{
				lblActualNumber.Foreground = Brushes.Black;
			}
			lblActualNumberInfo.Text = info;
		}

		private void setActualNumberText(string value)
		{
			lblActualNumber.Text = value;
			textActualNumber.Text = value;
		}

		private void SetNeighborPrimes(PrimesBigInteger value)
		{
			lblNextPrime.Text = value.NextProbablePrime().ToString("D");
			lblPriorPrime.Text = value.PriorProbablePrime(true).ToString("D");
		}

		private void SetCountPrimes()
		{
			ControlHandler.SetPropertyValue(lblCountPrimesPi, "Text", string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_countprimespin, CountPrimesPi.ToString()));
			ControlHandler.SetPropertyValue(lblCountPrimesGauss, "Text", string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_countprimesgauss, CountPrimesGauss.ToString("N")));
			//ControlHandler.SetPropertyValue(lblCountPrimesPi, "Text", CountPrimesPi.ToString());
			//ControlHandler.SetPropertyValue(lblCountPrimesGauss, "Text", CountPrimesGauss.ToString("N"));
		}

		private int CountPrimesPi
		{
			get
			{
				int result = 0;
				foreach (PrimesBigInteger key in m_ButtonsDict.Keys)
				{
					result += (key.IsPrime(10)) ? 1 : 0;
				}
				return result;
			}
		}

		private double CountPrimesGauss
		{
			get
			{
				double a = double.Parse(m_Buttons[0].BINumber.ToString());
				double b = double.Parse(m_Buttons[m_Buttons.Count - 1].BINumber.ToString());
				double result = (b / (Math.Log(b))) - (a / (Math.Log(a)));
				return result;
			}
		}

		private void SetTwinPrimes(PrimesBigInteger value)
		{
			PrimesBigInteger twin = null;
			if (value.IsTwinPrime(ref twin))
			{
                //if (!m_ButtonsDict.ContainsKey(twin))
                //{
                //    if (value.CompareTo(twin) < 0)
                //        DoAtomicScroll(PrimesBigInteger.Two);
                //    else
                //        DoAtomicScroll(PrimesBigInteger.ValueOf(-2));
                //}
				if (m_ButtonsDict.ContainsKey(value) && m_ButtonsDict.ContainsKey(twin))
				{
					MarkNumber(m_ButtonsDict[value]);
					MarkNumber(m_ButtonsDict[twin]);
					lblTwinPrimes.Text = string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_istwinprime, new object[] { PrimesBigInteger.Min(value, twin).ToString("D"), PrimesBigInteger.Max(value, twin).ToString("D") });
					lblTwinPrimes.Visibility = Visibility.Visible;
				}
			}
		}

		private void SetNeighborTwinPrimes(PrimesBigInteger value)
		{
			PrimesBigInteger a = null;
			PrimesBigInteger b = null;

            string text="";

			if (value.PriorTwinPrime(ref a, ref b))
            {
                text += string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_priortwinprime, PrimesBigInteger.Min(a, b), PrimesBigInteger.Max(a, b) );
                text += " ";
                //lblPriorTwinPrime.Text = string.Format("({0},{1})", new object[] { PrimesBigInteger.Min(a, b).ToString("D"), PrimesBigInteger.Max(a, b).ToString("D") });
			}
			else
			{
                //lblPriorTwinPrime.Text = "-";
			}

			if (value.NextTwinPrime(ref a, ref b))
            {
                text += string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_nexttwinprime, PrimesBigInteger.Min(a, b), PrimesBigInteger.Max(a, b) );
                //lblNextTwinPrime.Text = string.Format("({0},{1})", new object[] { PrimesBigInteger.Min(a, b).ToString("D"), PrimesBigInteger.Max(a, b).ToString("D") });
			}

            lblTwinPrimes2.Text = text;
		}

		private void SetQuadrupletPrimes(PrimesBigInteger value)
		{
			PrimesBigInteger first = null;
			PrimesBigInteger second = null;
			PrimesBigInteger third = null;
			PrimesBigInteger fourth = null;

			bool scrollup = false;
			bool scrolldown = false;

			if (GetQuadrupletPrimes(value, ref first, ref second, ref third, ref fourth, ref scrollup, ref scrolldown))
			{
				if (scrollup) DoAtomicScroll(PrimesBigInteger.Six);
				if (scrolldown) DoAtomicScroll(PrimesBigInteger.Six.Multiply(PrimesBigInteger.NegativeOne));
				if( m_ButtonsDict.ContainsKey(first) ) MarkNumber(m_ButtonsDict[first]);
                if (m_ButtonsDict.ContainsKey(second)) MarkNumber(m_ButtonsDict[second]);
                if (m_ButtonsDict.ContainsKey(third)) MarkNumber(m_ButtonsDict[third]);
                if (m_ButtonsDict.ContainsKey(fourth)) MarkNumber(m_ButtonsDict[fourth]);
				lblQuadrupletPrimes.Text =
					string.Format(
							Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_isquadtrupletprime,
							new object[] { first, second, third, fourth });
				pnlQuadrupletPrimes.Visibility = Visibility.Visible;
			}
		}

		private bool GetQuadrupletPrimes(
				PrimesBigInteger value,
				ref PrimesBigInteger first,
				ref PrimesBigInteger second,
				ref PrimesBigInteger third,
				ref PrimesBigInteger fourth,
				ref bool scrollup,
				ref bool scrolldown)
		{
			bool result = false;
			PrimesBigInteger twin = null;
			if (value.IsTwinPrime(ref twin))
			{
				PrimesBigInteger candidate = PrimesBigInteger.Max(value, twin).Add(PrimesBigInteger.ValueOf(4));
				PrimesBigInteger quadruplet2 = null;
				if (candidate.IsTwinPrime(ref quadruplet2))
				{
					PrimesBigInteger tmp = PrimesBigInteger.Max(candidate, quadruplet2);

					if (!m_ButtonsDict.ContainsKey(tmp))
						scrollup = true;
				}
				else
				{
					candidate = PrimesBigInteger.Min(value, twin).Subtract(PrimesBigInteger.ValueOf(4));
					if (candidate.IsTwinPrime(ref quadruplet2))
					{
						PrimesBigInteger tmp = PrimesBigInteger.Min(candidate, quadruplet2);

						if (!m_ButtonsDict.ContainsKey(tmp))
							scrolldown = true;
					}
				}

				if (quadruplet2 != null)
				{
					result = true;
					List<PrimesBigInteger> l = new List<PrimesBigInteger>();
					l.Add(value);
					l.Add(twin);
					l.Add(candidate);
					l.Add(quadruplet2);
					l.Sort(new Comparison<PrimesBigInteger>(SortCompareGmpBigIntegers));
					first = l[0];
					second = l[1];
					third = l[2];
					fourth = l[3];
				}
			}
			return result;
		}

		private void SetSextupletPrimes(PrimesBigInteger value)
		{
			PrimesBigInteger first = null;
			PrimesBigInteger second = null;
			PrimesBigInteger third = null;
			PrimesBigInteger fourth = null;
			PrimesBigInteger fifth = null;
			PrimesBigInteger sixth = null;

			bool scrollup = false;
			bool scrolldown = false;
			if (value.IsProbablePrime(10))
			{
				if (!GetQuadrupletPrimes(value, ref second, ref third, ref fourth, ref fifth, ref scrollup, ref scrolldown))
				{
					value = value.Subtract(PrimesBigInteger.Four);
					if (!GetQuadrupletPrimes(value, ref second, ref third, ref fourth, ref fifth, ref scrollup, ref scrolldown))
					{
						value = value.Add(PrimesBigInteger.Eight);
					}
				}
			}
			if (GetQuadrupletPrimes(value, ref second, ref third, ref fourth, ref fifth, ref scrollup, ref scrolldown))
			{
				first = second.Subtract(m_Start);
				sixth = fifth.Add(PrimesBigInteger.Four);
				if (first.IsPrime(10) && sixth.IsPrime(10))
				{
					if (scrollup) DoAtomicScroll(sixth.Subtract(first));
					if (scrolldown)
					{
						PrimesBigInteger diff = value.Subtract(first);
						DoAtomicScroll(diff.Multiply(PrimesBigInteger.NegativeOne));
					}
					try
					{
						MarkNumber(m_ButtonsDict[first]);
						MarkNumber(m_ButtonsDict[second]);
						MarkNumber(m_ButtonsDict[third]);
						MarkNumber(m_ButtonsDict[fourth]);
						MarkNumber(m_ButtonsDict[fifth]);
						MarkNumber(m_ButtonsDict[sixth]);
						lblSixTupletPrimes.Text =
							string.Format(
									Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_issixtupletprime,
									new object[] { first.ToString("D"), second.ToString("D"), third.ToString("D"), fourth.ToString("D"), fifth.ToString("D"), sixth.ToString("D") });
						pnlSixTupletPrimes.Visibility = Visibility.Visible;
					}
					catch { }
				}
			}
		}

#endregion

#region Factorization

		private event VoidDelegate FactorizationDone;

		void NumberlineControl_FactorizationDone()
		{
			ControlHandler.SetPropertyValue(lblCalcFactorizationInfo, "Text", "(fertig)");
			ControlHandler.SetPropertyValue(lblCalcFactorization, "Text", "");

			CancelFactorization();
		}

		private void Factorize(PrimesBigInteger value)
		{
			//DoFactorize(value);
			CancelFactorization();
			m_FactorizeThread = new Thread(new ParameterizedThreadStart(new ObjectParameterDelegate(DoFactorize)));
			m_FactorizeThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			m_FactorizeThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			m_FactorizeThread.Priority = ThreadPriority.Normal;
			m_FactorizeThread.Start(value);
		}

		private void DoFactorize(object o)
		{
			if (o != null && o.GetType() == typeof(PrimesBigInteger))
			{
				ControlHandler.SetPropertyValue(lblCalcFactorizationInfo, "Text", Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_factorizationcalculating);
				ControlHandler.SetPropertyValue(lblFactors, "Visibility", Visibility.Visible);
				PrimesBigInteger value = o as PrimesBigInteger;
				ControlHandler.SetPropertyValue(lblFactors, "Text", " "+value.ToString() + " = ");
				PrimesBigInteger factor = PrimesBigInteger.Two;
				if (!value.IsProbablePrime(10))
				{
					while (!value.IsProbablePrime(10) && !value.Equals(PrimesBigInteger.One))
					{
						string text = ControlHandler.GetPropertyValue(lblFactors, "Text") as string;
						while (!value.Mod(factor).Equals(PrimesBigInteger.Zero))
						{
							factor = factor.NextProbablePrime();
						}

						int count = 0;

						while (value.Mod(factor).Equals(PrimesBigInteger.Zero) && !value.Equals(PrimesBigInteger.One))
						{
							value = value.Divide(factor);
							count++;
							string factors = string.Empty;
							if (count == 1)
								factors = text + factor.ToString() + " * ";
							else
								factors = text + factor.ToString() + "^" + count.ToString() + " * ";
							ControlHandler.SetPropertyValue(lblFactors, "Text", factors);
						}
					}
					string txt = (ControlHandler.GetPropertyValue(lblFactors, "Text") as string);
					if (!value.Equals(PrimesBigInteger.One))
					{
						txt += value.ToString();
					}
					else
					{
						txt = txt.Trim();
						txt = txt.Substring(0, txt.Length - 2);
					}
					ControlHandler.SetPropertyValue(lblFactors, "Text", txt);
				}
				else
				{
					ControlHandler.SetPropertyValue(lblFactors, "Text", value.ToString());
				}
				if (FactorizationDone != null) FactorizationDone();
			}
		}

		private void DoFactorizeInfo()
		{
			ControlHandler.SetPropertyValue(lblCalcFactorizationInfo, "Visibility", Visibility.Visible);
			while (m_FactorizeThread != null && m_FactorizeThread.ThreadState == System.Threading.ThreadState.Running)
			{
				string text = ControlHandler.GetPropertyValue(lblCalcFactorization, "Text") as string;
				text += ".";

				if (text.Length > 3)
					text = "";
				ControlHandler.SetPropertyValue(lblCalcFactorization, "Text", text);
				Thread.Sleep(500);
			}
		}

		private void CancelFactorization()
		{
			if (m_FactorizeThread != null)
			{
				m_FactorizeThread.Abort();
				m_FactorizeThread = null;
			}
			setCancelAllEnabled();
			ControlHandler.SetPropertyValue(lblCalcFactorizationInfo, "Visibility", Visibility.Hidden);
		}

#endregion

#region Goldbach

		private event VoidDelegate GoldbachDone;

		void NumberlineControl_GoldbachDone()
		{
			CancelGoldbach();
		}

		private void CalculateGoldbach(PrimesBigInteger value)
		{
			CancelGoldbach();
			logGoldbach.Clear();
			logGoldbach.Columns = 1;
			m_GoldbachThread = new Thread(new ParameterizedThreadStart(new ObjectParameterDelegate(DoCalculateGoldbach)));
			m_GoldbachThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			m_GoldbachThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			m_GoldbachThread.Priority = ThreadPriority.Normal;
			m_GoldbachThread.Start(value);
			//DoCalculateGoldbach(value);
		}

		private void DoCalculateGoldbach(object o)
		{
			if (o == null || o.GetType() != typeof(PrimesBigInteger)) return;

			PrimesBigInteger value = o as PrimesBigInteger;

			if (value.Mod(PrimesBigInteger.Two).Equals(PrimesBigInteger.One)) // value is odd
			{
				//ControlHandler.SetPropertyValue(lblCalcGoldbachInfo, "Visibility", Visibility.Visible);
				//logGoldbach.Info(value.ToString() + Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_isodd);
				ControlHandler.SetPropertyValue(lblGoldbachInfoCalc, "Text", string.Format(Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_isodd, value));
			}
			else if (value.Equals(PrimesBigInteger.Two))  // value = 2
			{
				//ControlHandler.SetPropertyValue(lblCalcGoldbachInfo, "Visibility", Visibility.Visible);
				//logGoldbach.Info(value.ToString() + Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_isprime);
				ControlHandler.SetPropertyValue(lblGoldbachInfoCalc, "Text", Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_istwo);
			}
			else // value is even and not prime
			{
				//ControlHandler.SetPropertyValue(lblCalcGoldbachInfo, "Visibility", Visibility.Visible);

				//logGoldbach.Info(value.ToString() + " = ");
				PrimesBigInteger counter = PrimesBigInteger.Zero;
				if (!value.IsProbablePrime(10))
				{
					PrimesBigInteger sum1 = PrimesBigInteger.Two;
					while (sum1.CompareTo(value.Divide(PrimesBigInteger.Two)) <= 0)
					{
						PrimesBigInteger sum2 = value.Subtract(sum1);
						if (sum2.IsProbablePrime(10))
						{
							counter = counter.Add(PrimesBigInteger.One);

							string text = string.Format("{0} + {1}   ", new object[] { sum2.ToString("D"), sum1.ToString("D") });
							logGoldbach.Info(text);

							string fmt = (counter.CompareTo(PrimesBigInteger.One) == 0)
								? Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_goldbachfoundsum
								: Primes.Resources.lang.WpfControls.Distribution.Distribution.numberline_goldbachfoundsums;
							ControlHandler.SetPropertyValue(lblGoldbachInfoCalc, "Text", string.Format(fmt, counter.ToString("D"), value.ToString()));
						}
						sum1 = sum1.NextProbablePrime();
					}
				}
			}

			if (GoldbachDone != null) GoldbachDone();
		}

		private void CancelGoldbach()
		{
			if (m_GoldbachThread != null)
			{
				m_GoldbachThread.Abort();
				m_GoldbachThread = null;
			}
			setCancelAllEnabled();
		}

#endregion

#region Counting Primes

		public void CountPrimes(PrimesBigInteger value)
		{
			CancelCountPrimes();
			m_CountPrimesThread = new Thread(new ParameterizedThreadStart(new ObjectParameterDelegate(DoCountPrimes)));
			m_CountPrimesThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			m_CountPrimesThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			m_CountPrimesThread.Priority = ThreadPriority.Normal;
			m_CountPrimesThread.Start(value);
		}

		public void DoCountPrimes(object o)
		{
			if(o!=null && o.GetType()==typeof(PrimesBigInteger))
			{
				FunctionPiX func = new FunctionPiX();
				func.ShowIntermediateResult = true;
				func.Executed += new ObjectParameterDelegate(func_Executed);
				double erg = func.Execute((o as PrimesBigInteger).DoubleValue);
				ControlHandler.SetPropertyValue(lblInfoCountPrimes, "Text", StringFormat.FormatDoubleToIntString(erg));
			}
		}

		void func_Executed(object obj)
		{
			ControlHandler.SetPropertyValue(lblInfoCountPrimes, "Text", obj.ToString());
		}

		public void CancelCountPrimes()
		{
			if (m_CountPrimesThread != null)
			{
				m_CountPrimesThread.Abort();
				m_CountPrimesThread = null;
			}
			setCancelAllEnabled();
		}

#endregion

#region Euler-Phi

		//public void EulerPhi(PrimesBigInteger value)
		//{
		//  CancelEulerPhi();
		//  m_EulerPhiThread = new Thread(new ParameterizedThreadStart(DoEulerPhi));
		//  m_EulerPhiThread.Start(value);
		//}

		//public void DoEulerPhi(object o)
		//{
		//  PrimesBigInteger value = o as PrimesBigInteger;
		//  ControlHandler.SetPropertyValue(lblCalcEulerPhiInfo, "Visibility", Visibility.Visible);
		//  if (value.IsPrime(20))
		//  {
		//    StringBuilder sbInfo = new StringBuilder();
		//    sbInfo.Append(value.ToString("D"));
		//    sbInfo.Append(" ist eine Primzahl. Darum ist die Anzahl der zu ");
		//    sbInfo.Append(value.ToString("D"));
		//    sbInfo.Append(" teilerfremden Zahlen ");
		//    sbInfo.Append(value.ToString("D"));
		//    sbInfo.Append(" - 1 = ");
		//    sbInfo.Append(value.Subtract(PrimesBigInteger.One));
		//    logEulerPhi.Info(sbInfo.ToString());
		//    ControlHandler.SetPropertyValue(lblCalcEulerPhiInfo, "Text", sbInfo.ToString());
		//  }
		//  else
		//  {
		//    PrimesBigInteger d = PrimesBigInteger.One;
		//    PrimesBigInteger counter = PrimesBigInteger.One;
		//    while (d.CompareTo(value) < 0)
		//    {
		//      if (PrimesBigInteger.GCD(d, value).Equals(PrimesBigInteger.One))
		//      {
		//        logEulerPhi.Info(d.ToString());
		//        counter = counter.Add(PrimesBigInteger.One);
		//        ControlHandler.SetPropertyValue(lblCalcEulerPhiInfo, "Text", "(" + counter.ToString("D") + " teilerfremde Zahlen zur aktuellen Zahl " + value.ToString("D") + " gefunden.)");


		//      }
		//      d = d.Add(PrimesBigInteger.One);

		//    }


		//  }
		//}

		//void CancelEulerPhi()
		//{
		//  if (m_EulerPhiThread != null)
		//  {
		//    m_EulerPhiThread.Abort();
		//    m_EulerPhiThread = null;
		//  }
		//}
		//
		void m_EulerPhi_OnStop()
		{
			m_EulerPhi.Stop();
			setCancelAllEnabled();
		}

#endregion


#region Tau

		void m_Tau_OnStop()
		{
			m_Tau.Stop();
			setCancelAllEnabled();
		}

#endregion

#region Rho

		void m_Rho_OnStop()
		{
			m_Rho.Stop();
			setCancelAllEnabled();
		}

#endregion

#region EulerPhiSum

		void m_DivSum_OnStop()
		{
			m_DivSum.Stop();
			setCancelAllEnabled();
		}

#endregion

#region Misc

		private int SortCompareGmpBigIntegers(PrimesBigInteger a, PrimesBigInteger b)
		{
			return a.CompareTo(b);
		}

		private void MarkNumber(NumberButton nb)
		{
			if (!markedNumbers.Contains(nb))
				markedNumbers.Add(nb);
			ControlHandler.SetPropertyValue(nb, "Background", Brushes.Red);
		}

		private void UnmarkAllNumbers()
		{
			foreach (NumberButton nb in m_Buttons)
			{
				SetButtonColor(nb);
			}
			markedNumbers.Clear();
		}

		private bool DictContainsKey(PrimesBigInteger key)
		{
			bool result = false;
			foreach (PrimesBigInteger _key in m_ButtonsDict.Keys)
			{
				if (_key.Equals(key)) result = true;
			}
			return result;
		}

#endregion

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			//Clipboard.SetText(lblGoldbach.Text, TextDataFormat.Text);
		}

		private void btnHelp_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			OnlineHelp.OnlineHelpActions action = Primes.OnlineHelp.OnlineHelpActions.None;
			if (sender == btnHelpCountPrimes)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Graph_PrimesCount;
			}
			else if (sender == btnHelpFactorize)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Factorization_Factorization;
			}
			else if (sender == btnHelpGoldbach)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Distribution_Goldbach;
			}
			else if (sender == btnHelpTwinPrimes)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Distribution_TwinPrimes;
			}
			else if (sender == btnHelpQuadrupletPrimes)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Distribution_QuadrupletPrimes;
			}
			else if (sender == btnHelpSixTupletPrimes)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Distribution_SixTupletPrimes;
			}
			else if (sender == btnHelpEulerPhi)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Distribution_EulerPhi;
			}
			else if (sender == btnHelpTau)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Distribution_Tau;
			}
			else if (sender == btnHelpRho)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Distribution_Sigma;
			}
			else if (sender == btnHelpDivSum)
			{
				action = Primes.OnlineHelp.OnlineHelpActions.Distribution_EulerPhiSum;
			}

			if (action != Primes.OnlineHelp.OnlineHelpActions.None)
			{
				OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(action);
			}

			e.Handled = true;
		}

#region IPrimeDistribution Members

		public void Init()
		{
			if (!m_Initialized)
			{
				DrawButtons();
				SetCountPrimes();
				m_Initialized = true;
				SetPointerActualNumber(PrimesBigInteger.Two);
			}
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			DrawButtons();

			Canvas.SetLeft(pFromNumber, 5);
			PrimesBigInteger key = m_Start.Add(PrimesBigInteger.ValueOf(m_ButtonsDict.Count - 1));
			if (m_ButtonsDict.ContainsKey(key))
			{
				NumberButton nb = m_ButtonsDict[key];
				Canvas.SetLeft(pToNumber, Canvas.GetLeft(nb) - 5);
			}
		}

		public void Dispose()
		{
			CancelThreads();
		}

#endregion

		private void lblCalcInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			GroupBox gb = null;
			TextBlock _sender = sender as TextBlock;
			if (_sender == lblCalcEulerPhiInfo) gb = gbEulerPhi;
			else if (_sender == lblCalcTauInfo) gb = gbTau;
			else if (_sender == lblCalcRhoInfo) gb = gbRho;
			else if (_sender == lblCalcDividerSum) gb = gbDivSum;
			else if (_sender == lblGoldbachInfoCalc) gb = gbGoldbach;

			if (gb != null)
			{
				if (gb.Visibility != Visibility)
				{
					gb.Visibility = Visibility.Visible;
					//logRho.Width = this.ActualWidth-50;
					//logDivSum.Width = this.ActualWidth - 50;
					//logEulerPhi.Width = this.ActualWidth - 50;
				}
				else
					gb.Visibility = Visibility.Collapsed;
			}
		}

		private void ActuallNumberButtonArea_MouseMove(object sender, MouseEventArgs e)
		{
			//NumberButton nb = null;
			//if (m_ActualNumber != null)
			//{
			//  if (m_ButtonsDict.ContainsKey(m_ActualNumber))
			//  {
			//    nb = m_ButtonsDict[m_ActualNumber];
			//  }
			//}
			//nb_MouseLeave(nb, null);
			double x = e.GetPosition(ActuallNumberButtonArea).X - PADDINGLEFT;
			int div = (int)(x / m_UnitSize + 0.5);
			Canvas.SetLeft(pActualNumber, div*m_UnitSize+PADDINGLEFT-5);
			PrimesBigInteger val = null;
			try { val = m_Buttons[div].BINumber; }
			catch { val = m_Start;}
			if (val != null && !val.Equals(m_ActualNumber))
				MarkNumber(val);
		}

		private void ActuallNumberButtonArea_MouseLeave(object sender, MouseEventArgs e)
		{
			//NumberButton nb = null;
			//if (m_ActualNumber != null)
			//{
			//  if (m_ButtonsDict.ContainsKey(m_ActualNumber))
			//  {
			//    nb = m_ButtonsDict[m_ActualNumber];
			//  }
			//}
			//nb_MouseLeave(nb, null);
		}

		private void pActualNumber_DragEnter(object sender, DragEventArgs e)
		{
		}

#region events

		public event VoidDelegate Execute;

		public void FireExecute()
		{
			if (Execute != null) Execute();
		}

		public event VoidDelegate Stop;

		public void FireStop()
		{
			if (Stop != null) Stop();
		}

#endregion

		private void btnCancelAll_Click(object sender, RoutedEventArgs e)
		{
			UnmarkAllNumbers();
			CancelThreads();
		}

		private void setCancelAllEnabled()
		{
			//bool enabled = false;
			//enabled =
			//  (m_FactorizeThread != null && m_FactorizeThread.ThreadState == System.Threading.ThreadState.Running) ||
			//  (m_GoldbachThread != null && m_GoldbachThread.ThreadState == System.Threading.ThreadState.Running) ||
			//  (m_CountPrimesThread != null && m_CountPrimesThread.ThreadState == System.Threading.ThreadState.Running) ||
			//  m_Tau.IsRunning ||
			//  m_Rho.IsRunning ||
			//  m_EulerPhi.IsRunning ||
			//  m_DivSum.IsRunning;
			//Debug.WriteLine(enabled);
			//ControlHandler.SetButtonEnabled(btnCancelAll, enabled);
		}

	}
}
