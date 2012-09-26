using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Cryptool.PluginBase;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cryptool.CrypTutorials
{
    public class VideoInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return Title;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    [PluginBase.Attributes.Localization("Cryptool.CrypTutorials.Properties.Resources")]
    public partial class CrypTutorialsPresentation
    {
        private ObservableCollection<VideoInfo> videos = new ObservableCollection<VideoInfo>();
        public ObservableCollection<VideoInfo> Videos { get { return videos; } }

        private readonly List<VideoInfo> _Videos;
        private readonly CrypTutorials _crypTutorials;
        private const string _VideoUrl = "http://localhost/ct2/videos.xml";

        public CrypTutorialsPresentation(CrypTutorials crypTutorials)
        {
            int i = 0;
            while (i != 4)
            {
                videos.Add(new VideoInfo()
                {
                    Description = "From the given data set described below, the first program should produce a training set and a testing set after performing a simple “randomization” of data (see ...",
                    Id = i.ToString(),
                    Timestamp = DateTime.Now,
                    Title = "Standard Date and Time Format Strings",
                    Url = "C:/Code/CT2/CrypPlugins/CrypTutorials/sample_videos/CT2_Tutorium_Beispiel.wmv"
                });
                i++;
            }
            InitializeComponent();
            _crypTutorials = crypTutorials;
            _Videos = LoadVideos();
            this.DataContext = this;
        }

        private List<VideoInfo> LoadVideos()
        {
            try
            {
                var xraw = XElement.Load(_VideoUrl);
                var xroot = XElement.Parse(xraw.ToString());
                if (xroot != null)
                {
                    var links = (from item in xroot.Descendants("video")
                                 let id = item.Element("id")
                                 let title = item.Element("title")
                                 let description = item.Element("description")
                                 let icon = item.Element("icon")
                                 let url = item.Element("url")
                                 let timestamp = item.Element("timestamp")
                                 select new VideoInfo
                                    {
                                        Id = id.Value,
                                        Title = title.Value,
                                        Description = description.Value,
                                        Icon = icon.Value,
                                        Url = url.Value,
                                        Timestamp = new DateTime(long.Parse(timestamp.Value))
                                    });
                    return links.ToList();
                }
            }
            catch (Exception e)
            {
                _crypTutorials.GuiLogMessage(e.Message, NotificationLevel.Error);
            }
            return null;
        }
    }
}
