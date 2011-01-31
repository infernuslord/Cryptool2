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
using WorkspaceManager.Model;
using System.Windows.Controls.Primitives;
using WorkspaceManager.View.Interface;
using WorkspaceManager.View.Container;

namespace WorkspaceManager.View.VisualComponents
{

    /// <summary>
    /// Interaction logic for ImageWrapper.xaml
    /// </summary>
    public partial class ImageWrapper : UserControl
    {
        public event EventHandler<ImageDeleteEventArgs> Delete;

        public Image Image { get; set; }
        public Point Position { get; set; }
        public ImageModel Model { get; set; }

        private UserContentWrapper contentParent;

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(ImageWrapper));

        //private static void OnTestBoolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var vm = (ImageWrapper)d;
        //    vm.CoerceValue(TestDoubleProperty);
        //}


        public bool IsSelected
        {
            get
            {
                return (bool)base.GetValue(IsSelectedProperty);
            }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        }

        public ImageWrapper(ImageModel model, Point point, UserContentWrapper parent)
        {
            InitializeComponent();
            this.ParentPanel.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(ImageWrapper_PreviewMouseLeftButtonDown);
            this.Loaded += new RoutedEventHandler(ImageWrapper_Loaded);
            this.DataContext = this;
            this.Model = model;
            this.Image = model.getImage();
            this.contentParent = parent;
            this.Position = point;
            this.Model.Position = point;
            this.RenderTransform = new TranslateTransform(Position.X, Position.Y);
            this.root.Background = new ImageBrush() {ImageSource = this.Image.Source, Stretch = Stretch.Fill};
        }

        void ImageWrapper_Loaded(object sender, RoutedEventArgs e)
        {
            if (Model.Width == 0 || Model.Height == 0)
            {
                this.Width = Image.Source.Width;
                this.Height = Image.Source.Height;
            }
            else
            {
                this.Width = Model.Width;
                this.Height = Model.Height;
            }

            this.ParentPanel.IsEnabled = Model.IsEnabled;
        }

        void ImageWrapper_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.contentParent.SelectedItem = this;
            this.IsSelected = true;
        }

        public ImageWrapper()
        {
            InitializeComponent();
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb t = sender as Thumb;

            if (t.Name == "BottomRightDelta")
            {
                if ((this.ActualHeight + e.VerticalChange) > 0)
                    this.Height = this.ActualHeight + e.VerticalChange;

                if ((this.ActualWidth + e.HorizontalChange) > 0)
                    this.Width = this.ActualWidth + e.HorizontalChange;

                Model.Height = this.ActualHeight;
                Model.Width = this.ActualWidth;
            }
            e.Handled = true;
        }

        private void OverLayingControl_DragDelta_Move(object sender, DragDeltaEventArgs e)
        {
            (this.RenderTransform as TranslateTransform).X += e.HorizontalChange;
            (this.RenderTransform as TranslateTransform).Y += e.VerticalChange;
            Model.Position.X = (this.RenderTransform as TranslateTransform).X;
            Model.Position.Y = (this.RenderTransform as TranslateTransform).Y;
        }

        private void delete()
        {
            if (this.Delete != null)
                this.Delete.Invoke(this, new ImageDeleteEventArgs { img = this });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.delete();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (this.ParentPanel.IsEnabled == true)
            {
                this.ParentPanel.IsEnabled = false;
                this.Model.IsEnabled = false;
                this.contentParent.SelectedItem = null;
                return;
            }

            if (this.ParentPanel.IsEnabled == false)
            {
                this.ParentPanel.IsEnabled = true;
                this.Model.IsEnabled = true;
                this.contentParent.SelectedItem = this;
                return;
            }
        }

        private void IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel.IsEnabled == true)
            {
                FixUnfixMenuItem.Header = "Fix";
                return;
            }

            if (panel.IsEnabled == false)
            {
                FixUnfixMenuItem.Header = "Unfix";
                return;
            }
        }

    }

    public class ImageDeleteEventArgs : EventArgs
    {
        public ImageWrapper img;
    }
}
