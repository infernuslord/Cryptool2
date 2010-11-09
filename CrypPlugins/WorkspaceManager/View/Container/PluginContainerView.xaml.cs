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
using System.Windows.Threading;
using System.Threading;
using Cryptool.PluginBase;
namespace WorkspaceManager.View.Container
{
    public enum PluginViewState
    {   
        Min,
        Presentation,
        Data,
        Log,
        Setting,
        Description
    };

    /// <summary>
    /// Interaction logic for PluginContainerView.xaml
    /// </summary>
    public partial class PluginContainerView : UserControl, IDraggable, IUpdateableView
    {
        #region Events
        public event EventHandler<ConnectorViewEventArgs> ConnectorMouseLeftButtonDown;
        public event EventHandler<PluginContainerViewDeleteViewEventArgs> Delete;
        public event EventHandler<PluginContainerViewFullScreenViewEventArgs> FullScreen;
        public event EventHandler<ConnectorPanelDropEventArgs> ConnectorPanelDrop;
        #endregion

        #region Private Variables

        private List<UIElement> optionList = new List<UIElement>();
        private int optionPointer = 0;
        #endregion

        #region Properties

        internal Point GetRoutingPoint(int routPoint)
        {
            switch (routPoint)
            {
                case 0:
                    return new Point(GetPosition().X - 1, GetPosition().Y - 1);
                case 1:
                    return new Point(GetPosition().X - 1, GetPosition().Y + this.ActualHeight + 1);
                case 2:
                    return new Point(GetPosition().X + 1 + this.PluginBase.ActualWidth, GetPosition().Y + 1);
                case 3:
                    return new Point(GetPosition().X + this.PluginBase.ActualWidth + 1, GetPosition().Y + this.ActualHeight + 1);
            }
            return default(Point);
        }

        public Point[] RoutingPoints
        {
            get
            {
                return  new Point[] {
                        new Point(GetPosition().X-1 ,GetPosition().Y-1),
                        //new Point((this.RenderTransform as TranslateTransform).X + (this.ActualWidth / 2),(this.RenderTransform as TranslateTransform).Y-1),
                        //new Point((this.RenderTransform as TranslateTransform).X-1,(this.RenderTransform as TranslateTransform).Y + (this.ActualHeight / 2)),
                        new Point(GetPosition().X-1,GetPosition().Y + this.PluginBase.ActualHeight+1),
                        new Point(GetPosition().X+1 + this.PluginBase.ActualWidth,GetPosition().Y+1),
                        //new Point((this.RenderTransform as TranslateTransform).X + (this.ActualWidth / 2), (this.RenderTransform as TranslateTransform).Y + this.ActualHeight+1),
                        //new Point((this.RenderTransform as TranslateTransform).X + this.ActualWidth+1, (this.RenderTransform as TranslateTransform).Y + (this.ActualHeight / 2)),
                        new Point(GetPosition().X + this.PluginBase.ActualWidth+1, GetPosition().Y + this.PluginBase.ActualHeight+1)};
            }
        }

        public static readonly DependencyProperty IsFullscreenProperty = DependencyProperty.Register("IsFullscreen", typeof(bool), typeof(PluginContainerView));
        public bool IsFullscreen
        {
            get
            {
                return (bool)base.GetValue(IsFullscreenProperty);
            }
            set
            {
                base.SetValue(IsFullscreenProperty, value);
            }
        }

        public static readonly DependencyProperty ViewStateProperty = DependencyProperty.Register("ViewState", typeof(PluginViewState), typeof(PluginContainerView), new FrameworkPropertyMetadata(PluginViewState.Min));

