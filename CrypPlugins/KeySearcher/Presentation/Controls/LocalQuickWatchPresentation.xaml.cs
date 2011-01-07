using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using KeySearcher;

namespace KeySearcherPresentation.Controls
{
    [Cryptool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class LocalQuickWatchPresentation
    {        
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();

        public static readonly DependencyProperty IsOpenCLEnabledProperty =
            DependencyProperty.Register("IsOpenCLEnabled",
                typeof(Boolean),
                typeof(LocalQuickWatchPresentation), new PropertyMetadata(false));

        public Boolean IsOpenCLEnabled
        {
            get { return (Boolean)GetValue(IsOpenCLEnabledProperty); }
            set { SetValue(IsOpenCLEnabledProperty, value); }
        }

        public LocalQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }
    }
}
