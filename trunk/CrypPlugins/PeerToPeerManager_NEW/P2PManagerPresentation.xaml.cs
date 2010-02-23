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

        #region Different radient brushes

        public LinearGradientBrush GetGradientBlue()
        {
            LinearGradientBrush myBrush = new LinearGradientBrush();
            myBrush.GradientStops.Add(new GradientStop(Colors.DodgerBlue, 0.0)); //CadetBlue
            myBrush.GradientStops.Add(new GradientStop(Colors.CornflowerBlue, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 1.0));
            return myBrush;
        }

        public LinearGradientBrush GetGradientGray()
        {
            LinearGradientBrush myBrush = new LinearGradientBrush();
            myBrush.GradientStops.Add(new GradientStop(Colors.DarkGray, 0.0));
            myBrush.GradientStops.Add(new GradientStop(Colors.Gray, 0.5));
            myBrush.GradientStops.Add(new GradientStop(Colors.WhiteSmoke, 1.0));
            return myBrush;
        }

        #endregion

        public P2PManagerPresentation()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(P2PManagerPresentation_SizeChanged);
            this.DataContext = entries;
            // when expanding Expanders, resize layout
            this.Expander_JobStatus.MouseLeftButtonUp += new MouseButtonEventHandler(Expander_JobStatus_MouseLeftButtonUp);
            this.Expander_List.MouseLeftButtonUp += new MouseButtonEventHandler(Expander_List_MouseLeftButtonUp);
            this.Expander_WorkerInfo.MouseLeftButtonUp += new MouseButtonEventHandler(Expander_WorkerInfo_MouseLeftButtonUp);


            this.SourceUpdated += new EventHandler<DataTransferEventArgs>(P2PManagerPresentation_SourceUpdated);
            // when you uncomment this line, you burn 70% of the whole CPU time for Resizing this view...
            //this.LayoutUpdated += new EventHandler(P2PManagerPresentation_LayoutUpdated);
            this.Expander_JobStatus.Expanded += new RoutedEventHandler(Expander_JobStatus_Expanded);
            this.Expander_List.Expanded += new RoutedEventHandler(Expander_List_Expanded);
            this.Expander_WorkerInfo.Expanded += new RoutedEventHandler(Expander_WorkerInfo_Expanded);
            //this.ListView.DataContextChanged += new DependencyPropertyChangedEventHandler(ListView_DataContextChanged);
            //this.ListView.SizeChanged += new SizeChangedEventHandler(ListView_SizeChanged);
            this.ListView.SourceUpdated += new EventHandler<DataTransferEventArgs>(ListView_SourceUpdated);

            LinearGradientBrush blueBrush = GetGradientBlue();

            this.MngrMain.Background = blueBrush;
            this.Expander_JobStatus.Background = blueBrush;
            this.Expander_WorkerInfo.Background = blueBrush;
            this.Expander_List.Background = GetGradientGray();
        }

        void Expander_WorkerInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FunnyResize();
        }

        void Expander_List_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FunnyResize();
        }

        void Expander_JobStatus_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FunnyResize();
        }

        void txtTotal_TextChanged(object sender, TextChangedEventArgs e)
        {
            FunnyResize();
        }

        void P2PManagerPresentation_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            FunnyResize();
        }

        private void FunnyResize()
        {
            this.Canvas.RenderTransform = new ScaleTransform(this.ActualWidth / this.Grid.ActualWidth,
                                                       this.ActualHeight / this.Grid.ActualHeight);
        }

        void ListView_SourceUpdated(object sender, DataTransferEventArgs e)
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

        public void P2PManagerPresentation_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FunnyResize();
            //this.Grid.RenderTransform = new ScaleTransform(this.ActualWidth / this.Grid.ActualWidth,
                                                       //this.ActualHeight / this.Grid.ActualHeight);
        }

        //void Grid_LayoutUpdated(object sender, EventArgs e)
        //{
        //    FunnyResize();
        //}

        //void Canvas_LayoutUpdated(object sender, EventArgs e)
        //{
        //    FunnyResize();
        //}

        //void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    FunnyResize();
        //}

        //void ListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    FunnyResize();
        //}

        //void P2PManagerPresentation_LayoutUpdated(object sender, EventArgs e)
        //{
        //    FunnyResize();
        //}
    }
}
