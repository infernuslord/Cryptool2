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
using System.Windows.Media.Animation;

namespace WorkspaceManager.View.VisualComponents
{
    public class ImageSelectedEventArgs : EventArgs
    {
        public Uri uri;
    }

    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class BottomBox : UserControl
    {
        public event EventHandler<ImageSelectedEventArgs> ImageSelected;

        public BottomBox()
        {
            this.Loaded += new RoutedEventHandler(BottomBox_Loaded);
            InitializeComponent();
        }

        void BottomBox_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Main_MouseEnter(object sender, MouseEventArgs e)
        {
            //Sub.Visibility = Visibility.Collapsed;
            Main.BeginStoryboard((this.Resources["IncrementHeigth"] as Storyboard));
            //(this.Resources["Up"] as Storyboard).Stop(Sub);
        }

        private void Main_MouseLeave(object sender, MouseEventArgs e)
        {
            //Sub.Visibility = Visibility.Visible;
            Main.BeginStoryboard((this.Resources["DecrementHeigth"] as Storyboard));
            //Sub.BeginStoryboard((this.Resources["Up"] as Storyboard));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog diag = new System.Windows.Forms.OpenFileDialog();
            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Uri uriLocal = new Uri(diag.FileName);

                if (ImageSelected != null)
                    ImageSelected.Invoke(this, new ImageSelectedEventArgs() { uri = uriLocal });
            }
        }
    }
}
