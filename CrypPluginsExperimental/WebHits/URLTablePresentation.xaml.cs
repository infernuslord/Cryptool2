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
using System.Collections.ObjectModel;
using Cryptool.Plugins.WebHits;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace Cryptool.Plugins.WebHits
{
    /// <summary>
    /// Interaktionslogik für URLTablePresentation.xaml
    /// </summary>
    public partial class URLTablePresentation : UserControl
    {
        //List<ResultEntry> urls = new List<ResultEntry>();
        
        ResultEntry selectedURL;
        
        public URLTablePresentation()
        {
            InitializeComponent();                               
        }

        public void Assign_Values(RootObject obj, string searchVal)
        {            
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate { loadURLList(obj, searchVal); }, null);
        }

        //fill the table with urls
        public void loadURLList(RootObject obj, string searchVal)
        {
            //remove old items
            urlsListView.Items.Clear();
            urlsListView.Items.Refresh();
            
            
            //add infomration to the result table
            searchValue.Text = searchVal;
            searchTime.Text = obj.searchInformation.formattedSearchTime + " seconds";
            totalResults.Text = obj.searchInformation.formattedTotalResults;


            //add urls from the first Google page to the result table table
            ResultEntry re;
            int i = 1;
            foreach (Item item in obj.items)
            {
                re = new ResultEntry();
                re.Ranking = i;
                re.HitURL = item.link;
                urlsListView.Items.Add(re);
                i++;
            }
            //foreach (ResultEntry url in urls)
            //{
            //    urlsListView.Items.Add(url);
            //}
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            //doppelClick(sender, eventArgs);
        }

        //start the browser and open the website of the selected url by double click on an url in the table
        private void urlsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (urlsListView.SelectedIndex > -1)
            {
                selectedURL = new ResultEntry();
                selectedURL = (ResultEntry)urlsListView.SelectedItem;
                Process.Start(selectedURL.HitURL);
            }
        }

        private void urlsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //todo
        }
    }
}
