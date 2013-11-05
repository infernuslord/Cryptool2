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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Cryptool.Plugins.StegoLeastSignificantBit
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class StegoLeastSignificantBitPresentation : UserControl
    {
        private Bitmap bitmap;

        private int originalBitmapScan0 = 0;

        public void Init(Bitmap bitmap, int originalBitmapScan0)
        {
            this.originalBitmapScan0 = originalBitmapScan0;
            this.bitmap = (Bitmap)bitmap.Clone();
            ShowPicture();
        }

        private void ShowPicture()
        {
            image1.Source = null;
            
            MemoryStream stream = new MemoryStream();
            BitmapImage bmpimg = new BitmapImage();
            this.bitmap.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;

            bmpimg.BeginInit();
            bmpimg.StreamSource = stream;
            bmpimg.EndInit();

            image1.Source = bmpimg;
        }

        public void AddPixel(Int32 offsetToScan0)
        {
            int pixelIndex = (offsetToScan0 - originalBitmapScan0) / 3;
            //int y = (int)Math.Truncate((double)pixelIndex / this.bitmap.Width);
            int y = pixelIndex / this.bitmap.Width;
            int x = pixelIndex % this.bitmap.Width;
            int r = 2;

            using (Graphics graphics = Graphics.FromImage(this.bitmap))
            {
                graphics.FillEllipse(
                        new SolidBrush(System.Drawing.Color.Red),
                        x-r, y-r,
                        2*r, 2*r);

                graphics.DrawEllipse(
                        new System.Drawing.Pen(System.Drawing.Color.Black, 1),
                        x - r, y - r,
                        2 * r, 2 * r);
            }

            ShowPicture();       
        }

        public StegoLeastSignificantBitPresentation()
        {
            InitializeComponent();
        }
    }
}
