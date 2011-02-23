using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WorkspaceManager.View.Container;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Converter
{
    class ViewStateCaptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PluginViewState state = (PluginViewState)value;
            switch (state)
            {
                case PluginViewState.Data:
                    return "Data";

                case PluginViewState.Log:
                    return "Message Log";

                case PluginViewState.Presentation:
                    return "Presentation";

                case PluginViewState.Setting:
                    return "Settings";

                case PluginViewState.Description:
                    return "Description";
                default:
                    return "...";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
