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

namespace Cryptool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaktionslogik für DataBlockDisplay.xaml
    /// </summary>
    public partial class DataBlockDisplay : UserControl
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(IList<Byte>), typeof(DataBlockDisplay), null);
        public IList<Byte> Data { get { return (IList<Byte>)GetValue(DataProperty); } set { SetValue(DataProperty, value); } }
        
        public DataBlockDisplay()
        {
            InitializeComponent();
        }
    }
}
