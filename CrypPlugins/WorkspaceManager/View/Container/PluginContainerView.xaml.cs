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
using WorkspaceManager.View.VisualComponents;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace WorkspaceManager.View.Container
{
    /// <summary>
    /// Interaction logic for PluginContainerView.xaml
    /// </summary>
    public partial class PluginContainerView : UserControl, IDraggable, IUpdateableView
    {
        #region Events
        public event EventHandler<ConnectorViewEventArgs> ConnectorMouseLeftButtonDown;
        public event EventHandler<PluginContainerViewDeleteViewEventArgs> Delete;
        public event EventHandler<PluginContainerViewSettingsViewEventArgs> ShowSettings;
        #endregion

        #region Private Variables

        private static double MinHeight;
        private static double MinWidth;
        private bool p;
        #endregion

        #region Properties
        private Image icon;
        public Image Icon
        {
            get
            {
                return icon;
            }
            set
            {
                icon = value;
                icon.Stretch = Stretch.Uniform;
            }
        }

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

        #endregion

        #region Contructors
        public PluginContainerView()
        {
            InitializeComponent();
        }

        public PluginContainerView(PluginModel model)
        {
            setBaseControl(model);
            InitializeComponent();
        }
        #endregion

        #region Public Methods
        public void AddInputConnectorView(ConnectorView connector)
        {
            connector.OnConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(connector_OnConnectorMouseLeftButtonDown);
            this.ConnectorViewList.Add(connector);
            this.DataPresentationPanel.Children.Add(new DataPresentation(connector));
            this.InputConnectorPanel.Children.Add(connector);
            this.SetAllConnectorPositionX();
        }


        public void AddOutputConnectorView(ConnectorView connector)
        {
            connector.OnConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(connector_OnConnectorMouseLeftButtonDown);
            this.ConnectorViewList.Add(connector);
            this.DataPresentationPanel.Children.Add(new DataPresentation(connector));
            this.OutputConnectorPanel.Children.Add(connector);
            this.SetAllConnectorPositionX();
        }

        public void SetPosition(Point value)
        {
            TranslateTransform pos = (this.RenderTransform as TranslateTransform);
            pos.X = value.X;
            pos.Y = value.Y;
            this.Model.Position = new Point(pos.X, pos.Y);
            this.SetAllConnectorPositionX();
        }

        public Point GetPosition()
        {
            return new Point((this.RenderTransform as TranslateTransform).X, (this.RenderTransform as TranslateTransform).Y);
        }

        #endregion

        #region Private Methods

        private void setBaseControl(PluginModel model)
        {
            this.Loaded += new RoutedEventHandler(PluginContainerView_Loaded);
            this.MouseEnter += new MouseEventHandler(PluginContainerView_MouseEnter);
            this.MouseLeave += new MouseEventHandler(PluginContainerView_MouseLeave);
            this.Model = model;
            this.Model.UpdateableView = this;
            this.DataContext = Model;
            this.ConnectorViewList = new List<ConnectorView>();
            this.RenderTransform = new TranslateTransform();
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
        #endregion

        #region Controls

        void PluginContainerView_Loaded(object sender, RoutedEventArgs e)
        {
            MinHeight = this.PluginBase.MinHeight;
            MinWidth = this.PluginBase.MinWidth;
            this.BorderGradientStop.Color = ColorHelper.GetColor(this.Model.PluginType);
            this.BorderGradientStopSecond.Color = Color.FromArgb(100, this.BorderGradientStop.Color.R, this.BorderGradientStop.Color.G, this.BorderGradientStop.Color.B);

            if (this.Model.Minimized == null || this.Model.Minimized == true)
            {
                this.Icon = this.Model.getImage();
                this.PresentationPanel.Child = this.Icon;
                this.Model.Minimized = true;
            }
            else if (this.Model.Minimized == false)
            {
                this.PluginBase.MinHeight = model.MinHeight;
                this.PluginBase.MinWidth = model.MinWidth;
                this.PluginBase.Width = model.MinWidth;
                this.PluginBase.Height = model.MinHeight;
                this.BottomDelta.IsEnabled = true;
                this.RightDelta.IsEnabled = true;
                this.BottomRightDelta.IsEnabled = true;
                this.PresentationPanel.Child = model.PluginPresentation;
                this.Model.Minimized = false;
                this.MinMaxImage.Source = new BitmapImage(new Uri("/WorkspaceManager;component/View/Image/Min.png", UriKind.RelativeOrAbsolute));
            }

            foreach (ConnectorModel ConnectorModel in model.InputConnectors)
            {
                AddInputConnectorView(new ConnectorView(ConnectorModel));
            }

            foreach (ConnectorModel ConnectorModel in model.OutputConnectors)
            {
                AddOutputConnectorView(new ConnectorView(ConnectorModel));
            }
        }

        void connector_OnConnectorMouseLeftButtonDown(object sender, ConnectorViewEventArgs e)
        {
            if (this.ConnectorMouseLeftButtonDown != null)
            {
                this.ConnectorMouseLeftButtonDown.Invoke(this, e);
            }
        }

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

        void PluginContainerView_MouseLeave(object sender, MouseEventArgs e)
        {
            (Resources["FadeIn"] as Storyboard).Stop(ControlPanel);
            ControlPanel.BeginStoryboard(Resources["FadeOut"] as Storyboard);
        }

        void PluginContainerView_MouseEnter(object sender, MouseEventArgs e)
        {
            (Resources["FadeOut"] as Storyboard).Stop(ControlPanel);
            ControlPanel.BeginStoryboard(Resources["FadeIn"] as Storyboard);
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Thumb t = sender as Thumb;
            if (t.Cursor == Cursors.SizeWE)
            {
                if ((PluginBase.ActualWidth + e.HorizontalChange) > 0)
                    PluginBase.Width = PluginBase.ActualWidth + e.HorizontalChange;
            }

            if (t.Cursor == Cursors.SizeNS)
            {
                if ((PluginBase.ActualHeight + e.VerticalChange) > 0)
                    PluginBase.Height = PluginBase.ActualHeight + e.VerticalChange;
            }

            if (t.Cursor == Cursors.SizeNWSE)
            {
                if ((PluginBase.ActualHeight + e.VerticalChange) > 0)
                    PluginBase.Height = PluginBase.ActualHeight + e.VerticalChange;

                if ((PluginBase.ActualWidth + e.HorizontalChange) > 0)
                    PluginBase.Width = PluginBase.ActualWidth + e.HorizontalChange;
            }
        }

        private void MinMaxBorder_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (model.PluginPresentation != null && PresentationPanel.Child is Image)
            {
                PluginBase.MinHeight = model.MinHeight;
                PluginBase.MinWidth = model.MinWidth;
                PluginBase.Width = model.MinWidth;
                PluginBase.Height = model.MinHeight;
                BottomDelta.IsEnabled = true;
                RightDelta.IsEnabled = true;
                BottomRightDelta.IsEnabled = true;
                PresentationPanel.Child = model.PluginPresentation;
                model.Minimized = false;
                MinMaxImage.Source = new BitmapImage(new Uri("/WorkspaceManager;component/View/Image/Min.png", UriKind.RelativeOrAbsolute));
                return;
            }

            if (PresentationPanel.Child is UserControl)
            {
                PluginBase.MinHeight = MinHeight;
                PluginBase.MinWidth = MinWidth;
                PluginBase.Width = MinWidth;
                PluginBase.Height = MinHeight;
                BottomDelta.IsEnabled = false;
                RightDelta.IsEnabled = false;
                BottomRightDelta.IsEnabled = false;
                PresentationPanel.Child = this.Icon;
                MinMaxImage.Source = new BitmapImage(new Uri("/WorkspaceManager;component/View/Image/Max.png", UriKind.RelativeOrAbsolute));
                return;
            }
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShowAllData.Visibility == Visibility.Collapsed)
            {
                ShowAllData.Visibility = Visibility.Visible;
                return;
            }

            if (ShowAllData.Visibility == Visibility.Visible)
            {
                ShowAllData.Visibility = Visibility.Collapsed;
                return;
            }
        }
        #endregion

        #region IUpdateableView Members

        public void update()
        {
            //Color the view corresponding to warning or error state
            if (model.State == PluginModelState.Warning)
            {
                this.Window.Background = new SolidColorBrush(Colors.Yellow);
            }
            else if (model.State == PluginModelState.Error)
            {
                this.Window.Background = new SolidColorBrush(Colors.Red);
            }
            else
            {
                //todo: assign old color and appereance
            }
            Icon = model.getImage();
        }

        #endregion


        internal void LoadConnection()
        {
            throw new NotImplementedException();
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
