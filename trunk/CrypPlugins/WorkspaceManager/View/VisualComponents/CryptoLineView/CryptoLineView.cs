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
using System.Threading;
using WorkspaceManager.View.Container;
using System.Collections;

namespace WorkspaceManager.View.VisualComponents
{
	public sealed class CryptoLineView : Shape, IConnection, IUpdateableView
    {
        #region Variables

        private IntersectPoint intersectPoint;
        private List<FromTo> pointList = new List<FromTo>();
        public HashSet<CryptoLineView> UpdateList = new HashSet<CryptoLineView>();

        private ConnectionModel model;
        public ConnectionModel Model
        {
            get { return model; }
            private set { model = value; }
        }
        private static double offset = 6;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register("StartPoint", typeof(Point), typeof(CryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", typeof(Point), typeof(CryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

		#endregion

		#region CLR Properties

        public Point StartPoint
        {
            get { return (Point)GetValue(StartPointProperty); }
            set 
            {
                SetValue(StartPointProperty, value);
            }
        }

        public Point EndPoint
        {
            get { return (Point)GetValue(EndPointProperty); }
            set 
            {
                SetValue(EndPointProperty, value);
            }
        }

		#endregion

        public CryptoLineView()
        {
            Stroke = Brushes.Black;
            StrokeThickness = 2;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            foreach (CryptoLineView line in UpdateList)
            {
                line.InvalidateVisual();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs args)
        {
            if (args.RightButton == MouseButtonState.Pressed)
            {
                if (this.model != null && !this.model.WorkspaceModel.WorkspaceManagerEditor.isExecuting())
                {
                    this.model.WorkspaceModel.deleteConnectionModel(this.model);
                }
            }            
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
                 //check if is intersected at all
                if(isBetween(down.Y, up.Y, left.Y) && isBetween(left.X, right.X, up.X))
                {
                    if (up.Y == left.Y ||
                        down.Y == left.Y ||
                        left.X == up.X || right.X == up.X)
                    {
                        intersectPoint = new IntersectPoint(new Point(up.X, left.Y), IntersectPointMode.InnerIntersect);
                    }
                    else
                    {
                        intersectPoint = new IntersectPoint(new Point(up.X, left.Y), IntersectPointMode.NormalIntersect);
                    }
                    return true;
                }
                return false;
            }
        }

		private void internalGeometryDraw(StreamGeometryContext context)
		{
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
                                fromTo.Intersection.Add(intersectPoint);

                                if (fromTo.DirSort == DirSort.Y_ASC || fromTo.DirSort == DirSort.Y_DESC)
                                    this.UpdateList.Add(result);
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
                    foreach (IntersectPoint interPoint in fromTo.Intersection)
                    {
                        switch (fromTo.DirSort)
                        {
                            case DirSort.X_ASC:
                                if (interPoint.Mode == IntersectPointMode.NormalIntersect)
                                {
                                    context.LineTo(new Point(interPoint.Point.X - offset, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - offset), new Point(interPoint.Point.X + offset, interPoint.Point.Y), true, true);
                                }
                                else if (interPoint.Mode == IntersectPointMode.InnerIntersect)
                                {
                                    context.LineTo(new Point(interPoint.Point.X - 4, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - 5), new Point(interPoint.Point.X + 4, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y + 5), new Point(interPoint.Point.X - 4, interPoint.Point.Y), true, true);
                                }
                                break;
                            case DirSort.X_DESC:
                                if (interPoint.Mode == IntersectPointMode.NormalIntersect)
                                {
                                    context.LineTo(new Point(interPoint.Point.X + offset, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - offset), new Point(interPoint.Point.X - offset, interPoint.Point.Y), true, true);
                                }
                                else if (interPoint.Mode == IntersectPointMode.InnerIntersect)
                                {
                                    context.LineTo(new Point(interPoint.Point.X + 4, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - 5), new Point(interPoint.Point.X - 4, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y + 5), new Point(interPoint.Point.X + 4, interPoint.Point.Y), true, true);
                                }
                                break;
                            //case DirSort.Y_ASC:
                            //    context.LineTo(new Point(interPoint.X, interPoint.Y - offset), true, true);
                            //    context.QuadraticBezierTo(new Point(interPoint.X + offset, interPoint.Y), new Point(interPoint.X, interPoint.Y + offset), true, true);
                            //    break;
                            //case DirSort.Y_DESC:
                            //    context.LineTo(new Point(interPoint.X, interPoint.Y + offset), true, true);
                            //    context.QuadraticBezierTo(new Point(interPoint.X + offset, interPoint.Y), new Point(interPoint.X, interPoint.Y - offset), true, true);
                            //    break;
                        }
                    }
                    context.LineTo(fromTo.To, true, true);
                }
                else
                {
                    context.LineTo(fromTo.To, true, true);
                }
            }
        }

       

