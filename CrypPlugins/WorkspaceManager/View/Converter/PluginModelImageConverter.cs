using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WorkspaceManager.Model;
using System.Windows.Media;

namespace WorkspaceManager.View.Converter
{
    class PluginModelImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PluginModel model = (PluginModel)value;
            if (model == null)
                return Brushes.LightGreen;
            else
                return new ImageBrush(model.getImage().Source);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
