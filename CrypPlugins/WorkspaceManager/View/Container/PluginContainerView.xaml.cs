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
using WorkspaceManager;

namespace WorkspaceManager.View.Container
{
    public enum PluginViewState
    {   
        Min,
        Presentation,
        Data,
        Log,
        Setting
    };

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

        private List<UIElement> optionList = new List<UIElement>();
        private int optionPointer = 0;

        #endregion

        #region Properties

        public static readonly DependencyProperty ViewStateProperty = DependencyProperty.Register("ViewState", typeof(PluginViewState), typeof(PluginContainerView), new FrameworkPropertyMetadata(PluginViewState.Min));


        public PluginViewState ViewState
        {
            get 
            {
                return (PluginViewState)base.GetValue(ViewStateProperty);
            }
            set
            {
                if((PluginViewState)value != PluginViewState.Min)
                {
                    BottomDelta.IsEnabled = true;
                    RightDelta.IsEnabled = true;
                    BottomRightDelta.IsEnabled = true;
                }
                else
                {
                    BottomDelta.IsEnabled = false;
                    RightDelta.IsEnabled = false;
                    BottomRightDelta.IsEnabled = false;
                }
                base.SetValue(ViewStateProperty, value);
            }
        }

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
                icon.Width = 40;
                icon.Height = 40;
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
            DataContext = this;

            switch (ViewState)
            {
                case PluginViewState.Min:
                    IconPanel.Child = Icon;
                    break;

                case PluginViewState.Data:

                    break;

                case PluginViewState.Presentation:

                    break;

                case PluginViewState.Setting:

                    break;

                case PluginViewState.Log:

                    break;
            }

            West.PreviewDrop += new DragEventHandler(Connector_Drop);
            East.PreviewDrop += new DragEventHandler(Connector_Drop);
            North.PreviewDrop += new DragEventHandler(Connector_Drop);
            South.PreviewDrop += new DragEventHandler(Connector_Drop);

            LogPanel.Child = new LogPresentation();
            PresentationPanel.Child = Model.PluginPresentation;
            SettingsPanel.Child = null;

            foreach (ConnectorModel ConnectorModel in model.InputConnectors)
            {
                ConnectorView connector = new ConnectorView(ConnectorModel);
                AddConnectorView(connector);
                DataPanel.Children.Add(new DataPresentation(connector));
            }

