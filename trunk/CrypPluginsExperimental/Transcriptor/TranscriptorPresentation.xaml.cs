/*
   Copyright 2014 Olga Groh

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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
        String rectangleColor, selectedRectangleColor;
        int alphabetCount = 0, indexCount = 0, comparisonMethod, currentRectangeleWidth, currentRectangleHeight;
        bool mtOn, mouseDown, ctrlBtnPressed = false, firstSignOn = false;
        List<Sign> signList = new List<Sign>();
        ObservableCollection<Sign> signItems = new ObservableCollection<Sign>(); // Handels ListboxItems
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

        public String SelectedRectangleColor
        {
            get { return selectedRectangleColor; }
            set { selectedRectangleColor = value; }
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

        /// <summary>
        /// MouseDown is used for:
        /// 1. When Ctrl is pushed a Sign can be deleted
        /// 2. When FirstSign On is active a Sign can be marked as such
        /// 3. Startpoint of the rectangles are saved in xCordinateDown and yCordinateDown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    //Contains the Name of the clicked Object
                    var element = (e.OriginalSource as FrameworkElement).Name;

                    if (element.Contains("rectangle"))
                    {
                        // Gets the rectangle's ID
                        int number = Convert.ToInt32(element.Substring(9));

                        for (int i = 0; i < signList.Count; i++)
                        {
                            // The statsList's value is subtracted since the Sign is removed
                            if (number == signList[i].Id)
                            {
                                int value = statsList[signList[i].Letter];
                                statsList[signList[i].Letter] = value - 1;

                                //When the value is 0 the Letter is also removed from the ListBox
                                if (statsList[signList[i].Letter] == 0)
                                {
                                    /*Since the Sign Object to be earased isn't always
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

                                /*If the rectangle has a different Color, the rectangle
                                 * is a firstSign Object so it will be removed from the firstSign List, too.*/
                                if (signList[i].Rectangle.Stroke != (SolidColorBrush)new BrushConverter().ConvertFromString(SelectedRectangleColor))
                                {
                                    firstSigns.Remove(signList[i]);
                                }

                                //Finally the Sign will be also removed from the signList
                                signList.RemoveAt(i);
                                break;
                            }
                        }

                        //and the rectangle in the Canvas will be also removed
                        canvas.Children.Remove(e.OriginalSource as FrameworkElement);

                        if (statsList.Count <= transcriptor.Alphabet.Length)
                        {
                            addSignButton.IsEnabled = true;
                        }
                    }
                }
                else
                {
                    //The Cordinates will be saved in Variables so the rectangle can be draged
                    if (!firstSignOn)
                    {
                        mouseDown = true;
                        ctrlBtnPressed = false;

                        xCordinateDown = e.GetPosition(canvas).X;
                        yCordinateDown = e.GetPosition(canvas).Y;

                        rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(RectangleColor);
                        rectangle.StrokeThickness = 1;

                        Canvas.SetLeft(rectangle, xCordinateDown);
                        Canvas.SetTop(rectangle, yCordinateDown);
                        canvas.Children.Add(rectangle);
                    }
                    else
                    {
                        // If the Object is a rectangle and firstSign on
                        var element = (e.OriginalSource as FrameworkElement);

                        if (element.Name.Contains("rectangle"))
                        {
                            int number = Convert.ToInt32(element.Name.Substring(9));

                            for (int k = 0; k < signList.Count; k++)
                            {
                                /*If the Id is correct the Color of the rectangle will
                                be changed and the sign will be added to the firstSign List*/
                                if (signList[k].Id == number)
                                {
                                    if (signList[k].Rectangle.Stroke == (SolidColorBrush)new BrushConverter().ConvertFromString(SelectedRectangleColor))
                                    {
                                        signList[k].Rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(RectangleColor);
                                        firstSigns.Add(signList[k]);
                                        break;
                                    }
                                    else
                                    {
                                        //If the User clicks again on a firstSign rectangle it will be removed from the List
                                        signList[k].Rectangle.Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(SelectedRectangleColor);
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

        /// <summary>
        /// The rectanle can be draged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Draws the finished rectangle and crops the Image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                        //The Rectangle's Width and the Height is calculated
                        currentRectangeleWidth = (int)(Math.Max(xCordinateUp, xCordinateDown) - Math.Min(xCordinateDown, xCordinateUp));
                        currentRectangleHeight = (int)(Math.Max(yCordinateUp, yCordinateDown) - Math.Min(yCordinateDown, yCordinateUp)); ;

                        rectangle.Width = currentRectangeleWidth;
                        rectangle.Height = currentRectangleHeight;

                        Canvas.SetLeft(rectangle, Math.Min(xCordinateDown, xCordinateUp));
                        Canvas.SetTop(rectangle, Math.Min(yCordinateDown, yCordinateUp));
                        canvas.Children.Add(rectangle);
                        mouseDown = false;

                        //The Rectangle will be used to crop an Image
                        if (rectangle.Width != 0 && rectangle.Height != 0)
                        {
                            rect = new Rect(Canvas.GetLeft(rectangle), Canvas.GetTop(rectangle), rectangle.Width, rectangle.Height);
                            rcFrom = new Int32Rect();

                            rcFrom.X = (int)((rect.X) * ((picture.Source.Width) / (picture.Width)));
                            rcFrom.Y = (int)((rect.Y) * ((picture.Source.Height) / (picture.Height)));
                            rcFrom.Width = (int)((rect.Width) * ((picture.Source.Width) / (picture.Width)));
                            rcFrom.Height = (int)((rect.Height) * ((picture.Source.Height) / (picture.Height)));

                            /*croppedBitmap gets the rcFrom Objet and the picture. It returns a cut Image
                             * afterwards the Imgae is presented in the GUI*/
                            croppedBitmap = new CroppedBitmap(picture.Source as BitmapSource, rcFrom);
                            croppedImage.Source = croppedBitmap;

                            calculateProbability();
                        }
                    }
               }
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        /// <summary>
        /// Adds a Sign to the signList and to the ListBox.
        /// If Mode is set to semi-automatik the MatchTemplate Methode is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addSignButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //If the all the letters in the used alphabet is in the ListBox the Button is disabled
                if (statsList.Count >= transcriptor.Alphabet.Length)
                {
                    addSignButton.IsEnabled = false;
                }
                else
                {
                    char newLetter = transcriptor.Alphabet[alphabetCount++];

                    //Uses the next free Letter
                    while (statsList.ContainsKey(newLetter))
                    {
                        newLetter = transcriptor.Alphabet[alphabetCount++];

                        if (alphabetCount >= transcriptor.Alphabet.Length)
                        {
                            alphabetCount = 0;
                        }
                    }

                    //Creates a new Sign with the SignImage and letter afterwards the AddSignToLust is called
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

        /// <summary>
        /// When the user double clicks in an item a Sign will be created an added to signList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //item contains the ListBox Item
                var item = sender as ListBoxItem;

                if (item != null || item.IsSelected)
                {
                    //From the Item the letter can be extracted and added to the signList and canvas
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

        /// <summary>
        /// Sets either firstSign on or off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// The user can add spaces in the Text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spaceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Sign spaceSign = new Sign(indexCount, ' ', croppedBitmap);
                indexCount++;

                AddSignToList(spaceSign, currentRectangeleWidth, currentRectangleHeight,
                            Math.Min(xCordinateDown, xCordinateUp), Math.Min(yCordinateDown, yCordinateUp));

                spaceSign.Rectangle.ToolTip = "Space";
            }
            catch (Exception ex)
            {
                transcriptor.GuiLogMessage(ex);
            }
        }

        /// <summary>
        /// If mode is set to semi-Automatik the signList needs to sorted
        /// first after that the list will be saved in a String and handed over to the output Plugin
        /// If mode is set to manually the sorting has been done by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void generateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder textBuilder = new StringBuilder();
                double upperBound = 0, lowerBound = 0;

                if (MatchTemplateOn)
                {
                    //Fist signList will be first sorted after the X Cordinate
                    signList.Sort(new SignComparer(true));
                    /*firstSign will be sorted after the y Cordinate so its
                     * not necasary to click on the lines in the Text by order*/
                    firstSigns.Sort(new SignComparer(false));
                    
                    for (int i = 0; i < firstSigns.Count; i++)
                    {
                        upperBound = firstSigns[i].Y + (firstSigns[i].Rectangle.Height / 2);
                        lowerBound = firstSigns[i].Y - (firstSigns[i].Rectangle.Height / 2);

                        /*The signList objects which are in the upper and lower Bound will be append to the Text
                         * since the sortig of the signList is already done the Text will be presented properly*/
                        for (int j = 0; j < signList.Count; j++)
                        {
                            if (signList[j].Y > lowerBound && signList[j].Y < upperBound)
                            {
                                textBuilder.Append(signList[j]);
                            }
                        }
                        textBuilder.Append(" ");
                    }
                }
                else
                {
                    /*If the mode is set to manually the sorting needs to done by the user and
                     * the signList Letters will be append to the Text*/
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Draws a new Rectangle and ads newSign to the SignList
        /// </summary>
        /// <param name="newSign"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void AddSignToList(Sign newSign, int width, int height, double x, double y)
        {
            //Adds a fixed rectangle to the canvas
            Rectangle newRectangle = new Rectangle
            {
                Fill = Brushes.Transparent,
                Stroke = (SolidColorBrush)new BrushConverter().ConvertFromString(SelectedRectangleColor),
                StrokeThickness = 1,
                Width = width,
                Height = height,
                Name = "rectangle" +newSign.Id,
                ToolTip = newSign.Letter,
            };

            Canvas.SetLeft(newRectangle, x);
            Canvas.SetTop(newRectangle, y);
            canvas.Children.Add(newRectangle);

            //the rectangle is bound to a Sign
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
            Image<Gray, Byte> cImage = new Image<Gray, byte>(ToBitmap(croppedBitmap));
            cImage = cImage.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);

            if (signItems.Count > 0)
            {
                for (int i = 0; i < signItems.Count; i++)
                {
                    Image<Gray, Byte> sImage = new Image<Gray, byte>(ToBitmap(signItems[i].Image));
                    sImage = sImage.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);

                    Image<Gray, Byte> diffImage = cImage.AbsDiff(sImage);
                    System.Drawing.Bitmap bitmapDiff = diffImage.ToBitmap();
                    int pixelCounter = 0;

                    for (int x = 0; x < bitmapDiff.Width; x++)
                    {
                        for (int y = 0; y < bitmapDiff.Height; y++)
                        {
                            System.Drawing.Color pixelColor = bitmapDiff.GetPixel(x, y);

                            if ((pixelColor.R + pixelColor.G + pixelColor.B) == 0)
                            {
                                pixelCounter++;
                            }
                        }
                    }

                    signItems[i].Probability = pixelCounter * 0.01;
                }

                signListbox.Items.Refresh();
            }
        }

        /// <summary>
        /// Finds equal Signs and adds them to the signList
        /// </summary>
        /// <param name="newSign"></param>
        private void MatchSign(Sign newSign)
        {
            int cropWidth = currentRectangeleWidth;
            int cropHeight = currentRectangleHeight;

            //Since Emgu CV is not compatible to WPF the pictures have to be changed to System.Drawing objects 
            System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle((int)newSign.X, (int)newSign.Y, cropWidth, cropHeight);
            Image<Gray, Byte> sourceImage = new Image<Gray, byte>(ToBitmap(picture.Source as BitmapSource));

            //The Images have to be resized so they have the same Width and height like the picture.Source otherwise the coordinates are wrong
            sourceImage = sourceImage.Resize((int)picture.Width, (int)picture.Height, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            System.Drawing.Bitmap signBitmap = new System.Drawing.Bitmap(sourceImage.Bitmap);
            signBitmap = signBitmap.Clone(cropRect, System.Drawing.Imaging.PixelFormat.DontCare);

            //The templateImage is the Image with the Sign
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
                    /*The matchScore will be calculated for x and y and if its bigger then the choosen Threshold
                     * a Sign Object will be created*/
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

                        //a Skip is necasary beacause MatchTemplate would find Sign objects several times
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

        /// <summary>
        /// Converts a BitmapSource to a System.Drawing.Bitmap Object
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <returns>System.Drawing.Bitmap Object bitmap</returns>
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
