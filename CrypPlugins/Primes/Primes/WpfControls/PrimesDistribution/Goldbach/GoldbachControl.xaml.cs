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
using Primes.WpfControls.Components.Arrows;
using Primes.WpfControls.Components;
using Primes.WpfControls.Validation;
using Primes.WpfControls.Validation.Validator;

namespace Primes.WpfControls.PrimesDistribution.Goldbach
{
  /// <summary>
  /// Interaction logic for GoldbachControl.xaml
  /// </summary>
  public partial class GoldbachControl : UserControl, IPrimeDistribution
  {
    private bool m_Initialized;
    private PrimesBigInteger m_From;
    private PrimesBigInteger m_To;
    private Thread m_Thread;
    private const double PADDING_AXISTOP = 10;
    private const double PADDING_AXISLEFT = 30;
    private const double PADDING_AXISBOTTOM = 20;
    private const double PADDING_AXISRIGHT = 10;

    private const double PADDING_TOP = PADDING_AXISTOP;
    private const double PADDING_LEFT = PADDING_AXISLEFT+30;
    private const double PADDING_BOTTOM = PADDING_AXISBOTTOM;
    private const double PADDING_RIGHT = PADDING_AXISRIGHT+30;


    private PrimesBigInteger m_Max;
    private PrimesBigInteger m_Min;

    public GoldbachControl()
    {
      InitializeComponent();
      Init();

      this.Start += new VoidDelegate(GoldbachControl_Start);
      this.Stop += new VoidDelegate(GoldbachControl_Stop);
      this.Cancel += new VoidDelegate(GoldbachControl_Cancel);
    }



    void GoldbachControl_Cancel()
    {
      ircGoldbach.UnLockControls();
      CancelThread();
    }

    void GoldbachControl_Stop()
    {
      ircGoldbach.UnLockControls();
    }

    void GoldbachControl_Start()
    {
      ControlHandler.ClearChildren(PaintArea);
      ircGoldbach.LockControls();
    }

    void ircGoldbach_Cancel()
    {      
      FireCancelEvent();

    }

    void ircGoldbach_Execute(PrimesBigInteger from, PrimesBigInteger to)
    {
      m_From = from;
      m_To = to;

      StartThread();
    }

    #region Events
    private event VoidDelegate Start;
    private event VoidDelegate Stop;
    private event VoidDelegate Cancel;

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
    #region Thread
    private void StartThread()
    {
      m_Thread = new Thread(new ThreadStart(DoExecute));
      m_Thread.Start();
    }

    private void CancelThread()
    {
      if (m_Thread != null)
      {
        m_Thread.Abort();
        m_Thread = null;
      }
    }
    #endregion
    
    #region Properties
    private PrimesBigInteger Range
    {
      get { return m_Max.Subtract(m_Min); }
    }
    #endregion

    private PrimesBigInteger From
    {
      get 
      {
        PrimesBigInteger value = m_From;
        if (!value.Mod(PrimesBigInteger.Two).Equals(PrimesBigInteger.Zero))
        {
          value = value.Add(PrimesBigInteger.One);
        }
        return value;
      }
    }
    private void DoExecute()
    {
      FireStartEvent();
      m_Max = null;
      m_Min = null;
      IDictionary<PrimesBigInteger, PrimesBigInteger> points = new Dictionary<PrimesBigInteger, PrimesBigInteger>();
      PrimesBigInteger goldbachResult = null;
      ControlHandler.ExecuteMethod(this, "PaintCoordianteAxis");
      PrimesBigInteger value = From;
      bool repaint = false;
      while (value.CompareTo(m_To) <= 0)
      {
        goldbachResult = Calculate(value);
        if (m_Max == null) m_Max = goldbachResult.Add(PrimesBigInteger.Ten);
        if (m_Min == null) m_Min = goldbachResult;
        if (goldbachResult.CompareTo(m_Min) <= 0)
        {
          m_Min = goldbachResult;
        }
        if (m_Max.CompareTo(goldbachResult.Add(PrimesBigInteger.Ten)) < 0)
        {
          m_Max = goldbachResult.Add(PrimesBigInteger.Ten);
          repaint = true;
        }

        points.Add(value, goldbachResult);
        if (points.Count >= 100)
        {
          ControlHandler.ExecuteMethod(this, "PaintGoldbachResult", new object[] { points });
          points.Clear();
          if(repaint)
            ControlHandler.ExecuteMethod(this, "RepaintGoldbach");
        }
        value = value.Add(PrimesBigInteger.Two);
      }
      if (points.Count > 0)
      {
        ControlHandler.ExecuteMethod(this, "PaintGoldbachResult", new object[] { points });
        points.Clear();
        if (repaint)
          ControlHandler.ExecuteMethod(this, "RepaintGoldbach");
      }
      FireStopEvent();
    }

    #region Painting

    private double UnitHeight
    {
      get 
      {
        return ((PaintArea.ActualHeight - PADDING_BOTTOM) - PADDING_TOP) / Range.DoubleValue;
      }
    }

