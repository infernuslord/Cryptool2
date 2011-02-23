using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WorkspaceManager.View.Container;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Converter
{
    class BlurViewStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PluginViewState state = (PluginViewState)value;

            if (state == PluginViewState.Min)
                return 15;
            else
                return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
