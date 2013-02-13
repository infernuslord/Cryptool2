/*
    Copyright 2013 Christopher Konze, University of Kassel
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
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
        private int newCount = 0;
        private const int maxMessageCount = 100;

        public NetworkReceiverPresentation(NetworkReceiver networkReceiver)
        {
            InitializeComponent();
            this.DataContext = entries;
            caller = networkReceiver;
        }

        /// <summary>
        /// invoke presentation in order to set the starttime and the port
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="port"></param>
        public void SetStaticMetaData(string starttime, string port)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
                {
                    try
                    {
                        startTime.Content = starttime;
                        lisPort.Content = port;
                    }
                    catch (Exception e)
                    {
                        caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                    } 
                }), null);
        }

        /// <summary>
        ///  invoke presentation in order to add a new packet and refreshed counters
        ///  </summary>
        /// <param name="package"></param>
        /// <param name="amountOfReceivedPackages"></param>
        /// <param name="amountOfUniqueIps"></param>
        public void UpdatePresentation(PresentationPackage package, int amountOfReceivedPackages, int amountOfUniqueIps)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                   entries.Add(package);
                   ListView.ScrollIntoView(ListView.Items[ListView.Items.Count - 1]);
                   amount.Content = amountOfReceivedPackages;
                   uniqueIP.Content = amountOfUniqueIps;
                
                    //Delets entries from List if the amount is > 100
                   if (ListView.DataContext != null)
                   {
                       newCount++;
                       if (newCount >= maxMessageCount)
                       {
                           entries.Clear();
                           newCount = 0;
                       }
                   } 
                 
                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                } 
            }), null);
            
        }

        /// <summary>
        /// clears the packet list and resets packet count and uniueIp count
        /// </summary>
        public void ClearPresentation()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)(state =>
            {
                try
                {
                    entries.Clear();
                    amount.Content = 0;
                    uniqueIP.Content = 0;
                }
                catch (Exception e)
                {
                    caller.GuiLogMessage(e.Message, NotificationLevel.Error);
                } 
            }), null); 
        }
    }
}
