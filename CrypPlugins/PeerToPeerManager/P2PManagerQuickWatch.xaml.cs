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
using KeySearcher;

namespace Cryptool.Plugins.PeerToPeer
{
    /// <summary>
    /// Interaction logic for P2PManagerQuickWatch.xaml
    /// </summary>
    public partial class P2PManagerQuickWatch : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();

        public P2PManagerQuickWatch()
        {
            InitializeComponent();
            SizeChanged += new SizeChangedEventHandler(P2PManagerQuickWatch_SizeChanged);
            this.DataContext = entries;
        }

        void P2PManagerQuickWatch_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Grid.RenderTransform = new ScaleTransform(this.ActualWidth / this.Grid.ActualWidth,
                                                       this.ActualHeight / this.Grid.ActualHeight);  
            //double height = this.ActualHeight - this.Grid.ActualHeight;
            //if (height < 0)
            //{
            //    height = 0;
            //}
            //this.ListView.Height = height;
            //this.ListView.Width = this.ActualWidth;

            //double heightTransform = (this.ActualHeight - height) / this.Grid.ActualHeight;
            //double widthTransform = this.ActualWidth / this.Grid.ActualWidth;

            //if (widthTransform > heightTransform)
            //{
            //    widthTransform = heightTransform;
            //}


            //this.Grid.RenderTransform = new ScaleTransform(widthTransform, heightTransform);
        }
    }
}
