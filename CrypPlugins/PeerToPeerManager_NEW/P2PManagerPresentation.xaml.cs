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
            SizeChanged += new SizeChangedEventHandler(P2PManagerPresentation_SizeChanged);
            this.DataContext = entries;
            this.LayoutUpdated += new EventHandler(P2PManagerPresentation_LayoutUpdated);
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
