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

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinFunctionVisual.xaml
    /// </summary>
    public partial class BinFunctionVisual : UserControl, IRouting, INotifyPropertyChanged
    {

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
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

        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State",
            typeof(BinFuctionState), typeof(BinFunctionVisual), new FrameworkPropertyMetadata(BinFuctionState.Min, new PropertyChangedCallback(OnMyValueChanged)));

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
            }
        }

        public static readonly DependencyProperty InternalStateProperty = DependencyProperty.Register("InternalState",
            typeof(BinInternalState), typeof(BinFunctionVisual), new FrameworkPropertyMetadata(BinInternalState.Normal));

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
            typeof(Point), typeof(BinFunctionVisual), new FrameworkPropertyMetadata(new Point(0,0)));


        public static readonly DependencyProperty HasFunctionPresentationProperty = DependencyProperty.Register("HasFunctionPresentation",
            typeof(bool), typeof(BinFunctionVisual), new FrameworkPropertyMetadata(false));

        public bool HasFunctionPresentation
        {
            get
            {
                return (bool)base.GetValue(HasFunctionPresentationProperty);
            }
            set
            {
                base.SetValue(HasFunctionPresentationProperty, value);
            }
        }

        public static readonly DependencyProperty WindowHeightProperty = DependencyProperty.Register("WindowHeight",
            typeof(double), typeof(BinFunctionVisual), new FrameworkPropertyMetadata(double.Epsilon));

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
            typeof(double), typeof(BinFunctionVisual), new FrameworkPropertyMetadata(double.Epsilon));

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
            typeof(double), typeof(BinFunctionVisual), new FrameworkPropertyMetadata((double)0));

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
            typeof(string), typeof(BinFunctionVisual), new FrameworkPropertyMetadata(string.Empty));

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
        public BinFunctionVisual(PluginModel model)
        {
            #region test
            Model = model;
            #endregion
            FunctionName = Model.GetName();
            Presentations.Add(BinFuctionState.Presentation, model.PluginPresentation);
            Presentations.Add(BinFuctionState.Min, Model.getImage());
            InitializeComponent();
            setWindowColors(ColorHelper.GetColor(Model.GetType()), ColorHelper.GetColorLight(Model.GetType()));
        }
        #endregion

        #region public

        #endregion

        #region private

        private void setWindowColors(Color Border, Color Background)
        {
            Window.BorderBrush = new SolidColorBrush(Border);
            Window.Background = new SolidColorBrush(Background);
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
        #endregion

        #region Event Handler
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

        private void PositionDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Position = new Point(Position.X + e.HorizontalChange, Position.Y + e.VerticalChange);
        }

        private static void OnMyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinFunctionVisual bin = (BinFunctionVisual)d;
            bin.LastState = (BinFuctionState)e.OldValue;
            bin.OnPropertyChanged("LastState");
        }

        #endregion

        private void Close(object sender, RoutedEventArgs e)
        {
            IControlPopUp.IsOpen = false;
        }

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
    #endregion
}
