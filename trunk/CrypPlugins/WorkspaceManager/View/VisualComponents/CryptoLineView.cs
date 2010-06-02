using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Reflection;
using System.Windows.Threading;
using WorkspaceManager.View.Interface;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.VisualComponents
{
	public sealed class CryptoLineView : Shape, IConnection
    {
        #region Variables

        private Point iPoint = new Point();

        private static double offset = 5;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register("StartPoint", typeof(Point), typeof(CryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", typeof(Point), typeof(CryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public ConnectionModel connectionModel;

		#endregion

		#region CLR Properties

        public Point StartPoint
        {
            get { return (Point)GetValue(StartPointProperty); }
            set { SetValue(StartPointProperty, value); }
        }

        public Point EndPoint
        {
            get { return (Point)GetValue(EndPointProperty); }
            set { SetValue(EndPointProperty, value); }
        }

		#endregion

        public CryptoLineView()
        {
            Stroke = Brushes.Black;
            StrokeThickness = 2;
        }

        public CryptoLineView(ConnectionModel connectionModel) : this()
        {
            this.connectionModel = connectionModel;           
        }

		#region Overrides

		protected override Geometry DefiningGeometry
		{
			get
			{
				// Create a StreamGeometry for describing the shape
				StreamGeometry geometry = new StreamGeometry();
				geometry.FillRule = FillRule.EvenOdd;

				using (StreamGeometryContext context = geometry.Open())
				{
                    internalGeometryDraw(context);
				}

				// Freeze the geometry for performance benefits
				geometry.Freeze();
				return geometry;
			}
		}		

		#endregion

		#region Privates
        private bool isBetween(double min, double max, double between)
        {
            return min <= between && between <= max;
        }

        private bool findIntersection(Point StartPoint, Point EndPoint, Point StartPointSec, Point EndPointSec)
        {
            if (StartPoint.X != EndPoint.X &&
                StartPoint.Y != EndPoint.Y)
            {
                throw new ArgumentException("only 90° lines allowed");
            }
            if (StartPointSec.X != EndPointSec.X &&
                StartPointSec.Y != EndPointSec.Y)
            {
                throw new ArgumentException("only 90° lines allowed");
            }

            // TODO: handle parallel here
            if (false)
            {

                
            }
            else
            {
                Point up, down, left, right;
                if (StartPoint.X == EndPoint.X)
                {
                    up = StartPoint;
                    down = EndPoint;
                    left = StartPointSec;
                    right = EndPointSec;
                }
                else
                {
                    up = StartPointSec;
                    down = EndPointSec;
                    left = StartPoint;
                    right = EndPoint;
                }

                if (up.Y < down.Y)
                {
                    double swap = up.Y;
                    up.Y = down.Y;
                    down.Y = swap;
                }

                if (left.X > right.X)
                {
                    double swap = left.X;
                    left.X = right.X;
                    right.X = swap;
                }
                   
                if(isBetween(down.Y, up.Y, left.Y) &&
                    isBetween(left.X, right.X, up.X))
                {
                    iPoint = new Point(up.X, left.Y);
                    return true;
                }
                return false;
            }
        }

		private void internalGeometryDraw(StreamGeometryContext context)
		{
            double theta = Math.Atan2(StartPoint.Y - EndPoint.Y, StartPoint.X - EndPoint.X);
			double sint = Math.Sin(theta);
			double cost = Math.Cos(theta);

			Point start = new Point(StartPoint.X, StartPoint.Y);
            Point end = new Point(EndPoint.X, EndPoint.Y);

            //foreach (var element in (Parent as Panel).Children)
            //{
            //    if (element is CLine && !element.Equals(this))
            //    {
            //        if (findIntersection(StartPoint, EndPoint, (element as CLine).StartPoint, (element as CLine).EndPoint))
            //        {
            //            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate()
            //            {
            //                Ellipse ell = new Ellipse();
            //                ell.Fill = Brushes.Red;
            //                ell.Width = 20;
            //                ell.Height = 20;
            //                ell.RenderTransform = new TranslateTransform(iPoint.X - ell.Width/2, iPoint.Y - ell.Height/2);
            //                (Parent as Panel).Children.Add(ell);
            //            });

            //            Console.WriteLine("INTERSECTION FOUND " + iPoint.ToString());
            //        }
            //    }
            //}

            //Point pt3 = new Point(
            //    pt05.X + ( cost - offset * sint),
            //    pt05.Y + ( sint + offset * cost));

            context.BeginFigure(start, true, false);
            


            context.LineTo(end, true, true);
		}
		
		#endregion
	}
}
