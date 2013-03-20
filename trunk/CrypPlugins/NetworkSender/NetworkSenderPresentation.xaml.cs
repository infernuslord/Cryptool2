using System;
using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
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
        private const int MaxStoredPackage = 100;
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
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
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
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
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
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Insert(0, package);

                    //Delets old entries from List if the amount is > 100
                    if (entries.Count > MaxStoredPackage)
                    {
                        entries.RemoveAt(entries.Count-1);
                    }
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

        /// <summary>
        ///  invoke presentation in order to  update the speedrate
        ///  </summary>
        public void UpdateSpeedrate(String Speedrate)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    speedrate.Content = Speedrate;
                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                }
            }), null);
        }

    }
}
