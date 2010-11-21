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
using WorkspaceManager.View.Container;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Collections;
using System.ComponentModel;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for DataPresentation.xaml
    /// </summary>
    public partial class DataPresentation : UserControl
    {
        private ObservableCollection<CollectionElement> valueCollection;
        public ObservableCollection<CollectionElement> ValueCollection
        {
            get { return valueCollection; }
            set { valueCollection = value; }
        }

        public DataPresentation()
        {
            InitializeComponent();
            valueCollection = new ObservableCollection<CollectionElement>();
            listViewLogList.DataContext = ValueCollection;
            DataContext = this;
        }

        public DataPresentation(List<ConnectorView> list)
        {
            InitializeComponent();
            valueCollection = new ObservableCollection<CollectionElement>();
            listViewLogList.DataContext = ValueCollection;
            listViewLogList.SelectionChanged += new SelectionChangedEventHandler(listViewLogList_SelectionChanged);

            foreach (ConnectorView connector in list)
            {
                ValueCollection.Add(new CollectionElement(connector, DataBox));
            }
        }

        void listViewLogList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
                
            IList collection = (IList)e.AddedItems;
            var list = collection.Cast<View.VisualComponents.DataPresentation.CollectionElement>();

            list.First().Start();
            if(list.First().Connector.model.Outgoing)
                Input.Text = "Connector Type: Output";
            else
                Input.Text = "Connector Type: Input";
        }

        public class CollectionElement : ItemsControl
        {
            private String data;
            public String Data { get { return data; } set { Block.Text = value; data = value; } }
            public String Caption { get; set; }
            private DispatcherTimer timer = new DispatcherTimer();
            public ConnectorView Connector { get; set; }
            public TextBlock Block { get; set; }

            public CollectionElement(ConnectorView element, TextBlock textBlock)
            {
                Connector = element;
                this.Block = textBlock;
                Caption = element.Model.ConnectorType.Name;
                timer.Interval = new TimeSpan(0, 0, 5);
                timer.Tick +=new EventHandler(timer_Tick);
            }

            public void Start()
            {
                timer.Start();
                if (Connector.model.Data != null)
                    Data = Connector.model.Data.ToString();
            }

            public void Stop()
            {
                timer.Stop();
            }

            void timer_Tick(object sender, EventArgs e)
            {
                if (Connector.model.Data != null)
                    Data = Connector.model.Data.ToString();
            }
        }
    }
}
