using System;
using System.Globalization;
using System.Windows.Data;

namespace KeySearcherConverter
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "-";
            }

            var timeSpan = (TimeSpan) value;

            if (timeSpan == new TimeSpan(-1))
                return "~";

            if (timeSpan.Days > 0)
                return string.Format("{0:D2} days, {1:D2}:{2:D2}:{3:D2}", timeSpan.Days, timeSpan.Hours,
                                     timeSpan.Minutes, timeSpan.Seconds);

            return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
