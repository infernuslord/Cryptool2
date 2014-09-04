using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;

namespace Transcriptor
{
    /// <summary>
    /// Interaktionslogik für TranscriptorPresentation.xaml
    /// </summary>
    public partial class TranscriptorPresentation : UserControl
    {
        # region Variables

        private readonly Cryptool.Plugins.Transcriptor.Transcriptor transcriptor;
        String rectangleColor;
        int strokeThicknes, alphabetCount = 0, indexCount = 0, comparisonMethod;
        List<Sign> signList = new List<Sign>();
        ObservableCollection<Sign> signItems = new ObservableCollection<Sign>();
        double xCordinateDown, yCordinateDown, xCordinateUp, yCordinateUp;
        Rectangle rectangle;
        BitmapSource croppedBitmap;
        bool mouseDown;
        Int32Rect rcFrom;
        Rect rect;
        float threshold;

        #endregion

        public TranscriptorPresentation(Cryptool.Plugins.Transcriptor.Transcriptor transcriptor)
        {
            InitializeComponent();
            this.transcriptor = transcriptor;
            this.DataContext = this;
            mouseDown = false;
            
            rectangle = new Rectangle
            {
                Fill = Brushes.Transparent,
            };
        }

        #region Get\Set
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

        public int ComparisonMethod
        {
            get { return comparisonMethod; }
            set { comparisonMethod = value; }
        }

        public float Threshold
        {
            get { return threshold; }
            set { threshold = value; }
        }

        #endregion

        #region Events
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas.Children.Contains(rectangle))
            {
                canvas.Children.Remove(rectangle);
            }

            xCordinateDown = e.GetPosition(canvas).X;
            yCordinateDown = e.GetPosition(canvas).Y;
            rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(rectangleColor);
            rectangle.StrokeThickness = strokeThicknes;

            Canvas.SetLeft(rectangle, xCordinateDown);
            Canvas.SetTop(rectangle, yCordinateDown);
            canvas.Children.Add(rectangle);
            
