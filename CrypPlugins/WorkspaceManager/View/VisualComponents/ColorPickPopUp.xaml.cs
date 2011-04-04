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
using System.Collections;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Reflection;
using System.IO;
using System.Threading;

namespace WorkspaceManager.View.VisualComponents
{
    public partial class ColorPickPopUp : Popup
    {
        public Rectangle invoker;
        public event EventHandler<EventArgs> ColorPickerColorChanged;

        public ColorPickPopUp()
        {
            InitializeComponent();
            invoker = null; 
        }

        public ColorPickPopUp(Rectangle invoker)
        {
            InitializeComponent();
            this.invoker = invoker;
            Type t = typeof(Brushes);
            PropertyInfo[] colors = t.GetProperties();

            WrapPanel colorWrap = new WrapPanel();
            colorWrap.Width = 450;
            colorWrap.Height = 300;
            foreach (PropertyInfo color in colors)
            {
                BrushConverter convertor = new BrushConverter();
                Brush brush = convertor.ConvertFromString(color.Name) as Brush;
                string hex = string.Format("#{0}", brush.ToString().Substring(3));

                Rectangle rectangle = new Rectangle();

                if (color.Name == "Transparent")
                    rectangle.Fill = this.Resources["transparent"] as VisualBrush;
                else
                    rectangle.Fill = brush;

                rectangle.ToolTip = hex;
                rectangle.Height = 20;
                rectangle.Width = 20;
                rectangle.Margin = new Thickness(5, 5, 5, 5);
                rectangle.Stroke = Brushes.WhiteSmoke;

                rectangle.MouseDown += new MouseButtonEventHandler(rectangle_MouseDown);

                colorWrap.Children.Add(rectangle);
            }

            root.Children.Add(colorWrap);
        }

        private Rectangle makeRec(Uri uri)
        {
            Rectangle rect = new Rectangle();
            ImageBrush imgBrush = new ImageBrush();

            rect.Height = 20;
            rect.Width = 20;
            rect.Margin = new Thickness(5, 5, 5, 5);
            rect.Stroke = Brushes.WhiteSmoke;

            rect.MouseDown += new MouseButtonEventHandler(rectangle_MouseDown);

            BitmapImage img = new BitmapImage(uri);
            imgBrush.ImageSource = img;
            imgBrush.Stretch = Stretch.UniformToFill;
            rect.Fill = imgBrush;

            return rect;
        }


        void rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            invoker.Fill = (sender as Rectangle).Fill;
            if (ColorPickerColorChanged != null)
                ColorPickerColorChanged.Invoke(invoker, null);
            this.IsOpen = false;
        }
    }
}
