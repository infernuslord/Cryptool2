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

namespace WorkspaceManager.View.VisualComponents
{
    public class CollectionElement
    {
        private GuiLogEventArgs element;

        public String Message { get; set; }
        public NotificationLevel Level { get; set; }
        public DateTime Date { get; set; }

        public CollectionElement(GuiLogEventArgs element)
        {
            Message = element.Message;
            Level = element.NotificationLevel;
            Date = element.DateTime;
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

        public LogPresentation()
        {
            valueCollection = new ObservableCollection<CollectionElement>();
            DataContext = ValueCollection;
            InitializeComponent();

            ValueCollection.Add(new CollectionElement(new GuiLogEventArgs("Unterstützen Sie Ihr Team im Firefox Cup . Nutzen Sie das Persona Ihrer Mannschaft, verfolgen Sie die Ergebnisse, bleiben Sie am Ball!", null, NotificationLevel.Error)));
            ValueCollection.Add(new CollectionElement(new GuiLogEventArgs("By matching you with Diggers like you, the Recommendation Engine helps you Digg up the next big thing!", null, NotificationLevel.Error)));
            ValueCollection.Add(new CollectionElement(new GuiLogEventArgs("Zeit, persönlich zu werden. Es gibt tausende völlig freie Möglichkeiten, Ihren Firefox anzupassen, so dass er genau zu dem passt, was Sie im Internet tun möchten.", null, NotificationLevel.Warning)));
        }

        public void AddLogList(List<GuiLogEventArgs> list)
        {
            foreach (GuiLogEventArgs element in list)
            {
                ValueCollection.Add(new CollectionElement(element));
            }

            list.Clear();
        }
    }
}
