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


namespace Transcriptor
{
    /// <summary>
    /// Interaktionslogik für TranscriptorPresentation.xaml
    /// </summary>
    public partial class TranscriptorPresentation : UserControl
    {
        String rectangleColor = null;
        List<Rectangle> rectangleList = new List<Rectangle>();

        public TranscriptorPresentation()
        {
            InitializeComponent();


        }

        public String Color
        {
            get { return rectangleColor; }
            set { rectangleColor = value; }
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (picture.Source == null)
            {
            }
            else
            {
                //Draws a rectangles in the Image
                Rectangle rectangle = new Rectangle
                {
                    Fill = Brushes.Transparent,
                    Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(rectangleColor),
                    StrokeThickness = 2,
                    Width = 20,
                    Height = 20,
                };

                Canvas.SetLeft(rectangle, e.GetPosition(canvas).X);
                Canvas.SetTop(rectangle, e.GetPosition(canvas).Y);
                canvas.Children.Add(rectangle);

                Rect rect = new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height);
                Int32Rect rcFrom = new Int32Rect();
                rcFrom.X = (int)((rect.X) * (picture.Source.Width) / (picture.Width));
                rcFrom.Y = (int)((rect.Y) * (picture.Source.Height) / (picture.Height));
                rcFrom.Width = (int)((rect.Width) * (picture.Source.Width) / (picture.Width));
                rcFrom.Height = (int)((rect.Height) * (picture.Source.Height) / (picture.Height));
                BitmapSource bs = new CroppedBitmap(picture.Source as BitmapSource, rcFrom);
                croppedImage.Source = bs;  
            }
        }
    }
}
