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
using System.ComponentModel;
using WorkspaceManager.Model;
using System.Windows.Controls.Primitives;
using WorkspaceManagerModel.Model.Interfaces;
using System.Globalization;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for ConnectorView.xaml
    /// </summary>
    public partial class BinConnectorVisual : UserControl, IUpdateableView, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public string ConnectorName 
        { 
            get 
            {
                return Model != null ? Model.GetName() : "Error";
            } 
        }

        public string TypeName
        {
            get
            {
                return Model.ConnectorType != null ? Model.ConnectorType.Name : "Class Not Found";
            }
        }

        public string Data
        {
            get
            {
                if (Model == null)
                    return "No Data";

                if (Model.LastData == null)
                    return "No Data";

                if (Model.LastData is Byte[])
                {
                    StringBuilder builder = new StringBuilder();
                    Byte[] b = (Byte[])Model.Data;
                    foreach (var e in b)
                        builder.Append(e);
                    return builder.ToString();
                }

                return Model.LastData.ToString();
            }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model",
            typeof(ConnectorModel), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMyValueChanged)));

        public ConnectorModel Model
        {
            get
            {
                return (ConnectorModel)base.GetValue(ModelProperty);
            }
            set
            {
                base.SetValue(ModelProperty, value);
                Model.UpdateableView = this;
            }
        }

        public static readonly DependencyProperty IsMandatoryProperty = DependencyProperty.Register("IsMandatory",
            typeof(bool), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(false));

        public bool IsMandatory
        {
            get
            {
                return (bool)base.GetValue(IsMandatoryProperty);
            }
            set
            {
                base.SetValue(IsMandatoryProperty, value);
            }
        }

        public static readonly DependencyProperty IsOutgoingProperty = DependencyProperty.Register("IsOutgoing",
            typeof(bool), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(false));

        public bool IsOutgoing
        {
            get
            {
                return (bool)base.GetValue(IsOutgoingProperty);
            }
            set
            {
                base.SetValue(IsOutgoingProperty, value);
            }
        }

        public static readonly DependencyProperty RotationAngleProperty = DependencyProperty.Register("RotationAngle",
            typeof(double), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(double.Epsilon));

        public double RotationAngle
        {
            get
            {
                return (double)base.GetValue(RotationAngleProperty);
            }
            set
            {
                base.SetValue(RotationAngleProperty, value);
            }
        }

        public static readonly DependencyProperty FunctionColorProperty = DependencyProperty.Register("FunctionColor",
            typeof(SolidColorBrush), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(Brushes.Black));

        public SolidColorBrush FunctionColor
        {
            get
            {
                return (SolidColorBrush)base.GetValue(FunctionColorProperty);
            }
            set
            {
                base.SetValue(FunctionColorProperty, value);
            }
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
            typeof(ConnectorOrientation), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(ConnectorOrientation.Unset));

        public ConnectorOrientation Orientation
        {
            get
            {
                return (ConnectorOrientation)base.GetValue(OrientationProperty);
            }
            set
            {
                base.SetValue(OrientationProperty, value);
                Model.Orientation = value;
            }
        }

        public static readonly DependencyProperty WindowParentProperty = DependencyProperty.Register("WindowParent",
            typeof(BinComponentVisual), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(null));

        public BinComponentVisual WindowParent
        {
            get
            {
                return (BinComponentVisual)base.GetValue(WindowParentProperty);
            }
            set
            {
                base.SetValue(WindowParentProperty, value);
            }
        }

        public static readonly DependencyProperty MarkedProperty = DependencyProperty.Register("Marked",
            typeof(bool), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(false));

        public bool Marked
        {
            get
            {
                return (bool)base.GetValue(MarkedProperty);
            }
            set
            {
                base.SetValue(MarkedProperty, value);
            }
        }
        #endregion

        public BinConnectorVisual(ConnectorModel model, BinComponentVisual component)
        {
            // TODO: Complete member initialization
            this.Model = model;
            this.WindowParent = component;
            InitializeComponent();
        }

        private static void OnMyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinConnectorVisual bin = (BinConnectorVisual)d;
            bin.FunctionColor = new SolidColorBrush(ColorHelper.GetLineColor(bin.Model.ConnectorType));
            bin.IsMandatory = bin.Model.IsMandatory;
            bin.IsOutgoing = bin.Model.Outgoing;
            //bin.Model.Orientation = bin.Orientation;
        }

        public Point GetPosition()
        {
            try
            {
                if (!(VisualParent is Panel))
                    throw new Exception("Parent is not Panel");

                GeneralTransform gTransform, gTransformSec;
                Point point, relativePoint;
                Panel ic = (Panel)VisualParent;

                gTransform = ic.TransformToVisual(WindowParent);
                gTransformSec = this.TransformToVisual(ic);

                point = gTransform.Transform(new Point(0, 0));
                relativePoint = gTransformSec.Transform(new Point(0, 0));
                Point result = new Point(WindowParent.Position.X + point.X + relativePoint.X, WindowParent.Position.Y + point.Y + relativePoint.Y);
                return result;
            }
            catch (Exception)
            {
                return new Point(0, 0);
            }
        }

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

        private static void OnSelectedConnectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinConnectorVisual bin = (BinConnectorVisual)d;
        }

        public bool CanConnect
        {
            get { throw new NotImplementedException(); }
        }

        public void update()
        {
            throw new NotImplementedException();
        }

        private void ToolTipOpeningHandler(object sender, ToolTipEventArgs e)
        {
            OnPropertyChanged("Data");
        }
    }

    public class BinConnectorVisualBindingConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            BinConnectorVisual connector = (BinConnectorVisual)parameter;
            Point p = connector.GetPosition();
            switch (connector.Orientation)
            {
                case ConnectorOrientation.West:
                    return new Point(p.X, p.Y + connector.ActualHeight / 2);
                case ConnectorOrientation.East:
                    return new Point(p.X + connector.ActualWidth, p.Y + connector.ActualHeight / 2);
                case ConnectorOrientation.North:
                    return new Point(p.X + connector.ActualWidth / 2, p.Y);
                case ConnectorOrientation.South:
                    return new Point(p.X + connector.ActualWidth / 2, p.Y + connector.ActualHeight);
            }

            return new Point(0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
