using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cryptool.PluginBase.Attributes;
using System;
using System.Windows.Data;

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