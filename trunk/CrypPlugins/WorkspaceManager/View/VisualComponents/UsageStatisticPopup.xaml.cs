using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using WorkspaceManager.Model;
using WorkspaceManager.View.Base;
using WorkspaceManager.View.Visuals;
using WorkspaceManagerModel.Model.Operations;
using System.Windows.Media.Animation;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for UsageStatisticPopup.xaml
    /// </summary>
    public partial class UsageStatisticPopup : Popup
    {
        private readonly WorkspaceModel _model;
        private Adorner _currentAdorner;

        public static readonly DependencyProperty SelectedConnectorProperty = DependencyProperty.Register("SelectedConnector",
            typeof(ConnectorVisual),
            typeof(UsageStatisticPopup),
            new FrameworkPropertyMetadata(null));

        public ConnectorVisual SelectedConnector
        {
            get { return (ConnectorVisual)GetValue(SelectedConnectorProperty); }
            set
            {
                SetValue(SelectedConnectorProperty, value);
            }
        }
        DispatcherTimer timer = new DispatcherTimer();

        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register("Suggestions",
            typeof(ObservableCollection<SuggestionContainer>),
            typeof(UsageStatisticPopup),
            new FrameworkPropertyMetadata(null, null));

        private EditorVisual _editor;
        private Point position;

        public ObservableCollection<SuggestionContainer> Suggestions
        {
            get { return (ObservableCollection<SuggestionContainer>)GetValue(SuggestionsProperty); }
            set
            {
                SetValue(SuggestionsProperty, value);
            }
        }

        public UsageStatisticPopup(WorkspaceModel model)
        {
            InitializeComponent();
            _model = model;
            _editor = (EditorVisual)model.MyEditor.Presentation;
            this.Loaded += new RoutedEventHandler(ComponentConnectionStatisticsPopUpLoaded);
            this.MouseLeave += new MouseEventHandler(ComponentConnectionStatisticsPopUpMouseLeave);
            this.Opened += new EventHandler(ComponentConnectionStatisticsPopUpOpened);
            this.Closed += new EventHandler(ComponentConnectionStatisticsPopUpClosed);
            _editor.SelectedConnectorChanged += new EventHandler(_editorSelectedConnectorChanged);

            Suggestions = new ObservableCollection<SuggestionContainer>();
        }

        void ComponentConnectionStatisticsPopUpClosed(object sender, EventArgs e)
        {

        }

        void _editorSelectedConnectorChanged(object sender, EventArgs e)
        {
            var win = AdornerLayer.GetAdornerLayer((FrameworkElement)Window.GetWindow(this).Content);

            if (this._editor.SelectedConnector == null)
            {
                timer.Stop();
                if(_currentAdorner != null)
                    win.Remove(_currentAdorner);
                return;
            }
            SelectedConnector = this._editor.SelectedConnector;

            var list = ComponentConnectionStatistics.GetMostlyUsedComponentsFromConnector(SelectedConnector.Model.PluginModel.PluginType, SelectedConnector.Model.GetName());
            if (list == null)
                return;

            _currentAdorner = new CricularLineProgressAdorner((FrameworkElement)Window.GetWindow(this).Content, new CricularLineProgress());
            win.Add(_currentAdorner);
            Suggestions = new ObservableCollection<SuggestionContainer>();
            foreach (var componentConnector in list)
            {
                Type t = componentConnector.Component;
                string name = componentConnector.ConnectorName;
                Suggestions.Add(new SuggestionContainer(name, t));
            }

            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += delegate(object o, EventArgs args)
                              {
                                  timer.Stop();
                                  win.Remove(_currentAdorner);
                                  IsOpen = true;
                              };
            timer.Start();
        }

        void ComponentConnectionStatisticsPopUpOpened(object sender, EventArgs e)
        {
            position = Util.MouseUtilities.CorrectGetPosition(_editor.panel);
        }

        void ComponentConnectionStatisticsPopUpMouseLeave(object sender, MouseEventArgs e)
        {
            IsOpen = false;

        }

        void SelectedItem(object sender, SelectionChangedEventArgs e)
        {
            //IsOpen = false;
        }

        void ComponentConnectionStatisticsPopUpLoaded(object sender, RoutedEventArgs e)
        {
            _editor.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(_editorMouseLeftButtonUp);
        }

        void _editorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseOver)
            {
                IsOpen = false;
            }
            if (TopUsages.SelectedItem != null && IsMouseOver)
            {

                try
                {
                    var x = (SuggestionContainer)TopUsages.SelectedItem;
                    PluginModel pluginModel = (PluginModel)_model.ModifyModel(new NewPluginModelOperation(position, 0, 0, x.ComponentType));
                    _editor.AddBinComponentVisual(pluginModel, 0);
                    ConnectorVisual connector = null;
                    foreach (var con in pluginModel.GetInputConnectors())
                    {
                        if (con.GetName() == x.ConnectorName)
                            connector = (ConnectorVisual)con.UpdateableView;
                    }
                    foreach (var con in pluginModel.GetOutputConnectors())
                    {
                        if (con.GetName() == x.ConnectorName)
                            connector = (ConnectorVisual)con.UpdateableView;
                    }

                    if (connector == null)
                        throw new Exception();

                    var input = SelectedConnector.Model.Outgoing == true ? connector.Model : SelectedConnector.Model;
                    var output = SelectedConnector.Model.Outgoing == false ? connector.Model : SelectedConnector.Model;
                    ConnectionModel connectionModel = (ConnectionModel)_model.ModifyModel(new NewConnectionModelOperation(
                        output,
                        input,
                        output.ConnectorType));
                    _editor.AddConnection(SelectedConnector, connector, connectionModel);

                    position.X += 50;
                    position.Y += 50;
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

    }

    public class SuggestionContainer
    {
        public SuggestionContainer(string connectorName, Type componentType)
        {
            if (componentType == null) throw new ArgumentNullException("componentType");

            ComponentType = componentType;
            _plugin = componentType.CreateComponentInstance();
            _icon = _plugin.GetImage(0);

            ConnectorName = connectorName;

        }

        public SuggestionContainer()
        {
            // TODO: Complete member initialization
        }

        private ICrypComponent _plugin;
        private Image _icon;

        public Image Icon
        {
            get { return _icon; }
        }

        public string ConnectorName { get; set; }

        public Type ComponentType { get; set; }

        public string Test { get; set; }
    }

    public sealed class CricularLineProgress : Shape
    {
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register("StartPoint",
   typeof(Point), typeof(CricularLineProgress), new FrameworkPropertyMetadata(new Point(0, 0),
       FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Point StartPoint
        {
            get { return (Point)GetValue(StartPointProperty); }
            set
            {
                SetValue(StartPointProperty, value);
            }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress",
typeof(double), typeof(CricularLineProgress), new FrameworkPropertyMetadata((double)0, null));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set
            {
                SetValue(ProgressProperty, value);
            }
        }

        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius",
typeof(Double), typeof(CricularLineProgress), new FrameworkPropertyMetadata((Double)40, OnRadiusChanged));

        public Double Radius
        {
            get { return (Double)GetValue(RadiusProperty); }
            set
            {
                SetValue(RadiusProperty, value);
            }
        }

        private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CricularLineProgress c = (CricularLineProgress)d;
            c.InvalidateVisual();
        }

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

        public CricularLineProgress()
        {
            Stroke = Brushes.RoyalBlue;
            Opacity = 0.7;
            StrokeThickness = 3;
        }

        static double kappa = 4 * ((Math.Sqrt(2) - 1) / 3);

        void ellipse(double xm, double ym, double r, StreamGeometryContext context)
        {
            double ctrl_ell_x1 = Radius * kappa;
            double ctrl_ell_y1 = Radius;

            double ctrl_ell_x2 = Radius;
            double ctrl_ell_y2 = Radius * kappa;

            double ell_x = Radius * Math.Sin(2 * Math.PI * 0.25);
            double ell_y = Radius * Math.Cos(2 * Math.PI * 0.25);

            var ctrlPoint1 = new Point(xm + ctrl_ell_x1, ym - ctrl_ell_y1);
            var ctrlPoint2 = new Point(xm + ctrl_ell_x2, ym - ctrl_ell_y2);
            var point = new Point(xm + ell_x, ym - ell_y);

            context.BezierTo(ctrlPoint1, ctrlPoint2, point, true, true);
            //--------------------------------------------------------------------------

            ctrl_ell_x2 = Radius * kappa;
            ctrl_ell_y2 = -Radius;

            ctrl_ell_x1 = Radius;
            ctrl_ell_y1 = -Radius * kappa;

            ell_x = Radius * Math.Sin(2 * Math.PI * 0.50);
            ell_y = Radius * Math.Cos(2 * Math.PI * 0.50);

            ctrlPoint1 = new Point(xm + ctrl_ell_x1, ym - ctrl_ell_y1);
            ctrlPoint2 = new Point(xm + ctrl_ell_x2, ym - ctrl_ell_y2);
            point = new Point(xm + ell_x, ym - ell_y);

            context.BezierTo(ctrlPoint1, ctrlPoint2, point, true, true);
            //--------------------------------------------------------------------------
            ctrl_ell_x1 = -Radius * kappa;
            ctrl_ell_y1 = -Radius;

            ctrl_ell_x2 = -Radius;
            ctrl_ell_y2 = -Radius * kappa;

            ell_x = Radius * Math.Sin(2 * Math.PI * 0.75);
            ell_y = Radius * Math.Cos(2 * Math.PI * 0.75);

            ctrlPoint1 = new Point(xm + ctrl_ell_x1, ym - ctrl_ell_y1);
            ctrlPoint2 = new Point(xm + ctrl_ell_x2, ym - ctrl_ell_y2);
            point = new Point(xm + ell_x, ym - ell_y);

            context.BezierTo(ctrlPoint1, ctrlPoint2, point, true, true);
            //--------------------------------------------------------------------------
            ctrl_ell_x2 = -Radius * kappa;
            ctrl_ell_y2 = Radius;

            ctrl_ell_x1 = -Radius;
            ctrl_ell_y1 = Radius * kappa;

            ell_x = Radius * Math.Sin(2 * Math.PI * 1);
            ell_y = Radius * Math.Cos(2 * Math.PI * 1);

            ctrlPoint1 = new Point(xm + ctrl_ell_x1, ym - ctrl_ell_y1);
            ctrlPoint2 = new Point(xm + ctrl_ell_x2, ym - ctrl_ell_y2);
            point = new Point(xm + ell_x, ym - ell_y);

            context.BezierTo(ctrlPoint1, ctrlPoint2, point, true, true);
            //--------------------------------------------------------------------------
        }

        private void internalGeometryDraw(StreamGeometryContext context)
        {
            var realSP = new Point(StartPoint.X, StartPoint.Y - Radius);
            context.BeginFigure(realSP, true, false);
            ellipse(StartPoint.X, StartPoint.Y, Radius, context);
        }
    }

    public class CricularLineProgressAdorner : Adorner
    {
        private readonly UIElement _adornedElement;
        private readonly CricularLineProgress _clp;
        private Window _win;

        public CricularLineProgressAdorner(UIElement adornedElement, CricularLineProgress clp)
            : base(adornedElement)
        {
            _adornedElement = adornedElement;
            _clp = clp;
            _win = Window.GetWindow(adornedElement);
            _win.PreviewMouseMove += new MouseEventHandler(_win_PreviewMouseMove);
            var p = Mouse.GetPosition(_win);
            _clp.RenderTransform = new TranslateTransform(p.X, p.Y);

            DoubleAnimation anim = new DoubleAnimation(50, 0, new Duration(TimeSpan.FromSeconds(3)));
            Storyboard sb = new Storyboard();
            Storyboard.SetTarget(anim, _clp);
            Storyboard.SetTargetProperty(anim, new PropertyPath(CricularLineProgress.RadiusProperty));  
            sb.Children.Add(anim);
            sb.Begin();
        }

        void _win_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var trans = (TranslateTransform)_clp.RenderTransform;
            trans.X = e.GetPosition(_win).X;
            trans.Y = e.GetPosition(_win).Y;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this._clp.Measure(constraint);
            if (double.IsInfinity(constraint.Height))
            {
                constraint.Height = this._adornedElement.DesiredSize.Height;
            }
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this._clp.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this._clp;
        }
    }
}
