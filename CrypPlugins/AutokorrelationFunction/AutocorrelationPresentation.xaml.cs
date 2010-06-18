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
            SizeChanged += new SizeChangedEventHandler(AutocorrelationPresentation_SizeChanged);
        }

        private void AutocorrelationPresentation_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateScaling();
        }

        private void updateScaling()
        {
            this.scaler.ScaleX = this.ActualWidth / this.Grid.Width;
            this.scaler.ScaleY = this.scaler.ScaleX;
        }
    }
}
