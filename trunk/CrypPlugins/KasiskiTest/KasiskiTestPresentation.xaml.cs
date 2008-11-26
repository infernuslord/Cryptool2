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
using Cryptool.KasiskiTest;
using System.Windows.Threading;

namespace KasiskiTest
{
    /// <summary>
    /// Interaction logic for KasiskiTestPresentation.xaml
    /// </summary>
    public partial class KasiskiTestPresentation : UserControl
    {
        //public static DataSource source = KasiskiTest.Data ;


        //private FrequencyTest freqT;
        public KasiskiTestPresentation(KasiskiTest KasiskiTest)
        {
            InitializeComponent();
        }

        public void InitializeComponent()
            {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                // if (freqT.StringOutput != null)
                // {
                DataSource source = (DataSource)this.Resources["source"];
                source.ValueCollection.Clear();
                for (int i = 0; i < KasiskiTest.Data.ValueCollection.Count; i++)
                {
                    source.ValueCollection.Add(KasiskiTest.Data.ValueCollection[i]);
                }
                //}


               }, null);
            }
    }
}
