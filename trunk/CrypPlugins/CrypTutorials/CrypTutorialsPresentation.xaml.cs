using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Cryptool.PluginBase;

namespace Cryptool.CrypTutorials
{
    /// <summary>
    /// Interaction logic for CrypTutorialsPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("Cryptool.CrypTutorials.Properties.Resources")]
    public partial class CrypTutorialsPresentation
    {
        private readonly List<VideoInfo> _Videos;
        private readonly CrypTutorials _crypTutorials;
        private const string _VideoUrl = "http://localhost/ct2/videos.xml";

        public CrypTutorialsPresentation(CrypTutorials crypTutorials)
        {
            InitializeComponent();
            _crypTutorials = crypTutorials;
            _Videos = LoadVideos();
            Videos.DataContext = _Videos;
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
                                        Timestamp = timestamp.Value
                                    });
                    return links.ToList();
                }
            }
            catch (Exception e)
            {
                _crypTutorials.GuiLogMessage(e.Message,NotificationLevel.Error);
            }
            return null;
        }
    }
}
