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


namespace Cryptool.Plugins.CodeScanner
{
    /// <summary>
    /// Interaktionslogik für CodeScannerPresentation.xaml
    /// </summary>
    public partial class CodeScannerPresentation : System.Windows.Controls.UserControl
    {
        public CodeScannerPresentation()
        {
            InitializeComponent();
        }

        public void setImage(Bitmap Image)
        {
            var imageJar = new ImageSource[1];
            var c = new ImageConverter();
            imageJar[0] = (ImageSource)ConvertToBitmapSource(Image);
            imageJar[0].Freeze();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    image.Source = imageJar[0];
                }
                catch
                {
                }
            }), imageJar);
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
