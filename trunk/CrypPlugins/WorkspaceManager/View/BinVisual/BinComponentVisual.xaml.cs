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
using System.Collections.ObjectModel;
using WorkspaceManager.View.Base;
using WorkspaceManager.Model;
using WorkspaceManager.View.Base.Interfaces;
using WorkspaceManagerModel.Model.Operations;
using System.ComponentModel;
using WorkspaceManager.View.VisualComponents;
using System.Collections;
using WorkspaceManagerModel.Model.Interfaces;
using System.Windows.Controls.Primitives;
using Cryptool.PluginBase;
using System.Windows.Threading;
using System.Threading;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinFunctionVisual.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class BinComponentVisual : UserControl, IRouting, INotifyPropertyChanged, IUpdateableView
    {

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<VisualStateChangedArgs> StateChanged;
        public event EventHandler<PositionDeltaChangedArgs> PositionDeltaChanged;
        #endregion

        #region IRouting
        public ObjectSize ObjectSize
        {
            get
            {
                return new ObjectSize(this.ActualWidth, this.ActualHeight);
            }
        }

        public Point Position
        {
            get
            {
                return (Point)base.GetValue(PositionProperty);
            }
            set
            {
                base.SetValue(PositionProperty, value);
            }
        }

        public Point[] RoutingPoints
        {
            get
            {
                return new Point[] 
                {
                        new Point(Position.X-1, Position.Y-1),
                        new Point(Position.X-1, Position.Y + ObjectSize.Y + 1),
                        new Point(Position.X + ObjectSize.X + 1,Position.Y + 1),
                        new Point(Position.X + ObjectSize.X + 1, Position.Y + ObjectSize.Y + 1)
                };
            }
        }
        #endregion 

        #region Model

        private PluginModel model;
        public PluginModel Model
        {
            get { return model; }
            private set 
            { 
                model = value;
                State = (BinComponentState)Enum.Parse(typeof(BinComponentState), Model.ViewState.ToString());
            }
        }
        #endregion

        #region Fields

        #endregion

        #region Properties
        public Queue<Log> ErrorsTillReset { private set; get; }
        public ThumHack HackThumb = new ThumHack();
        public BinEditorVisual EditorVisual { private set; get; }

        public Vector Delta { private set; get; }
        public BinEditorVisual Editor { private set; get; }

        public bool HasComponentPresentation
        {
            get
            {
                UIElement e = null;
                Presentations.TryGetValue(BinComponentState.Presentation, out e);
                return e == null ? false : true;
            }
        }

        private Dictionary<BinComponentState, UIElement> presentations = new Dictionary<BinComponentState, UIElement>();
        public Dictionary<BinComponentState, UIElement> Presentations { get { return presentations; } }

        public UIElement ActivePresentation
        {
            get
            {
                UIElement o = null;
                Presentations.TryGetValue(State, out o);
                return o;
            }
        }

        private BinComponentState lastState;
        public BinComponentState LastState
        {
            set
            {
                lastState = value;
            }

            get
            {
                return lastState;
            }
        }

        private BinComponentState fullScreenState;
        public BinComponentState FullScreenState
        {
            set
            {
                fullScreenState = value;
            }

            get
            {
                return fullScreenState;
            }
        }

        public Image Icon
        {
            get
            {
                UIElement o = null;
                Presentations.TryGetValue(BinComponentState.Min, out o);
                return o as Image;
            }
        }

        private ObservableCollection<IControlMasterElement> iControlCollection = new ObservableCollection<IControlMasterElement>();
        public ObservableCollection<IControlMasterElement> IControlCollection { get { return iControlCollection; } }

        private ObservableCollection<BinConnectorVisual> connectorCollection = new ObservableCollection<BinConnectorVisual>();
        public ObservableCollection<BinConnectorVisual> ConnectorCollection { get { return connectorCollection; } }

        private ObservableCollection<BinConnectorVisual> southConnectorCollection = new ObservableCollection<BinConnectorVisual>();
        public ObservableCollection<BinConnectorVisual> SouthConnectorCollection { get { return southConnectorCollection; } }

        private ObservableCollection<BinConnectorVisual> northConnectorCollection = new ObservableCollection<BinConnectorVisual>();
        public ObservableCollection<BinConnectorVisual> NorthConnectorCollection { get { return northConnectorCollection; } }

        private ObservableCollection<BinConnectorVisual> eastConnectorCollection = new ObservableCollection<BinConnectorVisual>();
        public ObservableCollection<BinConnectorVisual> EastConnectorCollection { get { return eastConnectorCollection; } }

        private ObservableCollection<BinConnectorVisual> westConnectorCollection = new ObservableCollection<BinConnectorVisual>();
        public ObservableCollection<BinConnectorVisual> WestConnectorCollection { get { return westConnectorCollection; } }

        private ObservableCollection<Log> logMessages = new ObservableCollection<Log>();
        public ObservableCollection<Log> LogMessages { get { return logMessages; } }

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(BinComponentVisual), new FrameworkPropertyMetadata(false, OnIsSelectedChanged));

        public bool IsSelected
        {
            get { return (bool)base.GetValue(IsSelectedProperty); }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }

        public static readonly DependencyProperty LogNotifierProperty = DependencyProperty.Register("LogNotifier", typeof(BinLogNotifier),
            typeof(BinComponentVisual), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public BinLogNotifier LogNotifier
        {
            get { return (BinLogNotifier)base.GetValue(LogNotifierProperty); }
            set
            {
                base.SetValue(LogNotifierProperty, value);
            }
        }

        public static readonly DependencyProperty IsConnectorDragStartedProperty = DependencyProperty.Register("IsConnectorDragStarted", typeof(bool),
            typeof(BinComponentVisual), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public bool IsConnectorDragStarted
        {
            get { return (bool)base.GetValue(IsConnectorDragStartedProperty); }
            set
            {
                base.SetValue(IsConnectorDragStartedProperty, value);
            }
        }

        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.Register("IsDragging", typeof(bool),
            typeof(BinComponentVisual), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public bool IsDragging
        {
            get { return (bool)base.GetValue(IsDraggingProperty); }
            set
            {
                base.SetValue(IsDraggingProperty, value);
            }
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(BinComponentState), typeof(BinComponentVisual), new FrameworkPropertyMetadata(BinComponentState.Min, new PropertyChangedCallback(OnStateValueChanged)));

        public BinComponentState State
        {
            get
            {
                return (BinComponentState)base.GetValue(StateProperty);
            }
            set
            {
                base.SetValue(StateProperty, value);
            }
        }

        public static readonly DependencyProperty InternalStateProperty = DependencyProperty.Register("InternalState",
            typeof(PluginModelState), typeof(BinComponentVisual), new FrameworkPropertyMetadata(PluginModelState.Normal));

        public PluginModelState InternalState
        {
            get
            {
                return (PluginModelState)base.GetValue(InternalStateProperty);
            }
            set
            {
                base.SetValue(InternalStateProperty, value);
            }
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(Point), typeof(BinComponentVisual), new FrameworkPropertyMetadata(new Point(0, 0)));

        public static readonly DependencyProperty IsFullscreenProperty = DependencyProperty.Register("IsFullscreen",
                typeof(bool), typeof(BinComponentVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsFullscreenChanged)));

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

        public static readonly DependencyProperty IsICPopUpOpenProperty = DependencyProperty.Register("IsICPopUpOpen",
        typeof(bool), typeof(BinComponentVisual), new FrameworkPropertyMetadata(false));

        public bool IsICPopUpOpen
        {
            get
            {
                return (bool)base.GetValue(IsICPopUpOpenProperty);
            }
            set
            {
                base.SetValue(IsICPopUpOpenProperty, value);
            }
        }

        public static readonly DependencyProperty IsErrorDisplayVisibleProperty = DependencyProperty.Register("IsErrorDisplayVisible",
            typeof(bool), typeof(BinComponentVisual), new FrameworkPropertyMetadata(false));

        public bool IsErrorDisplayVisible
        {
            get
            {
                return (bool)base.GetValue(IsErrorDisplayVisibleProperty);
            }
            set
            {
                base.SetValue(IsErrorDisplayVisibleProperty, value);
            }
        }

        public static readonly DependencyProperty IsRepeatableProperty = DependencyProperty.Register("IsRepeatable",
            typeof(bool), typeof(BinComponentVisual), new FrameworkPropertyMetadata(false));

        public bool IsRepeatable
        {
            get
            {
                return (bool)base.GetValue(IsRepeatableProperty);
            }
            private set
            {
                base.SetValue(IsRepeatableProperty, value);
            }
        }

        public static readonly DependencyProperty RepeatProperty = DependencyProperty.Register("Repeat",
            typeof(bool), typeof(BinComponentVisual), new FrameworkPropertyMetadata(false));

        public bool Repeat
        {
            get
            {
                return (bool)base.GetValue(RepeatProperty);
            }
            private set
            {
                base.SetValue(RepeatProperty, value);
                Model.RepeatStart = value;
            }
        }

        public static readonly DependencyProperty CustomNameProperty = DependencyProperty.Register("CustomName",
            typeof(string), typeof(BinComponentVisual), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnCustomNameChanged)));

        public string CustomName
        {
            get
            {
                return (string)base.GetValue(CustomNameProperty);
            }
            set
            {
                base.SetValue(CustomNameProperty, value);
            }
        }

        public static readonly DependencyProperty IsICMasterProperty = DependencyProperty.Register("IsICMaster",
            typeof(bool), typeof(BinComponentVisual), new FrameworkPropertyMetadata(false));

        public bool IsICMaster
        {
            get
            {
                return (bool)base.GetValue(IsICMasterProperty);
            }
            set
            {
                base.SetValue(IsICMasterProperty, value);
            }
        }

        public static readonly DependencyProperty WindowHeightProperty = DependencyProperty.Register("WindowHeight",
            typeof(double), typeof(BinComponentVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double WindowHeight
        {
            get
            {
                return (double)base.GetValue(WindowHeightProperty);
            }
            set
            {
                if (value < 0)
                    return;

                base.SetValue(WindowHeightProperty, value);
            }
        }

        public static readonly DependencyProperty WindowWidthProperty = DependencyProperty.Register("WindowWidth",
            typeof(double), typeof(BinComponentVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double WindowWidth
        {
            get
            {
                return (double)base.GetValue(WindowWidthProperty);
            }
            set
            {
                if (value < 0)
                    return;

                base.SetValue(WindowWidthProperty, value);
            }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress",
            typeof(double), typeof(BinComponentVisual), new FrameworkPropertyMetadata((double)0));

        public double Progress
        {
            get
            {
                return (double)base.GetValue(ProgressProperty);
            }
            set
            {
                base.SetValue(ProgressProperty, value);
            }
        }

        public static readonly DependencyProperty FunctionNameProperty = DependencyProperty.Register("FunctionName",
            typeof(string), typeof(BinComponentVisual), new FrameworkPropertyMetadata(string.Empty));

        public string FunctionName
        {
            get
            {
                return (string)base.GetValue(FunctionNameProperty);
            }
            set
            {
                base.SetValue(FunctionNameProperty, value);
            }
        }
        #endregion

        #region Constructors
        public BinComponentVisual(PluginModel model)
        {
            Model = model;
            Model.UpdateableView = this;
            Editor = (BinEditorVisual)((WorkspaceManager)Model.WorkspaceModel.MyEditor).Presentation;
            ErrorsTillReset = new Queue<Log>();
            EditorVisual = (BinEditorVisual)((WorkspaceManager)Model.WorkspaceModel.MyEditor).Presentation;
            Presentations.Add(BinComponentState.Presentation, model.PluginPresentation);
            Presentations.Add(BinComponentState.Min, Model.getImage());
            Presentations.Add(BinComponentState.Data, new BinDataVisual(ConnectorCollection));
            Presentations.Add(BinComponentState.Log, new BinLogVisual(this));
            Presentations.Add(BinComponentState.Setting, new BinSettingsVisual(Model.Plugin, this,true));
            LastState = HasComponentPresentation ? BinComponentState.Presentation : BinComponentState.Log;
            InitializeComponent();
        }
        #endregion

        #region public
        public Point GetRoutingPoint(int routPoint)
        {
            switch (routPoint)
            {
                case 0:
                    return new Point(Position.X - 1, Position.Y - 1);
                case 1:
                    return new Point(Position.X - 1, Position.Y + ActualHeight + 1);
                case 2:
                    return new Point(Position.X + 1 + ActualWidth, Position.Y + 1);
                case 3:
                    return new Point(Position.X + ActualWidth + 1, Position.Y + ActualHeight + 1);
            }
            return default(Point);
        }

        public void update()
        {
            Progress = Model.PercentageFinished;
            Presentations[BinComponentState.Min] = Model.getImage();
            OnPropertyChanged("ActivePresentation");
        }
        #endregion

        #region private

        private void initializeVisual(PluginModel model)
        {
            IEnumerable<ConnectorModel> list = model.GetOutputConnectors().Concat<ConnectorModel>(model.GetInputConnectors());
            foreach (ConnectorModel m in list)
            {
                if (m.IControl && m.Outgoing)
                {
                    PluginModel pm = null;
                    if (m.GetOutputConnections().Count > 0)
                    {
                        pm = m.GetOutputConnections()[0].To.PluginModel;
                    }

                    IControlCollection.Add(new IControlMasterElement(m, pm));
                    continue;
                }

                if (m.IControl)
                    continue;

                addConnectorView(m);
            }

            SouthConnectorCollection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SouthConnectorCollectionCollectionChanged);
            LogNotifier = new BinLogNotifier(LogMessages, this);
            LogNotifier.ErrorMessagesOccured += new EventHandler<ErrorMessagesOccuredArgs>(LogNotifierErrorMessagesOccuredHandler);
            //LogMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(LogMessagesCollectionChanged);
            Model.Plugin.OnGuiLogNotificationOccured += new GuiLogNotificationEventHandler(OnGuiLogNotificationOccuredHandler);
            WindowWidth = Model.GetWidth();
            WindowHeight = Model.GetHeight();
            //IsRepeatable = Model.Startable;
            Repeat = Model.RepeatStart;
            Position = model.GetPosition();
            FunctionName = Model.Plugin.GetPluginInfoAttribute().Caption;
            CustomName = Model.GetName();
            //needs changes in Model
            IsICMaster = Model.HasIControlInputs();
            SetBinding(BinComponentVisual.IsDraggingProperty,
                Util.CreateIsDraggingBinding(new Thumb[] { ContentThumb, TitleThumb, ScaleThumb, HackThumb }));
            setWindowColors(ColorHelper.GetColor(Model.PluginType), ColorHelper.GetColorLight(Model.PluginType));
        }

        void SouthConnectorCollectionCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SouthConnectorCollection.Count"));
        }

        void LogNotifierErrorMessagesOccuredHandler(object sender, ErrorMessagesOccuredArgs e)
        {
            if (e.HasErrors)
                IsErrorDisplayVisible = true;
            else
                IsErrorDisplayVisible = false;
        }

        //void LogMessagesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        //    {
        //        Log log = (Log)e.NewItems[0];
        //        if (log.Level == NotificationLevel.Error)
        //        {
        //            IsErrorDisplayVisible = true;
        //        }
        //    }

        //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        //    {
        //        IsErrorDisplayVisible = false;
        //    }
        //}

        private void OnGuiLogNotificationOccuredHandler(IPlugin sender, GuiLogEventArgs args)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                LogMessages.Add(new Log(args));
            }
            , null);
        }

        private void setWindowColors(Color Border, Color Background)
        {
            Window.BorderBrush = new SolidColorBrush(Border);
            Window.Background = new SolidColorBrush(Background);
        }

        private void addConnectorView(ConnectorModel model)
        {
            BinConnectorVisual bin = new BinConnectorVisual(model, this);

            Binding bind = new Binding();
            bind.Path = new PropertyPath(BinEditorVisual.IsLinkingProperty);
            bind.Source = EditorVisual;
            bin.SetBinding(BinConnectorVisual.IsLinkingProperty, bind);

            switch (model.Orientation)
            {
                case ConnectorOrientation.Unset:
                    if(model.Outgoing)
                        EastConnectorCollection.Add(bin);
                    else
                        WestConnectorCollection.Add(bin);
                    break;
                case ConnectorOrientation.West:
                    WestConnectorCollection.Add(bin);
                    break;
                case ConnectorOrientation.East:
                    EastConnectorCollection.Add(bin);
                    break;
                case ConnectorOrientation.North:
                    NorthConnectorCollection.Add(bin);
                    break;
                case ConnectorOrientation.South:
                    SouthConnectorCollection.Add(bin);
                    break;
            }
            ConnectorCollection.Add(bin);
        }

        #endregion

        #region protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            initializeVisual(Model);
        }
        #endregion

        #region Event Handler
        #region DragDropHandler

        private void PreviewDropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("BinConnector"))
            {
                try
                {
                    ItemsControl items = (ItemsControl)sender;
                    BinConnectorVisual connector = (BinConnectorVisual)e.Data.GetData("BinConnector");

                    if (connector.WindowParent != this)
                        return;

                    switch (connector.Orientation)
                    {
                        case ConnectorOrientation.North:
                            NorthConnectorCollection.Remove(connector);
                            break;
                        case ConnectorOrientation.South:
                            SouthConnectorCollection.Remove(connector);
                            break;
                        case ConnectorOrientation.East:
                            EastConnectorCollection.Remove(connector);
                            break;
                        case ConnectorOrientation.West:
                            WestConnectorCollection.Remove(connector);
                            break;
                    }

                    IList itemsSource = (IList) items.ItemsSource;
                    itemsSource.Add(connector);
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.ToString());
                }
            }
        }
        #endregion

        private void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            BinComponentState localState = BinComponentState.Log;
            switch ((string)item.Tag)
            {
                case "presentation":
                    localState = BinComponentState.Presentation;
                    break;

                case "data":
                    localState = BinComponentState.Data;
                    break;

                case "log":
                    localState = BinComponentState.Log;
                    break;

                case "setting":
                    localState = BinComponentState.Setting;
                    break;

                case "help":
                    OnlineHelp.InvokeShowPluginDocPage(model.PluginType);
                    return;
            }
            Editor.SetFullscreen(this, localState);
        }

        private void ActionHandler(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;

            if (b.Content is BinComponentState)
            {
                State = (BinComponentState)b.Content;
                return;
            }

            if (b.Content is BinComponentAction && ((BinComponentAction)b.Content) == BinComponentAction.LastState)
            {
                State = (BinComponentState) LastState;
                return;
            }

            if (b.Content is string)
            {
                string s = (string)b.Content;

                if (s == "Info")
                    OnlineHelp.InvokeShowPluginDocPage(model.PluginType);
            }

            e.Handled = true;
        }

        private void ScaleDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (State == BinComponentState.Min)
            {
                if ((Window.ActualHeight + e.VerticalChange >= Window.MinHeight + 15) && (Window.ActualWidth + e.HorizontalChange >= Window.MinHeight + 15))
                {
                    Model.WorkspaceModel.ModifyModel(new ResizeModelElementOperation(Model, 300, 200));
                    State = LastState;
                }
                else
                { return; }
            }

            if ((Window.ActualHeight + e.VerticalChange <= 80 - 15) && (Window.ActualWidth + e.HorizontalChange <= 80 - 15))
            {
                State = BinComponentState.Min;
            }
            else
            {
                WindowHeight += e.VerticalChange;
                WindowWidth += e.HorizontalChange;
            }

            Model.WorkspaceModel.ModifyModel(new ResizeModelElementOperation(Model, WindowWidth, WindowHeight));
            e.Handled = true;
        }

        private void PositionDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Point point = new Point(Position.X + e.HorizontalChange, Position.Y + e.VerticalChange);
            Delta = new Vector(e.HorizontalChange, e.VerticalChange);
            if (PositionDeltaChanged != null)
                PositionDeltaChanged.Invoke(this, new PositionDeltaChangedArgs() { PosDelta = Delta });
            //Model.WorkspaceModel.ModifyModel(new MoveModelElementOperation(Model, point));
        }

        private static void OnStateValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinComponentVisual bin = (BinComponentVisual)d;
            bin.LastState = (BinComponentState)e.OldValue;
            bin.OnPropertyChanged("LastState");
            bin.OnPropertyChanged("ActivePresentation");
            bin.OnPropertyChanged("HasComponentPresentation");
            if(bin.StateChanged != null)
                bin.StateChanged.Invoke(bin,new VisualStateChangedArgs(){State = bin.State});
            bin.Model.ViewState = (PluginViewState)Enum.Parse(typeof(PluginViewState), e.NewValue.ToString());
            if (bin.State == BinComponentState.Log)
                bin.IsErrorDisplayVisible = false;
        }

        private static void OnIsFullscreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinComponentVisual bin = (BinComponentVisual)d;

            if (bin.IsFullscreen)
                bin.FullScreenState = bin.State;
            else
                bin.State = bin.FullScreenState;

            bin.OnPropertyChanged("ActivePresentation");
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinComponentVisual bin = (BinComponentVisual)d;
        }

        private static void OnCustomNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinComponentVisual bin = (BinComponentVisual)d;
            bin.Model.WorkspaceModel.ModifyModel(new RenameModelElementOperation(bin.Model, (string)e.NewValue));
            if (bin.Model.WorkspaceModel.MyEditor != null)
            {
                ((WorkspaceManager)bin.Model.WorkspaceModel.MyEditor).HasChanges = true;
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            // process only if workspace is not running
            if (Model != null && !((WorkspaceManager)Model.WorkspaceModel.MyEditor).isExecuting())
            {
                this.State = BinComponentState.Min;
                Model.WorkspaceModel.ModifyModel(new DeletePluginModelOperation(Model));
            }
        }

        private void RepeatHandler(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if(b == null)
                return;

            Repeat = (bool)b.Content;
        }
        #endregion

    }

    #region Events
    public class VisualStateChangedArgs : EventArgs
    {
        public BinComponentState State { get; set; }
    }

    public class PositionDeltaChangedArgs : EventArgs
    {
        public Vector PosDelta { get; set; }
    }
    #endregion

    #region Converter

    public class StateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is BinComponentState))
                return double.Epsilon;

            BinComponentState state = (BinComponentState)value;
            if (state != BinComponentState.Min)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateFullscreenConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || !(values[0] is BinComponentState) || !(values[1] is bool))
                return double.Epsilon;

            BinComponentState state = (BinComponentState)values[0];
            bool b = (bool)values[1];
            if (state != BinComponentState.Min && !b)
                return true;
            else
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsDraggingConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            if (value.Count() == 4)
            {
                bool b1 = (bool)value[0], b2 = (bool)value[1], b3 = (bool)value[2], b4 = (bool)value[3];
                if (b1 || b2 || b3 || b4)
                    return true;
                else
                    return false;
            }
            else
            {
                bool b1 = (bool)value[0], b2 = (bool)value[1];
                if (b1 || b2)
                    return true;
                else
                    return false;
            }
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class testconverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var baseElement = values[0] as FrameworkElement;
            var element = (double)values[1];

            return Math.Abs(element -baseElement.ActualWidth);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Custom class

    public class ThumHack : Thumb
    {
        public bool HackDrag 
        { 
            get 
            { 
                return IsDragging;
            }
            set 
            {
                IsDragging = value;
            }
        }
    }

    public class Log
    {
        public NotificationLevel Level { get; set; }
        public String Message { get; set; }
        public String Date { get; set; }
        public String ID { get; set; }

        public Log(GuiLogEventArgs element)
        {
            Message = element.Message;
            Level = element.NotificationLevel;
            Date = element.DateTime.ToString("dd.MM.yyyy, H:mm:ss");
        }

        public override string ToString()
        {
            return Message;
        }
    }

    public class CustomTextBox : TextBox, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
            typeof(bool), typeof(CustomTextBox), new FrameworkPropertyMetadata(false, OnIsSelectedChanged));

        public bool IsSelected
        {
            get { return (bool)base.GetValue(IsSelectedProperty); }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomTextBox bin = (CustomTextBox)d;
            if (bin.IsSelected)
                return;
            else
                bin.Focusable = false;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            Focusable = true;
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Focusable"));
            Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Focusable = false;
                if (PropertyChanged != null)
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Focusable"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class NSWEStackPanel : StackPanel
    {
        public PanelOrientation PanelOrientation { get; set; }

        protected override Size MeasureOverride(Size constraint)
        {
            IEnumerable<BinConnectorVisual> filter = Children.OfType<BinConnectorVisual>();

            foreach (BinConnectorVisual bin in filter)
            {
                switch (PanelOrientation)
                {
                    case Base.PanelOrientation.East:
                        if (bin.Orientation == ConnectorOrientation.East)
                            continue;

                        bin.Orientation = ConnectorOrientation.East;
                        if (bin.IsOutgoing)
                            bin.RotationAngle = (double)-90;
                        else
                            bin.RotationAngle = (double)90;
                        break;
                    case Base.PanelOrientation.South:
                        if (bin.Orientation == ConnectorOrientation.South)
                            continue;

                        bin.Orientation = ConnectorOrientation.South;
                        if (bin.IsOutgoing)
                            bin.RotationAngle = (double)0;
                        else
                            bin.RotationAngle = (double)180;
                        break;
                    case Base.PanelOrientation.West:
                        if (bin.Orientation == ConnectorOrientation.West)
                            continue;

                        bin.Orientation = ConnectorOrientation.West;
                        if (bin.IsOutgoing)
                            bin.RotationAngle = (double)90;
                        else
                            bin.RotationAngle = (double)-90;
                        break;
                    case Base.PanelOrientation.North:
                        if (bin.Orientation == ConnectorOrientation.North)
                            continue;

                        bin.Orientation = ConnectorOrientation.North;
                        if (bin.IsOutgoing)
                            bin.RotationAngle = (double)180;
                        else
                            bin.RotationAngle = (double)0;
                        break;
                }
            }

            return base.MeasureOverride(constraint);
        }
    }


    public class IControlMasterElement
    {
        public event EventHandler PluginModelChanged;

        private PluginModel pluginModel;
        public PluginModel PluginModel 
        { 
            get 
            {
                return pluginModel;
            } 
            set 
            {
                pluginModel = value;
                if (PluginModelChanged != null)
                    PluginModelChanged.Invoke(this, null);
            } 
        }
        public ConnectorModel ConnectorModel { get; private set; }

        public IControlMasterElement(ConnectorModel connectorModel, PluginModel pluginModel)
        {
            this.ConnectorModel = connectorModel;
            this.PluginModel = pluginModel;
        }
    }
    #endregion
}