    private double UnitWidth
    {
      get
      {
        return ((PaintArea.ActualWidth - PADDING_LEFT) - PADDING_RIGHT) / m_To.Subtract(From).DoubleValue;
      }
    }

    public void PaintGoldbachResult(IDictionary<PrimesBigInteger, PrimesBigInteger> points)
    {
      foreach (KeyValuePair<PrimesBigInteger, PrimesBigInteger> pair in points)
      {
        PrimesBigInteger x = pair.Key;
        PrimesBigInteger y = pair.Value;
        double size = 4;
        double val = y.DoubleValue;
        double unitwidth = UnitWidth;
        double unitheight = UnitHeight;

        double top = (PaintArea.ActualHeight - PADDING_TOP - PADDING_BOTTOM - (val * unitheight)) - size / 2;
        double left = (((x.DoubleValue - From.DoubleValue) * unitwidth) + PADDING_LEFT) - size / 2;
        Ellipse el = new Ellipse();
        el.Stroke = Brushes.Red;
        el.StrokeThickness = 1.0;
        el.Fill = Brushes.Red;
        el.Width = 4;
        el.Height = 4;
        el.Tag = new KeyValuePair<PrimesBigInteger, PrimesBigInteger>(x, y);
        el.MouseMove += new MouseEventHandler(el_MouseMove);
        Canvas.SetTop(el, top);
        Canvas.SetLeft(el, left);
        PaintArea.Children.Add(el);
      }
    }

    void el_MouseMove(object sender, MouseEventArgs e)
    {

      tbinfo.Text = ((KeyValuePair<PrimesBigInteger, PrimesBigInteger>)((Ellipse)sender).Tag).Key.ToString();
      tbinfo.Text += ":";
      tbinfo.Text += ((KeyValuePair<PrimesBigInteger, PrimesBigInteger>)((Ellipse)sender).Tag).Value.ToString();
    }

    public void RepaintGoldbach()
    {
      double size = 4;

      double unitwidth = UnitWidth;
      double unitheight = UnitHeight;


      IList<UIElement> toremove = new List<UIElement>();
      foreach (UIElement element in PaintArea.Children)
      {
        if (element.GetType() == typeof(Ellipse))
        {
          KeyValuePair<PrimesBigInteger, PrimesBigInteger> pair = (KeyValuePair<PrimesBigInteger,PrimesBigInteger>)((Ellipse)(element)).Tag;

          double top = (PaintArea.ActualHeight - PADDING_TOP - PADDING_BOTTOM - (pair.Value.DoubleValue * unitheight)) - size / 2;
          double left = (((pair.Key.DoubleValue - From.DoubleValue) * unitwidth) + PADDING_LEFT) - size / 2;
          Canvas.SetTop(element, top);
          Canvas.SetLeft(element, left);
        }
        else if (element.GetType() == typeof(Line))
        {
          Line l = (Line)element;
          if (l.Name.StartsWith("YAXSIS"))
          {
            toremove.Add(l);
          }
        }
        else if (element.GetType() == typeof(TextBlock))
        {
          TextBlock l = (TextBlock)element;
          if (l.Name.StartsWith("YAXSIS"))
          {
            toremove.Add(l);
          }
        }
      }
      foreach (UIElement e in toremove)
      {
        PaintArea.Children.Remove(e);
      }
      // Koordinaten neu zeichnen
      PrimesBigInteger inc = PrimesBigInteger.Max(m_Max.Subtract(m_Min).Divide(PrimesBigInteger.Ten), PrimesBigInteger.Two);
      PrimesBigInteger startx = m_Min;
      while (startx.CompareTo(m_Max.Subtract(PrimesBigInteger.Ten)) <= 0)
      {
        Line l = new Line();
        l.Name = "YAXSIS" + startx.ToString();

        l.X1 = PADDING_AXISLEFT - 2;
        l.X2 = PADDING_AXISLEFT + 2;

        l.Y1 = l.Y2 = PaintArea.ActualHeight - PADDING_TOP - PADDING_BOTTOM - (startx.DoubleValue * unitheight);

        l.Stroke = Brushes.Black;
        l.StrokeThickness = 1.0;
        PaintArea.Children.Add(l);

        TextBlock lbl = new TextBlock();
        lbl.Name = "YAXSISlbl" + startx.ToString();
        lbl.Text = startx.ToString("D");
        Canvas.SetLeft(lbl, PADDING_AXISLEFT - 20);
        Canvas.SetTop(lbl, l.Y1);
        PaintArea.Children.Add(lbl);
        startx = startx.Add(inc);
      }
    }

