using System;
using System.Globalization;
using System.Windows.Data;

namespace KeySearcherConverter
{
    [ValueConversion(typeof (TimeSpan), typeof (string))]
    public class TimeSpanToRoundedStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (!(value is TimeSpan))
            {
                return "-";
            }

            var timeSpan = (TimeSpan) value;
            return String.Format("{0}:{1}:{2}.{3}",
                                 timeSpan.Hours,
                                 timeSpan.Minutes,
                                 timeSpan.Seconds,
                                 Math.Round((decimal) timeSpan.Milliseconds, 3));

        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}