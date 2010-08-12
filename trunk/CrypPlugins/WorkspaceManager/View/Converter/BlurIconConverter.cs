using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace WorkspaceManager.View.Converter
{
    class BlurIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            double blur = 0;
            switch (visibility)
            {
                case Visibility.Hidden:
                    blur = 0;
                    break;
                case Visibility.Collapsed:
                    blur = 0;
                    break;
                case Visibility.Visible:
                    blur = 10;
                    break;
            }

            return blur;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
