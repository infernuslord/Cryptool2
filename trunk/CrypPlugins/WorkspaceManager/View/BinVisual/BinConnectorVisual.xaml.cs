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
using WorkspaceManager.View.Base;
using Cryptool.PluginBase;
using WorkspaceManagerModel.Model.Tools;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for ConnectorView.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
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
                return Model != null ? Model.GetName() : Properties.Resources.Error;
            }
        }

        public string TypeName
        {
            get
            {
                return Model.ConnectorType != null ? Model.ConnectorType.Name : Properties.Resources.Class_Not_Found;
            }
        }

        public string Data
        {
            get
            {
                if (Model == null || Model.LastData == null)
                    return Properties.Resources.No_data;

                return ViewHelper.GetDataPresentationString(Model.LastData);
            }
        }
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description",
            typeof(string), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(string.Empty));

        public string Description
        {
            get
            {
                return (string)base.GetValue(DescriptionProperty);
            }
            set
            {
                base.SetValue(DescriptionProperty, value);
            }
        }

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption",
            typeof(string), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(string.Empty));

        public string Caption
        {
            get
            {
                return (string)base.GetValue(CaptionProperty);
            }
            set
            {
                base.SetValue(CaptionProperty, value);
            }
        }

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
            typeof(bool), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnMarkedValueChanged)));

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

        public static readonly DependencyProperty CVLevelProperty = DependencyProperty.Register("CVLevel",
            typeof(ConversionLevelInformation), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(null));

        public ConversionLevelInformation CVLevel
        {
            get
            {
                return (ConversionLevelInformation)base.GetValue(CVLevelProperty);
            }
            set
            {
                base.SetValue(CVLevelProperty, value);
            }
        }

        public static readonly DependencyProperty IsLinkingProperty = DependencyProperty.Register("IsLinking",
            typeof(bool), typeof(BinConnectorVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsLinkingValueChanged)));

        public bool IsLinking
        {
            get
            {
                return (bool)base.GetValue(IsLinkingProperty);
            }
            set
            {
                base.SetValue(IsLinkingProperty, value);
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

        private static void OnMarkedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinConnectorVisual bin = (BinConnectorVisual)d;
            BinConnectorVisual selected = bin.WindowParent.EditorVisual.SelectedConnector;

            //if (selected == null)
            //    return;

            //if (selected.Equals(bin))
            //    bin.CVLevel = null;
            //else
            //    bin.CVLevel = Util.ConversionCheck(bin.Model, bin.WindowParent.EditorVisual.SelectedConnector.Model);
        }

        private static void OnIsLinkingValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinConnectorVisual bin = (BinConnectorVisual)d;

            if (bin.IsLinking == false)
            {
                bin.Marked = false;
                return;
            }

            BinConnectorVisual selected = bin.WindowParent.EditorVisual.SelectedConnector;
            if (bin == selected)
                bin.CVLevel = new ConversionLevelInformation() { Level = ConversionLevel.NA };
            else
            {
                ConversionLevel lvl = WorkspaceModel.compatibleConnectors(bin.Model, selected.Model);
                bin.CVLevel = new ConversionLevelInformation() { Level = lvl };
            }

            if (bin.CVLevel.Level != ConversionLevel.Red && bin.CVLevel.Level != ConversionLevel.NA)
                bin.Marked = true;
        }

        private static void OnMyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinConnectorVisual bin = (BinConnectorVisual)d;
            bin.FunctionColor = new SolidColorBrush(ColorHelper.GetLineColor(bin.Model.ConnectorType));
            bin.IsMandatory = bin.Model.IsMandatory;
            bin.IsOutgoing = bin.Model.Outgoing;
            bin.Description = bin.Model.ToolTip;
            bin.Caption = bin.Model.Caption;
            //bin.Model.Orientation = bin.Orientation;
        }

        public Point GetPosition()
        {
            Panel ic = VisualParent as Panel;
            if (ic == null)
                return new Point(0, 0);

            var gTransform = ic.TransformToVisual(WindowParent);
            var gTransformSec = this.TransformToVisual(ic);

            var point = gTransform.Transform(new Point(0, 0));
            var relativePoint = gTransformSec.Transform(new Point(0, 0));
            return new Point(WindowParent.Position.X + point.X + relativePoint.X, WindowParent.Position.Y + point.Y + relativePoint.Y);
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

        private void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement))
                return;
            OnPropertyChanged("Data");
            ToolTip.IsOpen = true;
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement))
                return;

            ToolTip.IsOpen = false;
        }
    }

    public class ConversionLevelInformation
    {
        public ConversionLevel Level { get; set; }
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }

        public string SourceTypeString
        { 
            get
            {
                if(SourceType == null)
                    return string.Empty;

                return SourceType.Name;
            }
        }

        public string TargetTypeName
        {
            get
            {
                if (TargetType == null)
                    return string.Empty;

                return TargetType.Name;
            }
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
