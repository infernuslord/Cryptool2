using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Net;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for RSSViewer.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class RSSViewer : UserControl
    {
        private List<RssItem> _rssItems;
        //private const string RSSUrl = "https://www.cryptool.org/trac/CrypTool2/timeline?ticket=on&changeset=on&wiki=on&max=50&authors=&daysback=90&format=rss";
        private const string RSSUrl = "http://www.facebook.com/feeds/page.php?id=243959195696509&format=rss20";

        public static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.Register("IsUpdating",
                                        typeof(Boolean),
                                        typeof(RSSViewer), new PropertyMetadata(false));

        public Boolean IsUpdating
        {
            get { return (Boolean)GetValue(IsUpdatingProperty); }
            set { SetValue(IsUpdatingProperty, value); }
        }

        public RSSViewer()
        {
            InitializeComponent();
            IsUpdating = true;
            var updateTimer = new Timer(ReadAndFillRSSItems);
            updateTimer.Change(0, 1000*60);
        }

        private void ReadAndFillRSSItems(object state)
        {
            try
            {
                _rssItems = ReadRSSItems(RSSUrl);
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                    {
                                        try
                                        {
                                            IsUpdating = false;
                                            rssListBox.DataContext = _rssItems;
                                        }
                                        catch (Exception)
                                        {
                                            //Uncritical failure: Do nothing
                                        }
                                    }, null);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                    {
                                        IsUpdating = false;
                                        var errorRSSFeed = new List<RssItem>(1);
                                        errorRSSFeed.Add(new RssItem() {Message = Properties.Resources.RSS_error_Message, Title = Properties.Resources.RSS_error_Message});
                                        errorRSSFeed.Add(new RssItem() {Message = ex.Message, Title = Properties.Resources.Exception});
                                        rssListBox.DataContext = errorRSSFeed;
                                    }, null);
            }
        }

        private List<RssItem> ReadRSSItems(string rssFeedURL)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(rssFeedURL);
                req.Method = "GET";
                req.UserAgent = "CrypZilla";

                var rep = req.GetResponse();
                var reader = XmlReader.Create(rep.GetResponseStream());

                var items = from x in XDocument.Load(reader).Descendants("channel").Descendants("item") where !(String.IsNullOrEmpty(x.Descendants("title").Single().Value.Trim()) || String.IsNullOrEmpty(x.Descendants("description").Single().Value.Trim()))
                            select new RssItem()
                                       {
                                           Title = WebUtility.HtmlDecode( x.Descendants("title").Single().Value.Trim() ),
                                           Message = WebUtility.HtmlDecode( x.Descendants("description").Single().Value
                                                                             .Replace("[", "(").Replace("]", ")")
                                                                             .Replace('<', '[').Replace('>', ']').Trim() ),
                                           PublishingDate = DateTime.Parse(x.Descendants("pubDate").Single().Value),
                                           URL = x.Descendants("link").Single().Value.Trim()
                                       };
                return items.ToList();
            }
            catch (Exception)
            {
                return new List<RssItem>();
            }
        }

        private void RSSItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start((string) ((FrameworkElement)sender).Tag);
        }  
    }

    class RssItem
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime PublishingDate { get; set; }
        public string URL { get; set; }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class FalseToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be of Visibility");

            if ((bool)value)
            {
                return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class TrueToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be of Visibility");

            if ((bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
