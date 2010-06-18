using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace KeySearcher.Presentation
{    
    public partial class KeySearcherQuickWatchPresentation : UserControl
    {        
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();

        public KeySearcherQuickWatchPresentation()
        {
            InitializeComponent();
            SizeChanged += sizeChanged;
            this.DataContext = entries;
        }

        public void sizeChanged(Object sender, EventArgs eventArgs)
        {
            double height = this.ActualHeight - this.Grid.ActualHeight;
            if(height<0){
                height=0;
            }
            this.ListView.Height = height;
            this.ListView.Width = this.ActualWidth;

            double heightTransform = (this.ActualHeight - height) / this.Grid.ActualHeight;
            double widthTransform = this.ActualWidth / this.Grid.ActualWidth;

            if (widthTransform > heightTransform)
            {
                widthTransform = heightTransform;
            }
                

            this.Grid.RenderTransform = new ScaleTransform(widthTransform, heightTransform);
        }
    }
}
