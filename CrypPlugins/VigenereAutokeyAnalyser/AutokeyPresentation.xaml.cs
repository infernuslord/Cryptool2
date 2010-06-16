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

namespace VigenereAutokeyAnalyser
{
    /// <summary>
    /// Interaction logic for AutokeyPresentation.xaml
    /// </summary>
    public partial class AutokeyPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();

        public AutokeyPresentation()
        {
            InitializeComponent();
            SizeChanged += sizeChanged;
            this.DataContext = entries;
            entries.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(entries_CollectionChanged);
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

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {

            updateScaling();
            
            //Console.WriteLine("Width: {0}, Heigth: {1}; GridWith: {2}, GridHeight: {3}", this.Width, this.Height, this.Grid.Width, this.Grid.Height);
            //Console.WriteLine("Actual- Width: {0}, Heigth: {1}; GridWith: {2}, GridHeight: {3}", this.ActualWidth, this.ActualHeight, this.Grid.ActualWidth, this.Grid.ActualHeight);
            //Console.WriteLine("ListViewWidth: {0}, ListViewHeigth: {1}; ActualListViewWith: {2}, ActualListViewHeight: {3}", this.ListView.Width, this.ListView.Height, this.ListView.ActualWidth, this.ListView.ActualHeight);
        }


        private void updateScaling()
        {
            this.scaler.ScaleX = this.ActualWidth / this.Grid.Width;
            this.scaler.ScaleY = this.scaler.ScaleX;
        }
    }
}
