using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WorkspaceManager.View.Container;
using System.Windows;

namespace WorkspaceManager.View.Converter
{
    class ViewStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PluginViewState state = (PluginViewState)value;
            String caption = (String)parameter;
            if (caption == "PresentationPanel" && state == PluginViewState.Presentation)
                return Visibility.Visible;

            if (caption == "PresentationOption" && state == PluginViewState.Presentation)
                return Visibility.Collapsed;
            else if (caption == "PresentationOption" && state != PluginViewState.Presentation)
                return Visibility.Visible;

            if (caption == "SettingsPanel" && state == PluginViewState.Setting)
                return Visibility.Visible;

            if (caption == "LogPanel" && state == PluginViewState.Log)
                return Visibility.Visible;

            if (caption == "DataPanel" && state == PluginViewState.Data)
                return Visibility.Visible;

            if(caption == "OptionPanel" && state == PluginViewState.Min)
                return Visibility.Collapsed;
            else if(caption == "OptionPanel" && state != PluginViewState.Min)
                return Visibility.Visible;

            if (caption == null && state == PluginViewState.Min)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
