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
    public partial class UsageStatisticPopup : UserControl
    {
        private Adorner _currentAdorner;
        private ControlAdorner adorner;
        private AdornerLayer currentAdornerlayer;
        private Window window;
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

        public bool IsOpen = false;

        DispatcherTimer timer= new DispatcherTimer();
        DispatcherTimer timer2 = new DispatcherTimer();

        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register("Suggestions",
            typeof(ObservableCollection<SuggestionContainer>),
            typeof(UsageStatisticPopup),
            new FrameworkPropertyMetadata(null, null));

        private EditorVisual _editor;
        private Point position;
        private EditorVisual editorVisual;

        public ObservableCollection<SuggestionContainer> Suggestions
        {
            get { return (ObservableCollection<SuggestionContainer>)GetValue(SuggestionsProperty); }
            set
            {
                SetValue(SuggestionsProperty, value);
            }
        }

        public UsageStatisticPopup(EditorVisual editor)
        {
            InitializeComponent();
            _editor = editor;
            _editor.SelectedConnectorChanged += new EventHandler(_editorSelectedConnectorChanged);
            Suggestions = new ObservableCollection<SuggestionContainer>();
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(UsageStatisticPopup_MouseLeftButtonDown);
        }

        void UsageStatisticPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TopUsages.SelectedItem != null)
            {
                try
                {
                    var x = (SuggestionContainer)TopUsages.SelectedItem;
                    PluginModel pluginModel = (PluginModel)_editor.Model.ModifyModel(new NewPluginModelOperation(position, 0, 0, x.ComponentType));
                    _editor.AddComponentVisual(pluginModel);
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

                    var input = SelectedConnector.Model.Outgoing == true ? connector : SelectedConnector;
                    var output = SelectedConnector.Model.Outgoing == false ? connector : SelectedConnector;
                    ConnectionModel connectionModel = (ConnectionModel)_editor.Model.ModifyModel(new NewConnectionModelOperation(
                        output.Model,
                        input.Model,
                        output.Model.ConnectorType));
                    _editor.AddConnectionVisual(output, input, connectionModel);

                    position.X += 50;
                    position.Y += 50;
                }
                catch (Exception)
                {
                    return;
                }
                Close();
            }
            //e.Handled = true;
        }


        public void Open()
        {
            if (!Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_ShowComponentConnectionProposition)
                return;

            if (_editor.SelectedConnector != null && currentAdornerlayer == null)
            {
                window = Window.GetWindow(_editor);
                currentAdornerlayer = AdornerLayer.GetAdornerLayer((FrameworkElement)window.Content);
                if (adorner != null)
                    adorner.RemoveRef();
                adorner = new ControlAdorner((FrameworkElement)window.Content, this);
                window.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(window_MouseLeftButtonUp);
                position = Util.MouseUtilities.CorrectGetPosition(_editor.panel);

                var p = Mouse.GetPosition(window);
                currentAdornerlayer.Add(adorner);
                this.RenderTransform = new TranslateTransform(p.X-620, p.Y-350);
                IsOpen = true;
            }
        }

        void window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var result = Util.TryFindParent<UsageStatisticPopup>(e.OriginalSource as UIElement);
            if (TopUsages.SelectedItem != null || result == null)
                Close();
        }

        void _editorSelectedConnectorChanged(object sender, EventArgs e)
        {
            if (!Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_ShowComponentConnectionProposition)
                return;

            window = Window.GetWindow(_editor);
            if (window != null)
            {
                if (_editor.SelectedConnector == null)
                {
                    return;
                }
                SelectedConnector = this._editor.SelectedConnector;

                Suggestions = new ObservableCollection<SuggestionContainer>();
                var list = ComponentConnectionStatistics.GetMostlyUsedComponentsFromConnector(SelectedConnector.Model.PluginModel.PluginType, SelectedConnector.Model.GetName());
                
                if (list == null)
                {
                    //we have no connectors accociated with this one. So we show AN EMPTY list...                    
                    return;
                }
                foreach (var componentConnector in list)
                {
                    Type t = componentConnector.Component;
                    string name = componentConnector.ConnectorName;
                    Suggestions.Add(new SuggestionContainer(name, t));
                }
            }
        }

        private void Close()
        {
            if (currentAdornerlayer == null)
                return;

            currentAdornerlayer.Remove(adorner);
            window.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(window_MouseLeftButtonUp);
            currentAdornerlayer = null;
            IsOpen = false;
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

    class ControlAdorner : Adorner
    {
        private UserControl _child;
        private UIElement _adornedElement;

        public ControlAdorner(UIElement adornedElement, UserControl ctrl)
            : base(adornedElement)
        {
            _child = ctrl;
            _adornedElement = adornedElement;
            AddLogicalChild(_child);
            AddVisualChild(_child);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // ... add custom rendering code here ...
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this._child.Measure(constraint);
            if (double.IsInfinity(constraint.Height))
            {
                constraint.Height = this._adornedElement.DesiredSize.Height;
            }
            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this._child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this._child;
        }

        internal void RemoveRef()
        {
            RemoveLogicalChild(_child);
            RemoveVisualChild(_child);
        }
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
        private UIElement _adornedElement;
        private CricularLineProgress _clp;
        private Window _win;

        public CricularLineProgressAdorner(UIElement adornedElement, CricularLineProgress clp)
            : base(adornedElement)
        {
            _adornedElement = adornedElement;
            _clp = clp;
            _clp.IsHitTestVisible = false;
            AddLogicalChild(clp);
            AddVisualChild(clp);
            _win = Window.GetWindow(adornedElement);
            _win.PreviewMouseMove += new MouseEventHandler(_win_PreviewMouseMove);
            var p = Mouse.GetPosition(_win);
            _clp.RenderTransform = new TranslateTransform(p.X, p.Y);

            DoubleAnimation anim = new DoubleAnimation(50, 0, new Duration(TimeSpan.FromSeconds(Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_BlingDelay)));
            DoubleAnimation anim2 = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_BlingDelay)));
            Storyboard sb = new Storyboard();
            Storyboard.SetTarget(anim, _clp);
            Storyboard.SetTarget(anim2, _clp);
            Storyboard.SetTargetProperty(anim, new PropertyPath(CricularLineProgress.RadiusProperty));
            Storyboard.SetTargetProperty(anim2, new PropertyPath(CricularLineProgress.OpacityProperty));
            sb.Children.Add(anim);
            sb.Children.Add(anim2);
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
