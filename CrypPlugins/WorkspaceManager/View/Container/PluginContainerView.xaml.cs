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
        public event EventHandler<PluginContainerViewDeleteViewEventArgs> Delete;
        public event EventHandler<PluginContainerViewSettingsViewEventArgs> ShowSettings;

        private List<ConnectorView> connectorViewList;
        public List<ConnectorView> ConnectorViewList
        {
            get { return connectorViewList; }
            private set { connectorViewList = value; }
        }
        private PluginModel model;
        public PluginModel Model
        {
            get { return model; }
            private set { model = value; }
        }

        public bool CanDrag { get; set; }

        public PluginContainerView()
        {
            this.Loaded += new RoutedEventHandler(PluginContainerView_Loaded);
            this.RenderTransform = new TranslateTransform();
            InitializeComponent();
        }

        public PluginContainerView(PluginModel model)
        {
            this.ConnectorViewList = new List<ConnectorView>();
            this.Loaded += new RoutedEventHandler(PluginContainerView_Loaded);
            this.MouseEnter += new MouseEventHandler(PluginContainerView_MouseEnter);
            this.MouseLeave += new MouseEventHandler(PluginContainerView_MouseLeave);
            this.model = model;
            this.RenderTransform = new TranslateTransform();
            InitializeComponent();

            if(model.Plugin.QuickWatchPresentation != null)
                this.PresentationPanel.Children.Add(model.Plugin.QuickWatchPresentation);
        }

        void PluginContainerView_MouseLeave(object sender, MouseEventArgs e)
        {
            ControlPanel.Visibility = Visibility.Hidden;
        }

        void PluginContainerView_MouseEnter(object sender, MouseEventArgs e)
        {
            ControlPanel.Visibility = Visibility.Visible;
        }

        void PluginContainerView_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: Better-> Bindings
            Image img = model.getImage();
            img.Stretch = Stretch.Uniform;
            PresentationPanel.Children.Add(img);
            PluginName.Content = model.PluginType.Name.ToString();
            foreach (ConnectorModel Model in model.InputConnectors)
            {
                AddInputConnectorView(new ConnectorView(Model));
            }

            foreach (ConnectorModel Model in model.OutputConnectors)
            {
                AddOutputConnectorView(new ConnectorView(Model));
            }
        }

        public void AddInputConnectorView(ConnectorView connector)
        {
            connector.OnConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(connector_OnConnectorMouseLeftButtonDown);
            this.ConnectorViewList.Add(connector);
            this.InputConnectorPanel.Children.Add(connector);
            this.SetAllConnectorPositionX();
        }


        public void AddOutputConnectorView(ConnectorView connector)
        {
            connector.OnConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(connector_OnConnectorMouseLeftButtonDown);
            this.ConnectorViewList.Add(connector);
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

        private void delete()
        {
            if (this.Delete != null)
            {
                this.Delete.Invoke(this, new PluginContainerViewDeleteViewEventArgs { container = this });
            }
        }

        private void showSettings()
        {
            if (this.ShowSettings != null)
            {
                this.ShowSettings.Invoke(this, new PluginContainerViewSettingsViewEventArgs { container = this });
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.showSettings();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.delete();
        }
    }

    public class PluginContainerViewDeleteViewEventArgs : EventArgs
    {
        public PluginContainerView container;
    }

    public class PluginContainerViewSettingsViewEventArgs : EventArgs
    {
        public PluginContainerView container;
    }
}
