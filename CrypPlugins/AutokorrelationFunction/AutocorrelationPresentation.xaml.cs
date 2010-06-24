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
using Cryptool.PluginBase.Utils.Graphics.Diagrams.Histogram;


namespace Cryptool.Plugins.AutokorrelationFunction
{
    /// <summary>
    /// Interaction logic for AutocorrelationPresentation.xaml
    /// </summary>
    public partial class AutocorrelationPresentation : UserControl
    {
        public AutocorrelationPresentation()
        {
            InitializeComponent();       
        }
    }

    //INFO TO ALL DEVELOPERS WORKING ON PRESENTATIONS:
    //To resize the quickview the same way as this plugin does just use a <Viewbow> around all your other Presentation Elements!
}
