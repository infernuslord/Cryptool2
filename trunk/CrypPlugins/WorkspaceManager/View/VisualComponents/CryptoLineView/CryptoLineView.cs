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
using WorkspaceManager.View.VisualComponents.StackFrameDijkstra;
using System.Windows.Data;

namespace WorkspaceManager.View.VisualComponents
{
	public sealed class CryptoLineView : Shape, IConnection, IUpdateableView
    {
        #region Variables

        public static readonly DependencyProperty IsDraggingDependencyProperty = DependencyProperty.Register("IsDragging", typeof(bool), typeof(CryptoLineView), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnDraggingPropertyChanged)));

        [TypeConverter(typeof(bool))]
        public bool IsDragging
        {
            get { return (bool)base.GetValue(IsDraggingDependencyProperty); }
            set
            {
                base.SetValue(IsDraggingDependencyProperty, value);
            }
        }

        private static void OnDraggingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CryptoLineView line = (CryptoLineView)d;
            Panel p = (line.Parent as Panel);
            if (p == null)
                return;
            foreach (UIElement shape in p.Children)
            {
                if (shape is CryptoLineView)
                    shape.InvalidateVisual();
            }
        }

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
        private ConnectionModel connectionModel;
        //private ConnectorView source;
        //private ConnectorView target;

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

        public CryptoLineView(ConnectionModel connectionModel, ConnectorView source, ConnectorView target)
        {
            this.Loaded += new RoutedEventHandler(CryptoLineView_Loaded);
            this.connectionModel = connectionModel;
            this.StartPointSource = source;
            this.EndPointSource = target;
        }

        void CryptoLineView_Loaded(object sender, RoutedEventArgs e)
        {
            Color color = ColorHelper.GetLineColor(connectionModel.ConnectionType);
            Stroke = new SolidColorBrush(color);
            StrokeThickness = 2;
        }

        private void makeBinding(ConnectorView source, ConnectorView target)
        {
            MultiBinding multiBinding = new MultiBinding();
            multiBinding.Converter = new MultiDragValueConverter();

            Binding bind = new Binding();
            bind.Source = source.Parent;
            bind.Path = new PropertyPath(PluginContainerView.IsDragStartedDependencyProperty);
            multiBinding.Bindings.Add(bind);

            bind = new Binding();
            bind.Source = target.Parent;
            bind.Path = new PropertyPath(PluginContainerView.IsDragStartedDependencyProperty);
            multiBinding.Bindings.Add(bind);

            SetBinding(CryptoLineView.IsDraggingDependencyProperty, multiBinding);
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

            // parallel, also overlapping case
            if (StartPoint.X == EndPoint.X && StartPointSec.X == EndPointSec.X ||
                StartPoint.Y == EndPoint.Y && StartPointSec.Y == EndPointSec.Y)
            {
                return false;
            }
            else
            {
                // orthogonal but maybe not intersected
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
                                    context.LineTo(new Point(interPoint.Point.X - 2.5, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - 3.5), new Point(interPoint.Point.X + 2.5, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y + 3.5), new Point(interPoint.Point.X - 2.5, interPoint.Point.Y), true, true);
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
                                    context.LineTo(new Point(interPoint.Point.X + 2.5, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - 3.5), new Point(interPoint.Point.X - 2.5, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y + 3.5), new Point(interPoint.Point.X + 2.5, interPoint.Point.Y), true, true);
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



        //private bool isConnectionPossible(Point p1, Point p2, QuadTreeLib.QuadTree<FakeNode> quad)
        //{
        //    if (p1.X != p2.X && p1.Y != p2.Y)
        //        throw new ArgumentException("only 90° allowed");

        //    if (p1.Y != p2.Y)
        //    {
        //        Point up = p2.Y < p1.Y ? p2 : p1;
        //        Point down = p2.Y < p1.Y ? p1 : p2;

        //        Panel parent = (Parent as Panel);
        //        foreach (var element in parent.Children)
        //        {
        //            PluginContainerView plug1 = element as PluginContainerView;
        //            if (plug1 == null)
        //                continue;
        //            Point pos = new Point((plug1.RenderTransform as TranslateTransform).X, (plug1.RenderTransform as TranslateTransform).Y);

        //            if (!isBetween(pos.X, pos.X + plug1.ActualWidth, up.X))
        //                continue;

        //            // case 1: one point is inside the plugin
        //            if (isBetween(pos.Y, pos.Y + plug1.ActualHeight, up.Y) ||
        //                isBetween(pos.Y, pos.Y + plug1.ActualHeight, down.Y))
        //            {
        //                return false;
        //            }

        //            // case 2: goes through
        //            if (pos.Y > up.Y && pos.Y + plug1.ActualHeight < down.Y)
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Point left = p2.X < p1.X ? p2 : p1;
        //        Point right = p2.X < p1.X ? p1 : p2;

        //        Panel parent = (Parent as Panel);
        //        foreach (var element in parent.Children)
        //        {
        //            PluginContainerView plug1 = element as PluginContainerView;
        //            if (plug1 == null)
        //                continue;
        //            Point pos = new Point((plug1.RenderTransform as TranslateTransform).X, (plug1.RenderTransform as TranslateTransform).Y);
        //            if (!isBetween(pos.Y, pos.Y + plug1.ActualHeight, left.Y))
        //                continue;
        //            // case 1: one point is inside the plugin
        //            if (isBetween(pos.X, pos.X + plug1.ActualWidth, left.X) ||
        //                isBetween(pos.X, pos.X + plug1.ActualWidth, right.X))
        //            {
        //                return false;
        //            }
        //            // case 2: goes through
        //            if (pos.X > left.X && pos.X + plug1.ActualWidth < right.X)
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        private bool isConnectionPossible(Point p1, Point p2, QuadTreeLib.QuadTree<FakeNode> quadTree)
        {
            if (p1.X != p2.X && p1.Y != p2.Y)
                throw new ArgumentException("only 90° allowed");

            System.Drawing.RectangleF queryRect;
            if (p1.Y != p2.Y)
            {
                Point up = p2.Y < p1.Y ? p2 : p1;
                Point down = p2.Y < p1.Y ? p1 : p2;

                queryRect = new System.Drawing.RectangleF((float)up.X, (float)up.Y, 1, (float)(down.Y - up.Y));
            }
            else
            {
                Point left = p2.X < p1.X ? p2 : p1;
                Point right = p2.X < p1.X ? p1 : p2;

                queryRect = new System.Drawing.RectangleF((float)left.X, (float)left.Y, (float)(right.X - left.X), 1);
            }
            return !quadTree.QueryAny(queryRect);
        }

        private bool performOrthogonalPointConnection(Node n1, Point p2, Node n3, List<Node> nodeList, QuadTreeLib.QuadTree<FakeNode> quadTreePlugins, QuadTreeLib.QuadTree<FakeNode> quadTreeLines)
        {
            //bool isHorizontal;
            //Point help1, help2;
            //foreach (FakeNode FKNode in getQueriesFromLine(n1.Point, p2, quadTreeLines, out isHorizontal))
            //{
            //    if (isHorizontal)
            //    {
            //        if (FKNode.Source != this.source)
            //            continue;

            //        help1 = new Point(n1.Point.X, 2);
            //        help2 = new Point(p
            //    }
            //    else
            //    {
            //        if (FKNode.Source != this.source)
            //            continue;
            //    }
            //}

            //foreach (FakeNode FKNode in getQueriesFromLine(p2, n3.Point, quadTreeLines, out isHorizontal))
            //{
            //    if (isHorizontal)
            //    {

            //    }
            //    else
            //    {

            //    }
            //}

            if (isConnectionPossible(n1.Point, p2, quadTreePlugins) && isConnectionPossible(p2, n3.Point, quadTreePlugins))
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

        private List<FakeNode> getQueriesFromLine(Point p1,Point p2,QuadTreeLib.QuadTree<FakeNode> quadTreeLines, out bool isHorizontal)
        {
            if (p1.X != p2.X && p1.Y != p2.Y)
                throw new ArgumentException("only 90° allowed");

            System.Drawing.RectangleF queryRect;

            if (p1.Y != p2.Y)
            {
                Point up = p2.Y < p1.Y ? p2 : p1;
                Point down = p2.Y < p1.Y ? p1 : p2;
                isHorizontal = false;

                queryRect = new System.Drawing.RectangleF((float)up.X, (float)up.Y, 1, (float)(down.Y - up.Y));
            }
            else
            {
                Point left = p2.X < p1.X ? p2 : p1;
                Point right = p2.X < p1.X ? p1 : p2;
                isHorizontal = true;

                queryRect = new System.Drawing.RectangleF((float)left.X, (float)left.Y, (float)(right.X - left.X), 1);
            }

            return quadTreeLines.Query(queryRect);
        }

        //private bool isSimpleOrthogonalConnectionPossible(Point p1, Point p2, QuadTreeLib.QuadTree<FakeNode> quadTree)
        //{
        //    if (p1.X != p2.X && p1.Y != p2.Y)
        //        throw new ArgumentException("only 90° allowed");

        //    List<FakeNode> list;
        //    System.Drawing.RectangleF queryRect;
        //    if (p1.Y != p2.Y)
        //    {
        //        Point up = p2.Y < p1.Y ? p2 : p1;
        //        Point down = p2.Y < p1.Y ? p1 : p2;

        //        queryRect = new System.Drawing.RectangleF((float)up.X, (float)up.Y, (float)5, (float)(down.Y - up.Y));
        //    }
        //    else
        //    {
        //        Point left = p2.X < p1.X ? p2 : p1;
        //        Point right = p2.X < p1.X ? p1 : p2;

        //        queryRect = new System.Drawing.RectangleF((float)left.X, (float)left.Y, (float)(right.X - left.X), (float)5);
        //    }

        //    list = quadTree.Query(queryRect);

        //    return !quadTree.Query(queryRect);
        //}

        private void performOrthogonalPointConnection(Node p1, Node p2, QuadTreeLib.QuadTree<FakeNode> quadTree)
        {
            if (isConnectionPossible(p1.Point, p2.Point, quadTree))
            {
                p1.Vertices.Add(p2);
                p2.Vertices.Add(p1);
            }
        }

        internal class FakeNode : QuadTreeLib.IHasRect
        {
            public System.Drawing.RectangleF Rectangle { get; set; }
            public ConnectorView Source { get; set; }
            public ConnectorView Target { get; set; }
        }

        private void makeOrthogonalPoints()
        {
            if (StartPointSource != null && EndPointSource != null && IsDragging == false)
            {
                List<Node> nodeList = new List<Node>();
                Panel parent = (Parent as Panel);

                // add start and end. Index will be 0 and 1
                Node startNode = new Node() { Point = cheat42(StartPoint , StartPointSource, 1)},
                    endNode = new Node() { Point = cheat42(EndPoint, EndPointSource, -1) };
                nodeList.Add(startNode);
                nodeList.Add(endNode);

                float actualWidth = (float)parent.ActualWidth, actualHeight = (float)parent.ActualWidth;
                //Consider zoom factor
                QuadTreeLib.QuadTree<FakeNode> quadTreePlugins = new QuadTreeLib.QuadTree<FakeNode>
                    (new System.Drawing.RectangleF(-actualWidth, -actualHeight, actualWidth * 5, actualHeight * 5));

                QuadTreeLib.QuadTree<FakeNode> quadTreeLines = new QuadTreeLib.QuadTree<FakeNode>
                    (new System.Drawing.RectangleF(-actualWidth, -actualHeight, actualWidth * 5, actualHeight * 5));



                //foreach (var element in parent.Children)
                //{
                //    if (element is PluginContainerView)
                //    {
                //        PluginContainerView p1 = element as PluginContainerView;
                //        foreach (var routPoint in p1.RoutingPoints)
                //        {
                //            nodeList.Add(new Node() { Point = routPoint });
                //        }
                //        quadTree.Insert(new FakeNode() { Rectangle = new System.Drawing.RectangleF((float)(p1.RenderTransform as TranslateTransform).X,
                //                                                                                    (float)(p1.RenderTransform as TranslateTransform).Y,
                //                                                                                    (float)p1.ActualWidth,
                //                                                                                    (float)p1.ActualHeight)});
                //    }
                //}
                for (int routPoint = 0; routPoint < 4; ++routPoint)
                {
                    foreach (var element in parent.Children)
                    {
                        if (element is PluginContainerView)
                        {
                            PluginContainerView p1 = element as PluginContainerView;
                            nodeList.Add(new Node() { Point = p1.GetRoutingPoint(routPoint) });
                            if (routPoint == 0)
                            {
                                quadTreePlugins.Insert(new FakeNode()
                                {
                                    Rectangle = new System.Drawing.RectangleF((float)p1.GetPosition().X,
                                                                               (float)p1.GetPosition().Y + (float)p1.ControlPanel.ActualHeight,
                                                                               (float)p1.PluginBase.ActualWidth,
                                                                               (float)p1.PluginBase.ActualHeight - (float)p1.ControlPanel.ActualHeight)
                                });
                            }
                        }

                        if (routPoint != 0)
                            continue;

                        if (element is CryptoLineView)
                        {
                            CryptoLineView l1 = element as CryptoLineView;
                            foreach (FromTo fromto in l1.pointList)
                            {
                                Point p1 = fromto.From, p2 = fromto.To;
                                if (p1.Y != p2.Y)
                                {
                                    Point up = p2.Y < p1.Y ? p2 : p1;
                                    Point down = p2.Y < p1.Y ? p1 : p2;

                                    quadTreeLines.Insert(new FakeNode()
                                    {
                                        Source = l1.StartPointSource,
                                        Target = l1.EndPointSource,
                                        Rectangle = new System.Drawing.RectangleF((float)up.X, (float)up.Y, 1, (float)(down.Y - up.Y))
                                    });
                                }
                                else
                                {
                                    Point left = p2.X < p1.X ? p2 : p1;
                                    Point right = p2.X < p1.X ? p1 : p2;

                                    quadTreeLines.Insert(new FakeNode()
                                    {
                                        Source = l1.StartPointSource,
                                        Target = l1.EndPointSource,
                                        Rectangle = new System.Drawing.RectangleF((float)left.X, (float)left.Y, (float)(right.X - left.X), 1)
                                    });
                                }
                            }

                        }
                    }
                }

                // connect points
                int loopCount = nodeList.Count;
                const int performanceTradeoffAt = 10;

                LinkedList<Node> path = null;

                for (int i = 0; i < loopCount; ++i)
                {
                    if (performanceTradeoffAt != 0 &&
                           i == performanceTradeoffAt)
                    {
                        StackFrameDijkstra.Dijkstra<Node> dijkstra = new StackFrameDijkstra.Dijkstra<Node>();
                        path = dijkstra.findPath(nodeList, startNode, endNode);
                        if (path != null)
                        {
                            break;
                        }
                    }

                    var p1 = nodeList[i];
                    // TODO: inner loop restriction! n²-n!
                    // is k=i instead of k=0 correct?
                    for (int k = i; k < loopCount; ++k)
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
                            performOrthogonalPointConnection(p1, p2, quadTreePlugins);
                        }
                        else
                        {
                            Point help = new Point(p1.Point.X, p2.Point.Y);

                            if (!performOrthogonalPointConnection(p1, help, p2, nodeList, quadTreePlugins, quadTreeLines))
                            {
                                help = new Point(p2.Point.X, p1.Point.Y);
                                if (!performOrthogonalPointConnection(p1, help, p2, nodeList, quadTreePlugins, quadTreeLines))
                                {
                                    // optional todo: double edge helping routes
                                }
                            }

                        }
                    }
                }

                if (path == null)
                {
                    StackFrameDijkstra.Dijkstra<Node> dijkstra = new StackFrameDijkstra.Dijkstra<Node>();
                    path = dijkstra.findPath(nodeList, startNode, endNode);
                }

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
                    this.pointList.Add(new FromTo(prevPoint, EndPoint));
                    return;
                }
            }
            //Failsafe
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

        private static Point cheat42(Point EndPoint, ConnectorView EndPointSource, int flipper)
        {
            double xoffset = 0;
            double yoffset = 0;
            double baseoffset = 15;
            switch (EndPointSource.Orientation)
            {
                case ConnectorOrientation.East:
                    xoffset = baseoffset;
                    break;
                case ConnectorOrientation.West:
                    xoffset = -baseoffset;
                    break;
                case ConnectorOrientation.North:
                    yoffset = -baseoffset;
                    break;
                case ConnectorOrientation.South:
                    yoffset = baseoffset;
                    break;
            }
            //xoffset *= flipper;
            //yoffset *= flipper;
            return new Point(EndPoint.X + xoffset, EndPoint.Y + yoffset);
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


        private ConnectorView startPointSource;
        public ConnectorView StartPointSource 
        { 
            get { return startPointSource; } 
            set 
            {
                startPointSource = value; 
                if (endPointSource == null || startPointSource == null)
                    return;
                makeBinding(startPointSource, endPointSource); 
            } 
        }

        private ConnectorView endPointSource;
        public ConnectorView EndPointSource 
        { 
            get { return endPointSource; } 
            set 
            { 
                endPointSource = value;
                if (endPointSource == null || startPointSource == null)
                    return;
                makeBinding(startPointSource, endPointSource); 
            } 
        }
    }

    public class MultiDragValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool a = (bool)values[0], b = (bool)values[1];
            if (a == true || b == true)
                return true;
            else
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
