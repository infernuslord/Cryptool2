using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WorkspaceManager.View.Container;
using System.Windows;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Converter
{
    class DragDeltaViewStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PluginViewState State = (PluginViewState)value;
            string Target = (string) parameter;

            if (State != PluginViewState.Min)
            {
                if (Target == "IsEnabled")
                    return true;
                if(Target == "Visibility")
                    return Visibility.Visible;
            }
            else
            {
                if (Target == "IsEnabled")
                    return false;
                if (Target == "Visibility")
                    return Visibility.Collapsed;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
