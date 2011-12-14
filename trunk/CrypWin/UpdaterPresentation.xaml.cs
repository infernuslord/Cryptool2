using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Xml.Linq;
using System.Windows.Threading;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.CrypWin
{
    /// <summary>
    /// Interaction logic for UpdaterPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.CrypWin.Properties.Resources")]
    public partial class UpdaterPresentation : UserControl
    {
        public delegate void RestartClickedHandler();
        public event RestartClickedHandler OnRestartClicked;
        private static UpdaterPresentation singleton = null;

        private List<RssItem> _rssItems;
        private bool rssFilled = false;

        private UpdaterPresentation()
        {
            InitializeComponent();
            Tag = FindResource("NoUpdate");

            if (AssemblyHelper.BuildType == Ct2BuildType.Nightly)
            {
                ChangelogText.Visibility = Visibility.Collapsed;
                ChangelogList.Visibility = Visibility.Visible;
            }
            else
            {
                ChangelogText.Visibility = Visibility.Visible;
                ChangelogList.Visibility = Visibility.Collapsed;
            }
        }

        public static UpdaterPresentation GetSingleton()
        {
            if (singleton == null)
                singleton = new UpdaterPresentation();
            return singleton;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            switch (AutoUpdater.GetSingleton().CurrentState)
            {
                case AutoUpdater.State.Idle:
                    AutoUpdater.GetSingleton().BeginCheckingForUpdates('M');
                    break;
                case AutoUpdater.State.Checking:
                    break;
                case AutoUpdater.State.UpdateAvailable:
                    AutoUpdater.GetSingleton().Download();
                    break;
                case AutoUpdater.State.Downloading:
                    break;
                case AutoUpdater.State.UpdateReady:
                    OnRestartClicked();
                    break;
            }
        }

        public void ReadAndFillRSSChangelog(string rssURL)
        {
            if (rssFilled)
                return;
            
            try
            {
                _rssItems = ReadRSSItems(rssURL);
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        ChangelogList.DataContext = _rssItems;
                        rssFilled = true;
                    }
                    catch (Exception)
                    {
                        //Uncritical failure: Do nothing
                    }
                }, null);
            }
            catch (Exception)
            {
                //Uncritical failure: Do nothing
            }
        }

        public void FillChangelogText(string changelogText)
        {
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        ChangelogText.Html = changelogText;
                    }
                    catch (Exception)
                    {
                        //Uncritical failure: Do nothing
                    }
                }, null);
            }
            catch (Exception)
            {
                //Uncritical failure: Do nothing
            }
        }

        private List<RssItem> ReadRSSItems(string rssFeedURL)
        {
            var items = from x in XDocument.Load(rssFeedURL).Descendants("channel").Descendants("item")
                        select new RssItem()
                        {
                            Title = x.Descendants("title").Single().Value,
                            Message = x.Descendants("description").Single().Value.Replace("[", "(").Replace("]", ")")
                                                                                 .Replace('<', '[').Replace('>', ']'),
                            PublishingDate = DateTime.Parse(x.Descendants("pubDate").Single().Value),
                            URL = x.Descendants("link").Single().Value
                        };
            return items.ToList();
        }
    }

    class RssItem
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime PublishingDate { get; set; }
        public string URL { get; set; }
    }
}
