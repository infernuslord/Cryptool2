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
        public event EventHandler<ConnectorViewEventArgs> OnConnectorMouseLeftButtonDown;

        private List<ConnectorView> inputConnectorViewList = new List<ConnectorView>();
        private List<ConnectorView> outputConnectorViewList = new List<ConnectorView>();
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
            Image img = model.getImage();
            img.Stretch = Stretch.Uniform;
            PresentationPanel.Children.Add(img);
            PluginName.Content = model.PluginType.Name.ToString();
            foreach (ConnectorModel cModel in model.InputConnectors)
            {
                AddInputConnectorView(new ConnectorView(cModel));
            }

            foreach (ConnectorModel cModel in model.OutputConnectors)
            {
                AddInputConnectorView(new ConnectorView(cModel));
            }
        }

        public void AddInputConnectorView(ConnectorView connector)
        {
            connector.OnConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(connector_OnConnectorMouseLeftButtonDown);
            this.inputConnectorViewList.Add(connector);
            this.InputConnectorPanel.Children.Add(connector);
            this.SetAllConnectorPositionX();
        }


        public void AddOutputConnectorView(ConnectorView connector)
        {
            connector.OnConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(connector_OnConnectorMouseLeftButtonDown);
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
            foreach (ConnectorView conn in InputConnectorPanel.Children)
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

            foreach (ConnectorView conn in OutputConnectorPanel.Children)
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

        void connector_OnConnectorMouseLeftButtonDown(object sender, ConnectorViewEventArgs e)
        {
            if (this.OnConnectorMouseLeftButtonDown != null)
            {
                this.OnConnectorMouseLeftButtonDown.Invoke(this, e);
            }
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.delete();
        }

        private void delete()
        {
            if (Parent is Panel)
            {
                (this.Parent as Panel).Children.Remove(this);
                this.model.WorkspaceModel.deletePluginModel(this.model);
            }
        }
    }
}