        public PluginViewState ViewState
        {
            get 
            {
                return (PluginViewState)base.GetValue(ViewStateProperty);
            }
            set
            {

                switch (value)
                {
                    case PluginViewState.Presentation:
                        if (PresentationPanel.Child == null)
                        {
                            PluginBase.Width = PluginBase.MinWidth;
                            PluginBase.Height = PluginBase.MinHeight;
                            ViewPanel.Visibility = Visibility.Collapsed;
                            break;
                        }
                        PluginBase.Width = Model.Width;
                        PluginBase.Height = Model.Height;
                        ViewPanel.Visibility = Visibility.Visible;
                        break;

                    case PluginViewState.Data:
                        PluginBase.Width = Model.Width;
                        PluginBase.Height = Model.Height;
                        ViewPanel.Visibility = Visibility.Visible;
                        break;
                    case PluginViewState.Log:
                        PluginBase.Width = Model.Width;
                        PluginBase.Height = Model.Height;
                        ViewPanel.Visibility = Visibility.Visible;
                        break;
                    case PluginViewState.Min:
                        PluginBase.Width = PluginBase.MinWidth;
                        PluginBase.Height = PluginBase.MinHeight;
                        ViewPanel.Visibility = Visibility.Collapsed;
                        break;

                    case PluginViewState.Setting:
                        PluginBase.Width = Model.Width;
                        PluginBase.Height = Model.Height;
                        ViewPanel.Visibility = Visibility.Visible;
                        break;

                    case PluginViewState.Description:
                        PluginBase.Width = Model.Width;
                        PluginBase.Height = Model.Height;
                        ViewPanel.Visibility = Visibility.Visible;
                        break;
                }

                base.SetValue(ViewStateProperty, value);
                this.Model.ViewState = value;
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
                if (value != null && icon != value)
                {
                    icon = value;
                    icon.Stretch = Stretch.Uniform;
                    icon.Width = 45;
                    icon.Height = 45;
                    IconPanel.Child = icon;
                    icon.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                }
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
            InitializeComponent();
            setBaseControl(model);
            DataContext = this;

            West.PreviewDrop += new DragEventHandler(Connector_Drop);
            West.PreviewDragEnter += new DragEventHandler(West_PreviewDragEnter);
            West.PreviewDragLeave += new DragEventHandler(South_PreviewDragLeave);

            East.PreviewDrop += new DragEventHandler(Connector_Drop);
            East.PreviewDragEnter += new DragEventHandler(West_PreviewDragEnter);
            East.PreviewDragLeave += new DragEventHandler(South_PreviewDragLeave);

            North.PreviewDrop += new DragEventHandler(Connector_Drop);
            North.PreviewDragEnter += new DragEventHandler(West_PreviewDragEnter);
            North.PreviewDragLeave += new DragEventHandler(South_PreviewDragLeave);

            South.PreviewDrop += new DragEventHandler(Connector_Drop);
            South.PreviewDragEnter += new DragEventHandler(West_PreviewDragEnter);
            South.PreviewDragLeave += new DragEventHandler(South_PreviewDragLeave);

            handleStartable();

            LogPanel.Child = new LogPresentation();
            PresentationPanel.Child = Model.PluginPresentation;
            TaskPaneCtrl Settings = new TaskPaneCtrl();
            SettingsPanel.Child = Settings;
            Settings.DisplayPluginSettings(Model.Plugin, Model.Plugin.GetPluginInfoAttribute().Caption, Cryptool.PluginBase.DisplayPluginMode.Normal);

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
            this.ViewState = Model.ViewState;
        }

        void South_PreviewDragLeave(object sender, DragEventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            sp.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00100000"));

        }

        void West_PreviewDragEnter(object sender, DragEventArgs e)
        {
            StackPanel sp = sender as StackPanel;
            sp.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#55FFFFFF"));
        }

        private void handleStartable()
        {
            if (Model.Startable)
                play.Visibility = Visibility.Visible;
            else
                play.Visibility = Visibility.Collapsed;

            if (model.RepeatStart)
            {
                playimg.Source = new BitmapImage(new Uri("../Image/play.png", UriKind.RelativeOrAbsolute));                
                
            }
            else
            {
                playimg.Source = new BitmapImage(new Uri("../Image/pause.png", UriKind.RelativeOrAbsolute));             
            }
        }

        void Connector_Drop(object sender, DragEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            try
            {
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
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
            finally
            {
                SetAllConnectorPositionX();
                panel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00100000"));

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

            //connector.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(connector_PreviewMouseLeftButtonDown);
            connector.MouseUp += new MouseButtonEventHandler(connector_MouseUp);
            connector.MouseLeave += new MouseEventHandler(connector_MouseLeave);
            connectorViewList.Add(connector);
        }

        void connector_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ConnectorView connector = (sender as ConnectorView);
            if (e.ButtonState == MouseButtonState.Released)
                if (ConnectorMouseLeftButtonDown != null)
                    ConnectorMouseLeftButtonDown.Invoke(this, new ConnectorViewEventArgs() { connector = connector });
        }

        void connector_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ConnectorView connector = (sender as ConnectorView);
                DataObject dragData = new DataObject("connector", connector);
                DragDrop.DoDragDrop(connector.Parent, dragData, DragDropEffects.Move);
            }
        }



