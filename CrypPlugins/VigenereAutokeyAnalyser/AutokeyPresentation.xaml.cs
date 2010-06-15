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

namespace VigenereAutokeyAnalyser
{
    /// <summary>
    /// Interaction logic for AutokeyPresentation.xaml
    /// </summary>
    public partial class AutokeyPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();

        public AutokeyPresentation()
        {
            InitializeComponent();
            SizeChanged += sizeChanged;
            this.DataContext = entries;
        }

        public void sizeChanged(Object sender, EventArgs eventArgs)
        {

        }
    }
}
