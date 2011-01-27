using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
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
using KeySearcher;
using KeySearcher.KeyPattern;

namespace KeySearcherPresentation.Controls
{
    /// <summary>
    /// Interaction logic for StatisticsPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class StatisticsPresentation : UserControl
    {
        public StatisticsPresentation()
        {
            InitializeComponent();
            ((InformationToProgressConverter)Resources["InformationToProgressConverter"]).StatisticsPresentation = this;
            ((InformationToProgressConverter2)Resources["InformationToProgressConverter2"]).StatisticsPresentation = this;
            ((ChunkSumConverter)Resources["ChunkSumConverter"]).StatisticsPresentation = this;
            ((StringLengthConverter)Resources["StringLengthConverter"]).StatisticsPresentation = this;
            ((MachineSumToProgressConverter)Resources["MachineSumToProgressConverter"]).StatisticsPresentation = this;
        }

        private Dictionary<string, Dictionary<long, Information>> statistics = null;
        public Dictionary<string, Dictionary<long, Information>> Statistics
        {
            get { return statistics; }
            set
            {
                lock (this)
                {
                    statistics = value;
                }
                if (statistics != null)
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                               {
                                                   if (statistics != null)
                                                   {
                                                       var orderedstats = statistics.OrderByDescending((x) => x.Value.Sum((z) => z.Value.Count));
                                                       statisticsTree.DataContext = orderedstats;
                                                       statisticsTree.Items.Refresh();
                                                   }
                                               }, null);

            }
        }

        private Dictionary<long, Maschinfo> machineHierarchy = null;
        public Dictionary<long, Maschinfo> MachineHierarchy
        {
            get { return machineHierarchy; }
            set
            {
                lock (this)
                {
                    machineHierarchy = value;
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                {
                                                    machineTree.DataContext = machineHierarchy;
                                                    machineTree.Items.Refresh();
                                                }, null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            if(statisticsTree.ItemContainerStyle == null)
            {
                b.Content = "Expand";
                statisticsTree.ItemContainerStyle = this.Resources["ItemStyle2"] as Style;
            }

            if (statisticsTree.ItemContainerStyle.Equals(this.Resources["ItemStyle"] as Style))
            {
                b.Content = "Expand";
                statisticsTree.ItemContainerStyle = this.Resources["ItemStyle2"] as Style;
                return;
            }

            if (statisticsTree.ItemContainerStyle.Equals(this.Resources["ItemStyle2"] as Style))
            {
                b.Content = "Collapse";
                statisticsTree.ItemContainerStyle = this.Resources["ItemStyle"] as Style;
                return;
            }
        }

        #region Informations

        public void UpdateInformation(KeySearcher.KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            if (keySearcher.Pattern == null || !keySearcher.Pattern.testWildcardKey(keySearcherSettings.Key) || keySearcherSettings.ChunkSize == 0)
            {
                return;
            }

            var keyPattern = new KeyPattern(keySearcher.ControlMaster.getKeyPattern()) { WildcardKey = keySearcherSettings.Key };
            var keysPerChunk = Math.Pow(2, keySearcherSettings.ChunkSize);
            var keyPatternPool = new KeyPatternPool(keyPattern, new BigInteger(keysPerChunk));

            if (keyPatternPool.Length > 9999999999)
            {
                TotalAmountOfBlocks.Content = keyPatternPool.Length.ToString().Substring(0, 10) + "...";
            }
            else
            {
                TotalAmountOfBlocks.Content = keyPatternPool.Length;
            }

            TotalAmountOfKeys.Content = new BigInteger(keysPerChunk) * keyPatternPool.Length;


            //Under Construction
            //--------
            TotalBlocksTested.Content = "???";
            TotalKeysTested.Content = "???";
            //--------
        }
        #endregion
    }

    #region Converters
    [ValueConversion(typeof(Dictionary<long, Information>), typeof(Double))]
    class InformationToProgressConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
            {
                lock (StatisticsPresentation)
                {
                    double allCount = (StatisticsPresentation.Statistics).Sum(i => i.Value.Sum(j => j.Value.Count));
                    double vCount = ((Dictionary<long, Information>) value).Sum(i => i.Value.Count);
                    return vCount/allCount;
                }
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Dictionary<long, Information>), typeof(Double))]
    class MachineSumToProgressConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (StatisticsPresentation != null && StatisticsPresentation.MachineHierarchy != null)
            {
                lock (StatisticsPresentation)
                {
                    double allCount = (StatisticsPresentation.MachineHierarchy).Sum(i => i.Value.Sum);
                    double vCount = (int) value;
                    return vCount/allCount;
                }
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(Double))]
    class ChunkSumConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
            {
                lock (StatisticsPresentation)
                {
                    string key = (string) value;
                    var data = (StatisticsPresentation.Statistics)[key];
                    return data.Sum(i => i.Value.Count);
                }
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    class StringLengthConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (StatisticsPresentation != null)
            {
                string name = (string)value;
                
                if(name.Length < 13)
                {
                    return name;
                }
                else
                {
                    return string.Format("{0}...", name.Substring(0, 9));
                }
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Int32), typeof(Double))]
    class InformationToProgressConverter2 : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (StatisticsPresentation != null && StatisticsPresentation.Statistics != null)
            {
                lock (StatisticsPresentation)
                {
                    double allCount = (StatisticsPresentation.Statistics).Sum(i => i.Value.Sum(j => j.Value.Count));
                    return (int) value/allCount;
                }
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToDateConverter : IMultiValueConverter
    {
        public static SolidColorBrush[] AlternationColors = {Brushes.LimeGreen, Brushes.Red, Brushes.Blue};

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime) values[0];
            SolidColorBrush brush = ColorToDateConverter.AlternationColors[(int) values[1]].Clone();
            TimeSpan diff = DateTime.UtcNow.Subtract(date);

            Color color;
            if (diff >= TimeSpan.FromDays(4))
            {
                color = Color.FromArgb(
                    (byte)50,
                    brush.Color.R, 
                    brush.Color.G, 
                    brush.Color.B);

                brush.Color = color;
                return brush;
            }

            if (diff >= TimeSpan.FromDays(3))
            {
                 color = Color.FromArgb(
                    (byte)100,
                    brush.Color.R, 
                    brush.Color.G, 
                    brush.Color.B);

                brush.Color = color;
                return brush;
            }

            if (diff >= TimeSpan.FromDays(2))
            {
                 color = Color.FromArgb(
                    (byte)150,
                    brush.Color.R, 
                    brush.Color.G, 
                    brush.Color.B);

                brush.Color = color;
                return brush;
            }

            if (diff >= TimeSpan.FromDays(1))
            {
                color = Color.FromArgb(
                    (byte)200,
                    brush.Color.R, 
                    brush.Color.G, 
                    brush.Color.B);

                brush.Color = color;
                return brush;
            }

            if (diff >= TimeSpan.FromDays(0))
            {

                color = Color.FromArgb(
                    (byte)255,
                    brush.Color.R,
                    brush.Color.G,
                    brush.Color.B);

                brush.Color = color;
                return brush;
            }
            return Brushes.AntiqueWhite;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

}
