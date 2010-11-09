using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WorkspaceManager.View.Converter
{
    class ConnectorPanelHeightWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double InnerWindow = (double)values[0], Panel1 = (double)values[1];

            if ((string)parameter == "W")
            {
                return InnerWindow - ((Panel1-5) * 1);
            }

            if ((string)parameter == "H")
            {
                return InnerWindow - ((Panel1 - 5) * 1);
            }

            return InnerWindow;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
