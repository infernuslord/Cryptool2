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
using WorkspaceManagerModel.Model;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinWindowVisual.xaml
    /// </summary>
    public partial class BinWindowVisual : UserControl
    {
        #region Properties
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
        #endregion


        #region DependencyProperties
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position",
            typeof(Point), typeof(BinComponentVisual), new FrameworkPropertyMetadata(new Point(0, 0)));

        public static readonly DependencyProperty IsLockedProperty = DependencyProperty.Register("IsLocked",
            typeof(bool), typeof(BinWindowVisual), new FrameworkPropertyMetadata(false, null));

        public bool IsLocked
        {
            get
            {
                return (bool)base.GetValue(IsLockedProperty);
            }
            set
            {
                base.SetValue(IsLockedProperty, value);
            }
        } 

        public static readonly DependencyProperty WindowNameProperty = DependencyProperty.Register("WindowName",
            typeof(string), typeof(BinWindowVisual), new FrameworkPropertyMetadata(string.Empty, null));

        public string WindowName
        {
            get
            {
                return (string)base.GetValue(WindowNameProperty);
            }
            set
            {
                base.SetValue(WindowNameProperty, value);
            }
        } 


        #endregion

        public BinWindowVisual(ImageModel model)
        {
            ImgModel = model;
            InitializeComponent();
        }

        public BinWindowVisual(TextModel model)
        {
            TxtModel = model;
            InitializeComponent();
        }

        public ImageModel ImgModel { get; set; }

        public TextModel TxtModel { get; set; }

        private void CloseClick(object sender, RoutedEventArgs e)
        {

        }

        private void ActionHandler(object sender, RoutedEventArgs e)
        {

        }
    }
}
