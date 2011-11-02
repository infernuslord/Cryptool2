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

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinSideBarVisual.xaml
    /// </summary>
    public partial class BinSideBarVisual : UserControl
    {
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen",
            typeof(bool), typeof(BinSideBarVisual), new FrameworkPropertyMetadata(false));

        public bool IsOpen
        {
            get
            {
                return (bool)base.GetValue(IsOpenProperty);
            }
            set
            {
                base.SetValue(IsOpenProperty, value);
            }
        }

        public BinSideBarVisual()
        {
            InitializeComponent();
        }

        private void ActionHandler(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b.Content is bool)
            {
                IsOpen = (bool)b.Content;
                return;
            }
        }
    }
}
