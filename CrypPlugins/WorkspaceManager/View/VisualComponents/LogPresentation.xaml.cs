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

namespace WorkspaceManager.View.VisualComponents
{
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

        public int ErrorCount { get; set; }

        public int WarningCount { get; set; }

        public int DebugCount { get; set; }

        public int InfoCount { get; set; }

        public event EventHandler<LogUpdated> LogUpdated;

        public LogPresentation()
        {
            ErrorCount = 0;
            WarningCount = 0;
            DebugCount = 0;
            InfoCount = 0;
            valueCollection = new ObservableCollection<CollectionElement>();
            DataContext = ValueCollection;
            InitializeComponent();
        }

        public void AddLogList(List<GuiLogEventArgs> list)
        {
            try
            {
                foreach (GuiLogEventArgs element in list)
                {
                    if (element.NotificationLevel == NotificationLevel.Error)
                        ErrorCount++;

                    if (element.NotificationLevel == NotificationLevel.Warning)
                        WarningCount++;

                    if (element.NotificationLevel == NotificationLevel.Info)
                        InfoCount++;

                    if (element.NotificationLevel == NotificationLevel.Debug)
                        DebugCount++;

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
    }
}
