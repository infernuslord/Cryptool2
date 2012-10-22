using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using UDPReceiver.Model;

namespace UDPReceiver
{
    /// <summary>
    /// Interaktionslogik für UDPReceiverQuickWatchPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("UDPReceiver.Properties.Resources")]
    public partial class UDPReceiverQuickWatchPresentation : UserControl
    {
        private ObservableCollection<PresentationPackage> entries = new ObservableCollection<PresentationPackage>();

        public UDPReceiverQuickWatchPresentation()
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

        public void SetStartTime(string s)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) (state =>
                {
                    try
                    {
                        startTime.Content = s;
                    } catch { } // dont throw an error in invoke threat
                }), s);
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
