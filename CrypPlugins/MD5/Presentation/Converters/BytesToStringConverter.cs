using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Cryptool.MD5.Presentation.Converters
{
    class BytesToStringConverter : IValueConverter
    {
        #region IValueConverter Member

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte[] byteArray = (byte[])value;
            if (byteArray == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
