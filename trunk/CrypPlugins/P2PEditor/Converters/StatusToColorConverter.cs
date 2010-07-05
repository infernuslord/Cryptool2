using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Cryptool.P2PEditor.Distributed;

namespace Cryptool.P2PEditor.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Brushes.DarkGray;
            }
            
            var status = (DistributedJobStatus.Status)value;
            switch (status)
            {
                case DistributedJobStatus.Status.New:
                    return Brushes.DeepSkyBlue;
                case DistributedJobStatus.Status.Active:
                    return Brushes.DarkOrange;
                case DistributedJobStatus.Status.Finished:
                    return Brushes.GreenYellow;
            }

            return Brushes.DarkGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
