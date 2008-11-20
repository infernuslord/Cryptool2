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
       // public static DependencyProperty  DataSource = FrequencyTest.Data ;


        private FrequencyTest freqT;
        public  FrequencyTestPresentation(FrequencyTest FrequencyTest)
        {
           InitializeComponent();
           this.freqT = FrequencyTest;
           //this.freqT.Settings.PropertyChanged+=Settings_PropertyChanged;
           //freqT.OnPluginProgressChanged
        }
       // void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //{
     // try
     // {
      //  if (e.PropertyName == "StringOutput")
      //  {
          
       //   OpenPresentationFile();
       // }
     // }
     // catch {}
    //}
        public void OpenPresentationFile()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (freqT.StringOutput != null)
                {
                    DataSource source = (DataSource)this.Resources["source"];
                    source.ValueCollection.Clear();
                    for (int i = 0; i < FrequencyTest.Data.ValueCollection.Count; i++)
                    {

                        source.ValueCollection.Add(FrequencyTest.Data.ValueCollection[i]);
                    }
                }
                    
                
            }, null);
        }
    }
}
