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
        private readonly List<YouTubeInfo> _youTubeVideos;
        private readonly CrypTutorials _crypTutorials;
        private const string Search = "http://gdata.youtube.com/feeds/api/videos?q={0}&alt=rss&&max-results=20&v=1";

        public CrypTutorialsPresentation(CrypTutorials crypTutorials)
        {
            InitializeComponent();
            _crypTutorials = crypTutorials;
            _youTubeVideos = LoadVideosKey("cryptography");
            YoutubeVideos.DataContext = _youTubeVideos;
        }

        private List<YouTubeInfo> LoadVideosKey(string keyWord)
        {
            try
            {
                var xraw = XElement.Load(string.Format(Search, keyWord));
                var xroot = XElement.Parse(xraw.ToString());
                var xElement = xroot.Element("channel");
                if (xElement != null)
                {
                    var links = (from item in xElement.Descendants("item")
                                 let link = item.Element("link")
                                 let title = item.Element("title")
                                 let description = item.Element("description")
                                 where link != null
                                 select new YouTubeInfo
                                            {
                                                Title = title.Value,
                                                Description = description.Value,
                                                LinkUrl = link.Value,
                                                EmbedUrl = GetEmbedUrlFromLink(link.Value),
                                                ThumbNailUrl = GetThumbNailUrlFromLink(item),                                                
                                            }).Take(20);

                    return links.ToList();
                }
            }
            catch (Exception e)
            {
                _crypTutorials.GuiLogMessage(e.Message,NotificationLevel.Error);
            }
            return null;
        }

        private static string GetEmbedUrlFromLink(string link)
        {
            try
            {
                var embedUrl = link.Substring(0, link.IndexOf("&", StringComparison.Ordinal)).Replace("watch?v=", "embed/");
                return embedUrl;
            }
            catch
            {
                return link;
            }
        }


        private string GetThumbNailUrlFromLink(XElement element)
        {
            var thumbnailUrl = "/CrypTutorials;component/no_preview.png";
            try
            {
                var group = element.Descendants().FirstOrDefault(desc => desc.Name.LocalName == "group");
                if (group != null)
                {
                    foreach (
                        var xAttribute in
                            from desc in @group.Descendants()
                            where desc.Name.LocalName == "thumbnail"
                            select desc.Attribute("url"))
                    {
                        if (xAttribute != null)
                        {
                            var value = xAttribute.Value;
                            thumbnailUrl = value;
                        }
                        break;
                    }
                }
            }
            catch(Exception e)
            {
                _crypTutorials.GuiLogMessage(e.Message, NotificationLevel.Error);
            }
            return thumbnailUrl;
        }

        private void PlayVideoButtonClick(object sender, RoutedEventArgs args)
        {
            try
            {
                var cmd = (Button) sender;
                var youTubeInfo = cmd.DataContext as YouTubeInfo;
                if (youTubeInfo == null)
                {
                    return;
                }
                ScrollViewer.Visibility = Visibility.Hidden;
                WebBrowser.Source = new Uri((youTubeInfo).EmbedUrl);
                WebBrowserGrid.Visibility = Visibility.Visible;
            }
            catch(Exception e)
            {
                _crypTutorials.GuiLogMessage(e.Message, NotificationLevel.Error);
            }
        }

        private void ImageClick(object sender, MouseButtonEventArgs args)
        {
            try
            {
                if(args.ClickCount < 2)
                {
                    return;
                }
                var cmd = (Image)sender;
                var youTubeInfo = cmd.DataContext as YouTubeInfo;
                if (youTubeInfo == null)
                {
                    return;
                }
                ScrollViewer.Visibility = Visibility.Hidden;
                WebBrowser.Source = new Uri((youTubeInfo).EmbedUrl);
                WebBrowserGrid.Visibility = Visibility.Visible;
            }
            catch (Exception e)
            {
                _crypTutorials.GuiLogMessage(e.Message, NotificationLevel.Error);
            }
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            WebBrowser.Source = null;
            ScrollViewer.Visibility = Visibility.Visible;
            WebBrowserGrid.Visibility = Visibility.Hidden;            
        }

        private void WebBrowserOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.N && e.KeyboardDevice.Modifiers == ModifierKeys.Control;
        }

        private void WebBrowserNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri != null && !e.Uri.Authority.Equals("www.youtube.com"))
            {
                e.Cancel = true;
            }
        }

    }
}
