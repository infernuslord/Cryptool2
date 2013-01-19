using System;
using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.NetworkSender
{
    /// <summary>
    /// Interaktionslogik für NetworkInputPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("NetworkSender.Properties.Resources")]
    public partial class NetworkSenderPresentation : UserControl
    {

        private readonly ObservableCollection<PresentationPackage> entries = new ObservableCollection<PresentationPackage>();
        private readonly NetworkInput caller;

        public NetworkSenderPresentation(NetworkInput networkInput)
        {
            InitializeComponent();
            this.DataContext = entries;
            caller = networkInput;
        }
        public void RefreshMetaData(int amountOfSendedPackages)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    amount.Content = amountOfSendedPackages;
                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                } 
            }), null);
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
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                } 
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
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                } 
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
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                } 
            }), null);
        }


    }
}
