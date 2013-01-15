using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.NetworkReceiver
{
    /// <summary>
    /// Interaktionslogik für UDPReceiverQuickWatchPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("NetworkReceiver.Properties.Resources")]
    public partial class NetworkReceiverPresentation : UserControl
    {
        private readonly ObservableCollection<PresentationPackage> entries = new ObservableCollection<PresentationPackage>();
        private readonly NetworkReceiver caller;

        public NetworkReceiverPresentation(NetworkReceiver networkReceiver)
        {
            InitializeComponent();
            this.DataContext = entries;
            caller = networkReceiver;
        }

        public void RefreshMetaData(int amountOfReceivedPackages, int amountOfUniqueIps)
        {
            var jar = new int[2]{amountOfReceivedPackages, amountOfUniqueIps};
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>{
              try
              {
                  amount.Content = jar[0];
                  uniqueIP.Content = jar[1];
              }
              catch (Exception e)
              {
                  caller.GuiLogMessage(e.Message,NotificationLevel.Error);
              } 
            }),jar);
        }

        public void SetStaticMetaData(string starttime, string port)
        {
            var jar = new string[2] { starttime, port };
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) (state =>
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