            mouseDown = true;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown == true)
            {
                rectangle.Width = (int)(Math.Max(e.GetPosition(canvas).X, xCordinateDown) - Math.Min(xCordinateDown, e.GetPosition(canvas).X));
                rectangle.Height = (int)(Math.Max(e.GetPosition(canvas).Y, yCordinateDown) - Math.Min(yCordinateDown, e.GetPosition(canvas).Y));

                Canvas.SetLeft(rectangle, Math.Min(xCordinateDown, e.GetPosition(canvas).X));
                Canvas.SetTop(rectangle, Math.Min(yCordinateDown, e.GetPosition(canvas).Y));
            }
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

                rectangle.Width = (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp));
                rectangle.Height = (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp));

                Canvas.SetLeft(rectangle, Math.Min(xCordinateDown, xCordinateUp));
                Canvas.SetTop(rectangle, Math.Min(yCordinateDown, yCordinateUp));
                canvas.Children.Add(rectangle);
                mouseDown = false;

                if (rectangle.Width != 0)
                {
                    rect = new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height);
                    rcFrom = new Int32Rect();

                    rcFrom.X = (int)((rect.X) * ((picture.Source.Width) / (picture.Width)));
                    rcFrom.Y = (int)((rect.Y) * ((picture.Source.Height) / (picture.Height)));
                    rcFrom.Width = (int)((rect.Width) * ((picture.Source.Width) / (picture.Width)));
                    rcFrom.Height = (int)((rect.Height) * ((picture.Source.Height) / (picture.Height)));

                    croppedBitmap = new CroppedBitmap(picture.Source as BitmapSource, rcFrom);
                    croppedImage.Source = croppedBitmap;

                    //calculateProbability();
                }
            }
        }

        private void addSignButton_Click(object sender, RoutedEventArgs e)
        {
            Sign newSign = new Sign(indexCount, transcriptor.Alphabet[alphabetCount++], croppedBitmap);
            signItems.Add(newSign);
            signListbox.ItemsSource = signItems;
            indexCount++;
                        
            AddSignToList(newSign, (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp)),
                (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp)));

            if (alphabetCount >= transcriptor.Alphabet.Length)
            {
                addSignButton.IsEnabled = false;
            }
            
            MatchSign(newSign);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;

            if (item != null || item.IsSelected)
            {
                Sign sign = new Sign(indexCount, item.Content.ToString()[0], croppedBitmap);
                indexCount++;
                AddSignToList(sign, (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp)),
                (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp)));
            }
        }

        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder textBuilder = new StringBuilder();
            String outputText = null;

            for (int i = 0; i < signList.Count; i++)
            {

                textBuilder.Append(signList[i].Letter);
            }

            outputText = textBuilder.ToString();
            transcriptor.GenerateText(outputText);
  
        }

        #endregion

        #region Helper Methods
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

        private void calculateProbability()
        {
            if (signItems.Count > 0)
            {
                signItems.Clear();
                for (int i = 0; i < signList.Count; i++)
                {
                    signItems.Add(signList[i]);
                }
                signListbox.ItemsSource = signItems;
            }
        }

        private void MatchSign(Sign newSign)
        {
            int cropWidth = (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp));
            int cropHeight = (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp));
            System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle((int)newSign.X, (int)newSign.Y, cropWidth, cropHeight);
            Image<Gray, Byte> sourceImage = new Image<Gray, byte>(ToBitmap(picture.Source as BitmapSource));

            sourceImage = sourceImage.Resize((int)picture.Width, (int)picture.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            System.Drawing.Bitmap signBitmap = new System.Drawing.Bitmap(sourceImage.Bitmap);
            signBitmap = signBitmap.Clone(cropRect, System.Drawing.Imaging.PixelFormat.DontCare);

            Image<Gray, Byte> templateImage = new Image<Gray, byte>(signBitmap);
            Image<Gray, float> resultImage;

            switch (comparisonMethod)
            {
                case 0: resultImage = sourceImage.MatchTemplate(templateImage, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCOEFF); break;
                case 1: resultImage = sourceImage.MatchTemplate(templateImage, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCOEFF_NORMED); break;
                case 2: resultImage = sourceImage.MatchTemplate(templateImage, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCORR); break;
                case 3: resultImage = sourceImage.MatchTemplate(templateImage, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCORR_NORMED); break;
                case 4: resultImage = sourceImage.MatchTemplate(templateImage, Emgu.CV.CvEnum.TM_TYPE.CV_TM_SQDIFF); break;
                case 5: resultImage = sourceImage.MatchTemplate(templateImage, Emgu.CV.CvEnum.TM_TYPE.CV_TM_SQDIFF_NORMED); break;
                default: resultImage = sourceImage.MatchTemplate(templateImage, Emgu.CV.CvEnum.TM_TYPE.CV_TM_CCOEFF_NORMED); break;
            }

            float[, ,] matches = resultImage.Data;

            for (int y = 0; y < matches.GetLength(0); y++)
            {
                for (int x = 0; x < matches.GetLength(1); x++)
                {
                    double matchScore = matches[y, x, 0];

                    if (matchScore > Threshold)
                    {
                        Rectangle rec = new Rectangle
                        {
                            Fill = Brushes.Transparent,
                            Stroke = Brushes.Red,
                            StrokeThickness = 1,
                            Width = (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp)),
                            Height = (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp)),
                        };

                        Canvas.SetLeft(rec, x);
                        Canvas.SetTop(rec, y);
                        canvas.Children.Add(rec);
                    }
                }
            }
        }

        private System.Drawing.Bitmap ToBitmap(BitmapSource bitmapSource)
        {
            System.Drawing.Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                // from System.Media.BitmapImage to System.Drawing.Bitmap
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);

                return bitmap;
            }
        }

        #endregion
    }
}
