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
        private double _curTime;
        public VideoPlayer()
        {
            DataContext = this;
            InitializeComponent();
            myMediaElement.Volume = 0.5;
            myMediaElement.SpeedRatio = 1;

            myMediaElement.BufferingStarted += myMediaElement_BufferingStarted;
            myMediaElement.BufferingEnded += myMediaElement_BufferingEnded;
            myMediaElement.MediaFailed += myMediaElement_MediaFailed;

            PreviewMouseMove += VideoPlayer_PreviewMouseMove;

            _timer.Tick += delegate(object o, EventArgs args)
            {
                var seSliderValue = (double)myMediaElement.Position.TotalSeconds;
                timelineSlider.Value = seSliderValue;
            };

            _timer2.Tick += delegate(object o, EventArgs args)
            {
                Controls.Visibility = Visibility.Collapsed;
                _timer2.Stop();
            };

        }

        void VideoPlayer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Controls.Visibility = Visibility.Visible;
            _timer2.Start();
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

        private readonly DispatcherTimer _timer2 = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 3) }; 
        private readonly DispatcherTimer _timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) }; 
        private static void OnIsPlaying(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var player = (VideoPlayer)sender;
            if (player.IsPlaying)
            {
                player._timer.Start();
            }
            else{
                player._timer.Stop();
            }
        }

        private static void OnIsActive(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {

        }

        private static void OnUrlChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            try
            {
                var player = (VideoPlayer)sender;
                var uriString = eventArgs.NewValue.ToString();
                player.myMediaElement.Source = new Uri(uriString);
            }
            catch (Exception)
            {
                //wtf ?   
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

        public void Stop()
        {
            myMediaElement.Stop();
            IsPlaying = false;            
        }

        public void Close()
        {
            if (_fullScreen.IsVisible)
            {
                CloseFullscreen();
            }
            myMediaElement.Close();
            IsPlaying = false;
        }

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
            var sliderValue = (int)timelineSlider.Value;

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds.
            // Create a TimeSpan with miliseconds equal to the slider value.
            var ts = TimeSpan.FromSeconds(sliderValue);
            myMediaElement.Position = ts;
        }

        void seek(double time)
        {

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds.
            // Create a TimeSpan with miliseconds equal to the slider value.

            TimeSpan ts = TimeSpan.FromSeconds(time);
            myMediaElement.Position = ts;
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var x = sender as FrameworkElement;
            double value = double.Parse(x.Tag.ToString(), CultureInfo.InvariantCulture);

            if (myMediaElement != null)
            {
                if (myMediaElement.Volume == value)
                {
                    myMediaElement.Volume = 0;
                }
                else
                {
                    myMediaElement.Volume = value;
                }
            }
        }

        private Panel _preMaximizedVisualParent;
        private readonly Window _fullScreen = new Window() { WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize, WindowState = WindowState.Maximized };

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_preMaximizedVisualParent != null)
            {
                CloseFullscreen();
            }
            else
            {
                if (IsPlaying)
                {
                    _curTime = myMediaElement.Position.TotalSeconds;
                    myMediaElement.Stop();
                }

                _preMaximizedVisualParent = (Panel)this.VisualParent;
                _preMaximizedVisualParent.Children.Remove(this);
                _fullScreen.Content = this;
                _fullScreen.Show();

                _fullScreen.ContentRendered += fullScreen_ContentRendered;
            }
        }

        private void CloseFullscreen()
        {
            if (IsPlaying)
            {
                _curTime = myMediaElement.Position.TotalSeconds;
                myMediaElement.Stop();
            }

            _fullScreen.Content = null;
            _fullScreen.Hide();
            _preMaximizedVisualParent.Children.Add(this);
            if (IsPlaying)
            {
                myMediaElement.Play();
                seek(_curTime);
            }
            _preMaximizedVisualParent = null;
        }

        void fullScreen_ContentRendered(object sender, EventArgs e)
        {
            if (IsPlaying)
            {
                myMediaElement.Play();
                seek(_curTime);
            }

            var window = sender as Window;
            if (window != null) window.ContentRendered -= fullScreen_ContentRendered;

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
            var x = (double) value;

            var y = Double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);

            if (x >= y)
            {
                var fromString = ColorConverter.ConvertFromString("#00a8ff");
                if (fromString != null)
                {
                    return new SolidColorBrush((Color) fromString);
                }
            }

            var convertFromString = ColorConverter.ConvertFromString("#ccc");
            return convertFromString != null ? new SolidColorBrush((Color)convertFromString) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}