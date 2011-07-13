using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinTextEditor.xaml
    /// </summary>
    public partial class BinTextEditor : UserControl
    {
        public BinTextEditor()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(BinTextEditor_Loaded);
        }

        void BinTextEditor_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }

    public class RectConvertor : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double height = (double)values[1], width = (double)values[0];
            Rect rect = new Rect(new Point(0,0), new Size(height, width));
            return rect;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class RectConvertorSec : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var text = values[0] as BinTextVisual;
            if (text == null)
                return new Rect(0, 0, 0, 0);
            Rect rect = new Rect(text.Position, new Size(text.WindowWidth, text.WindowHeight));
            return rect;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
