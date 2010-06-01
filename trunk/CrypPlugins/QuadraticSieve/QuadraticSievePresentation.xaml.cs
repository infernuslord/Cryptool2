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
        private ProgressYields progressYields = new ProgressYields();
        public ProgressYields ProgressYields
        {
            get { return progressYields; }
        }

        public QuadraticSievePresentation()
        {
            InitializeComponent();            
            SizeChanged += sizeChanged;

            Grid grid = ((Grid)peer2peerExpander.Content);
            Grid.SetRow(progressYields, 0);
            grid.Children.Add(progressYields);
            progressYields.MaxWidth = 200;
        }

        public void sizeChanged(Object sender, EventArgs eventArgs)
        {
            this.Grid.RenderTransform = new ScaleTransform( this.ActualWidth / this.Grid.ActualWidth,
                                                       this.ActualHeight / this.Grid.ActualHeight);
        }

        private void peer2peerExpander_Expanded(object sender, RoutedEventArgs e)
        {
            //TODO: Refresh here
        }
    }
}
