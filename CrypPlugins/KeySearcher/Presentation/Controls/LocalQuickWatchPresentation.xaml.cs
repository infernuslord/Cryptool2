using System;
using System.Windows.Media;
using System.Collections.ObjectModel;
using KeySearcher;

namespace KeySearcherPresentation.Controls
{    
    public partial class LocalQuickWatchPresentation
    {        
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();

        public LocalQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }
    }
}
