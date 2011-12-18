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
using WorkspaceManagerModel.Model.Interfaces;
using WorkspaceManager.Model;
using WorkspaceManager.View.BinVisual;
using WorkspaceManager.View.VisualComponents.StackFrameDijkstra;
using WorkspaceManagerModel.Model.Operations;
using System.ComponentModel;
using System.Collections.ObjectModel;
using WorkspaceManager.View.Base;
using System.Windows.Controls.Primitives;
using WorkspaceManager.View.Base.Interfaces;

namespace WorkspaceManager.View.VisualComponents.CryptoLineView
{
    /// <summary>
    /// Interaction logic for CryptoLineView.xaml
    /// </summary>
    public partial class CryptoLineView : UserControl, INotifyPropertyChanged, IUpdateableView
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private InternalCryptoLineView line = null;
        private ConnectionModel model = null;
        public ConnectionModel Model
        {
            get { return model; }
            set
            {
                model = value;

                if (model.UpdateableView == null)
                    model.UpdateableView = this;
            }
        }
        private BinConnectorVisual source;
        private BinConnectorVisual target;

        public static readonly DependencyProperty PathGeoProperty = DependencyProperty.Register("PathGeo", typeof(PathGeometry),
            typeof(CryptoLineView), new FrameworkPropertyMetadata(null));

        public PathGeometry PathGeo
        {
            get { return (PathGeometry)base.GetValue(PathGeoProperty); }
            set
            {
                base.SetValue(PathGeoProperty, value);
            }
        }

        public static readonly DependencyProperty LineProperty = DependencyProperty.Register("Line", typeof(InternalCryptoLineView),
            typeof(CryptoLineView), new FrameworkPropertyMetadata(null));