            foreach (ConnectorModel ConnectorModel in model.OutputConnectors)
            {
                ConnectorView connector = new ConnectorView(ConnectorModel);
                AddConnectorView(connector);
                DataPanel.Children.Add(new DataPresentation(connector));
            }
        }

        void Connector_Drop(object sender, DragEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            if (e.Data.GetDataPresent("connector"))
            {
                ConnectorView connector = e.Data.GetData("connector") as ConnectorView;
                if (panel.Children.Contains(connector))
                    return;

                switch (connector.Orientation)
                {
                    case ConnectorOrientation.West:
                        this.West.Children.Remove(connector);
                        break;
                    case ConnectorOrientation.East:
                        this.East.Children.Remove(connector);
                        break;
                    case ConnectorOrientation.North:
                        this.North.Children.Remove(connector);
                        break;
                    case ConnectorOrientation.South:
                        this.South.Children.Remove(connector);
                        break;
                }

                switch (panel.Name)
                {
                    case "West":
                        connector.Orientation = ConnectorOrientation.West;
                        this.West.Children.Add(connector);
                        break;
                    case "East":
                        connector.Orientation = ConnectorOrientation.East;
                        this.East.Children.Add(connector);
                        break;
                    case "North":
                        connector.Orientation = ConnectorOrientation.North;
                        this.North.Children.Add(connector);
                        break;
                    case "South":
                        connector.Orientation = ConnectorOrientation.South;
                        this.South.Children.Add(connector);
                        break;
                }

                SetAllConnectorPositionX();
                e.Handled = true;
            }
        }

        #endregion

        #region Public Methods
        public void AddConnectorView(ConnectorView connector)
        {
            switch (connector.Orientation)
            {
                case ConnectorOrientation.West:
                    this.West.Children.Add(connector);
                    break;
                case ConnectorOrientation.East:
                    this.East.Children.Add(connector);
                    break;
                case ConnectorOrientation.North:
                    this.North.Children.Add(connector);
                    break;
                case ConnectorOrientation.South:
                    this.South.Children.Add(connector);
                    break;
            }

            connector.OnConnectorMouseLeftButtonDown += new EventHandler<ConnectorViewEventArgs>(connector_OnConnectorMouseLeftButtonDown);
            connectorViewList.Add(connector);
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
            this.Icon = this.Model.getImage();
        }

        private void SetAllConnectorPositionX()
        {
            try
            {
                GeneralTransform gTransform, gTransformSec;
                Point point, relativePoint;
                double x, y;

                foreach (ConnectorView conn in West.Children)
                {
                    gTransform = this.West.TransformToVisual(this);
                    gTransformSec = conn.TransformToVisual(this.West);

                    point = gTransform.Transform(new Point(0, 0));
                    relativePoint = gTransformSec.Transform(new Point(0, 0));

                    x = (RenderTransform as TranslateTransform).X + point.X + relativePoint.X;
                    y = (RenderTransform as TranslateTransform).Y + point.Y + relativePoint.Y;

                    conn.PositionOnWorkSpaceX = x;
                    conn.PositionOnWorkSpaceY = y;
                }

                foreach (ConnectorView conn in East.Children)
                {
                    gTransform = this.East.TransformToVisual(this);
                    gTransformSec = conn.TransformToVisual(this.East);

                    point = gTransform.Transform(new Point(0, 0));
                    relativePoint = gTransformSec.Transform(new Point(0, 0));

                    x = (RenderTransform as TranslateTransform).X + point.X + relativePoint.X;
                    y = (RenderTransform as TranslateTransform).Y + point.Y + relativePoint.Y;

                    conn.PositionOnWorkSpaceX = x;
                    conn.PositionOnWorkSpaceY = y;
                }

                foreach (ConnectorView conn in North.Children)
                {
                    gTransform = this.North.TransformToVisual(this);
                    gTransformSec = conn.TransformToVisual(this.North);

                    point = gTransform.Transform(new Point(0, 0));
                    relativePoint = gTransformSec.Transform(new Point(0, 0));

                    x = (RenderTransform as TranslateTransform).X + point.X + relativePoint.X;
                    y = (RenderTransform as TranslateTransform).Y + point.Y + relativePoint.Y;

                    conn.PositionOnWorkSpaceX = x;
                    conn.PositionOnWorkSpaceY = y;
                }

                foreach (ConnectorView conn in South.Children)
                {
                    gTransform = this.South.TransformToVisual(this);
                    gTransformSec = conn.TransformToVisual(this.South);

                    point = gTransform.Transform(new Point(0, 0));
                    relativePoint = gTransformSec.Transform(new Point(0, 0));

                    x = (RenderTransform as TranslateTransform).X + point.X + relativePoint.X;
                    y = (RenderTransform as TranslateTransform).Y + point.Y + relativePoint.Y;

                    conn.PositionOnWorkSpaceX = x;
                    conn.PositionOnWorkSpaceY = y;
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }
        }
        #endregion

        #region Controls

        void PluginContainerView_Loaded(object sender, RoutedEventArgs e)
        {
            
            this.BorderGradientStop.Color = ColorHelper.GetColor(this.Model.PluginType);
            this.BorderGradientStopSecond.Color = Color.FromArgb(100, this.BorderGradientStop.Color.R, this.BorderGradientStop.Color.G, this.BorderGradientStop.Color.B);

            optionList.Add(Resources["PresentationButton"] as UIElement);
            optionList.Add(Resources["DataButton"] as UIElement);
            optionList.Add(Resources["LogButton"] as UIElement);
            optionList.Add(Resources["MinimizeButton"] as UIElement);

            Options.Child = optionList.ElementAt(optionPointer);
            OptionCaption.Text = (optionList.ElementAt(optionPointer) as Button).ToolTip as String;

            SetAllConnectorPositionX();
            
        }

        void connector_OnConnectorMouseLeftButtonDown(object sender, ConnectorViewEventArgs e)
        {
            DataObject dragData = new DataObject("connector", e.connector);
            DragDrop.DoDragDrop(e.connector.Parent, dragData, DragDropEffects.Move);

            if (this.ConnectorMouseLeftButtonDown != null)
                this.ConnectorMouseLeftButtonDown.Invoke(this, e);
        }

        private void delete()
        {
            if (this.Delete != null)
                this.Delete.Invoke(this, new PluginContainerViewDeleteViewEventArgs { container = this });
        }

        private void showSettings()
        {
            if (this.ShowSettings != null)
                this.ShowSettings.Invoke(this, new PluginContainerViewSettingsViewEventArgs { container = this });
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
            if(ViewState != PluginViewState.Min)
                OptionPanel.Visibility = Visibility.Visible;
            else
                OptionPanel.Visibility = Visibility.Collapsed;
            (Resources["FadeIn"] as Storyboard).Stop(ControlPanel);
            ControlPanel.BeginStoryboard(Resources["FadeOut"] as Storyboard);
        }

        void PluginContainerView_MouseEnter(object sender, MouseEventArgs e)
        {
            OptionPanel.Visibility = Visibility.Visible;
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
            this.SetAllConnectorPositionX();
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region IUpdateableView Members

        public void update()
        {
            ProgressBar.Value = Model.PercentageFinished;

            if (ViewState == PluginViewState.Data)
            {
                foreach (UIElement element in DataPanel.Children)
                {
                    DataPresentation data = element as DataPresentation;
                    data.update();
                }
            }

            if (ViewState == PluginViewState.Log)
            {
                LogPresentation log = LogPanel.Child as LogPresentation;
                log.AddLogList(Model.GuiLogEvents);
            }

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
            
        }

        #endregion

        private void OptionClickHandler(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "Left":
                    optionPointer = (optionPointer + 1) % optionList.Count;
                    Options.Child = optionList.ElementAt(optionPointer);
                    OptionCaption.Text = (optionList.ElementAt(optionPointer) as Button).ToolTip as String;
                    break;

                case "Right":
                    optionPointer = (optionPointer - 1) % optionList.Count;

                    if (optionPointer < 0)
                    {
                        optionPointer += optionList.Count;
                        Options.Child = optionList.ElementAt(optionPointer);
                        OptionCaption.Text = (optionList.ElementAt(optionPointer) as Button).ToolTip as String;
                    }
                    else
                    {
                        OptionCaption.Text = (optionList.ElementAt(optionPointer) as Button).ToolTip as String;
                        Options.Child = optionList.ElementAt(optionPointer);
                    }

                    break;
            }
            e.Handled = true;
        }

        private void OptionChooseHandler(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "PresentationButton":
                    if (PresentationPanel.Child == null)
                    {
                        PluginBase.Width = PluginBase.MinWidth;
                        PluginBase.Height = PluginBase.MinHeight;
                        ViewPanel.Visibility = Visibility.Collapsed;
                        ViewState = PluginViewState.Min;
                        break;
                    }
                    PluginBase.Width = 400;
                    PluginBase.Height = 300;
                    ViewPanel.Visibility = Visibility.Visible;
                    ViewState = PluginViewState.Presentation;
                    break;

                case "DataButton":
                    PluginBase.Width = 400;
                    PluginBase.Height = 300;
                    ViewPanel.Visibility = Visibility.Visible;
                    ViewState = PluginViewState.Data;
                    break;
                case "LogButton":
                    PluginBase.Width = 400;
                    PluginBase.Height = 300;
                    ViewPanel.Visibility = Visibility.Visible;
                    ViewState = PluginViewState.Log;

                    break;
                case "MinimizeButton":
                    PluginBase.Width = PluginBase.MinWidth;
                    PluginBase.Height = PluginBase.MinHeight;
                    ViewPanel.Visibility = Visibility.Collapsed;
                    ViewState = PluginViewState.Min;
                    break;

                case "SettingButton":
                    PluginBase.Width = 400;
                    PluginBase.Height = 300;
                    ViewPanel.Visibility = Visibility.Visible;
                    ViewState = PluginViewState.Setting;
                    break;
            }
            e.Handled = true;
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
