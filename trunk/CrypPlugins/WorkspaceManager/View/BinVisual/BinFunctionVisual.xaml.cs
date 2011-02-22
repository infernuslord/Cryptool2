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
using WorkspaceManager.View.Base;
using WorkspaceManager.Model;
using WorkspaceManager.View.Base.Interfaces;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinFunctionVisual.xaml
    /// </summary>
    public partial class BinFunctionVisual : UserControl, IRouting
    {
        public static class BinFunctionVisualManager 
        {
 
        }

        #region IRouting
        public ObjectSize ObjectSize
        {
            get
            {
                return new ObjectSize(this.ActualWidth, this.ActualHeight);
            }
        }

        private Point position;
        public Point Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
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

        #region DependencyProperties
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", 
            typeof(BinFuctionState), typeof(BinFunctionVisual), new FrameworkPropertyMetadata(BinFuctionState.Presentation));

        public BinFuctionState State
        {
            get
            {
                return (BinFuctionState)base.GetValue(StateProperty);
            }
            set
            {
                base.SetValue(StateProperty, value);
                //this.Model.BinState = value;
            }
        }
        #endregion

        #region Constructors
        public BinFunctionVisual()
        {
            Model = new PluginModel();
            InitializeComponent();
        }
        #endregion
        #region Event Handler
        private void ActionHandler(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            State = (BinFuctionState) b.Content;
        }

        private void ScaleDragDeltaHandler(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            this.Height = Model.Height = this.ActualHeight + e.VerticalChange;
            this.Width = Model.Width = this.ActualWidth + e.HorizontalChange;
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

            BinFuctionState state = (BinFuctionState) value;
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
}
