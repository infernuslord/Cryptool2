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

namespace QuadraticSieve
{


    /// <summary>
    /// Interaction logic for QuadraticSievePresentation.xaml
    /// </summary>
    public partial class QuadraticSievePresentation : UserControl
    {

        public QuadraticSievePresentation()
        {
            InitializeComponent();            
            SizeChanged += sizeChanged;
        }

        public void sizeChanged(Object sender, EventArgs eventArgs)
        {
            this.Grid.RenderTransform = new ScaleTransform( this.ActualWidth / this.Grid.ActualWidth,
                                                       this.ActualHeight / this.Grid.ActualHeight);                        
        }
    }
}
