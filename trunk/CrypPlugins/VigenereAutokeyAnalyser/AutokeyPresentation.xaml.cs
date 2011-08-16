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
using System.Windows.Threading;
using System.Threading;

namespace VigenereAutokeyAnalyser
{
    /// <summary>
    /// Interaction logic for AutokeyPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("VigenereAutokeyAnalyser.Properties.Resources")]
    public partial class AutokeyPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        public event MouseButtonEventHandler SelectedIndexChanged; 

        public AutokeyPresentation()
        {
            InitializeComponent();
            SizeChanged += new SizeChangedEventHandler(AutokeyPresentation_SizeChanged);
            ListView.MouseDoubleClick += new MouseButtonEventHandler(ListView_MouseDoubleClick);
            this.DataContext = entries;
            entries.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(entries_CollectionChanged);
        }

        void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedIndexChanged != null)
            {
                SelectedIndexChanged(sender, e);
            }
        }

        private void AutokeyPresentation_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            updateScaling();
        }

        private void entries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

           

            if (e.NewItems != null)
            {
                foreach (ResultEntry entry in e.NewItems)
                {

                    // measure length of text in pixels
                    FormattedText text = new FormattedText(
                            entry.Key + "XXX",
                            System.Globalization.CultureInfo.GetCultureInfo("en-US"),
                            FlowDirection.LeftToRight,
                            new Typeface(ListView.FontFamily, ListView.FontStyle, ListView.FontWeight, ListView.FontStretch),
                            ListView.FontSize,
                            Brushes.Black);

                    if (text.Width > this.KeyCol.Width)
                    {
                        this.KeyCol.Width = text.Width;
                        this.Grid.Width = this.KeyCol.Width + this.ICCol.Width;
                        updateScaling();            
                    }
                }
            }
            else
            {

                // reset the view - if we get null the list was cleared; is this always the case?
                this.KeyCol.Width = 90; // fixed initial value
                this.Grid.Width = this.KeyCol.Width + this.ICCol.Width;
                updateScaling();
            }
            
        }

        private void updateScaling()
        {
            this.scaler.ScaleX = this.ActualWidth / this.Grid.Width;
            this.scaler.ScaleY = this.scaler.ScaleX;
        }

        public void selectIndex(int index)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ListView.SelectedIndex = index;   

            }, null);

            
        }

        public void Add(ResultEntry item)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                entries.Add(item);

            }, null);
           
        }

        public void Clear()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                entries.Clear();
            
            }, null);
        }
       
        
    }
}
