using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Cryptool.Plugins.NetworkSender
{
    /// <summary>
    /// Interaktionslogik für NetworkInputPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("NetworkSender.Properties.Resources")]
    public partial class NetworkSenderPresentation : UserControl
    {

        private readonly ObservableCollection<PresentationPackage> entries = new ObservableCollection<PresentationPackage>();
        public NetworkSenderPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }
        public void RefreshMetaData(int amountOfSendedPackages)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    amount.Content = amountOfSendedPackages;
                }
                catch { } // dont throw an error in invoke threat
            }), amountOfSendedPackages);
        }

        public void SetStaticMetaData(string starttime, int port)
        {
            var jar = new string[2] {starttime, port.ToString()};
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    startTime.Content = jar[0];
                    lisPort.Content = jar[1];
                }
                catch { } // dont throw an error in invoke threat
            }), jar);
        }

        public void AddPresentationPackage(PresentationPackage package)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Add(package);
                    ListView.ScrollIntoView(ListView.Items[ListView.Items.Count - 1]);
                }
                catch { } // dont throw an error in invoke threat
            }), package);

        }

        public void ClearList()
        {

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Clear();
                }
                catch { }  // dont throw an error in invoke threat
            }), null);
        }


    }
}
