using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Globalization;
using System.Windows.Threading;

namespace Cryptool.CrypTutorials
{

    public partial class VideoPlayer : UserControl
    {
        private double curTime = 0;
        public VideoPlayer()
        {
            this.DataContext = this;
            InitializeComponent();
            myMediaElement.Volume = (double)0.5;
            myMediaElement.SpeedRatio = (double)1;

            myMediaElement.BufferingStarted += new RoutedEventHandler(myMediaElement_BufferingStarted);
            myMediaElement.BufferingEnded += new RoutedEventHandler(myMediaElement_BufferingEnded);
            myMediaElement.MediaFailed += new EventHandler<ExceptionRoutedEventArgs>(myMediaElement_MediaFailed);


            timer.Tick += delegate(object o, EventArgs args)
            {
                double seSliderValue = (double)myMediaElement.Position.TotalSeconds;
                timelineSlider.Value  = seSliderValue;
            };

        }

        void myMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            
        }

        void myMediaElement_BufferingEnded(object sender, RoutedEventArgs e)
        {
            LoadingVisual.Visibility = Visibility.Collapsed;
        }

        void myMediaElement_BufferingStarted(object sender, RoutedEventArgs e)
        {
            LoadingVisual.Visibility = Visibility.Visible;
        }

        public static readonly DependencyProperty UrlProperty =
        DependencyProperty.Register("Url", typeof(string),
        typeof(VideoPlayer), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnUrlChanged));

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool),
            typeof(VideoPlayer), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnIsActive));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty =
    DependencyProperty.Register("IsPlaying", typeof(bool),
    typeof(VideoPlayer), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnIsPlaying));

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }


        private DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) }; 
        private static void OnIsPlaying(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            VideoPlayer player = (VideoPlayer)sender;
            if (player.IsPlaying)
                player.timer.Start();
            else
                player.timer.Stop();
        }

        private static void OnIsActive(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {

        }

        private static void OnUrlChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            try
            {
                VideoPlayer player = (VideoPlayer)sender;
                string uriString = eventArgs.NewValue.ToString();
                player.myMediaElement.Source = new Uri(uriString);
            }
            catch (Exception e)
            {
                
            }
        }

        public void PlayOrPause()
        {
            if (IsPlaying)
            {
                myMediaElement.Pause();
                IsPlaying = false;
            }
            else
            {
                myMediaElement.Play();
                IsPlaying = true;
            }
        }


        // Play the media.
        void PlayClick(object sender, RoutedEventArgs args)
        {
            // The Play method will begin the media if it is not currently active or 
            // resume media if it is paused. This has no effect if the media is
            // already running.

            PlayOrPause();

        }

        // Pause the media.
        void PauseClick(object sender, RoutedEventArgs args)
        {

            // The Pause method pauses the media if it is currently running.
            // The Play method can be used to resume.
            myMediaElement.Pause();

        }

        // Stop the media.
        void StopClick(object sender, RoutedEventArgs args)
        {

            // The Stop method stops and resets the media to be played from
            // the beginning.
            Stop();

        }

        public void Stop()
        {
            myMediaElement.Stop();
            IsPlaying = false;
        }

        public void Close()
        {
            myMediaElement.Close();
            IsPlaying = false;
        }

        // Change the volume of the media.
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            //myMediaElement.Volume = (double)volumeSlider.Value;
        }

        //// Change the speed of the media.
        //private void ChangeMediaSpeedRatio(object sender, RoutedPropertyChangedEventArgs<double> args)
        //{
        //    myMediaElement.SpeedRatio = (double)speedRatioSlider.Value;
        //}

        // When the media opens, initialize the "Seek To" slider maximum value
        // to the total number of miliseconds in the length of the media clip.
        private void Element_MediaOpened(object sender, EventArgs e)
        {
            timelineSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
        }

        // When the media playback is finished. Stop() the media to seek to media start.
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            myMediaElement.Stop();
        }

        void seek()
        {
            int SliderValue = (int)timelineSlider.Value;

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds.
            // Create a TimeSpan with miliseconds equal to the slider value.
            TimeSpan ts = TimeSpan.FromSeconds(SliderValue);
            myMediaElement.Position = ts;
        }

        void seek(double time)
        {

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds.
            // Create a TimeSpan with miliseconds equal to the slider value.

            TimeSpan ts = TimeSpan.FromSeconds(time);
            myMediaElement.Position = ts;
        }

        // Jump to different parts of the media (seek to). 
        private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            seek();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var x = sender as FrameworkElement;
            double value = double.Parse(x.Tag.ToString(), CultureInfo.InvariantCulture);

            if (myMediaElement.Volume == value)
                myMediaElement.Volume = 0;
            else
                myMediaElement.Volume = value;
        }

        private Panel preMaximizedVisualParent;
        private Window fullScreen = new Window() { WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize, WindowState = WindowState.Maximized };

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (preMaximizedVisualParent != null)
            {
                if (IsPlaying) 
                {
                    curTime = myMediaElement.Position.TotalSeconds;
                    myMediaElement.Stop();
                }


                fullScreen.Content = null;
                fullScreen.Hide();
                preMaximizedVisualParent.Children.Add(this);
                if (IsPlaying)
                {
                    myMediaElement.Play();
                    seek(curTime);
                }
                preMaximizedVisualParent = null;
            }
            else
            {
                if (IsPlaying)
                {
                    curTime = myMediaElement.Position.TotalSeconds;
                    myMediaElement.Stop();
                }

                preMaximizedVisualParent = (Panel)this.VisualParent;
                preMaximizedVisualParent.Children.Remove(this);
                fullScreen.Content = this;
                fullScreen.Show();

                fullScreen.ContentRendered += new EventHandler(fullScreen_ContentRendered);
            }
        }

        void fullScreen_ContentRendered(object sender, EventArgs e)
        {
            if (IsPlaying)
            {
                myMediaElement.Play();
                seek(curTime);
            }

            (sender as Window).ContentRendered -= fullScreen_ContentRendered;
        }

        private void SeekToMediaPosition(object sender, MouseButtonEventArgs e)
        {
            seek();
        }
    }

    public class VolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var x = (double)value;

            var y = Double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);

            if (x >= y)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00a8ff"));
            else
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ccc"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}