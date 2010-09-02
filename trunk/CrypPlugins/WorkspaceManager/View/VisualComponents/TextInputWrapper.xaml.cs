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

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for TextInputWrapper.xaml
    /// </summary>
    public partial class TextInputWrapper : UserControl
    {
        public event EventHandler<TextInputDeleteEventArgs> Delete;

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TextInputWrapper));
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

        public TextModel Model { get; set; }
        public Point Position { get; set; }

        private UserContentWrapper ContentParent;

        public TextInputWrapper()
        {
            InitializeComponent();
        }

        public TextInputWrapper(Model.TextModel model, Point point, UserContentWrapper userContentWrapper)
        {
            InitializeComponent();
            this.ParentPanel.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(TextInputWrapper_PreviewMouseLeftButtonDown);
            //this.MouseRightButtonDown += new MouseButtonEventHandler(TextInputWrapper_MouseRightButtonDown);
            this.Loaded += new RoutedEventHandler(TextInputWrapper_Loaded);
            this.DataContext = this;
            this.Model = model;
            this.Model.loadRTB(this.mainRTB);
            this.mainRTB.TextChanged += MainRTBTextChanged;
            this.Position = point;
            this.ContentParent = userContentWrapper;
            this.RenderTransform = new TranslateTransform(point.X, point.Y);            
        }

        /// <summary>
        /// Serializes the content of the RTB to the TextModel and
        /// sets the editor to HasChanges=true
        /// 
        /// called if the RTB is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MainRTBTextChanged(object sender, TextChangedEventArgs args)
        {
            this.Model.saveRTB(this.mainRTB);            
        }

        void TextInputWrapper_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = Model.Width;
            this.Height = Model.Height;
            this.ParentPanel.IsEnabled = Model.IsEnabled;            
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
                this.Delete.Invoke(this, new TextInputDeleteEventArgs { txt = this });
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
                this.ContentParent.SelectedItem = null;
                return;
            }

            if (this.ParentPanel.IsEnabled == false)
            {
                this.ParentPanel.IsEnabled = true;
                this.Model.IsEnabled = true;
                this.ContentParent.SelectedItem = this;
                return;
            }
        }

        void TextInputWrapper_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ContentParent.SelectedItem = this;
            this.IsSelected = true;
        }

        //void ImageWrapper_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    this.ContextMenu.PlacementTarget = this;
        //    this.ContextMenu.IsOpen = true;
        //}

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
        }        
    }

    public class TextInputDeleteEventArgs : EventArgs
    {
        public TextInputWrapper txt;
    }
}
