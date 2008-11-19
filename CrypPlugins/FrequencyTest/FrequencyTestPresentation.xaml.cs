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
namespace Cryptool.FrequencyTest
{
    /// <summary>
    /// Interaction logic for FrequencyTestPresentation.xaml
    /// </summary>
    public partial class FrequencyTestPresentation : UserControl
    {
       // public static DependencyProperty  DataSource = FrequencyTest.Data ;
       
        

        public  FrequencyTestPresentation()
        {
           InitializeComponent();
        }

             public void Refresh(object sender, RoutedEventArgs e)
        {
           
            DataSource source = (DataSource)this.Resources["source"];
            source.ValueCollection.Clear();
            for (int i = 0; i < FrequencyTest.Data.ValueCollection.Count; i++)
            {

                source.ValueCollection.Add(FrequencyTest.Data.ValueCollection[i]);
            }
        }
                       

            
            
       
        
        
    }
}
