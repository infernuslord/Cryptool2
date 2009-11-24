using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Cryptool.MD5.Presentation.Helpers;
using Cryptool.MD5.Algorithm;

namespace Cryptool.MD5.Presentation.Converters
{
    class MD5StateToUserControlConverter : IValueConverter
    {
        private PresentationControlFactory controlFactory = new PresentationControlFactory();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PresentableMD5State md5State = (PresentableMD5State) value;
            return controlFactory.GetPresentationControlForState(md5State.State);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
