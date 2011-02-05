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
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class BottomBox : UserControl
    {
        public event EventHandler<ImageSelectedEventArgs> ImageSelected;
        public event EventHandler<AddTextEventArgs> AddText;
        public event EventHandler<FitToScreenEventArgs> FitToScreen;

        public BottomBox()
        {
            this.Loaded += new RoutedEventHandler(BottomBox_Loaded);
            InitializeComponent();
        }

        void BottomBox_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_Full_inc(object sender, RoutedEventArgs e)
        {
            FullScreenScaleSlider.Value += 0.15;
        }

        private void Button_Click_Full_dec(object sender, RoutedEventArgs e)
        {
            FullScreenScaleSlider.Value -= 0.15;
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
            Button btn = sender as Button;
            if (btn.Name == "ADDIMG")
            {
                System.Windows.Forms.OpenFileDialog diag = new System.Windows.Forms.OpenFileDialog();
                if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Uri uriLocal = new Uri(diag.FileName);

                    if (ImageSelected != null)
                        ImageSelected.Invoke(this, new ImageSelectedEventArgs() { uri = uriLocal });
                }
                return;
            }

            if (btn.Name == "ADDTXT")
            {
                if (AddText != null)
                    AddText.Invoke(this, new AddTextEventArgs());
            }


            if (btn.Name == "F2S")
            {
                if (FitToScreen != null)
                    FitToScreen.Invoke(this, new FitToScreenEventArgs());
            }
        }
    }

    public class ImageSelectedEventArgs : EventArgs
    {
        public Uri uri;
    }

    public class AddTextEventArgs : EventArgs
    {
    }

    public class FitToScreenEventArgs : EventArgs
    {
    }

}
