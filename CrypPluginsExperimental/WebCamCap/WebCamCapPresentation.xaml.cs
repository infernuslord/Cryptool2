using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Cryptool.Plugins.WebCamCap;

namespace WebCamCap
{
    /// <summary>
    /// Interaktionslogik für WebCamCapPresentation.xaml
    /// </summary>
    /// 
    public partial class WebCamCapPresentation : UserControl
    {
        private readonly EventHandler captureImageExecuted;

        public WebCamCapPresentation(EventHandler captureImageExecuted)
        {
            InitializeComponent();
            this.captureImageExecuted = captureImageExecuted;
        }

        /// <summary>
        /// starts the cam, representated by the given device string and registers the capture handle methode
        /// </summary>
        /// <param name="device"></param>
        public void StartCam(string device)
        {
            this.SelectedWebcam = new CapDevice("")
                                      {
                                          MonikerString = device
                                      };

            //register output change eventhandler
            SelectedWebcam.NewBitmapReady += captureImageExecuted;
        }

        /// <summary>
        /// stops the current cam and deregister the capture handle methode
        /// </summary>
        public void StopCam()
        {
            SelectedWebcam.NewBitmapReady -= captureImageExecuted;
            SelectedWebcam = null;
        }

   

        #region Properties
        /// <summary>
        /// Wrapper for the WebcamRotation dependency property
        /// </summary>
        public double WebcamRotation
        {
            get { return (double)GetValue(WebcamRotationProperty); }
            set { SetValue(WebcamRotationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WebcamRotation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WebcamRotationProperty =
            DependencyProperty.Register("WebcamRotation", typeof(double), typeof(WebCamCapPresentation), new UIPropertyMetadata(180d));



        /// <summary>
        /// Wrapper for the SelectedWebcam dependency property
        /// </summary>
        public CapDevice SelectedWebcam
        {
            get { return (CapDevice)GetValue(SelectedWebcamProperty); }
            set { SetValue(SelectedWebcamProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedWebcam.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedWebcamProperty =
            DependencyProperty.Register("SelectedWebcam", typeof(CapDevice), typeof(WebCamCapPresentation), new UIPropertyMetadata(null));

        /// <summary>
        /// Wrapper for the SelectedWebcamMonikerString dependency property
        /// </summary>
        public string SelectedWebcamMonikerString
        {
            get { return (string)GetValue(SelectedWebcamMonikerStringProperty); }
            set { SetValue(SelectedWebcamMonikerStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedWebcamMonikerString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedWebcamMonikerStringProperty = DependencyProperty.Register("SelectedWebcamMonikerString", typeof(string),
            typeof(WebCamCapPresentation), new UIPropertyMetadata("", new PropertyChangedCallback(SelectedWebcamMonikerString_Changed)));

      
        #endregion

        #region Methods
        /// <summary>
        /// Invoked when the SelectedWebcamMonikerString dependency property has changed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        private static void SelectedWebcamMonikerString_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            
        }
        #endregion
    }
}
