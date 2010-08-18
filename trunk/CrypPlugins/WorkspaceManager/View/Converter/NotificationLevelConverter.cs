using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Cryptool.PluginBase;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WorkspaceManager.View.Converter
{
    class NotificationLevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            NotificationLevel level = (NotificationLevel)value;
            BitmapImage image = null;
            switch (level)
            {
                case NotificationLevel.Error:
                    image = new BitmapImage(new Uri("../Image/error2.png", UriKind.RelativeOrAbsolute));
                    break;
                case NotificationLevel.Warning:
                    image = new BitmapImage(new Uri("../Image/warn.png", UriKind.RelativeOrAbsolute));
                    break;
                case NotificationLevel.Info:
                    image = new BitmapImage(new Uri("../Image/info.png", UriKind.RelativeOrAbsolute));
                    break;
                case NotificationLevel.Debug:
                    image = new BitmapImage(new Uri("../Image/debug.png", UriKind.RelativeOrAbsolute));
                    break;
            }

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
