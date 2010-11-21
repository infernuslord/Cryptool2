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
using Cryptool.PluginBase;
using WorkspaceManager.Model;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for LogPresentation.xaml
    /// </summary>
    public partial class LogPresentation : UserControl
    {
        private ObservableCollection<CollectionElement> valueCollection;

        public ObservableCollection<CollectionElement> ValueCollection
        {
            get { return valueCollection; }
            set { valueCollection = value; }
        }

        private List<NotificationLevel> listFilter;

        public int ErrorCount { get; set; }

        public int WarningCount { get; set; }

        public int DebugCount { get; set; }

        public int InfoCount { get; set; }

        public event EventHandler<LogUpdated> LogUpdated;

        public LogPresentation()
        {
            InitializeComponent();
            valueCollection = new ObservableCollection<CollectionElement>();
            listViewLogList.DataContext = ValueCollection;
            DataContext = this;
            listFilter = new List<NotificationLevel>();
            listFilter.Add(NotificationLevel.Info);
            listFilter.Add(NotificationLevel.Warning);
            listFilter.Add(NotificationLevel.Error);
            valueCollection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(valueCollection_CollectionChanged);
        }

        void valueCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ErrorCount = 0;
            WarningCount = 0;
            DebugCount = 0;
            InfoCount = 0;
            foreach (CollectionElement element in ValueCollection)
            {
                if (element.Level == NotificationLevel.Debug)
                    DebugCount++;
                if (element.Level == NotificationLevel.Info)
                    InfoCount++;
                if (element.Level == NotificationLevel.Error)
                    ErrorCount++;
                if (element.Level == NotificationLevel.Warning)
                    WarningCount++;
            }

            textBlockDebugsCount.Text = DebugCount.ToString();
            textBlockErrosCount.Text = ErrorCount.ToString();
            textBlockInfosCount.Text = InfoCount.ToString();
            textBlockWarningsCount.Text = WarningCount.ToString();
            
        }

        public void AddLogList(List<GuiLogEventArgs> list)
        {   
            try
            {
                foreach (GuiLogEventArgs element in list)
                {
                    ValueCollection.Add(new CollectionElement(element));

                    if (this.LogUpdated != null)
                        this.LogUpdated.Invoke(this, new LogUpdated { log = element });
                }
                list.Clear();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }
        }

        private bool FilterCallback(object item)
        {
            return listFilter.Contains(((CollectionElement)item).Level);
        }

        private void buttonError_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Error)) listFilter.Remove(NotificationLevel.Error);
            else listFilter.Add(NotificationLevel.Error);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = "Hide Errors";
                else tb.ToolTip = "Show Errors";
            }
        }

        private void buttonWarning_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Warning)) listFilter.Remove(NotificationLevel.Warning);
            else listFilter.Add(NotificationLevel.Warning);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = "Hide Warnings";
                else tb.ToolTip = "Show Warnings";
            }
        }

        private void buttonInfo_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Info)) listFilter.Remove(NotificationLevel.Info);
            else listFilter.Add(NotificationLevel.Info);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = "Hide Infos";
                else tb.ToolTip = "Show Infos";
            }
        }

        private void buttonDebug_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Debug)) listFilter.Remove(NotificationLevel.Debug);
            else listFilter.Add(NotificationLevel.Debug);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = "Hide Debugs";
                else tb.ToolTip = "Show Debugs";
            }
        }

        private void buttonBalloon_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(listViewLogList.ItemsSource);
            if (listFilter.Contains(NotificationLevel.Balloon)) listFilter.Remove(NotificationLevel.Balloon);
            else listFilter.Add(NotificationLevel.Balloon);
            view.Filter = new Predicate<object>(FilterCallback);

            ToggleButton tb = sender as ToggleButton;
            if (tb != null)
            {
                if (tb.IsChecked == true) tb.ToolTip = "Hide Balloons";
                else tb.ToolTip = "Show Balloons";
            }
        }

        public class CollectionElement
        {
            private GuiLogEventArgs element;

            public String Message { get; set; }
            public NotificationLevel Level { get; set; }
            public String Date { get; set; }
            public String ID { get; set; }

            public CollectionElement(GuiLogEventArgs element)
            {
                Message = element.Message;
                Level = element.NotificationLevel;
                Date = element.DateTime.ToString("dd.MM.yyyy, H:mm:ss");
            }
        }

        private void ButtonDeleteMessages_Click(object sender, RoutedEventArgs e)
        {
            valueCollection.Clear();
        }
    }
}