    public void ClearCoordinateAxis()
    {
      IList<UIElement> toremove = new List<UIElement>();
      foreach (UIElement element in PaintArea.Children)
      {
        if (element.GetType() == typeof(Line) || element.GetType() == typeof(ArrowLine)||element.GetType() == typeof(TextBlock))
        {
          toremove.Add(element);
        }
      }
      foreach (UIElement e in toremove)
      {
        PaintArea.Children.Remove(e);
      }


    }
    public void PaintCoordianteAxis()
    {
      ArrowLine y = new ArrowLine();
      y.X1 = PADDING_AXISLEFT;
      y.X2 = PADDING_AXISLEFT;
      y.Y1 = PaintArea.ActualHeight-PADDING_AXISBOTTOM;
      y.Y2 = PADDING_AXISTOP;
      y.Stroke = Brushes.Black;
      y.StrokeThickness = 1.0;

      ArrowLine x = new ArrowLine();
      x.X1 = PADDING_AXISLEFT;
      x.X2 = PaintArea.ActualWidth - PADDING_AXISRIGHT;
      x.Y1 = x.Y2 = PaintArea.ActualHeight - PADDING_AXISBOTTOM;
      x.Stroke = Brushes.Black;
      x.StrokeThickness = 1.0;

      PaintArea.Children.Add(x);
      PaintArea.Children.Add(y);

      TextBlock lblX = new TextBlock();
      lblX.Text = "n";
      lblX.Name = "AxisLabelX";
      Canvas.SetTop(lblX, x.Y2);
      Canvas.SetLeft(lblX, x.X2);
      PaintArea.Children.Add(lblX);

      TextBlock lblY = new TextBlock();
      lblY.Text = "y(n)";
      lblY.Name = "AxisLabelY";
      Canvas.SetTop(lblY, y.Y2);
      Canvas.SetLeft(lblY, y.X2-25);
      PaintArea.Children.Add(lblY);

      double unitwidth = UnitWidth;

      PrimesBigInteger inc = PrimesBigInteger.Max(m_To.Subtract(From).Divide(PrimesBigInteger.Ten),PrimesBigInteger.Two);
      PrimesBigInteger startx = From;
      while(startx.CompareTo(m_To)<=0)
      {
        Line l = new Line();

        l.X1 = l.X2 = (((startx.DoubleValue - From.DoubleValue) * unitwidth) + PADDING_LEFT);
        l.Y1 = x.Y1 - 2;
        l.Y2 = x.Y1 + 2;
        l.Stroke = Brushes.Black;
        l.StrokeThickness = 1.0;
        PaintArea.Children.Add(l);

        TextBlock lbl = new TextBlock();
        lbl.Text = startx.ToString("D");
        Canvas.SetLeft(lbl, l.X1);
        Canvas.SetTop(lbl, x.Y1+4);
        PaintArea.Children.Add(lbl);
        startx = startx.Add(inc);
      }

    }

    #endregion

    #region Calculating
    private PrimesBigInteger Calculate(PrimesBigInteger value)
    {
      PrimesBigInteger result = PrimesBigInteger.Zero;
      if (value != null && value.Mod(PrimesBigInteger.Two).Equals(PrimesBigInteger.Zero))
      {
        PrimesBigInteger sum1 = PrimesBigInteger.Two;
        while (sum1.CompareTo(value.Divide(PrimesBigInteger.Two)) <= 0)
        {
          PrimesBigInteger sum2 = value.Subtract(sum1);
          if (sum2.IsProbablePrime(10))
          {
            result = result.Add(PrimesBigInteger.One);


          }
          sum1 = sum1.NextProbablePrime();
        }

      }
      return result;
    }
    #endregion

    #region IPrimeDistribution Members

    public void Init()
    {
      m_Initialized = true;
      ircGoldbach.SetButtonCancelButtonEnabled(false);
      InitInput();
    }

    private void InitInput()
    {
      ircGoldbach.Execute += new Primes.WpfControls.Components.ExecuteDelegate(ircGoldbach_Execute);
      ircGoldbach.Cancel += new VoidDelegate(ircGoldbach_Cancel);

      ircGoldbach.SetText(InputRangeControl.FreeFrom, "4");
      ircGoldbach.SetText(InputRangeControl.FreeTo, "1000");

      InputValidator<PrimesBigInteger> ivFrom = new InputValidator<PrimesBigInteger>();
      ivFrom.Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.Four,PrimesBigInteger.ValueOf(100000-1));
      ircGoldbach.AddInputValidator(InputRangeControl.FreeFrom, ivFrom);
      InputValidator<PrimesBigInteger> ivTo = new InputValidator<PrimesBigInteger>();
      ivTo.Validator = new BigIntegerMinValueMaxValueValidator(null, PrimesBigInteger.Five, PrimesBigInteger.ValueOf(100000));
      ircGoldbach.AddInputValidator(InputRangeControl.FreeTo, ivTo);
    }


    public void Dispose()
    {
    }

    #endregion

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      if (m_Initialized)
      {
        PaintArea.Width = gridContent.ActualWidth;
        PaintArea.Height = gridContent.ActualHeight - spInput.ActualHeight - spSlider.ActualHeight;


      }
    }

    private void PaintArea_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (m_From != null && m_To != null)
      {
        ClearCoordinateAxis();
        PaintCoordianteAxis();
        RepaintGoldbach();
      }

    }
  }
}
