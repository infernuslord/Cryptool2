using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using NetworkInput.Model;

namespace NetworkInput
{
    /// <summary>
    /// Interaktionslogik für NetworkInputPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("NetworkInput.Properties.Resources")]
    public partial class NetworkInputPresentation : UserControl
    {

        private readonly ObservableCollection<PresentationPackage> entries = new ObservableCollection<PresentationPackage>();
        public NetworkInputPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }
        public void RefreshMetaData(int amountOfReceivedPackages)
        {
            int[] jar = { amountOfReceivedPackages};
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    amount.Content = jar[0];
                }
                catch { } // dont throw an error in invoke threat
            }), jar);
        }

        public void SetStaticMetaData(string starttime)
        {
            string[] jar = { starttime};
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    startTime.Content = jar[0];
                }
                catch { } // dont throw an error in invoke threat
            }), jar);
        }

        public void AddPresentationPackage(PresentationPackage package)
        {
            if (package.Payload.Length > 85) // cut payload if it is too long
                package.Payload = package.Payload.Substring(0, 86) + " ... ";

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Add(package);
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

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
