using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace Cryptool.P2PEditor.Converters
{
    public class UtcDateTimeToLocalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var startTimeUtc = DateTime.SpecifyKind((DateTime) value, DateTimeKind.Utc);
            var localTime = startTimeUtc.ToLocalTime();

            if (startTimeUtc == DateTime.MinValue) return "-";

            return localTime.ToString("g", Thread.CurrentThread.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
