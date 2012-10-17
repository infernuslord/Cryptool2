using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UDPReceiver
{
    /// <summary>
    /// Interaktionslogik für UDPReceiverPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("UDPReceiver.Properties.Resources")]
    public partial class UDPReceiverPresentation : UserControl
    {
        public UDPReceiverPresentation()
        {
            InitializeComponent();
        }

        public void AddPackage(byte[] data)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) (state => listBox.Items.Add(DateTime.Now.ToString("HH:mm:ss:ffff") + "\t" +BitConverter.ToString(data))), data);
        }

        public void ClearList()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)(state => listBox.Items.Clear()), null); 
        }



    }
}
