using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using WorkspaceManager.View.Container;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Converter
{
    public class ConnectorBindingConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ConnectorView connector = (ConnectorView)parameter;
            Point p = connector.GetPositionOnWorkspace();
            double X = p.X;
            double Y = p.Y;
            double Height = System.Convert.ToDouble(values[2]);
            double Width = System.Convert.ToDouble(values[3]);
            switch (connector.Orientation)
            {
                case ConnectorOrientation.West:
                    return new Point(X , Y + Height / 2);
                case ConnectorOrientation.East:
                    return new Point(X + Width, Y + Height / 2);
                case ConnectorOrientation.North:
                    return new Point(X + Width / 2, Y);
                case ConnectorOrientation.South:
                    return new Point(X + Width / 2, Y + Height);
            }

            return new Point(0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
