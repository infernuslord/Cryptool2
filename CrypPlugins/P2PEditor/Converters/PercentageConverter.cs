using System;
using System.Globalization;
using System.Windows.Data;

namespace Cryptool.P2PEditor.Converters
{
    [ValueConversion(typeof (Double), typeof (Double))]
    public class PercentageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            double percentage = Double.Parse((string) parameter);
            double val = (double)value;
            return val*(percentage/100);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}