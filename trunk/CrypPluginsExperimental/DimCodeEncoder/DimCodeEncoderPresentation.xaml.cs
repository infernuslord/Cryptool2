/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cryptool.Plugins.DimCodeEncoder;
using DimCodeEncoder.model;
using Color = System.Drawing.Color;

namespace DimCodeEncoder
{
    /// <summary>
    /// Interaktionslogik für DimCodeEncoderPresentation.xaml
    /// </summary>
    public partial class DimCodeEncoderPresentation : UserControl
    {
        public DimCodeEncoderPresentation()
        {
            InitializeComponent();
        }

        #region secure setter

        public void SetImages(byte[] explaindImg, byte[] pureImg  )
        {
            var bgc = Color.FromArgb(0xAF,0xFF,0xD4,0xC1);

            var imageJar = new ImageSource[2];
            var c = new ImageConverter();
            imageJar[0] = (ImageSource) ConvertToBitmapSource(ReplaceColorWith((Bitmap)c.ConvertFrom(explaindImg), Color.White, bgc));
            imageJar[1] = (ImageSource) ConvertToBitmapSource(ReplaceColorWith((Bitmap)c.ConvertFrom(pureImg), Color.White, bgc)); 
            
            imageJar[0].Freeze();
            imageJar[1].Freeze();

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>  
            {
                try
                {
                    ExplImage.Source = imageJar[0];
                    PureImage.Source = imageJar[1];
                   UpdateImage();
                }
                catch
                {
                    throw; //TODO REMOVE 
                }
            }), imageJar);
            
        }

        public void SetList(List<LegendItem> legend)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    legend1.Visibility = System.Windows.Visibility.Hidden;
                    legend2.Visibility = System.Windows.Visibility.Hidden;
                    legend3.Visibility = System.Windows.Visibility.Hidden;
                    legend4.Visibility = System.Windows.Visibility.Hidden;
                    legend5.Visibility = System.Windows.Visibility.Hidden;

                    if (legend.Count >= 1)
                    {
                        legend1.Visibility = System.Windows.Visibility.Visible;
                        lable1.Content = legend[0].LableValue;
                        disc1.Content = legend[0].DiscValue;
                        ellipse1.Fill = ContvertColorToBrush(legend[0].ColorBlack);
                    }
                   if (legend.Count >= 2) 
                    {
                        legend2.Visibility = System.Windows.Visibility.Visible;
                        lable2.Content = legend[1].LableValue;
                        disc2.Content = legend[1].DiscValue;
                        ellipse2.Fill = ContvertColorToBrush(legend[1].ColorBlack);
                    }
                   if (legend.Count >= 3)
                   {
                       legend3.Visibility = System.Windows.Visibility.Visible;
                       lable3.Content = legend[2].LableValue;
                       disc3.Content = legend[2].DiscValue;
                       ellipse3.Fill = ContvertColorToBrush(legend[2].ColorBlack);
                   }
                   if (legend.Count >= 4)
                   {
                       legend4.Visibility = System.Windows.Visibility.Visible;
                       lable4.Content = legend[3].LableValue;
                       disc4.Content = legend[3].DiscValue;
                       ellipse4.Fill = ContvertColorToBrush(legend[3].ColorBlack);
                   }
                   if (legend.Count >= 5)
                   {
                       legend5.Visibility = System.Windows.Visibility.Visible;
                       lable5.Content = legend[4].LableValue;
                       disc5.Content = legend[4].DiscValue;
                       ellipse5.Fill = ContvertColorToBrush(legend[4].ColorBlack);
                   }
                 }
                catch
                {
                    throw; //TODO REMOVE 
                }
            }), legend);
        }

        #endregion

        #region helper
        private static Bitmap ReplaceColorWith(Bitmap image, Color to, Color with)
        {
           LockBitmap lockBitmap = new LockBitmap(image);
           lockBitmap.LockBits();
           for (int y = 0; y < lockBitmap.Height; ++y)
            {
                for (int x = 0; x < lockBitmap.Width; ++x)
                {
                    var p = lockBitmap.GetPixel(x, y);
                    if (p.R == to.R && p.G == to.G && p.B == to.B)
                    {
                        lockBitmap.SetPixel(x, y, with);
                    }
                }
            }
            lockBitmap.UnlockBits();
            return image;

        }

        private static ImageSource ConvertToBitmapSource(Bitmap gdiPlusBitmap)
        {
            IntPtr hBitmap = gdiPlusBitmap.GetHbitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }


        private static SolidColorBrush ContvertColorToBrush(Color colorValue)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(colorValue.A,
                                                                           colorValue.R,
                                                                           colorValue.G,
                                                                           colorValue.B));
        }

        private void Explain_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            panel.Width = 365;
            UpdateImage();
        }

        private void Explain_Collapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            panel.Width = 565;
            UpdateImage();
        }

        private void UpdateImage()
        {
            Image.Source = !Explain.IsExpanded ? ExplImage.Source : PureImage.Source;
            Image.Width = !Explain.IsExpanded ? ExplImage.Width : PureImage.Width;
            Image.Height = !Explain.IsExpanded ? ExplImage.Height : PureImage.Height;
        }
        #endregion 
    }
}