        //public void ResetPopUp()
        //{
        //    Random random = new Random();
        //    BubblePopup.PlacementRectangle = new Rect(new Point(random.NextDouble() / 1000, 0), new Size(0, 0));
        //    //ProgressPopup.PlacementRectangle = new Rect(new Point(random.NextDouble() / 1000, 0), new Size(0, 0));
        //}

        public void SetPosition(Point value)
        {
            if (value.Y < 0)
                Canvas.SetTop(this, 0);
            else
                Canvas.SetTop(this, value.Y);

            if (value.X < 0)
                Canvas.SetLeft(this, 0);
            else
                Canvas.SetLeft(this, value.X);

            //ResetPopUp();
            Model.Position = GetPosition();
            SetAllConnectorPositionX();
        }

        public Point GetPosition()
        {
            return new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
        }

        #endregion

        #region Private Methods

        private void setBaseControl(PluginModel model)
        {
            this.Loaded += new RoutedEventHandler(PluginContainerView_Loaded);
            this.MouseDoubleClick += new MouseButtonEventHandler(PluginContainerView_MouseDoubleClick);
            this.Model = model;
            this.Model.UpdateableView = this;
            this.Model.LogUpdated += new EventHandler<LogUpdated>(Model_LogUpdated);
            this.DataContext = Model;
            this.ConnectorViewList = new List<ConnectorView>();
            this.RenderTransform = new TranslateTransform();
            this.Icon = this.Model.getImage();
        }

