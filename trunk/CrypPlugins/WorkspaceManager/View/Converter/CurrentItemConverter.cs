using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;

namespace WorkspaceManager.View.Converter
{
    class CurrentItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IList collection = (IList)value;
            var list = collection.Cast<View.VisualComponents.DataPresentation.CollectionElement>();
            if (list.Count() == 0)
                return null;
            return list.First().Data;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
