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

namespace WorkspaceManager.View.Container
{
    /// <summary>
    /// Interaction logic for ConntectorView.xaml
    /// </summary>
    public partial class ConntectorView : UserControl, IConnectable
    {
        public static readonly DependencyProperty PositionOnWorkSpaceXProperty = DependencyProperty.Register("PositionOnWorkSpaceX", typeof(double), typeof(ConntectorView), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty PositionOnWorkSpaceYProperty = DependencyProperty.Register("PositionOnWorkSpaceY", typeof(double), typeof(ConntectorView), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public event EventHandler<ConntectorViewEventArgs> OnConnectorMouseLeftButtonDown;
        private Model.ConnectorModel cModel;

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

        public ConntectorView()
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ConntectorView_MouseLeftButtonDown);
            InitializeComponent();
        }

        public ConntectorView(Model.ConnectorModel cModel)
        {
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ConntectorView_MouseLeftButtonDown);
            this.cModel = cModel;
            InitializeComponent();
        }

        void ConntectorView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.OnConnectorMouseLeftButtonDown != null)
            {
                this.OnConnectorMouseLeftButtonDown.Invoke(this, new ConntectorViewEventArgs { connector = this });
            }
        }

        public bool CanConnect
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class ConntectorViewEventArgs : EventArgs
    {
        public ConntectorView connector;
    }
}
