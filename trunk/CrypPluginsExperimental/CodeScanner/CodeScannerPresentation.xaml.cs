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
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections;
using System.Text;


namespace Cryptool.Plugins.CodeScanner
{
    /// <summary>
    /// Interaktionslogik für CodeScannerPresentation.xaml
    /// </summary>
    public partial class CodeScannerPresentation : System.Windows.Controls.UserControl
    {

        /*
        #region webcam

        IntPtr deviceHandle;

        public const uint WM_CAP = 0x400;
        public const uint WM_CAP_DRIVER_CONNECT = 0x40a;
        public const uint WM_CAP_DRIVER_DISCONNECT = 0x40b;
        public const uint WM_CAP_EDIT_COPY = 0x41e;
        public const uint WM_CAP_SET_PREVIEW = 0x432;
        public const uint WM_CAP_SET_OVERLAY = 0x433;
        public const uint WM_CAP_SET_PREVIEWRATE = 0x434;
        public const uint WM_CAP_SET_SCALE = 0x435;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_VISIBLE = 0x10000000;


        [DllImport("avicap32.dll")]
        public extern static IntPtr capGetDriverDescription(ushort index, StringBuilder name, int nameCapacity, StringBuilder description,
            int descriptionCapacity);

        [DllImport("avicap32.dll")]
        public extern static IntPtr capCreateCaptureWindow(string title, uint style, int x, int y, int width, int height, IntPtr window,
        int id);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public void Attach()
        {

            deviceHandle = capCreateCaptureWindow(string.Empty, WS_VISIBLE | WS_CHILD, 0, 0, (int)this.ActualWidth - 150, (int)this.ActualHeight, new System.Windows.Interop.WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle, 0);

            if (SendMessage(deviceHandle, WM_CAP_DRIVER_CONNECT, (IntPtr)0, (IntPtr)0).ToInt32() > 0)
            {
                SendMessage(deviceHandle, WM_CAP_SET_SCALE, (IntPtr)(-1), (IntPtr)0);
                SendMessage(deviceHandle, WM_CAP_SET_PREVIEWRATE, (IntPtr)0x42, (IntPtr)0);
                SendMessage(deviceHandle, WM_CAP_SET_PREVIEW, (IntPtr)(-1), (IntPtr)0);
                SetWindowPos(deviceHandle, new IntPtr(0), 0, 0, (int)this.ActualWidth - 150, (int)this.ActualHeight, 6);
            }       
        }

        #endregion
        */

        public CodeScannerPresentation()
        {
            InitializeComponent();
        }


        public void setImage(Bitmap Image)
        {

            
            //var imageJar = new ImageSource[1];
           // var c = new ImageConverter();
          //  imageJar[0] = (ImageSource)ConvertToBitmapSource(Image);
         //   imageJar[0].Freeze();
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    picLiveView.Image = Image;
                //   picLiveView = imageJar[0];
                }
                catch
                {
                }
            }), Image); 
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
