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

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for ImageWrapper.xaml
    /// </summary>
    public partial class ImageWrapper : UserControl
    {
        public Image Image { get; set; }
        public Point Position { get; set; }
        private Point previousDragPoint = new Point();
        private ImageModel model = null;

        public ImageWrapper(ImageModel model, Point point)
        {
            this.Image = model.getImage();
            this.Position = point;
            model.Position = point;
            InitializeComponent();
            this.MouseMove += new MouseEventHandler(ImageWrapper_MouseMove);
            this.MouseLeftButtonDown +=new MouseButtonEventHandler(ImageWrapper_MouseLeftButtonDown);
            this.RenderTransform = new TranslateTransform(Position.X, Position.Y);
            this.root.Children.Add(Image);
            this.model = model;
        }

        void ImageWrapper_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = new Point(Mouse.GetPosition(this.Parent as Panel).X - previousDragPoint.X, (Mouse.GetPosition(this.Parent as Panel).Y - previousDragPoint.Y));
                (this.RenderTransform as TranslateTransform).X = p.X;
                (this.RenderTransform as TranslateTransform).Y = p.Y;
                this.model.Position = p;
            }
        }

        void ImageWrapper_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.previousDragPoint = Mouse.GetPosition(this); 
        }

        /*private Image makeImage(Uri imgUri)
        {
            try 
            {
                Image img = new Image();
                img.Source = new BitmapImage(imgUri);
                return img;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }
            return null;
        }*/

        public ImageWrapper()
        {
            InitializeComponent();
        }
    }
}
