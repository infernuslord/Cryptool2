using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Cryptool.PluginBase;

namespace Cryptool.P2PEditor.Converters
{
    [ValueConversion(typeof (DisplayLevel), typeof (Visibility))]
    public class IsBeginnerDisplayModeToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a boolean");

            return (DisplayLevel) value == DisplayLevel.Beginner ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}