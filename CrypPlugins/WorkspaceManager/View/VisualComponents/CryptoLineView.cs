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
	public sealed class CryptoLineView : Shape, IConnection, IUpdateableView
    {
        #region Variables

        private Point iPoint = new Point();

        private ConnectionModel model;
        public ConnectionModel Model
        {
            get { return model; }
            private set { model = value; }
        }
        private static double offset = 10;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register("StartPoint", typeof(Point), typeof(CryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", typeof(Point), typeof(CryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

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
            this.Model = connectionModel;
            Color color = ColorHelper.GetDataColor(connectionModel.ConnectionType);
            Stroke = new SolidColorBrush(color);
            StrokeThickness = 2;
        }

		#region Overrides

		protected override Geometry DefiningGeometry
		{
			get
			{
				StreamGeometry geometry = new StreamGeometry();
				geometry.FillRule = FillRule.EvenOdd;

				using (StreamGeometryContext context = geometry.Open())
				{
                    internalGeometryDraw(context);
				}

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
                return false;
            }
            if (StartPointSec.X != EndPointSec.X &&
                StartPointSec.Y != EndPointSec.Y)
            {
                return false;
            }

            // parallel
            if (StartPoint.X == EndPoint.X && StartPointSec.X == EndPointSec.X ||
                StartPoint.Y == EndPoint.Y && StartPointSec.Y == EndPointSec.Y)
            {
                return false;
            }
            else
            {
                // orthonogal 
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

            context.BeginFigure(StartPoint, true, false);

            foreach (var element in (Parent as Panel).Children)
            {
                if (element is CryptoLineView && !element.Equals(this))
                {
                    if (findIntersection(StartPoint, EndPoint, (element as CryptoLineView).StartPoint, (element as CryptoLineView).EndPoint))
                    {
                        if (StartPoint.X == EndPoint.X)
                        {
                            context.LineTo(new Point(iPoint.X, iPoint.Y - offset), true, true);
                            context.QuadraticBezierTo(new Point(iPoint.X + offset, iPoint.Y), new Point(iPoint.X, iPoint.Y + offset), true, true);
                            continue;
                        }
                        if (StartPoint.Y == EndPoint.Y)
                        {
                            context.LineTo(new Point(iPoint.X - offset, iPoint.Y), true, true);
                            context.QuadraticBezierTo(new Point(iPoint.X, iPoint.Y + offset), new Point(iPoint.X + offset, iPoint.Y), true, true);
                            continue;
                        }
                    }
                }
            }

            //Point pt3 = new Point(
            //    pt05.X + ( cost - offset * sint),
            //    pt05.Y + ( sint + offset * cost));
            context.LineTo(EndPoint, true, true);
		}
		
		#endregion

        #region IUpdateableView Members

        public void update()
        {
           
        }

        #endregion
    }
}
