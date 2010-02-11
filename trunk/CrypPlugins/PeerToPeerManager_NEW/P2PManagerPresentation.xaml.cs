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
    public partial class P2PManagerPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();

        public P2PManagerPresentation()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(P2PManagerPresentation_SizeChanged);
            this.DataContext = entries;
            // when you uncomment this line, you burn 70% of the whole CPU time for Resizing this view...
            //this.LayoutUpdated += new EventHandler(P2PManagerPresentation_LayoutUpdated);
            this.Expander_JobStatus.Expanded += new RoutedEventHandler(Expander_JobStatus_Expanded);
            this.Expander_List.Expanded += new RoutedEventHandler(Expander_List_Expanded);
            this.Expander_WorkerInfo.Expanded += new RoutedEventHandler(Expander_WorkerInfo_Expanded);
            //this.ListView.DataContextChanged += new DependencyPropertyChangedEventHandler(ListView_DataContextChanged);
            //this.ListView.SizeChanged += new SizeChangedEventHandler(ListView_SizeChanged);
            this.ListView.SourceUpdated += new EventHandler<DataTransferEventArgs>(ListView_SourceUpdated);
        }

        void Grid_LayoutUpdated(object sender, EventArgs e)
        {
            FunnyResize();
        }

        void Canvas_LayoutUpdated(object sender, EventArgs e)
        {
            FunnyResize();
        }

        void ListView_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            FunnyResize();
        }

        void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FunnyResize();
        }

        void ListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FunnyResize();
        }

        void Expander_WorkerInfo_Expanded(object sender, RoutedEventArgs e)
        {
            FunnyResize();
        }

        void Expander_List_Expanded(object sender, RoutedEventArgs e)
        {
            FunnyResize();
        }

        void Expander_JobStatus_Expanded(object sender, RoutedEventArgs e)
        {
            FunnyResize();
        }

        void P2PManagerPresentation_LayoutUpdated(object sender, EventArgs e)
        {
            FunnyResize();
        }

        public void P2PManagerPresentation_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FunnyResize();
            //this.Grid.RenderTransform = new ScaleTransform(this.ActualWidth / this.Grid.ActualWidth,
                                                       //this.ActualHeight / this.Grid.ActualHeight);
        }

        private void FunnyResize()
        {
            this.Canvas.RenderTransform = new ScaleTransform(this.ActualWidth / this.Grid.ActualWidth,
                                                       this.ActualHeight / this.Grid.ActualHeight);
        }
    }
}
