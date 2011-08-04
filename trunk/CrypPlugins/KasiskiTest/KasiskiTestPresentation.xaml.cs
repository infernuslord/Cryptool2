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


namespace Cryptool.KasiskiTest
{
    [Cryptool.PluginBase.Attributes.Localization("KasiskiTest.Properties.Resources")]
    public partial class KasiskiTestPresentation: UserControl
    {

        //private KasiskiTest kTest;
        public KasiskiTestPresentation(KasiskiTest KasiskiTest)
        {
            //this.kTest = KasiskiTest;
            InitializeComponent();
            //OpenPresentationFile();
           
        }
        
        public void OpenPresentationFile()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
               
                DataSource source = (DataSource)this.Resources["source"];
                source.ValueCollection.Clear();
                for (int i = 0; i < KasiskiTest.Data.ValueCollection.Count; i++)
                {
                    source.ValueCollection.Add(KasiskiTest.Data.ValueCollection[i]);
                }
                


            }, null);
        }
    }
}
