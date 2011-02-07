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
                                                       statisticsTree.DataContext = statistics;
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
                                                    if (machineHierarchy != null)
                                                    {
                                                        machineTree.DataContext = machineHierarchy;
                                                        machineTree.Items.Refresh();
                                                    }
                                                }, null);
            }
        }

        #region Information

        private string days = "??? Days";
        public string Days
        {
            get { return days; }
            set
            {
                lock (this)
                {
                    days = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    WorkingDays.Content = days;
                }, null);
            }
        }

        private BigInteger totalBlocks = 0;
        public BigInteger TotalBlocks
        {
            get { return totalBlocks; }
            set
            {
                lock (this)
                {
                    totalBlocks = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    TotalAmountOfBlocks.Content = totalBlocks;
                }, null);
            }
        }

        private BigInteger calculatedBlocks = 0;
        public BigInteger CalculatedBlocks
        {
            get { return calculatedBlocks; }
            set
            {
                lock (this)
                {
                    calculatedBlocks = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    TotalBlocksTested.Content = calculatedBlocks;
                }, null);
            }
        }

        private BigInteger totalKeys = 0;
        public BigInteger TotalKeys
        {
            get { return totalKeys; }
            set
            {
                lock (this)
                {
                    totalKeys = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    TotalAmountOfKeys.Content = totalKeys;
                }, null);
            }
        }

        private BigInteger calculatedKeys = 0;
        public BigInteger CalculatedKeys
        {
            get { return calculatedKeys; }
            set
            {
                lock (this)
                {
                    calculatedKeys = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    TotalKeysTested.Content = calculatedKeys;
                }, null);
            }
        }

        private double percent = 0;
        public double Percent
        {
            get { return percent; }
            set
            {
                lock (this)
                {
                    if (totalBlocks != 0)
                    {
                        percent = Math.Round((value/(double) totalBlocks)*Math.Pow(10, totalKeys.ToString().Length)) / Math.Pow(10, totalKeys.ToString().Length-2);
                    }
                    else
                    {
                        percent = 0;
                    }
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                     PercentsComplete.Content = string.Format("{0:g} %", percent);
                }, null);
            }
        }

        private BigInteger users = 1;
        public BigInteger Users
        {
            get { return users; }
            set
            {
                lock (this)
                {
                    users = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    UserCount.Content = users + " users are working on this job";
                }, null);
            }
        }

        private string beeusers = "-";
        public string BeeUsers
        {
            get { return beeusers; }
            set
            {
                lock (this)
                {
                    beeusers = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    BestUser.Content = " Top user: " + beeusers;
                }, null);
            }
        }

        private BigInteger machines = 1;
        public BigInteger Machines
        {
            get { return machines; }
            set
            {
                lock (this)
                {
                    machines = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    MachineCount.Content = machines + " machines are working on this job";
                }, null);
            }
        }

        private string beemachines = "-";
        public string BeeMachines
        {
            get { return beemachines; }
            set
            {
                lock (this)
                {
                    beemachines = value;
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    BestMachine.Content = " Top machine: " + beemachines;
                }, null);
            }
        }

        private double rate = 0;
        public double SetRate
        {
            get { return percent; }
            set
            {
                lock (this)
                {
                    if (false)
                    {
                        rate = 0;
                    }
                    else
                    {
                        rate = 0;
                    }
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Rate.Content = " Overall rate: " + rate + " key/sec";
                }, null);
            }
        }

        #endregion

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

        private QuickWatch ParentQuickWatch
        {
            get { return (QuickWatch) ((Grid) ((Grid) Parent).Parent).Parent; }
        }

        private void SwitchView(object sender, RoutedEventArgs e)
        {
            ParentQuickWatch.ShowStatistics = false;
        }
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
