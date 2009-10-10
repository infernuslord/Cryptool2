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
using System.Threading;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Peers;
using Cryptool.FrequencyTest;
using System.Windows.Threading;
namespace Cryptool.FrequencyTest
{
    /// <summary>
    /// Interaction logic for FrequencyTestPresentation.xaml
    /// </summary>
    public partial class FrequencyTestPresentation : UserControl
    {

        public  FrequencyTestPresentation()
        {
           InitializeComponent();
        }


        public void ShowData(DataSource data)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                DataSource source = (DataSource)this.Resources["source"];
                source.ValueCollection.Clear();
                for (int i = 0; i < data.ValueCollection.Count; i++)
                {
                    source.ValueCollection.Add(data.ValueCollection[i]);
                }                
            }, null);
        }


        public void SetHeadline(string text)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                chartHeadline.Text = text;
            }, null);
        }

        public void SetScaler(double value)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                sli.Value = value;
            }, null);
        }

    }
}
