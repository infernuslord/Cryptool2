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
using System.Windows.Shapes;
using System.Windows.Threading;
using Cryptool.PluginBase.Attributes;

namespace Cryptool.CrypWin
{
    /// <summary>
    /// Interaction logic for GeneratingDocWindow.xaml
    /// </summary>
    [Localization("Cryptool.CrypWin.Properties.Resources")]
    public partial class GeneratingDocWindow : Window
    {
        public GeneratingDocWindow()
        {
            InitializeComponent();
        }
    }
}
