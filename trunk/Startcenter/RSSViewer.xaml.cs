using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for RSSViewer.xaml
    /// </summary>
    public partial class RSSViewer : UserControl
    {
        private List<RssItem> _rssItems;
        private const string RSSUrl = "https://www.cryptool.org/trac/CrypTool2/timeline?ticket=on&changeset=on&wiki=on&max=50&authors=&daysback=90&format=rss";

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
            var loadThread = new Thread(ReadAndFillRSSItems);
            InitializeComponent();
            IsUpdating = true;
            loadThread.Start();
        }

        private void ReadAndFillRSSItems()
        {
            _rssItems = ReadRSSItems(RSSUrl);
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                  {
                                                                                      IsUpdating = false;
                                                                                      rssListBox.DataContext = _rssItems;
                                                                                  }, null);
        }

        private List<RssItem> ReadRSSItems(string rssFeedURL)
        {
            var items = from x in XDocument.Load(rssFeedURL).Descendants("channel").Descendants("item")
                        select new RssItem()
                                   {
                                       Title = x.Descendants("title").Single().Value,
                                       Message = x.Descendants("description").Single().Value,
                                       PublishingDate = DateTime.Parse(x.Descendants("pubDate").Single().Value),
                                       URL = x.Descendants("link").Single().Value
                                   };
            return items.ToList();

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
