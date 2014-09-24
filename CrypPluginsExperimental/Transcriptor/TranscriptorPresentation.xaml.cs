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
        int strokeThicknes, alphabetCount = 0, indexCount = 0, comparisonMethod, currentRectangeleWidth, currentRectangleHeight;
        bool mtOn;
        List<Sign> signList = new List<Sign>();
        ObservableCollection<Sign> signItems = new ObservableCollection<Sign>();
        double xCordinateDown, yCordinateDown, xCordinateUp, yCordinateUp;
        Rectangle rectangle;
        BitmapSource croppedBitmap;
        bool mouseDown, ctrlBtnPressed = false;
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

        public bool MatchTemplateOn
        {
            get { return mtOn; }
            set { mtOn = value; }
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
            
            // Removes a Sign Element from Canvas and signList
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ctrlBtnPressed = true;
                var element = (e.OriginalSource as FrameworkElement).Name;

                if (element.Contains("rectangle"))
                {
                    int number = Convert.ToInt32(element.Substring(9));

                    for (int i = 0; i < signList.Count; i++)
                    {
                        if (number == signList[i].Id)
                        {
                            signList.RemoveAt(i);
                            break;
                        }
                    }

                    canvas.Children.Remove(e.OriginalSource as FrameworkElement);
                }
            }
            else
            {
                xCordinateDown = e.GetPosition(canvas).X;
                yCordinateDown = e.GetPosition(canvas).Y;
                rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(rectangleColor);
                rectangle.StrokeThickness = strokeThicknes;

                Canvas.SetLeft(rectangle, xCordinateDown);
                Canvas.SetTop(rectangle, yCordinateDown);
                canvas.Children.Add(rectangle);

                mouseDown = true;
                ctrlBtnPressed = false;
            }
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

                if (!ctrlBtnPressed)
                {
                    xCordinateUp = e.GetPosition(canvas).X;
                    yCordinateUp = e.GetPosition(canvas).Y;
                    currentRectangeleWidth = (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp));
                    currentRectangleHeight = (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp)); ;

                    rectangle.Width = currentRectangeleWidth;
                    rectangle.Height = currentRectangleHeight;

                    Canvas.SetLeft(rectangle, Math.Min(xCordinateDown, xCordinateUp));
                    Canvas.SetTop(rectangle, Math.Min(yCordinateDown, yCordinateUp));
                    canvas.Children.Add(rectangle);
                    mouseDown = false;

                    if (rectangle.Width != 0 && rectangle.Height != 0)
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
        }

        private void addSignButton_Click(object sender, RoutedEventArgs e)
        {
            Sign newSign = new Sign(indexCount, transcriptor.Alphabet[alphabetCount++], croppedBitmap);
            signItems.Add(newSign);
            signListbox.ItemsSource = signItems;
            indexCount++;

            AddSignToList(newSign, currentRectangeleWidth, currentRectangleHeight, Math.Min(xCordinateDown, xCordinateUp), Math.Min(yCordinateDown, yCordinateUp));

            if (alphabetCount >= transcriptor.Alphabet.Length)
            {
                addSignButton.IsEnabled = false;
            }

            if (MatchTemplateOn)
            {
                MatchSign(newSign);
            }
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;

            if (item != null || item.IsSelected)
            {
                Sign sign = new Sign(indexCount, item.Content.ToString()[0], croppedBitmap);
                indexCount++;
                AddSignToList(sign, currentRectangeleWidth, currentRectangleHeight, Math.Min(xCordinateDown, xCordinateUp), Math.Min(yCordinateDown, yCordinateUp));
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
        private void AddSignToList(Sign newSign, int width, int height, double x, double y)
        {
            Rectangle newRectangle = new Rectangle
            {
                Fill = Brushes.Transparent,
                Stroke = rectangle.Stroke,
                StrokeThickness = 1,
                Width = width,
                Height = height,
                Name = "rectangle" +newSign.Id,
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
            int cropWidth = currentRectangeleWidth;
            int cropHeight = currentRectangleHeight;
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
            bool skip = false;
            bool firstSign = true;

            for (int y = 0; y < matches.GetLength(0); y++)
            {
                for (int x = 0; x < matches.GetLength(1); x++)
                {
                    double matchScore = matches[y, x, 0];

                    if (matchScore > Threshold)
                    {
                        if (firstSign)
                        {
                            firstSign = false;
                        }
                        else
                        {
                            Sign equalSign = new Sign(indexCount, newSign.Letter, newSign.Image);
                            indexCount++;

                            AddSignToList(equalSign, currentRectangeleWidth, currentRectangleHeight, x, y); 
                        }
                        
                        x += currentRectangeleWidth;
                        skip = true;
                    }
                }

                if (skip)
                {
                    y += currentRectangleHeight;
                    skip = false;
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
