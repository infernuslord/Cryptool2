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
using System.ComponentModel;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Container
{
    /// <summary>
    /// Interaction logic for ConnectorView.xaml
    /// </summary>
    public partial class ConnectorView : UserControl, IConnectable
    {
        public static readonly DependencyProperty PositionOnWorkSpaceXProperty = DependencyProperty.Register("PositionOnWorkSpaceX", typeof(double), typeof(ConnectorView), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty PositionOnWorkSpaceYProperty = DependencyProperty.Register("PositionOnWorkSpaceY", typeof(double), typeof(ConnectorView), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public event EventHandler<ConnectorViewEventArgs> OnConnectorMouseLeftButtonDown;
        public ConnectorModel model;
        public ConnectorModel Model
        {
            get { return model; }
            private set { model = value; }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double PositionOnWorkSpaceX
        {
            get { return (double)base.GetValue(PositionOnWorkSpaceXProperty); }
            set
            {
                base.SetValue(PositionOnWorkSpaceXProperty, value);
            }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double PositionOnWorkSpaceY
        {
            get { return (double)base.GetValue(PositionOnWorkSpaceYProperty); }
            set
            {
                base.SetValue(PositionOnWorkSpaceYProperty, value);
            }
        }

        public ConnectorView()
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ConnectorView_MouseLeftButtonDown);
            InitializeComponent();
        }

        public ConnectorView(Model.ConnectorModel cModel)
        {                        
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ConnectorView_MouseLeftButtonDown);
            this.Model = cModel;
            InitializeComponent();
            Color color = ColorHelper.getColor(cModel.ConnectorType);
            this.Ellipse.Fill = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            this.Ellipse.ToolTip = cModel.ToolTip;
        }

        void ConnectorView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.OnConnectorMouseLeftButtonDown != null)
            {
                this.OnConnectorMouseLeftButtonDown.Invoke(this, new ConnectorViewEventArgs { connector = this });
            }
        }

        public bool CanConnect
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class ConnectorViewEventArgs : EventArgs
    {
        public ConnectorView connector;
    }
}
