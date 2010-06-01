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
using WorkspaceManager.View.Interface;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Container
{
    /// <summary>
    /// Interaction logic for PluginContainerView.xaml
    /// </summary>
    public partial class PluginContainerView : UserControl, IDraggable
    {
        public event EventHandler<ConntectorViewEventArgs> OnConnectorMouseLeftButtonDown;

        private List<ConntectorView> inputConnectorViewList = new List<ConntectorView>();
        private List<ConntectorView> outputConnectorViewList = new List<ConntectorView>();
        private PluginModel model;

        public bool CanDrag { get; set; }

        public PluginContainerView()
        {
            this.Loaded += new RoutedEventHandler(PluginContainerView_Loaded);
            this.RenderTransform = new TranslateTransform();
            InitializeComponent();
        }

        public PluginContainerView(PluginModel model)
        {
            Loaded += new RoutedEventHandler(PluginContainerView_Loaded);
            this.model = model;
            this.RenderTransform = new TranslateTransform();
            InitializeComponent();

            if(model.Plugin.QuickWatchPresentation != null)
                this.PresentationPanel.Children.Add(model.Plugin.QuickWatchPresentation);
        }

        void PluginContainerView_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: Better-> Bindings
            PluginName.Content = model.PluginType.Name.ToString();
            foreach (ConnectorModel cModel in model.InputConnectors)
            {
                AddInputConnectorView(new ConntectorView(cModel));
            }

            foreach (ConnectorModel cModel in model.OutputConnectors)
            {
                AddInputConnectorView(new ConntectorView(cModel));
            }
        }

        public void AddInputConnectorView(ConntectorView connector)
        {
            connector.OnConnectorMouseLeftButtonDown += new EventHandler<ConntectorViewEventArgs>(connector_OnConnectorMouseLeftButtonDown);
            this.inputConnectorViewList.Add(connector);
            this.InputConnectorPanel.Children.Add(connector);
            this.SetAllConnectorPositionX();
        }


        public void AddOutputConnectorView(ConntectorView connector)
        {
            connector.OnConnectorMouseLeftButtonDown += new EventHandler<ConntectorViewEventArgs>(connector_OnConnectorMouseLeftButtonDown);
            this.outputConnectorViewList.Add(connector);
            this.OutputConnectorPanel.Children.Add(connector);
            this.SetAllConnectorPositionX();
        }

        public void SetPosition(Point value)
        {
            (this.RenderTransform as TranslateTransform).X = value.X;
            (this.RenderTransform as TranslateTransform).Y = value.Y;
            this.SetAllConnectorPositionX();
        }

        private void SetAllConnectorPositionX()
        {
            GeneralTransform gTransform, gTransformSec;
            Point point, relativePoint;
            double x, y;
            foreach (ConntectorView conn in InputConnectorPanel.Children)
            {
                gTransform = this.InputConnectorPanel.TransformToVisual(this);
                gTransformSec = conn.TransformToVisual(this.InputConnectorPanel);

                point = gTransform.Transform(new Point(0, 0));
                relativePoint = gTransformSec.Transform(new Point(0, 0));

                x = (RenderTransform as TranslateTransform).X + point.X + relativePoint.X;
                y = (RenderTransform as TranslateTransform).Y + point.Y + relativePoint.Y;

                conn.PositionOnWorkSpaceX = x;
                conn.PositionOnWorkSpaceY = y;
            }

            foreach (ConntectorView conn in OutputConnectorPanel.Children)
            {
                gTransform = this.OutputConnectorPanel.TransformToVisual(this);
                gTransformSec = conn.TransformToVisual(this.OutputConnectorPanel);

                point = gTransform.Transform(new Point(0, 0));
                relativePoint = gTransformSec.Transform(new Point(0, 0));

                x = (RenderTransform as TranslateTransform).X + point.X + relativePoint.X;
                y = (RenderTransform as TranslateTransform).Y + point.Y + relativePoint.Y;

                conn.PositionOnWorkSpaceX = x;
                conn.PositionOnWorkSpaceY = y;
            }

        }

        public Point GetPosition()
        {
            return new Point((this.RenderTransform as TranslateTransform).X, (this.RenderTransform as TranslateTransform).Y);
        }

        #region Controls

        void connector_OnConnectorMouseLeftButtonDown(object sender, ConntectorViewEventArgs e)
        {
            if (this.OnConnectorMouseLeftButtonDown != null)
            {
                this.OnConnectorMouseLeftButtonDown.Invoke(this, e);
            }
        }

        #endregion
    }
}
