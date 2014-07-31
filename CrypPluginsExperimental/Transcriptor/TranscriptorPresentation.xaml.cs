using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Transcriptor
{
    /// <summary>
    /// Interaktionslogik für TranscriptorPresentation.xaml
    /// </summary>
    public partial class TranscriptorPresentation : UserControl
    {
        String rectangleColor, alphabet;
        int strokeThicknes, alphabetCount = 0, indexCount = 0;
        List<Sign> signList = new List<Sign>();
        ObservableCollection<Sign> signItems = new ObservableCollection<Sign>();
        double xCordinateDown, yCordinateDown, xCordinateUp, yCordinateUp;
        Rectangle rectangle;
        BitmapSource bitMap;
                
        public TranscriptorPresentation()
        {
            InitializeComponent();
            this.DataContext = this;
            rectangle = new Rectangle
            {
                Fill = Brushes.Transparent,
            };
        }

        public String Alphabet
        {
            get { return alphabet; }
            set { alphabet = value; }
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
            if (picture.Source != null)
            {
                if (canvas.Children.Contains(rectangle))
                {
                    canvas.Children.Remove(rectangle);
                }

                xCordinateUp = e.GetPosition(canvas).X;
                yCordinateUp = e.GetPosition(canvas).Y;

                rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(rectangleColor);
                rectangle.StrokeThickness = strokeThicknes;
                rectangle.Width = (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp));
                rectangle.Height = (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp));

                Canvas.SetLeft(rectangle, Math.Min(xCordinateDown, xCordinateUp));
                Canvas.SetTop(rectangle, Math.Min(yCordinateDown, yCordinateUp));
                canvas.Children.Add(rectangle);
                
                Rect rect = new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height);
                Int32Rect rcFrom = new Int32Rect();
                rcFrom.X = (int)((rect.X) * (picture.Source.Width) / (picture.Width));
                rcFrom.Y = (int)((rect.Y) * (picture.Source.Height) / (picture.Height));
                rcFrom.Width = (int)((rect.Width) * (picture.Source.Width) / (picture.Width));
                rcFrom.Height = (int)((rect.Height) * (picture.Source.Height) / (picture.Height));
                bitMap = new CroppedBitmap(picture.Source as BitmapSource, rcFrom);
                croppedImage.Source = bitMap;
            }
        }

        private void addSignButton_Click(object sender, RoutedEventArgs e)
        {
            Sign newSign = new Sign(indexCount, Alphabet[alphabetCount++], bitMap);
            signItems.Add(newSign);
            signListbox.ItemsSource = signItems;
            indexCount++;

            AddSignToList(newSign, (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp)),
                (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp)));

            if (alphabetCount >= Alphabet.Length)
            {
                addSignButton.IsEnabled = false;
            }
        }

        private void AddSignToList(Sign newSign, int width, int height)
        {
            double x = Math.Min(xCordinateDown, xCordinateUp);
            double y = Math.Min(yCordinateDown, yCordinateUp);

            Rectangle newRectangle = new Rectangle
            {
                Fill = Brushes.Transparent,
                Stroke = rectangle.Stroke,
                StrokeThickness = 1,
                Width = width,
                Height = height,
            };

            Canvas.SetLeft(newRectangle, x);
            Canvas.SetTop(newRectangle, y);
            canvas.Children.Add(newRectangle);

            newSign.Rectangle = newRectangle;
            newSign.X = x;
            newSign.Y = y;

            signList.Add(newSign);
        }
    }
}