        private bool isConnectionPossible(Point p1, Point p2)
        {
            if (p1.X != p2.X && p1.Y != p2.Y)
                throw new ArgumentException("only 90° allowed");

            if (p1.Y != p2.Y)
            {
                Point up = p2.Y < p1.Y ? p1 : p2;
                Point down = p2.Y < p1.Y?p2 : p1;

                Panel parent = (Parent as Panel);
                foreach (var element in parent.Children)
                {
                    PluginContainerView plug1 = element as PluginContainerView;
                    if (plug1 == null)
                        continue;
                    Point pos = new Point((plug1.RenderTransform as TranslateTransform).X, (plug1.RenderTransform as TranslateTransform).Y);

                    if (!isBetween(pos.X, pos.X + plug1.ActualWidth, up.X))
                        continue;

                    // case 1: one point is inside the plugion
                    if (isBetween(pos.Y, pos.Y + plug1.ActualHeight, up.Y) ||
                        isBetween(pos.Y, pos.Y + plug1.ActualHeight, down.Y))
                    {
                        return false;
                    }

                    // case 2: goes through
                    if (pos.Y > up.Y && pos.Y + plug1.ActualHeight < down.Y)
                    {
                        return false;
                    }
                }
            }
            else
            {
                Point left = p2.X < p1.X ? p2 : p1;
                Point right = p2.X < p1.X ? p1 : p2; 

                Panel parent = (Parent as Panel);
                foreach (var element in parent.Children)
                {
                    PluginContainerView plug1 = element as PluginContainerView;
                    if (plug1 == null)
                        continue;
                    Point pos = new Point((plug1.RenderTransform as TranslateTransform).X, (plug1.RenderTransform as TranslateTransform).Y);

                    if (!isBetween(pos.Y, pos.Y + plug1.ActualHeight, left.Y))
                        continue;

                    // case 1: one point is inside the plugion
                    if(isBetween(pos.X, pos.X + plug1.ActualWidth, left.X) ||
                        isBetween(pos.X, pos.X + plug1.ActualWidth, right.X))
                    {
                        return false;
                    }

                    // case 2: goes through
                    if(pos.X > left.X && pos.X + plug1.ActualWidth < right.X)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal class Node : StackFrameDijkstra.Node<Node>
        {
            public Point Point { get; set; }
            public HashSet<Node> Vertices { get; private set; }
            public Node()
            {
                Vertices = new HashSet<Node>();
            }

            public double pathCostEstimate(Node goal)
            {
                return 0;
            }

            public double traverseCost(Node dest)
            {
                if (!Vertices.Contains(dest))
                    return Double.PositiveInfinity;

                if (dest.Point.X == Point.X)
                    return Math.Abs(dest.Point.Y - Point.Y);
                return Math.Abs(dest.Point.X - Point.X);
            }

            public IEnumerable<Node> neighbors()
            {
                return Vertices;
            }
        }

        private bool performOrthogonalPointConnection(Node n1, Point p2, Node n3, List<Node> nodeList)
        {
            if (isConnectionPossible(n1.Point, p2) && isConnectionPossible(p2, n3.Point))
            {
                Node n2 = new Node() { Point = p2 };
                n1.Vertices.Add(n2);

                n2.Vertices.Add(n1);
                n2.Vertices.Add(n3);

                n3.Vertices.Add(n2);

                nodeList.Add(n2);
                return true;
            }
            return false;
        }

        private void performOrthogonalPointConnection(Node p1, Node p2)
        {
            if (isConnectionPossible(p1.Point, p2.Point))
            {
                p1.Vertices.Add(p2);
                p2.Vertices.Add(p1);
            }
        }

        private void makeOrthogonalPoints()
        {
            List<Node> nodeList = new List<Node>();
            Panel parent = (Parent as Panel);

            // add start and end. Index will be 0 and 1
            Node startNode = new Node() { Point = StartPoint },
                endNode = new Node() { Point = EndPoint };
            nodeList.Add(startNode);
            nodeList.Add(endNode);

            foreach (var element in parent.Children)
            {
                if (element is PluginContainerView)
                {
                    PluginContainerView p1 = element as PluginContainerView;
                    foreach (var routPoint in p1.RoutingPoints)
                    {
                        nodeList.Add(new Node() { Point = routPoint });
                    }
                }
            }
            
            // connect points
            int loopCount = nodeList.Count;
            for(int i=0; i<loopCount; ++i)
            {
                var p1 = nodeList[i];
                // TODO: inner loop restriction! n²-n!
                // is k=i instead of k=0 correct?
                for(int k=0; k<loopCount; ++k)
                {
                    var p2 = nodeList[k];
                    if (p1 == p2)
                        continue;
                    if (p1.Vertices.Contains(p2))
                        continue;

                    // no helping point required?
                    if (p1.Point.X == p2.Point.X ||
                        p1.Point.Y == p2.Point.Y)
                    {
                        performOrthogonalPointConnection(p1, p2);
                    }
                    else
                    {
                        Point help1 = new Point(p1.Point.X, p2.Point.Y);

                        if (!performOrthogonalPointConnection(p1, help1, p2, nodeList))
                        {
                            Point help2 = new Point(p2.Point.X, p1.Point.Y);
                            performOrthogonalPointConnection(p1, help2, p2, nodeList);
                            // optinal TODO: other possible helping points
                        }
                       
                    }
                }
            }

            StackFrameDijkstra.Dijkstra<Node> dijkstra = new StackFrameDijkstra.Dijkstra<Node>();
            var path = dijkstra.findPath(nodeList, startNode, endNode);

            if (path != null)
            {
                pointList.Clear();
                Point prevPoint = StartPoint;

                foreach (var i in path)
                {
                    Point thisPoint = i.Point;
                    this.pointList.Add(new FromTo(prevPoint, thisPoint));
                    prevPoint = thisPoint;
                }
            }
                //Failsafe
            else if (StartPoint.X < EndPoint.X)
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

        internal void Reset()
        {
            Color color = ColorHelper.GetColor(Model.ConnectionType);
            Stroke = new SolidColorBrush(color);
        }
    }
}
