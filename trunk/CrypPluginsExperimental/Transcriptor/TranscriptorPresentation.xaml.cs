using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Transcriptor
{
    /// <summary>
    /// Interaktionslogik für TranscriptorPresentation.xaml
    /// </summary>
    public partial class TranscriptorPresentation : UserControl
    {
        String rectangleColor;
        int strokeThicknes;
        List<Sign> SignList = new List<Sign>();
        double xCordinateDown, yCordinateDown, xCordinateUp, yCordinateUp;
                
        public TranscriptorPresentation()
        {
            InitializeComponent();
        }

        public int StrokeThicknes
        {
            get { return strokeThicknes;  }
            set { strokeThicknes = value; }
        }

        public String RectangleColor
        {
            get { return rectangleColor; }
            set { rectangleColor = value; }
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            xCordinateDown = e.GetPosition(canvas).X;
            yCordinateDown = e.GetPosition(canvas).Y;
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (picture.Source == null)
            {
            }
            else
            {
                xCordinateUp = e.GetPosition(canvas).X;
                yCordinateUp = e.GetPosition(canvas).Y;

                //Draws a rectangle in the Image
                Rectangle rectangle = new Rectangle
                {
                    Fill = Brushes.Transparent,
                    Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(rectangleColor),
                    StrokeThickness = strokeThicknes,
                    Width = (int) (Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp)),
                    Height = (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp)),
                };

                Canvas.SetLeft(rectangle, Math.Min(xCordinateDown, xCordinateUp));
                Canvas.SetTop(rectangle, Math.Min(yCordinateDown, yCordinateUp));
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

        private void addSignButton_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
