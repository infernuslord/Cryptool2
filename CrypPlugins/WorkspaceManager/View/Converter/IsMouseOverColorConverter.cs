using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace WorkspaceManager.View.Converter
{
    class IsMouseOverColorConverter : IMultiValueConverter
    {

        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BrushConverter bc = new BrushConverter();
            try
            {
                bool IsMouseOver = (bool)value[0], IsSelected = (bool)value[1];                
                if (!IsSelected)
                {
                    if (IsMouseOver)
                        return (Brush)bc.ConvertFrom("#FFF");
                    else
                        return (Brush)bc.ConvertFrom("#444");
                }
                else
                    return (Brush)bc.ConvertFrom("#FFF");
            }
            catch (Exception)
            {
                return (Brush)bc.ConvertFrom("#444");
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
