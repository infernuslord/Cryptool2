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
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;

namespace Cryptool.Plugins.VisualDecoder
{
    /// <summary>
    /// Interaktionslogik für DimCodeEncoderPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("VisualDecoder.Properties.Resources")]
    public partial class VisualDecoderPresentation : UserControl
    {
        public VisualDecoderPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// simple image setter
        /// </summary>
        public void SetImages(byte[] img)
        {
            var imageJar = new ImageSource[1];
            var c = new ImageConverter();
            imageJar[0] = (ImageSource)ConvertToBitmapSource((Bitmap)c.ConvertFrom(img));
            imageJar[0].Freeze();

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                Image.Source = imageJar[0];
            }), imageJar);
        }

        /// <summary>
        /// sets the payload and the detected codetype and change the color of the presentation
        /// </summary>
        public void SetData(string payload, string codetype)
        {
            string[] jar = {payload, codetype};
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                Payload.Text = jar[0];
                codeType.Content = jar[1];

                BodyBorder.Background = new SolidColorBrush(jar[0].Length == 0 ? Color.FromArgb(0xAF, 0xFF, 0xD4, 0xC1)
                                                                                : Color.FromArgb(0xAF, 0xE2, 0xFF, 0xCE));

                HeaderBorder.Background = new SolidColorBrush(jar[0].Length == 0 ? Color.FromArgb(0xFF, 0xE5, 0x6B, 0x00)
                                                                                : Color.FromArgb(0xFF, 0x47, 0x93, 0x08));
            }), jar);
        }
    
        /// <summary>
        /// reset the presentation 
        /// </summary>
        public void ClearPresentation()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                Image.Source = null;
                Payload.Text = "";
                codeType.Content = "";
                BodyBorder.Background = new SolidColorBrush(Color.FromArgb(0xAF, 0xFF, 0xD4, 0xC1));
                HeaderBorder.Background =new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0x6B, 0x00));
            }), null);
        }
        
        #region helper

        private static ImageSource ConvertToBitmapSource(Bitmap gdiPlusBitmap)
        {
            IntPtr hBitmap = gdiPlusBitmap.GetHbitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }



        #endregion
    }
}
