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
using System.Windows.Documents;
using System.Collections.Generic;

namespace WorkspaceManager.View.VisualComponents
{
	public sealed class CryptoLineView : Shape, IConnection, IUpdateableView
    {
        #region Variables

        private Point iPoint = new Point();
        private List<FromTo> pointList = new List<FromTo>();

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
            Color color = ColorHelper.GetColor(connectionModel.ConnectionType);
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

            makeOrthogonalPoints();
            foreach (var element in (Parent as Panel).Children)
            {
                if (element is CryptoLineView && !element.Equals(this))
                {
                    CryptoLineView result = element as CryptoLineView;
                    foreach (FromTo fromTo in pointList)
                    {
                        foreach (FromTo resultFromTo in result.pointList)
                        {
                            if (findIntersection(fromTo.From, fromTo.To, resultFromTo.From, resultFromTo.To))
                            {
                                fromTo.Intersection.Add(iPoint);
                            }
                        }
                    }
                }
            }

            context.BeginFigure(StartPoint, true, false);

            foreach (FromTo fromTo in pointList)
            {
                if (fromTo.Intersection.Count > 0)
                {
                    foreach (Point interPoint in fromTo.Intersection)
                    {
                        if (fromTo.From.X == fromTo.To.X)
                        {
                            context.LineTo(new Point(iPoint.X, iPoint.Y - offset), true, true);
                            context.QuadraticBezierTo(new Point(iPoint.X + offset, iPoint.Y), new Point(iPoint.X, iPoint.Y + offset), true, true);
                        }
                        else if (fromTo.From.Y == fromTo.To.Y)
                        {
                            context.LineTo(new Point(iPoint.X - offset, iPoint.Y), true, true);
                            context.QuadraticBezierTo(new Point(iPoint.X, iPoint.Y + offset), new Point(iPoint.X + offset, iPoint.Y), true, true);
                        }
                        context.LineTo(fromTo.To, true, true);
                    }

                }
                else
                {
                    context.LineTo(fromTo.To, true, true);
                }
            }

            //Point pt3 = new Point(
            //    pt05.X + ( cost - offset * sint),
            //    pt05.Y + ( sint + offset * cost));
            //context.LineTo(EndPoint, true, true);
		}

        private void makeOrthogonalPoints()
        {
            if (StartPoint.X < EndPoint.X)
            {
                pointList.Clear();
                pointList.Add(new FromTo(StartPoint, new Point((EndPoint.X + StartPoint.X) / 2, StartPoint.Y)));
                pointList.Add(new FromTo(new Point((EndPoint.X + StartPoint.X) / 2, StartPoint.Y), new Point((EndPoint.X + StartPoint.X) / 2, EndPoint.Y)));
                pointList.Add(new FromTo(new Point((EndPoint.X + StartPoint.X) / 2, EndPoint.Y), EndPoint));
            }
            else
            {
                if (StartPoint.X > EndPoint.X)
                {
                    pointList.Clear();
                    pointList.Add(new FromTo(StartPoint, new Point((StartPoint.X + EndPoint.X) / 2, StartPoint.Y)));
                    pointList.Add(new FromTo(new Point((StartPoint.X + EndPoint.X) / 2, StartPoint.Y), new Point((StartPoint.X + EndPoint.X) / 2, EndPoint.Y)));
                    pointList.Add(new FromTo(new Point((StartPoint.X + EndPoint.X) / 2, EndPoint.Y), EndPoint));
                }
            }
        }
		
		#endregion

        #region IUpdateableView Members

        public void update()
        {
            Stroke = Brushes.Green;
        }

        #endregion
    }
}
