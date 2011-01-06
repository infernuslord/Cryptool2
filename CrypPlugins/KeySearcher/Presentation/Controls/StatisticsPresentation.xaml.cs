using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

namespace KeySearcherPresentation.Controls
{

    [ValueConversion(typeof(Dictionary<long, Information>), typeof(Double))]
    class InformationToProgressConverter : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (StatisticsPresentation != null)
            {
                double allCount = (StatisticsPresentation.Statistics).Sum(i => i.Value.Sum(j => j.Value.Count));
                double vCount = ((Dictionary<long, Information>)value).Sum(i => i.Value.Count);
                return vCount / allCount;
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
            if (StatisticsPresentation != null)
            {
                double allCount = (StatisticsPresentation.MachineHierarchy).Sum(i => i.Value.Sum);
                double vCount = (int)value;
                return vCount / allCount;
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
            if (StatisticsPresentation != null)
            {
                string key = (string) value;
                var data = (StatisticsPresentation.Statistics)[key];
                return data.Sum(i => i.Value.Count);
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

    [ValueConversion(typeof(Int32), typeof(Double))]
    class InformationToProgressConverter2 : IValueConverter
    {
        public StatisticsPresentation StatisticsPresentation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (StatisticsPresentation != null)
            {
                double allCount = (StatisticsPresentation.Statistics).Sum(i => i.Value.Sum(j => j.Value.Count));
                return (int)value / allCount;
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

    /// <summary>
    /// Interaction logic for StatisticsPresentation.xaml
    /// </summary>
    public partial class StatisticsPresentation : UserControl
    {
        public StatisticsPresentation()
        {
            InitializeComponent();
            ((InformationToProgressConverter)Resources["InformationToProgressConverter"]).StatisticsPresentation = this;
            ((InformationToProgressConverter2)Resources["InformationToProgressConverter2"]).StatisticsPresentation = this;
            ((ChunkSumConverter)Resources["ChunkSumConverter"]).StatisticsPresentation = this;
            ((MachineSumToProgressConverter)Resources["MachineSumToProgressConverter"]).StatisticsPresentation = this;
        }

        private Dictionary<string, Dictionary<long, Information>> statistics = null;
        public Dictionary<string, Dictionary<long, Information>> Statistics
        {
            get { return statistics; }
            set
            {
                statistics = value;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                               {
                                                   var orderedstats = statistics.OrderByDescending((x) => x.Value.Sum((z) => z.Value.Count));
                                                   statisticsTree.DataContext = orderedstats;
                                                   statisticsTree.Items.Refresh();
                                                }, null);

            }
        }

        private Dictionary<long, Maschinfo> machineHierarchy = null;
        public Dictionary<long, Maschinfo> MachineHierarchy
        {
            get { return machineHierarchy; }
            set
            { 
                machineHierarchy = value;
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                {
                                                    var orderedstats = machineHierarchy.OrderByDescending((x) => x.Value.Sum);
                                                    machineTree.DataContext = orderedstats;
                                                    machineTree.Items.Refresh();
                                                }, null);
            }
        }
    }
}
