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

        public ProgressChunks PrgChunks = new ProgressChunks();

        #endregion

        public P2PManagerPresentation()
        {
            InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(P2PManagerPresentation_SizeChanged);
            this.DataContext = entries;

            this.txtTotal.SizeChanged += new SizeChangedEventHandler(txtTotal_SizeChanged);
            //this.txtTotal.TextChanged += new TextChangedEventHandler(txtTotal_TextChanged);


            this.Expander_WorkerInfo.SizeChanged += new SizeChangedEventHandler(Expander_WorkerInfo_SizeChanged);
            this.Expander_List.SizeChanged += new SizeChangedEventHandler(Expander_List_SizeChanged);
            this.Expander_JobStatus.SizeChanged += new SizeChangedEventHandler(Expander_JobStatus_SizeChanged);

            this.SourceUpdated += new EventHandler<DataTransferEventArgs>(P2PManagerPresentation_SourceUpdated);
            // when you uncomment this line, you burn 70% of the whole CPU time for Resizing this view...
            //this.LayoutUpdated += new EventHandler(P2PManagerPresentation_LayoutUpdated);
            this.ListView.SourceUpdated += new EventHandler<DataTransferEventArgs>(ListView_SourceUpdated);

            LinearGradientBrush blueBrush = GetGradientBlue();
            LinearGradientBrush grayBrush = GetGradientGray();

            this.MngrMain.Background = blueBrush;
            this.Expander_JobStatus.Background = blueBrush;
            
            Expander exp_chunk = new Expander();
            exp_chunk.Content = PrgChunks;
            exp_chunk.Header = "Visual Job Distribution";
            exp_chunk.FontSize = 10;
            exp_chunk.Background = grayBrush;
            exp_chunk.SizeChanged += new SizeChangedEventHandler(exp_chunk_SizeChanged);
            this.PrgChunks.Width = 200;
            this.PrgChunks.Height = 55;
            Grid.SetRow(exp_chunk, 1);
            this.Grid1.Children.Add(exp_chunk);

            this.Expander_List.Background = blueBrush;
            this.Expander_WorkerInfo.Background = grayBrush;
        }

        void exp_chunk_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FunnyResize();
        }

        void txtTotal_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FunnyResize();
        }

        void Expander_JobStatus_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FunnyResize();
        }

        void Expander_List_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FunnyResize();
        }

        void Expander_WorkerInfo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FunnyResize();
        }

        void P2PManagerPresentation_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            FunnyResize();
        }

        private void FunnyResize()
        {
            this.Canvas.RenderTransform = new ScaleTransform(this.ActualWidth / this.Grid1.ActualWidth,
                                                       this.ActualHeight / this.Grid1.ActualHeight);
        }

        void ListView_SourceUpdated(object sender, DataTransferEventArgs e)
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
