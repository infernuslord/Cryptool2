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

        #region secure setter

        public void SetImages(byte[] img)
        {

            var imageJar = new ImageSource[1];
            var c = new ImageConverter();
            imageJar[0] = (ImageSource)ConvertToBitmapSource((Bitmap)c.ConvertFrom(img));
            imageJar[0].Freeze();

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    Image.Source = imageJar[0];
                }
                catch
                {
                }
            }), imageJar);
        }

        public void SetPayload(string payload)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    Payload.Text = payload;
                }
                catch
                {
                }
            }), payload);
        }

        /// <summary>
        ///  switches presentation backgroundcolors to the corespording mode color
        ///  Mode 0 -> decode failed -> set colors red
        ///  Mode 1(actually every number different from 0) -> decode succed -> set colors green
        /// </summary>
        public void SetColorMode(int mode)
        {

            //TODO
            if (mode == 0)
            {
                //HeaderBorder.Background = "#FFE56B00";
            }
            else
            {
                
            }
        }


        #endregion

        #region helper


        private static ImageSource ConvertToBitmapSource(Bitmap gdiPlusBitmap)
        {
            IntPtr hBitmap = gdiPlusBitmap.GetHbitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }



        #endregion
    }
}
