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
        bool mtOn, mouseDown, ctrlBtnPressed = false, firstSignOn = false;
        List<Sign> signList = new List<Sign>();
        ObservableCollection<Sign> signItems = new ObservableCollection<Sign>();
        Dictionary<char, int> statsList = new Dictionary<char, int>();
        List<Sign> firstSigns = new List<Sign>();
        double xCordinateDown, yCordinateDown, xCordinateUp, yCordinateUp;
        Rectangle rectangle;
        BitmapSource croppedBitmap;
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
            try
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
                                int value = statsList[signList[i].Letter];
                                statsList[signList[i].Letter] = value - 1;

                                if (statsList[signList[i].Letter] == 0)
                                {
                                    /*Since the to be earased Sign Object isn't always
                                     * the one that is in the signItems List an if-case is
                                     * needed to find the right one in the List. So it can be removed*/
                                    for (int j = 0; j < signItems.Count; j++)
                                    {
                                        if (signItems[j].Letter == signList[i].Letter)
                                        {
                                            signItems.RemoveAt(j);
                                            break;
                                        }
                                    }

                                    signListbox.Items.Refresh();
                                    statsList.Remove(signList[i].Letter);
                                }

                                if (signList[i].Rectangle.Stroke != (SolidColorBrush)new BrushConverter().ConvertFromString(rectangleColor))
                                {
                                    firstSigns.Remove(signList[i]);
                                }
                                signList.RemoveAt(i);
                                break;
                            }
                        }

                        canvas.Children.Remove(e.OriginalSource as FrameworkElement);

                        if (statsList.Count <= transcriptor.Alphabet.Length)
                        {
                            addSignButton.IsEnabled = true;
                        }
                    }
                }
                else
                {
                    if (!firstSignOn)
                    {
                        mouseDown = true;
                        ctrlBtnPressed = false;

                        xCordinateDown = e.GetPosition(canvas).X;
                        yCordinateDown = e.GetPosition(canvas).Y;

                        rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(rectangleColor);
                        rectangle.StrokeThickness = strokeThicknes;

                        Canvas.SetLeft(rectangle, xCordinateDown);
                        Canvas.SetTop(rectangle, yCordinateDown);
                        canvas.Children.Add(rectangle);
                    }
                    else
                    {
                        var element = (e.OriginalSource as FrameworkElement);

                        if (element.Name.Contains("rectangle"))
                        {
                            int number = Convert.ToInt32(element.Name.Substring(9));

                            for (int k = 0; k < signList.Count; k++)
                            {
                                if (signList[k].Id == number)
                                {
                                    if (signList[k].Rectangle.Stroke == (SolidColorBrush)new BrushConverter().ConvertFromString(rectangleColor))
                                    {
                                        signList[k].Rectangle.Stroke = Brushes.Blue;
                                        firstSigns.Add(signList[k]);
                                        break;
                                    }
                                    else
                                    {
                                        signList[k].Rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(rectangleColor);
                                        firstSigns.Remove(signList[k]);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (mouseDown)
                {
                    rectangle.Width = (int)(Math.Max(e.GetPosition(canvas).X, xCordinateDown) - Math.Min(xCordinateDown, e.GetPosition(canvas).X));
                    rectangle.Height = (int)(Math.Max(e.GetPosition(canvas).Y, yCordinateDown) - Math.Min(yCordinateDown, e.GetPosition(canvas).Y));

                    Canvas.SetLeft(rectangle, Math.Min(xCordinateDown, e.GetPosition(canvas).X));
                    Canvas.SetTop(rectangle, Math.Min(yCordinateDown, e.GetPosition(canvas).Y));
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (picture.Source != null)
                {
                    if (canvas.Children.Contains(rectangle))
                    {
                        canvas.Children.Remove(rectangle);
                    }

                    xCordinateUp = e.GetPosition(canvas).X;
                    yCordinateUp = e.GetPosition(canvas).Y;

                    if (!ctrlBtnPressed && !firstSignOn)
                    {
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
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        private void addSignButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (statsList.Count >= transcriptor.Alphabet.Length)
                {
                    addSignButton.IsEnabled = false;
                }
                else
                {
                    char newLetter = transcriptor.Alphabet[alphabetCount++];

                    while (statsList.ContainsKey(newLetter))
                    {
                        newLetter = transcriptor.Alphabet[alphabetCount++];

                        if (alphabetCount >= transcriptor.Alphabet.Length)
                        {
                            alphabetCount = 0;
                        }
                    }

                    Sign newSign = new Sign(indexCount, newLetter, croppedBitmap);
                    signItems.Add(newSign);
                    signListbox.ItemsSource = signItems;
                    indexCount++;

                    AddSignToList(newSign, currentRectangeleWidth, currentRectangleHeight,
                        Math.Min(xCordinateDown, xCordinateUp), Math.Min(yCordinateDown, yCordinateUp));

                    if (alphabetCount >= transcriptor.Alphabet.Length)
                    {
                        alphabetCount = 0;
                    }

                    if (MatchTemplateOn)
                    {
                        MatchSign(newSign);
                    }
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = sender as ListBoxItem;

                if (item != null || item.IsSelected)
                {
                    Sign sign = new Sign(indexCount, item.Content.ToString()[0], croppedBitmap);
                    indexCount++;
                    AddSignToList(sign, currentRectangeleWidth, currentRectangleHeight, Math.Min(xCordinateDown, xCordinateUp),
                        Math.Min(yCordinateDown, yCordinateUp));
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder textBuilder = new StringBuilder();
                double upperBound = 0, lowerBound = 0;

                if (MatchTemplateOn)
                {
                    signList.Sort(new SignComparer(true));
                    firstSigns.Sort(new SignComparer(false));

                    for (int i = 0; i < firstSigns.Count; i++)
                    {
                        upperBound = firstSigns[i].Y + (firstSigns[i].Rectangle.Height / 2);
                        lowerBound = firstSigns[i].Y - (firstSigns[i].Rectangle.Height / 2);
                        for (int j = 0; j < signList.Count; j++)
                        {
                            if (signList[j].Y > lowerBound && signList[j].Y < upperBound)
                            {
                                textBuilder.Append(signList[j]);
                            }
                        }
                        textBuilder.Append("\n");
                    }
                }
                else
                {
                    for (int i = 0; i < signList.Count; i++)
                    {
                        textBuilder.Append(signList[i].Letter);
                    }
                }

                transcriptor.GenerateText(textBuilder.ToString());
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        private void TransformButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (firstSignOn == false)
                {

                    TransformButton.Content = "First Sign On";
                    firstSignOn = true;
                }
                else
                {
                    TransformButton.Content = "First Sign Off";
                    firstSignOn = false;
                }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
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
                ToolTip = newSign.Letter,
            };

            Canvas.SetLeft(newRectangle, x);
            Canvas.SetTop(newRectangle, y);
            canvas.Children.Add(newRectangle);

            newSign.Rectangle = newRectangle;
            newSign.X = x;
            newSign.Y = y;

            signList.Add(newSign);

            if (statsList.ContainsKey(newSign.Letter))
            {
                int value = statsList[newSign.Letter];
                statsList[newSign.Letter] = value + 1;
            }
            else
            {
                statsList.Add(newSign.Letter, 1);
            }
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