        void PluginContainerView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Model.PluginPresentation != null && !IsFullscreen)
            {
                ViewState = PluginViewState.Presentation;
                showFullScreen();
            }
            else if(Model.PluginPresentation == null && !IsFullscreen)
            {
                ViewState = PluginViewState.Data;
                showFullScreen();
            }
        }

        void Model_LogUpdated(object sender, LogUpdated e)
        {
            LogPresentation log = LogPanel.Child as LogPresentation;
            log.AddLogList(Model.GuiLogEvents);
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

                    x = GetPosition().X + point.X + relativePoint.X;
                    y = GetPosition().Y + point.Y + relativePoint.Y;

                    conn.PositionOnWorkSpaceX = x;
                    conn.PositionOnWorkSpaceY = y;
                }

                foreach (ConnectorView conn in East.Children)
                {
                    gTransform = this.East.TransformToVisual(this);
                    gTransformSec = conn.TransformToVisual(this.East);

                    point = gTransform.Transform(new Point(0, 0));
                    relativePoint = gTransformSec.Transform(new Point(0, 0));

                    x = GetPosition().X + point.X + relativePoint.X;
                    y = GetPosition().Y + point.Y + relativePoint.Y;

                    conn.PositionOnWorkSpaceX = x;
                    conn.PositionOnWorkSpaceY = y;
                }

                foreach (ConnectorView conn in North.Children)
                {
                    gTransform = this.North.TransformToVisual(this);
                    gTransformSec = conn.TransformToVisual(this.North);

                    point = gTransform.Transform(new Point(0, 0));
                    relativePoint = gTransformSec.Transform(new Point(0, 0));

                    x = GetPosition().X + point.X + relativePoint.X;
                    y = GetPosition().Y + point.Y + relativePoint.Y;

                    conn.PositionOnWorkSpaceX = x;
                    conn.PositionOnWorkSpaceY = y;
                }

                foreach (ConnectorView conn in South.Children)
                {
                    gTransform = this.South.TransformToVisual(this);
                    gTransformSec = conn.TransformToVisual(this.South);

                    point = gTransform.Transform(new Point(0, 0));
                    relativePoint = gTransformSec.Transform(new Point(0, 0));

                    x = GetPosition().X + point.X + relativePoint.X;
                    y = GetPosition().Y + point.Y + relativePoint.Y;

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
            Color clr = ColorHelper.GetColor(this.Model.PluginType);
            System.Drawing.Color clr2 = System.Windows.Forms.ControlPaint.Dark(System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B));
            clr = Color.FromArgb(clr2.A, clr2.R, clr2.G, clr2.B);
            BorderGradientStop.Color = clr;
            BorderGradientStopSecond.Color = clr;
            clr2 = System.Windows.Forms.ControlPaint.LightLight(System.Windows.Forms.ControlPaint.LightLight(System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B)));
            clr = Color.FromArgb(clr2.A, clr2.R, clr2.G, clr2.B);
            BG.Background = new SolidColorBrush(clr);

            if (Model.PluginPresentation != null)
            {
                optionList.Add(Resources["PresentationButton"] as UIElement);
                optionList.Add(Resources["DataButton"] as UIElement);
                optionList.Add(Resources["LogButton"] as UIElement);
                optionList.Add(Resources["SettingButton"] as UIElement);
            }
            else 
            {
                optionList.Add(Resources["DataButton"] as UIElement);
                optionList.Add(Resources["LogButton"] as UIElement);
                optionList.Add(Resources["SettingButton"] as UIElement);
            }

            if (model.RepeatStart)
            {
                playimg.Source = new BitmapImage(new Uri("../Image/play.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                playimg.Source = new BitmapImage(new Uri("../Image/pause.png", UriKind.RelativeOrAbsolute));
            }
                
            reAssambleOptions();

            LogPresentation LogView = LogPanel.Child as LogPresentation;
            LogView.LogUpdated += new EventHandler<LogUpdated>(LogView_LogUpdated);

            SetAllConnectorPositionX();
            
        }

        void LogView_LogUpdated(object sender, LogUpdated e)
        {
            //LogPresentation logView = sender as LogPresentation;
            //ErrorCount.Text = logView.ErrorCount.ToString();
            //WarningCount.Text = logView.WarningCount.ToString();
            //DebugCount.Text = logView.DebugCount.ToString();
            //InfoCount.Text = logView.InfoCount.ToString();
            //LogReport.Text = e.log.Message;
            //BubblePopup.IsOpen = true;
        }

        private int optionModulo(int value)
        {
            int x = value % optionList.Count;

            if (x < 0)
            {
                x += optionList.Count;
            }

            return x;
        }

        private void reAssambleOptions()
        {
            SlotOne.Child = null;
            SlotTwo.Child = null;
            SlotThree.Child = null;
            SlotFour.Child = null;
            SlotFive.Child = null;
            if (Model.PluginPresentation != null)
            {
                SlotOne.Visibility = Visibility.Visible;
                SlotFive.Visibility = Visibility.Visible;

                SlotOne.Child = null;
                SlotTwo.Child = null;
                SlotThree.Child = null;
                SlotFour.Child = null;
                SlotFive.Child = null;

                SlotOne.Child = optionList.ElementAt(optionModulo(optionPointer - 2));
                SlotTwo.Child = optionList.ElementAt(optionModulo(optionPointer - 1));
                SlotThree.Child = optionList.ElementAt(optionPointer);
                SlotFour.Child = optionList.ElementAt(optionModulo(optionPointer + 1));  
            }
            else 
            {
                //SlotOne.Visibility = Visibility.Collapsed;
                //SlotFive.Visibility = Visibility.Collapsed;

                SlotOne.Child = null;
                SlotTwo.Child = null;
                SlotThree.Child = null;
                SlotFour.Child = null;
                SlotFive.Child = null;

                SlotTwo.Child = optionList.ElementAt(optionModulo(optionPointer - 1));
                SlotThree.Child = optionList.ElementAt(optionPointer);
                SlotFour.Child = optionList.ElementAt(optionModulo(optionPointer + 1));
            }
        }


        private void delete()
        {
            if (this.Delete != null)
                this.Delete.Invoke(this, new PluginContainerViewDeleteViewEventArgs { container = this });
        }

        private void showFullScreen()
        {
            if (this.FullScreen != null)
            {
                IsFullscreen = true;
                this.FullScreen.Invoke(this, new PluginContainerViewFullScreenViewEventArgs { container = this });
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            
        }

        public void PrepareFullscreen()
        {
            this.ViewPanelParent.Children.Clear();
            this.OptPanelParent.Children.Remove(OptionPanel);
            this.ProgressbarRoot.Children.Clear();
            this.ProgressPercentageRoot.Children.Clear();
        }

        public void Reset()
        {
            if (this.IsFullscreen)
            {
                this.ViewPanelParent.Children.Add(ViewPanel);
                this.OptPanelParent.Children.Add(OptionPanel);
                this.ProgressbarRoot.Children.Add(ProgressbarParent);
                this.ProgressPercentageRoot.Children.Add(ProgressPercentage);
                this.IsFullscreen = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button bttn = sender as Button;
            if (bttn.Name == "play")
            {
                if (model.RepeatStart)
                {
                    model.RepeatStart = false;
                    playimg.Source = new BitmapImage(new Uri("../Image/pause.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    model.RepeatStart = true;
                    playimg.Source = new BitmapImage(new Uri("../Image/play.png", UriKind.RelativeOrAbsolute));
                }
                return;
            }
            if (bttn.Name == "del")
            {
                this.delete();
                return;
            }
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Thumb t = sender as Thumb;
            if (t.Cursor == Cursors.SizeWE)
            {
                if ((PluginBase.ActualWidth + e.HorizontalChange) > Model.MinWidth)
                    PluginBase.Width = PluginBase.ActualWidth + e.HorizontalChange;
            }

            if (t.Cursor == Cursors.SizeNS)
            {
                if ((PluginBase.ActualHeight + e.VerticalChange) > Model.MinHeight)
                    PluginBase.Height = PluginBase.ActualHeight + e.VerticalChange;
            }

            if (t.Cursor == Cursors.SizeNWSE)
            {
                if ((PluginBase.ActualHeight + e.VerticalChange) > Model.MinHeight)
                    PluginBase.Height = PluginBase.ActualHeight + e.VerticalChange;

                if ((PluginBase.ActualWidth + e.HorizontalChange) > Model.MinWidth)
                    PluginBase.Width = PluginBase.ActualWidth + e.HorizontalChange;
            }

            Model.Height = PluginBase.ActualHeight;
            Model.Width = PluginBase.ActualWidth;
            
     
        }

        private void MinMaxBorder_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            this.SetAllConnectorPositionX();
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

            if (Model.GuiLogEvents.Count != 0)
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

            //perhaps the icon changed so we have to update it
            //the property will do nothing if the icon is the same as the old one
            this.Icon = model.getImage();

        }

        #endregion

        private void OptionClickHandler(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "Left":
                    optionPointer = optionModulo(optionPointer + 1);
                    OptionCaption.Text = (optionList.ElementAt(optionPointer) as Button).ToolTip as String;
                    reAssambleOptions();
                    break;

                case "Right":
                    optionPointer = optionModulo(optionPointer - 1);;
                    OptionCaption.Text = (optionList.ElementAt(optionPointer) as Button).ToolTip as String;
                    reAssambleOptions();
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
                        ViewState = PluginViewState.Min;
                        break;
                    }
                    ViewState = PluginViewState.Presentation;
                    break;

                case "DataButton":
                    ViewState = PluginViewState.Data;
                    break;

                case "LogButton":
                    ViewState = PluginViewState.Log;
                    break;

                case "MinimizeButton":
                    if(ViewState == PluginViewState.Min)
                        ViewState = PluginViewState.Log;
                    else
                        ViewState = PluginViewState.Min;
                    break;

                case "SettingButton":
                    ViewState = PluginViewState.Setting;
                    break;

                case "MaxButton":
                    showFullScreen();
                    break;
            }
        }

        private void SettingButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            OptionCaption.Text = btn.ToolTip as String;
        }

        private void Thumb_DragDelta_1(object sender, DragDeltaEventArgs e)
        {
            this.SetPosition(new Point((Math.Round((Canvas.GetLeft(this) + e.HorizontalChange) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale,
                                                            (Math.Round((Canvas.GetTop(this) + e.VerticalChange) / Properties.Settings.Default.GridScale)) * Properties.Settings.Default.GridScale));
            Model.WorkspaceModel.WorkspaceManagerEditor.HasChanges = true;
        }

    }

    public class ConnectorPanelDropEventArgs : EventArgs
    { }

    public class PluginContainerViewDeleteViewEventArgs : EventArgs
    {
        public PluginContainerView container;
    }

    public class PluginContainerViewFullScreenViewEventArgs : EventArgs
    {
        public PluginContainerView container;
    }
}