        public InternalCryptoLineView Line
        {
            get { return (InternalCryptoLineView)base.GetValue(LineProperty); }
            set
            {
                base.SetValue(LineProperty, value);
            }
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool),
            typeof(CryptoLineView), new FrameworkPropertyMetadata(null));

        public bool IsSelected
        {
            get { return (bool)base.GetValue(IsSelectedProperty); }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }

        public CryptoLineView(ObservableCollection<UIElement> visuals)
        {
            InitializeComponent();
            Canvas.SetZIndex(this, -1);
            Line = new InternalCryptoLineView(visuals);
            Line.PointList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(PointListCollectionChanged);
            Line.ComputationDone += new EventHandler(LineComputationDone);
        }

        public CryptoLineView(ConnectionModel model, BinConnectorVisual source, BinConnectorVisual target, ObservableCollection<UIElement> visuals)
        {
            // TODO: Complete member initialization
            InitializeComponent();
            Canvas.SetZIndex(this, -1);
            this.Model = model;
            this.source = source;
            this.target = target;

            Line = new InternalCryptoLineView(model, source, target, visuals);
            Line.SetBinding(InternalCryptoLineView.StartPointProperty, Util.CreateConnectorBinding(source, this));
            Line.SetBinding(InternalCryptoLineView.EndPointProperty, Util.CreateConnectorBinding(target, this));
            Line.SetBinding(InternalCryptoLineView.IsDraggedProperty, Util.CreateIsDraggingBinding(
                new BinComponentVisual[] { target.WindowParent, source.WindowParent }));
            Line.PointList.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(PointListCollectionChanged);
            Line.ComputationDone += new EventHandler(LineComputationDone);
        }

        void LineComputationDone(object sender, EventArgs e)
        {
            PathGeometry geo = new PathGeometry();
            PathFigure myPathFigure = new PathFigure();
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigure.StartPoint = Line.PointList[0].From;

            foreach (var fromto in Line.PointList)
            {
                LineSegment myLineSegment = new LineSegment();
                myLineSegment.Point = fromto.To;
                myPathSegmentCollection.Add(myLineSegment);
            }

            myPathFigure.Segments = myPathSegmentCollection;
            myPathFigureCollection.Add(myPathFigure);

            geo.Figures = myPathFigureCollection;
            PathGeo = geo;
        }

        void PointListCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {


        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void ThumbDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var t = (Thumb)sender;
            var ft = t.DataContext as FromTo;
            if (ft == null)
                return;

            Line.IsEditingPoint = true;
            ft.To = new Point(ft.To.X + e.HorizontalChange, ft.To.Y + e.VerticalChange);
            Line.InvalidateAllLines();

        }

        private void ThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            //Line.IsEditingPoint = false;
        }

        //private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        //{
        //    model.UpdateableView = this;
        //    if (this.model != null && !((WorkspaceManagerClass)this.model.WorkspaceModel.MyEditor).isExecuting())
        //    {
        //        this.model.WorkspaceModel.ModifyModel(new DeleteConnectionModelOperation(this.model));
        //    }
        //}

        public void update()
        {
            //throw new NotImplementedException();
        }
    }

    	public sealed class InternalCryptoLineView : Shape, IUpdateableView
    {
        #region Variables

        private IntersectPoint intersectPoint;
        private static double baseoffset = 15;
        public event EventHandler ComputationDone;

        private ObservableCollection<FromTo> pointList = new ObservableCollection<FromTo>();
        public ObservableCollection<FromTo> PointList
        {
            get { return pointList; }
        }

        private ConnectionModel model = null;
        public ConnectionModel Model
        {
            get { return model; }
            set 
            { 
                model = value;

                if (model.UpdateableView == null)
                    model.UpdateableView = this;

                if (model.PointList != null)
                {
                    PointList.Clear();
                    for (int i = 0; i <= model.PointList.Count()-2;i++)
                    {
                        PointList.Add(new FromTo(model.PointList[i], model.PointList[i+1]));
                    }
                    HasComputed = true;
                }
            }
        }

        private ObservableCollection<UIElement> visuals = null;
        public ObservableCollection<UIElement> Visuals
        {
            get { return visuals; }
            private set { visuals = value; }
        }
        private static double offset = 6;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register("StartPoint",
            typeof(Point), typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0), 
                FrameworkPropertyMetadataOptions.AffectsRender |FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint",
            typeof(Point), typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0), 
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        
		#endregion

		#region CLR Properties

        public static readonly DependencyProperty IsDraggedProperty = DependencyProperty.Register("IsDragged", typeof(bool),
            typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsDraggedValueChanged)));

        public bool IsDragged
        {
            get { return (bool)base.GetValue(IsDraggedProperty); }
            set
            {
                base.SetValue(IsDraggedProperty, value);
            }
        }

        public static readonly DependencyProperty HasComputedProperty = DependencyProperty.Register("HasComputed", typeof(bool),
            typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnHasComputedChanged)));

        public bool HasComputed
        {
            get { return (bool)base.GetValue(HasComputedProperty); }
            set
            {
                base.SetValue(HasComputedProperty, value);
            }
        }

        public static readonly DependencyProperty IsEditingPointProperty = DependencyProperty.Register("IsEditingPoint", typeof(bool),
            typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(false));

        public bool IsEditingPoint
        {
            get { return (bool)base.GetValue(IsEditingPointProperty); }
            set
            {
                base.SetValue(IsEditingPointProperty, value);
            }
        }

        private static void OnIsDraggedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InternalCryptoLineView l = (InternalCryptoLineView)d;
            l.HasComputed = false;
            l.IsEditingPoint = false;
            if (l.IsDragged == false)
                l.InvalidateVisual();
        }

        private static void OnHasComputedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InternalCryptoLineView l = (InternalCryptoLineView)d;
            if (l.model == null)
                return;

            if (l.HasComputed == true)
            {
                l.Model.PointList = new List<Point>();
                foreach (var p in l.PointList)
                {
                    l.Model.PointList.Add(p.From);
                }
                l.Model.PointList.Add(l.PointList.Last().To);
            }
        }

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

        public InternalCryptoLineView(ObservableCollection<UIElement> visuals)
        {
            Stroke = Brushes.Black;
            StrokeThickness = 2;
            this.Visuals = visuals;
        }

            
        public InternalCryptoLineView(ConnectionModel connectionModel, BinConnectorVisual source,
            BinConnectorVisual target, ObservableCollection<UIElement> visuals)
        {
            this.Loaded += new RoutedEventHandler(CryptoLineView_Loaded);
            this.Model = connectionModel;
            this.StartPointSource = source;
            this.EndPointSource = target;
            this.Visuals = visuals;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        public void InvalidateAllLines()
        {
            var col = Visuals.OfType<CryptoLineView>();
            foreach (var x in col)
            {
                x.Line.InvalidateVisual();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs args)
        {

        }

        void CryptoLineView_Loaded(object sender, RoutedEventArgs e)
        {
            Color color = ColorHelper.GetLineColor(Model.ConnectionType);
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
            foreach (var element in Visuals)
            {
                if (element is CryptoLineView)
                {
                    CryptoLineView result = element as CryptoLineView;

                    if (result.Line.Equals(this))
                        continue;

                    foreach (FromTo fromTo in PointList)
                    {
                        fromTo.Intersection.Clear();
                        foreach (FromTo resultFromTo in result.Line.PointList)
                        {
                            if (findIntersection(fromTo.From, fromTo.To, resultFromTo.From, resultFromTo.To))
                            {
                                fromTo.Intersection.Add(intersectPoint);
                            }
                        }
                    }
                }
            }

            context.BeginFigure(StartPoint, true, false);

            foreach (FromTo fromTo in PointList)
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
            public BinConnectorVisual Source { get; set; }
            public BinConnectorVisual Target { get; set; }
        }

        private void makeOrthogonalPoints()
        {
            bool failed = false;
            if (!IsEditingPoint)
            {
                //Problem Liegt hier. Der Elsefall wird durchlaufen obwohl computed ist.
                if (StartPointSource != null && Model != null && EndPointSource != null && !IsDragged && !HasComputed)
                {
                    List<Node> nodeList = new List<Node>();
                    FrameworkElement parent = Model.WorkspaceModel.MyEditor.Presentation;

                    // add start and end. Index will be 0 and 1 
                    Node startNode = new Node() { Point = cheat42(StartPoint, StartPointSource, 1) },
                        endNode = new Node() { Point = cheat42(EndPoint, EndPointSource, -1) };
                    nodeList.Add(startNode);
                    nodeList.Add(endNode);

                    float actualWidth = (float)parent.ActualWidth, actualHeight = (float)parent.ActualWidth;
                    //Consider zoom factor
                    QuadTreeLib.QuadTree<FakeNode> quadTreePlugins = new QuadTreeLib.QuadTree<FakeNode>
                        (new System.Drawing.RectangleF(-actualWidth, -actualHeight, actualWidth * 5, actualHeight * 5));

                    QuadTreeLib.QuadTree<FakeNode> quadTreeLines = new QuadTreeLib.QuadTree<FakeNode>
                        (new System.Drawing.RectangleF(-actualWidth, -actualHeight, actualWidth * 5, actualHeight * 5));

                    for (int routPoint = 0; routPoint < 4; ++routPoint)
                    {
                        foreach (var element in Visuals)
                        {
                            if (element is BinComponentVisual)
                            {
                                IRouting p1 = element as IRouting;
                                nodeList.Add(new Node() { Point = p1.GetRoutingPoint(routPoint) });
                                if (routPoint == 0)
                                {
                                    quadTreePlugins.Insert(new FakeNode()
                                    {
                                        Rectangle = new System.Drawing.RectangleF((float)p1.Position.X,
                                                                                   (float)p1.Position.Y,
                                                                                   (float)p1.ObjectSize.X,
                                                                                   (float)p1.ObjectSize.Y)
                                    });
                                }
                            }

                            if (routPoint != 0)
                                continue;

                            if (element is CryptoLineView)
                            {
                                CryptoLineView l1 = element as CryptoLineView;
                                foreach (FromTo fromto in l1.Line.PointList)
                                {
                                    Point p1 = fromto.From, p2 = fromto.To;
                                    if (p1.Y != p2.Y)
                                    {
                                        Point up = p2.Y < p1.Y ? p2 : p1;
                                        Point down = p2.Y < p1.Y ? p1 : p2;

                                        quadTreeLines.Insert(new FakeNode()
                                        {
                                            Source = l1.Line.StartPointSource,
                                            Target = l1.Line.EndPointSource,
                                            Rectangle = new System.Drawing.RectangleF((float)up.X, (float)up.Y, 1, (float)(down.Y - up.Y))
                                        });
                                    }
                                    else
                                    {
                                        Point left = p2.X < p1.X ? p2 : p1;
                                        Point right = p2.X < p1.X ? p1 : p2;

                                        quadTreeLines.Insert(new FakeNode()
                                        {
                                            Source = l1.Line.StartPointSource,
                                            Target = l1.Line.EndPointSource,
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
                        PointList.Clear();
                        Point prevPoint = StartPoint;

                        foreach (var i in path)
                        {
                            Point thisPoint = i.Point;
                            this.PointList.Add(new FromTo(prevPoint, thisPoint));
                            prevPoint = thisPoint;
                        }
                        this.PointList.Add(new FromTo(prevPoint, EndPoint));
                        HasComputed = true;
                        raiseComputationDone();
                        return;
                    }
                    failed = true;     
                }
                //Failsafe
                if (IsDragged || failed)
                {
                    if (StartPoint.X < EndPoint.X)
                    {
                        PointList.Clear();
                        PointList.Add(new FromTo(StartPoint, new Point((EndPoint.X + StartPoint.X) / 2, StartPoint.Y)));
                        PointList.Add(new FromTo(new Point((EndPoint.X + StartPoint.X) / 2, StartPoint.Y), new Point((EndPoint.X + StartPoint.X) / 2, EndPoint.Y)));
                        PointList.Add(new FromTo(new Point((EndPoint.X + StartPoint.X) / 2, EndPoint.Y), EndPoint));
                        raiseComputationDone();
                    }
                    else
                    {
                        if (StartPoint.X > EndPoint.X)
                        {
                            PointList.Clear();
                            PointList.Add(new FromTo(StartPoint, new Point((StartPoint.X + EndPoint.X) / 2, StartPoint.Y)));
                            PointList.Add(new FromTo(new Point((StartPoint.X + EndPoint.X) / 2, StartPoint.Y), new Point((StartPoint.X + EndPoint.X) / 2, EndPoint.Y)));
                            PointList.Add(new FromTo(new Point((StartPoint.X + EndPoint.X) / 2, EndPoint.Y), EndPoint));
                            raiseComputationDone();
                        }
                    }
                }
            }
            else { raiseComputationDone(); }
        }

        private void raiseComputationDone()
        {
            if (this.ComputationDone != null)
                ComputationDone.Invoke(this, null);
        }

        private static Point cheat42(Point EndPoint, BinConnectorVisual EndPointSource, int flipper)
        {
            double xoffset = 0;
            double yoffset = 0;
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

        private Brush ActiveColorBrush;
        private Brush NonActiveColorBrush;

        public void update()
        {
            if (this.Model.Active)
            {
                if (ActiveColorBrush == null)
                {
                    Color ActiveColor = ColorHelper.GetLineColor(this.Model.ConnectionType);
                    ActiveColor.ScR = (ActiveColor.ScR * 2f < 255 ? ActiveColor.ScR * 2f : 255f);
                    ActiveColor.ScG = (ActiveColor.ScG * 2f < 255 ? ActiveColor.ScG * 2f : 255f);
                    ActiveColor.ScB = (ActiveColor.ScB * 2f < 255 ? ActiveColor.ScB * 2f : 255f);
                    ActiveColorBrush = new SolidColorBrush(ActiveColor);
                }
                Stroke = ActiveColorBrush;                
                StrokeThickness = 4;
            }
            else
            {
                Reset();
            }
        }

        #endregion

        internal void Reset()
        {
            if (NonActiveColorBrush == null)
            {
                NonActiveColorBrush = new SolidColorBrush(ColorHelper.GetLineColor(Model.ConnectionType));
            }
            Stroke = NonActiveColorBrush;
            StrokeThickness = 2;
        }


        private BinConnectorVisual startPointSource;
        public BinConnectorVisual StartPointSource 
        { 
            get { return startPointSource; } 
            set 
            {
                startPointSource = value;  
            } 
        }

        private BinConnectorVisual endPointSource;

        public BinConnectorVisual EndPointSource 
        { 
            get { return endPointSource; } 
            set 
            { 
                endPointSource = value;
            } 
        }
    }

    public class MultiDragValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
            /*bool a = (bool)values[0], b = (bool)values[1];
            if (a == true || b == true)
                return true;
            else
                return false;*/
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
