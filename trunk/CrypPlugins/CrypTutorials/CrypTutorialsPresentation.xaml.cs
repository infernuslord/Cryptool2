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
using System.IO;

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
        private readonly ObservableCollection<Category> _categories = new ObservableCollection<Category>();

        private VideoInfo playingItem = null;
        private ListCollectionView _videosView;
        private string _filterString = "";

        private Category selectedCategory;
        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                _videosView.Refresh();
                OnPropertyChanged("SelectedCategory");
            }
        }

        public VideoInfo PlayingItem
        {
            get { return playingItem; }
            set
            {
                playingItem = value;
                if (playingItem == null)
                {
                    Player.Visibility = Visibility.Collapsed;
                    Player.Close();
                }
                else
                {
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
            _tutorialVideosManager.OnCategoriesFetched += new EventHandler<CategoriesFetchedEventArgs>(_tutorialVideosManager_OnCategoriesFetched);
            _tutorialVideosManager.OnVideosFetchErrorOccured += new EventHandler<ErrorEventArgs>(_tutorialVideosManager_OnVideosFetchErrorOccured);
            //has to be replaced later on by "GetVideoInformationFromServer"
            //_tutorialVideosManager.GenerateTestData("http://localhost/ct2/videos.xml", 16);
            _tutorialVideosManager.GetVideoInformationFromServer();
            _videosView = CollectionViewSource.GetDefaultView(Videos) as ListCollectionView;
            _videosView.CustomSort = new VideoSorter();
            _videosView.Filter = videoFilter;
        }

        void _tutorialVideosManager_OnVideosFetchErrorOccured(object sender, ErrorEventArgs e)
        {
            var exception = e.GetException();
            if (exception is System.Net.WebException)
            {
                NetworkErrorPanel.Visibility = Visibility.Visible;
            }
            else 
            {
                XMLParseError.Visibility = Visibility.Visible;
            }
        }


        private Dictionary<int, Category> catMap = new Dictionary<int,Category>();


        void makeHashMap(List<Category> cats)
        {
            if (cats.Count == 0)
                return;

            foreach (var cat in cats)
            {
                catMap.Add(cat.Id, cat);
                makeHashMap(cat.Children);
            }
        }

        void _tutorialVideosManager_OnCategoriesFetched(object sender, CategoriesFetchedEventArgs e)
        {
            _categories.Clear();
            _categories.Add(new Category() { Id = int.MaxValue, Name = "All"});
            _categories.Add(new Category() { Id = int.MinValue, Name = "Misc" });
            foreach (var cat in e.Categories)
            {
                _categories.Add(cat);
            }
            makeHashMap(new List<Category>(_categories));
        }


        void findAllVideos(List<Category> cats, List<Category> allCats)
        {
            if (cats.Count == 0)
                return;

            foreach(var cat in cats)
            {
                allCats.AddRange(cats);
                findAllVideos(cat.Children, allCats);
            }
        }

        private bool videoFilter(object item)
        {
            var videoinfo = item as VideoInfo;
            if (_filterString != string.Empty)
            {
                return videoinfo.Title.Contains(_filterString);
            }

            if (SelectedCategory != null)
            {
                if (SelectedCategory.Id == int.MaxValue)
                {
                    return true;
                }
            }

            if (SelectedCategory != null)
            {
                var list = new List<Category>();
                list.Add(selectedCategory);
                findAllVideos(SelectedCategory.Children, list);
                return list.Contains(catMap[videoinfo.Category]);
            }
            return true;
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

        public ObservableCollection<Category> Categories
        {
            get { return _categories; }
        }

        private void _tutorialVideosManager_OnVideosFetched(object sender, VideosFetchedEventArgs videosFetchedEventArgs)
        {
            _videos.Clear();
            foreach (var videoInfo in videosFetchedEventArgs.VideoInfos)
            {
                _videos.Add(videoInfo);

                Category cat;
                if (catMap.TryGetValue(videoInfo.Category, out cat))
                    cat.Count++;
                else {
                    videoInfo.Category = int.MinValue;
                    catMap[int.MinValue].Count++;
                }
 
                catMap[int.MaxValue].Count++;
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


        private Category searchCat = new Category() { Name = "Search" };
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (sender as TextBox).Text;
            if (_videosView != null) 
            {
                FilterString = text;
                if (text == string.Empty)
                {
                    SelectedCategory = viewsTreeView.SelectedItem as Category;
                    return;
                }
    
                _videosView.Refresh();
            }

        }

        private void viewsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var cat = e.NewValue as Category;
            if(cat == null)
                return;

            SelectedCategory = cat;
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
        public int Category { get; set; }
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

    public class ChildrenCountEventArgs : EventArgs
    {
        public int count { get; set; } 
    }

    public class Category : INotifyPropertyChanged
    {
        public Category Parent { get; set; }
        public event EventHandler<ChildrenCountEventArgs> ChildrenCountChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public string Name { get; set; }
        private List<Category> children;
        public List<Category> Children 
        {
            get { return children; }
            set 
            {
                children = value;
            } 
        }


        private int count = 0;
        private void incrementCount(Category parent)
        {
            if (Parent == null)
                return;

            parent.Count++;
        }

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                incrementCount(Parent);
                OnPropertyChanged("Count");
            }
        }

        public Category()
        {
            Children = new List<Category>();
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
            return Name;
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