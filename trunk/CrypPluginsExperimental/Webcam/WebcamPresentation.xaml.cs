using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Cryptool.Plugins.Webcam.Libaries;


namespace Webcam
{
    /// <summary>
    /// Interaktionslogik für WebCamPresentation.xaml
    /// </summary>
    public partial class WebcamPresentation : UserControl
    {
         private readonly EventHandler newCamEstablished;

        public WebcamPresentation(EventHandler newCamEstablished)
        {
            InitializeComponent();
            this.newCamEstablished = newCamEstablished;
        }

        /// <summary>
        /// starts the cam, representated by the given device string and registers the capture handle methode
        /// </summary>
        /// <param name="device"></param>
        public void StartCam(string device)
        {
            if (SelectedWebcam != null && device.Equals(SelectedWebcam.MonikerString))
            {
                SelectedWebcam.Start();
            }
            else
            {
                SelectedWebcam = new CapDevice("")
                {
                    MonikerString = device
                };
            }
            //register output change eventhandler
            SelectedWebcam.NewBitmapReady += newCamEstablished;
        }

        /// <summary>
        /// stops the current cam and deregister the capture handle methode
        /// </summary>
        public void StopCam()
        {
            SelectedWebcam.Stop();
            SelectedWebcam.NewBitmapReady -= newCamEstablished;
        }

        public bool IsCamRunning()
        {
            return SelectedWebcam != null && SelectedWebcam.IsRunning;
        }

        #region Properties

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
            DependencyProperty.Register("SelectedWebcam", typeof(CapDevice), typeof(WebcamPresentation), new UIPropertyMetadata(null));

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
            typeof(WebcamPresentation), new UIPropertyMetadata("", new PropertyChangedCallback(SelectedWebcamMonikerString_Changed)));

        /// <summary>
        /// Wrapper for the SelectedImages dependency property
        /// </summary>
        public ObservableCollection<BitmapSource> SelectedImages
        {
            get { return (ObservableCollection<BitmapSource>)GetValue(SelectedImagesProperty); }
            set { SetValue(SelectedImagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedImages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedImagesProperty = DependencyProperty.Register("SelectedImages", typeof(ObservableCollection<BitmapSource>),
            typeof(WebcamPresentation), new UIPropertyMetadata(new ObservableCollection<BitmapSource>()));

        #endregion

        #region Methods


        /// <summary>
        /// Invoked when the SelectedWebcamMonikerString dependency property has changed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        private static void SelectedWebcamMonikerString_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        { }

        #endregion
    }
}
