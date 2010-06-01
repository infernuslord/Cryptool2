using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace WorkspaceManager.View.Converter
{
    public class ConnectorBindingConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double X = System.Convert.ToDouble(values[0]);
            double Y = System.Convert.ToDouble(values[1]);
            double Height = System.Convert.ToDouble(values[2]);
            double Width = System.Convert.ToDouble(values[3]);
            return new Point(X + Width, Y + Height / 2);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
