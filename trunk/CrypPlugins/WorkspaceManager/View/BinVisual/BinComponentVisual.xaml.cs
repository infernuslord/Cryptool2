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

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinFunctionVisual.xaml
    /// </summary>
    public partial class BinComponentVisual : UserControl, IRouting, INotifyPropertyChanged
    {

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Close;
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
            private set { model = value; }
        }
        #endregion

        #region Properties

        public bool HasComponentPresentation
        {
            get
            {
                UIElement e = null;
                Presentations.TryGetValue(BinFuctionState.Presentation, out e);
                return e == null ? false : true;
            }
        }

        private Dictionary<BinFuctionState, UIElement> presentations = new Dictionary<BinFuctionState, UIElement>();
        public Dictionary<BinFuctionState, UIElement> Presentations { get { return presentations; } }

        public UIElement ActivePresentation
        {
            get
            {
                UIElement o = null;
                Presentations.TryGetValue(State, out o);
                return o;
            }
        }

        private BinFuctionState lastState;
        public BinFuctionState LastState
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

        private ObservableCollection<BinConnectorVisual> southConnectorCollection = new ObservableCollection<BinConnectorVisual>();
        public ObservableCollection<BinConnectorVisual> SouthConnectorCollection { get { return southConnectorCollection; } }

        private ObservableCollection<BinConnectorVisual> northConnectorCollection = new ObservableCollection<BinConnectorVisual>();
        public ObservableCollection<BinConnectorVisual> NorthConnectorCollection { get { return northConnectorCollection; } }

        private ObservableCollection<BinConnectorVisual> eastConnectorCollection = new ObservableCollection<BinConnectorVisual>();
        public ObservableCollection<BinConnectorVisual> EastConnectorCollection { get { return eastConnectorCollection; } }

        private ObservableCollection<BinConnectorVisual> westConnectorCollection = new ObservableCollection<BinConnectorVisual>();
        public ObservableCollection<BinConnectorVisual> WestConnectorCollection { get { return westConnectorCollection; } }

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool),
            typeof(BinComponentVisual), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public bool IsSelected
        {
            get { return (bool)base.GetValue(IsSelectedProperty); }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(BinFuctionState), typeof(BinComponentVisual), new FrameworkPropertyMetadata(BinFuctionState.Min, new PropertyChangedCallback(OnMyValueChanged)));

        public BinFuctionState State
        {
            get
            {
                return (BinFuctionState)base.GetValue(StateProperty);
            }
            set
            {
                base.SetValue(StateProperty, value);
                OnPropertyChanged("ActivePresentation");
                OnPropertyChanged("HasComponentPresentation");
            }
        }

        public static readonly DependencyProperty InternalStateProperty = DependencyProperty.Register("InternalState",
            typeof(BinInternalState), typeof(BinComponentVisual), new FrameworkPropertyMetadata(BinInternalState.Normal));

        public BinInternalState InternalState
        {
            get
            {
                return (BinInternalState)base.GetValue(InternalStateProperty);
            }
            set
            {
                base.SetValue(InternalStateProperty, value);
            }
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(Point), typeof(BinComponentVisual), new FrameworkPropertyMetadata(new Point(0,0)));

        public static readonly DependencyProperty IsFullscreenProperty = DependencyProperty.Register("IsFullscreen",
                typeof(bool), typeof(BinComponentVisual), new FrameworkPropertyMetadata(false));

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


        public static readonly DependencyProperty IsRepeatableProperty = DependencyProperty.Register("IsRepeatable",
            typeof(bool), typeof(BinComponentVisual), new FrameworkPropertyMetadata(false));

        public bool IsRepeatable
        {
            get
            {
                return (bool)base.GetValue(IsRepeatableProperty);
            }
            set
            {
                base.SetValue(IsRepeatableProperty, value);
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
            Presentations.Add(BinFuctionState.Presentation, model.PluginPresentation);
            Presentations.Add(BinFuctionState.Min, Model.getImage());
            Presentations.Add(BinFuctionState.Data, new DataPresentation());
            Presentations.Add(BinFuctionState.Log, new LogPresentation());
            Presentations.Add(BinFuctionState.Setting, new TaskPaneCtrl());
            InitializeComponent();
        }
        #endregion

        #region public

        #endregion

        #region private

        private void initializeVisual(PluginModel model)
        {
            IEnumerable<ConnectorModel> list = model.GetOutputConnectors().Concat<ConnectorModel>(model.GetInputConnectors());
            foreach (ConnectorModel m in list)
            {
                if (m.IControl)
                    continue;

                AddConnectorView(m);
            }

            Position = model.GetPosition();
            FunctionName = Model.GetName();
            setWindowColors(ColorHelper.GetColor(Model.GetType()), ColorHelper.GetColorLight(Model.GetType()));
        }

        private void setWindowColors(Color Border, Color Background)
        {
            Window.BorderBrush = new SolidColorBrush(Border);
            Window.Background = new SolidColorBrush(Background);
        }

        public void AddConnectorView(ConnectorModel model)
        {
            switch (model.Orientation)
            {
                case ConnectorOrientation.Unset:
                    if(model.Outgoing)
                        EastConnectorCollection.Add(new BinConnectorVisual(model, this));
                    else
                        WestConnectorCollection.Add(new BinConnectorVisual(model, this));
                    break;
                case ConnectorOrientation.West:
                    WestConnectorCollection.Add(new BinConnectorVisual(model, this));
                    break;
                case ConnectorOrientation.East:
                    EastConnectorCollection.Add(new BinConnectorVisual(model, this));
                    break;
                case ConnectorOrientation.North:
                    NorthConnectorCollection.Add(new BinConnectorVisual(model, this));
                    break;
                case ConnectorOrientation.South:
                    SouthConnectorCollection.Add(new BinConnectorVisual(model, this));
                    break;
            }
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
        private void PreviewDragEnterHandler(object sender, DragEventArgs e)
        {

        }

        private void PreviewDragLeaveHandler(object sender, DragEventArgs e)
        {

        }

        private void PreviewDropHandler(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("BinConnector"))
            {
                try
                {
                    ItemsControl items = (ItemsControl)sender;
                    IEnumerable itemsSource = items.ItemsSource;
                    BinConnectorVisual connector = e.Data.GetData("BinConnector") as BinConnectorVisual;

                    if (itemsSource is IList && !((IList)itemsSource).Contains(connector))
                        ((IList)itemsSource).Add(connector);
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.ToString());
                }
            }
        }
        #endregion

        private void ActionHandler(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;

            if (b.Content is BinFuctionState)
                State = (BinFuctionState)b.Content;

            if (b.Content is bool)
                IControlPopUp.IsOpen = (bool)b.Content;
        }

        private void ScaleDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            WindowHeight += e.VerticalChange;
            WindowWidth += e.HorizontalChange;
            Model.WorkspaceModel.ModifyModel(new ResizeModelElementOperation(Model, WindowWidth, WindowHeight)); 
        }

        private void ClosePopUp(object sender, RoutedEventArgs e)
        {
            IControlPopUp.IsOpen = false;
        }

        private void PositionDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Position = new Point(Position.X + e.HorizontalChange, Position.Y + e.VerticalChange);
            Model.WorkspaceModel.ModifyModel(new MoveModelElementOperation(Model,Position)); 
        }

        private static void OnMyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinComponentVisual bin = (BinComponentVisual)d;
            bin.LastState = (BinFuctionState)e.OldValue;
            bin.OnPropertyChanged("LastState");
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            if (Close != null)
                Close.Invoke(this, new EventArgs());
        }
        #endregion
    }

    #region Converter
    public class StateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is BinFuctionState))
                return double.Epsilon;

            BinFuctionState state = (BinFuctionState)value;
            if (state != BinFuctionState.Min)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
    #endregion

    #region Custom class
    public class CustomTextBox : TextBox
    {

        protected override void OnInitialized(EventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(TextBox),
                TextBox.KeyUpEvent,
                new System.Windows.Input.KeyEventHandler(TextBox_KeyUp));
            base.OnInitialized(e);
        }

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;
            ((TextBox)sender).CaretBrush = Brushes.Transparent;
            e.Handled = true;
        }

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender).CaretBrush = Brushes.Gray;
        }

        private void CTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).CaretBrush = Brushes.Transparent;
        }
    }

    public class NSWEStackPanel : StackPanel
    {
        public PanelOrientation PanelOrientation { get; set; }

        protected override Size MeasureOverride(Size constraint)
        {
            IEnumerable<BinConnectorVisual> filter = Children.OfType<BinConnectorVisual>();

            foreach (BinConnectorVisual bin in filter)
            {
                RotateTransform t = (RotateTransform)bin.LayoutTransform;

                switch (PanelOrientation)
                {
                    case Base.PanelOrientation.East:
                        if (bin.Orientation == ConnectorOrientation.East)
                            continue;

                        bin.Orientation = ConnectorOrientation.East;
                        if (bin.IsOutgoing)
                            t.Angle = (double)-90;
                        else
                            t.Angle = (double)90;
                        break;
                    case Base.PanelOrientation.South:
                        if (bin.Orientation == ConnectorOrientation.South)
                            continue;

                        bin.Orientation = ConnectorOrientation.South;
                        if (bin.IsOutgoing)
                            t.Angle = (double)0;
                        else
                            t.Angle = (double)180;
                        break;
                    case Base.PanelOrientation.West:
                        if (bin.Orientation == ConnectorOrientation.West)
                            continue;
                                                
                        bin.Orientation = ConnectorOrientation.East;
                        if (bin.IsOutgoing)
                            t.Angle = (double)90;
                        else
                            t.Angle = (double)-90;
                        break;
                    case Base.PanelOrientation.North:
                        if (bin.Orientation == ConnectorOrientation.North)
                            continue;

                        bin.Orientation = ConnectorOrientation.East;
                        if (bin.IsOutgoing)
                            t.Angle = (double)180;
                        else
                            t.Angle = (double)0;
                        break;
                }
            }

            return base.MeasureOverride(constraint);
        }
    }
    #endregion
}
