using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cryptool.PluginBase.Attributes;
using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Collections;
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.CrypTutorials
{
    [Localization("Cryptool.CrypTutorials.Properties.Resources")]
    public partial class CrypTutorialsPresentation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //private readonly CrypTutorials _crypTutorials;
        //private const string _VideoUrl = "http://localhost/ct2/videos.xml";
        private readonly TutorialVideosManager _tutorialVideosManager = new TutorialVideosManager();
        private readonly ObservableCollection<VideoInfo> _videos = new ObservableCollection<VideoInfo>();

        private VideoInfo playingItem = null;
        private ListCollectionView _videosView;
        private string _filterString = "";
        public VideoInfo PlayingItem
        {
            get { return playingItem; }
            set
            {
                playingItem = value;
                if (playingItem == null)
                {
                    Player.Visibility = Visibility.Collapsed;
                    Player.Stop();
                }
                else {
                    Player.Visibility = Visibility.Visible;
                    Player.Url = playingItem.Url;
                    Player.PlayOrPause();
                }

                OnPropertyChanged("PlayingItem");
            }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                OnPropertyChanged("FilterString");
            }
        }

        //public CrypTutorialsPresentation(CrypTutorials crypTutorials)
        public CrypTutorialsPresentation()
        {
            DataContext = this;       
            InitializeComponent();
            //_crypTutorials = crypTutorials;
     
            _tutorialVideosManager.OnVideosFetched += _tutorialVideosManager_OnVideosFetched;
            //has to be replaced later on by "GetVideoInformationFromServer"
            //_tutorialVideosManager.GenerateTestData("http://localhost/ct2/videos.xml", 16);
            _tutorialVideosManager.GetVideoInformationFromServer();
            _videosView = CollectionViewSource.GetDefaultView(Videos) as ListCollectionView;
            _videosView.CustomSort = new VideoSorter();
            _videosView.Filter = videoFilter;
        }

        private bool videoFilter(object item)
        {
            VideoInfo customer = item as VideoInfo;
            return customer.Title.Contains(_filterString);
        }

        internal class VideoSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                var vidx = x as VideoInfo;
                var vidY = y as VideoInfo;
                return vidx.Timestamp.CompareTo(vidY.Timestamp);
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public ObservableCollection<VideoInfo> Videos
        {
            get { return _videos; }
        }

        private void _tutorialVideosManager_OnVideosFetched(object sender, VideosFetchedEventArgs videosFetchedEventArgs)
        {
            _videos.Clear();
            Console.Out.WriteLine("lol");
            foreach (var videoInfo in videosFetchedEventArgs.VideoInfos)
            {
                _videos.Add(videoInfo);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (VideoListView.SelectedItem != null)
            {
                PlayingItem = (VideoInfo)VideoListView.SelectedItem;
                //Player.
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            PlayingItem = null;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_videosView != null) 
            {
                FilterString = (sender as TextBox).Text;
                _videosView.Refresh();
            }

        }
    }

    public class RandomMaxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var x = value.ToString();
            var seed = value.GetHashCode();
            var rand = generateRandomNumber(seed);
            return rand;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private int generateRandomNumber(int seed)
        {
            Random random = new Random(seed);
            return random.Next(300, 700);
        }
    }

    public class VideoInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    } 
}