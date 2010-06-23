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

namespace Cryptool.Plugins.QuadraticSieve
{
    /// <summary>
    /// Interaction logic for QuadraticSievePresentation.xaml
    /// </summary>
    public partial class QuadraticSievePresentation : UserControl
    {
        private ProgressRelationPackages progressRelationPackages;
        public ProgressRelationPackages ProgressRelationPackages
        {
            get { return progressRelationPackages; }
        }

        public QuadraticSievePresentation()
        {
            InitializeComponent();            
            SizeChanged += sizeChanged;

            ScrollViewer sviewer = new ScrollViewer();
            sviewer.CanContentScroll = true;
            sviewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            sviewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            progressRelationPackages = new ProgressRelationPackages(sviewer);
            sviewer.Content = progressRelationPackages;
            Grid grid = ((Grid)peer2peer.Content);
            Grid.SetRow(sviewer, 0);
            grid.Children.Add(sviewer);
            sviewer.MinHeight = 100;
            sviewer.MaxHeight = 100;
            progressRelationPackages.MaxWidth = factorList.Width - 10;
        }

        public void sizeChanged(Object sender, EventArgs eventArgs)
        {
            double scale = Math.Min((this.ActualWidth / this.Grid.ActualWidth), (this.ActualHeight / this.Grid.ActualHeight));
            this.Grid.RenderTransform = new ScaleTransform(scale, scale);
        }

    }
}
