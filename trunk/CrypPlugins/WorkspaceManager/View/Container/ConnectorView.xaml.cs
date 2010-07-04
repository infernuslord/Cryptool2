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
using System.Windows.Controls.Primitives;

namespace WorkspaceManager.View.Container
{
    /// <summary>
    /// Interaction logic for ConnectorView.xaml
    /// </summary>
    public partial class ConnectorView : UserControl, IConnectable, IUpdateableView
    {
        public static readonly DependencyProperty X = DependencyProperty.Register("PositionOnWorkSpaceX", typeof(double), typeof(ConnectorView), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty Y = DependencyProperty.Register("PositionOnWorkSpaceY", typeof(double), typeof(ConnectorView), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

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
            get { return (double)base.GetValue(X); }
            set
            {
                ResetPopUp();
                base.SetValue(X, value);
            }
        }

        [TypeConverter(typeof(LengthConverter))]
        public double PositionOnWorkSpaceY
        {
            get { return (double)base.GetValue(Y); }
            set
            {
                ResetPopUp();
                base.SetValue(Y, value);
            }
        }

        public ConnectorView()
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ConnectorView_MouseLeftButtonDown);
            InitializeComponent();
        }

        public ConnectorView(ConnectorModel Model)
        {
            setBaseControl(Model);
            InitializeComponent();
 
            Color color = ColorHelper.GetColor(Model.ConnectorType);
            this.Ellipse.Fill = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            this.Ellipse.ToolTip = Model.ToolTip;
        }

        private void setBaseControl(ConnectorModel Model)
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ConnectorView_MouseLeftButtonDown);
            this.MouseRightButtonDown += new MouseButtonEventHandler(ConnectorView_MouseRightButtonDown);
            this.MouseRightButtonUp += new MouseButtonEventHandler(ConnectorView_MouseRightButtonUp);
            this.MouseLeave += new MouseEventHandler(ConnectorView_MouseLeave);
            this.Model = Model;
            this.DataContext = Model;
            this.Model.UpdateableView = this;
        }

        void ConnectorView_MouseLeave(object sender, MouseEventArgs e)
        {
            BubblePopup.StaysOpen = false;
        }

        void ConnectorView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            BubblePopup.StaysOpen = false;
        }

        void ConnectorView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.BubblePopup.IsOpen = true;
            BubblePopup.StaysOpen = true;
        }

        void ConnectorView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.OnConnectorMouseLeftButtonDown != null)
            {
                this.OnConnectorMouseLeftButtonDown.Invoke(this, new ConnectorViewEventArgs { connector = this });
            }
        }

        public void ResetPopUp()
        {
            Random random = new Random();
            BubblePopup.PlacementRectangle = new Rect(new Point(random.NextDouble() / 1000, 0), new Size(75, 25));
        }

        public bool CanConnect
        {
            get { throw new NotImplementedException(); }
        }

        public void update()
        {
            if (model.HasData)
            {
                ToolTip = model.Data;
            }

        }

    }

    public class ConnectorViewEventArgs : EventArgs
    {
        public ConnectorView connector;
    }
}
