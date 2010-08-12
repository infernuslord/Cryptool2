using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace WorkspaceManager.View.Converter
{
    class IconPanelVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            switch (visibility)
                {
                    case Visibility.Hidden:
                        visibility = Visibility.Visible;
                        break;
                    case Visibility.Collapsed:
                        visibility = Visibility.Visible;
                        break;
                    case Visibility.Visible:
                        visibility = Visibility.Collapsed;
                        break;
                }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
