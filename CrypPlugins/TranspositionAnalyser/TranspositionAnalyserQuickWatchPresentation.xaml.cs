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
using System.Collections.ObjectModel;

namespace TranspositionAnalyser
{
    /// <summary>
    /// Interaktionslogik für TranspositionAnalyserQuickWatchPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("TranspositionAnalyser.Properties.Resources")]
    public partial class TranspositionAnalyserQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        public event EventHandler doppelClick;

        public TranspositionAnalyserQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
         
        }

        

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
               doppelClick(sender,eventArgs);
        }
    }
}
