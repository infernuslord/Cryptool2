using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cryptool.PluginBase.Attributes;
using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;

namespace Cryptool.CrypTutorials
{

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
        private bool isSelected = false;
        public bool IsSelected { 
            get{
                return isSelected;
            }
            set{
                isSelected = value;
                OnPropertyChanged("IsSelected");
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

        public override string ToString()
        {
            return Title;
        }
    }

    [Localization("Cryptool.CrypTutorials.Properties.Resources")]
    public partial class CrypTutorialsPresentation
    {
        //private readonly CrypTutorials _crypTutorials;
        //private const string _VideoUrl = "http://localhost/ct2/videos.xml";
        private readonly TutorialVideosManager _tutorialVideosManager = new TutorialVideosManager();
        private readonly ObservableCollection<VideoInfo> _videos = new ObservableCollection<VideoInfo>();

        //public CrypTutorialsPresentation(CrypTutorials crypTutorials)
        public CrypTutorialsPresentation()
        {            
            InitializeComponent();
            //_crypTutorials = crypTutorials;
            DataContext = this;            
            _tutorialVideosManager.OnVideosFetched += _tutorialVideosManager_OnVideosFetched;
            //has to be replaced later on by "GetVideoInformationFromServer"
            _tutorialVideosManager.GenerateTestData("http://localhost/ct2/videos.xml", 16);
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
            foreach (var videoInfo in videosFetchedEventArgs.VideoInfos)
            {
                _videos.Add(videoInfo);
            }
        }
    }
}