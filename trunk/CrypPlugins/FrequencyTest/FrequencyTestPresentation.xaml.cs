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
        private DataSource source = FrequencyTest.Data ;
       
        

        public  FrequencyTestPresentation()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
               // Log error (including InnerExceptions!)
                //Handle exception
               
            }

            
                       

            FrequencyTest.Data = (DataSource)this.Resources["source"];
            
        }
       
        public void UpdateData(DataSource ds)
        {
            source = ds;
        }        
        
        
    }
}
