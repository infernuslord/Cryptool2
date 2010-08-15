using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WorkspaceManager.View.Container;
using System.Windows;
using System.Windows.Controls;

namespace WorkspaceManager.View.Converter
{
    class OverlapConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PluginViewState state = (PluginViewState)value;
            String caption = (String)parameter;
            if (caption == "PresentationPanel" && state == PluginViewState.Presentation)
                return 1000;

            if (caption == "SettingsPanel" && state == PluginViewState.Setting)
                return 1000;

            if (caption == "LogPanel" && state == PluginViewState.Log)
                return 1000;

            if (caption == "DataPanel" && state == PluginViewState.Data)
                return 1000;

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
