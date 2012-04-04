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

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for UsageStatisticPopup.xaml
    /// </summary>
    public partial class UsageStatisticPopup : Popup
    {
        private readonly WorkspaceModel _model;

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

        public static readonly DependencyProperty SuggestionsProperty = DependencyProperty.Register("Suggestions",
            typeof(ObservableCollection<SuggestionContainer>),
            typeof(UsageStatisticPopup),
            new FrameworkPropertyMetadata(null,null));

        private EditorVisual _editor;

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
            _editor = (EditorVisual) model.MyEditor.Presentation;
            this.Loaded += new RoutedEventHandler(ComponentConnectionStatisticsPopUpLoaded);
            this.MouseLeave += new MouseEventHandler(ComponentConnectionStatisticsPopUpMouseLeave);
            this.Opened += new EventHandler(ComponentConnectionStatisticsPopUpOpened);
            this.Closed += new EventHandler(ComponentConnectionStatisticsPopUpClosed);
            _editor.SelectedConnectorChanged += new EventHandler(_editorSelectedConnectorChanged);
            
            Suggestions = new ObservableCollection<SuggestionContainer>();
            Suggestions.Add(new SuggestionContainer());

        }

        void ComponentConnectionStatisticsPopUpClosed(object sender, EventArgs e)
        {
            
        }

        void _editorSelectedConnectorChanged(object sender, EventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            if (this._editor.SelectedConnector == null)
            {
                timer.Stop();
                return;
            }
            SelectedConnector = this._editor.SelectedConnector;
            timer.Interval = new TimeSpan(0,0,0,3);
            timer.Tick += delegate(object o, EventArgs args)
                              {
                                  timer.Stop();
                                  Suggestions = new ObservableCollection<SuggestionContainer>();
                                  var list = ComponentConnectionStatistics.GetMostlyUsedComponentsFromConnector(SelectedConnector.Model.PluginModel.PluginType, SelectedConnector.Model.GetName());
                                  foreach (var componentConnector in list)
                                  {
                                      Type t = componentConnector.Component;
                                      string name = componentConnector.ConnectorName;
                                      Suggestions.Add(new SuggestionContainer(name,t));
                                  }
                                  this.IsOpen = true;
                              };
            timer.Start();
        }

        void ComponentConnectionStatisticsPopUpOpened(object sender, EventArgs e)
        {
           
        }

        void ComponentConnectionStatisticsPopUpMouseLeave(object sender, MouseEventArgs e)
        {
            IsOpen = false;
        }

        void SelectedItem(object sender, SelectionChangedEventArgs e)
        {
            
        }

        void ComponentConnectionStatisticsPopUpLoaded(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(WinPreviewMouseLeftButtonUp);
            }
        }

        void WinPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(!IsMouseOver)
            {
                IsOpen = false;
            }
            if (TopUsages.SelectedItem != null && IsMouseOver)
            {

                try
                {
                    var x = (SuggestionContainer)TopUsages.SelectedItem;
                    PluginModel pluginModel = (PluginModel)_model.ModifyModel(new NewPluginModelOperation(Util.MouseUtilities.CorrectGetPosition(_editor.panel), 0, 0, x.ComponentType));
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
typeof(double), typeof(CricularLineProgress), new FrameworkPropertyMetadata((double)40, null));

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set
            {
                SetValue(RadiusProperty, value);
            }
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
            Stroke = Brushes.Black;
            StrokeThickness = 2;
            this.Loaded += new RoutedEventHandler(CricularLineProgressLoaded);
        }

        void CricularLineProgressLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
                window.KeyDown += new KeyEventHandler(CricularLineProgressKeyDown);
        }

        void CricularLineProgressKeyDown(object sender, KeyEventArgs e)
        {
            Radius += 1;
            this.InvalidateVisual();
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
}
