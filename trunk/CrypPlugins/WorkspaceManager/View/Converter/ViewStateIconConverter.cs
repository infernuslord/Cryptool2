using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WorkspaceManager.View.Container;
using System.Windows.Media.Imaging;

namespace WorkspaceManager.View.Converter
{
    class ViewStateIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PluginViewState state = (PluginViewState)value;
            BitmapImage image = null;
            if (state == PluginViewState.Min)
            {
                image = new BitmapImage(new Uri("../Image/maxi.png", UriKind.RelativeOrAbsolute));
                return image;
            }
            else
            {
                image = new BitmapImage(new Uri("../Image/minimize.png", UriKind.RelativeOrAbsolute));
                return image;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
