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
    public enum ConnectorOrientation
    {
        North,
        South,
        West,
        East,
        Unset
    };

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

        private ConnectorOrientation orientation;
        public ConnectorOrientation Orientation 
        {
            get 
            {
                return orientation;
            }
            set
            {
                orientation = value;
                switch (value)
                {
                    case ConnectorOrientation.West:
                        if (model.Outgoing)
                            Rotation.Angle = 90;
                        else
                            Rotation.Angle = -90;
                        break;
                    case ConnectorOrientation.East:
                        if (model.Outgoing)
                            Rotation.Angle = -90;
                        else
                            Rotation.Angle = 90;
                        break;
                    case ConnectorOrientation.North:
                        if (model.Outgoing)
                            Rotation.Angle = 180;
                        else
                            Rotation.Angle = 0;
                        break;
                    case ConnectorOrientation.South:
                        if (model.Outgoing)
                            Rotation.Angle = 0;
                        else
                            Rotation.Angle = 180;
                        break;
                }

                this.Model.Orientation = value;
            }
        }

        public ConnectorView()
        {
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ConnectorView_MouseLeftButtonDown);
            InitializeComponent();
        }

        public ConnectorView(ConnectorModel Model)
        {
            setBaseControl(Model);
            InitializeComponent();

            if (Model.IsMandatory)
            {
                Scale.ScaleX = 1.35;
                Scale.ScaleY = 1.35;
            }

            if (Model.Orientation == ConnectorOrientation.Unset)
            {
                if (model.Outgoing)
                    this.Orientation = ConnectorOrientation.East;
                else
                    this.Orientation = ConnectorOrientation.West;
            }
            else
                this.Orientation = Model.Orientation;

            Color color = ColorHelper.GetColor(Model.ConnectorType);
            this.ConnectorRep.Fill = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            this.ConnectorRep.ToolTip = Model.ToolTip;
        }

        private void setBaseControl(ConnectorModel Model)
        {
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ConnectorView_MouseLeftButtonDown);
            this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(ConnectorView_MouseRightButtonDown);
            this.PreviewMouseRightButtonUp += new MouseButtonEventHandler(ConnectorView_MouseRightButtonUp);
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
