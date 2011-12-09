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

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for ZoomScrollViewer.xaml
    /// </summary>
    public partial class ZoomScrollViewer : ScrollViewer
    {

        private double min = Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MinScale,
                       max = Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_MaxScale;

        public ZoomScrollViewer()
        {
            InitializeComponent();
        }

        private void IncZoom(object sender, RoutedEventArgs e)
        {
            double scale = Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale;

            Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale = scale < max ? scale + 0.15 : scale;
        }

        private void DecZoom(object sender, RoutedEventArgs e)
        {
            double scale = Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale;

            Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale = scale > min ? scale - 0.15 : scale;
        }

        private void TextBlockMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double scale = 1.0;

            Cryptool.PluginBase.Properties.Settings.Default.WorkspaceManager_EditScale = scale;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

    }
}
