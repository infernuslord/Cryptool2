using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Cryptool.Plugins.NetworkOutput
{
    /// <summary>
    /// Interaktionslogik für UDPReceiverQuickWatchPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("NetworkOutput.Properties.Resources")]
    public partial class NetworkOutputPresentation : UserControl
    {
        private readonly ObservableCollection<PresentationPackage> entries = new ObservableCollection<PresentationPackage>();

        public NetworkOutputPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }

        public void RefreshMetaData(int amountOfReceivedPackages, int amountOfUniqueIps)
        {
            int[] jar = {amountOfReceivedPackages, amountOfUniqueIps};
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>{
              try
              {
                amount.Content = jar[0];
                uniqueIP.Content = jar[1];
              } catch{} // dont throw an error in invoke threat
            }),jar);
        }

        public void SetStaticMetaData(string starttime, string port)
        {
            string[] jar = { starttime, port};
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) (state =>
                {
                    try
                    {
                        startTime.Content = jar[0];
                        lisPort.Content = jar[1];
                    } catch { } // dont throw an error in invoke threat
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
                } catch { } // dont throw an error in invoke threat
            }), package);
            
        }

        public void ClearList()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Clear(); 
                } catch{}  // dont throw an error in invoke threat
            }), null); 
        }
    }
}
