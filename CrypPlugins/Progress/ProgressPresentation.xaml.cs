using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cryptool.Progress
{
    /// <summary>
    /// Interaction logic for ProgressPresentation.xaml
    /// </summary>
    public partial class ProgressPresentation : UserControl
    {
        public ProgressPresentation()
        {
            InitializeComponent();
        }

        public void Set(int value, int max)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.SystemIdle, (SendOrPostCallback) delegate
            {
                if (max <= 0)
                {
                    Bar.Maximum = 100;
                }
                else
                {
                    Bar.Maximum = max;
                }
                Bar.Value = value;
            }, null);
        }
    }
}
