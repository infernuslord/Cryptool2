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
using System.Windows.Shapes;
using System.Windows.Threading;
using Cryptool.Core;
using System.ComponentModel;
using System.Threading;

namespace CTWin
{
    public partial class SplashScreen : Window
    {
        public static readonly DependencyProperty ValueProperty =
                    DependencyProperty.Register(
                    "Value",
                    typeof(double),
                    typeof(SplashScreen), new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.AffectsRender, null));

        [TypeConverter(typeof(double))]
        public double Value
        {
            get { return (int)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
            }
        }


        public static readonly DependencyProperty TextProperty =
                    DependencyProperty.Register(
                    "Text",
                    typeof(string),
                    typeof(SplashScreen),
                    new FrameworkPropertyMetadata((string)"N/A", FrameworkPropertyMetadataOptions.AffectsRender, null));

        [TypeConverter(typeof(int))]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public SplashScreen()
        {
            InitializeComponent();
        }

        public void HandleUpdate(PluginLoadedEventArgs args)
        {
            Text = args.AssemblyName;
            Value = args.CurrentPluginNumber == 0 ? 0 : ((double)args.CurrentPluginNumber / (double)args.NumberPluginsFound * 100);
        }
    }

    public class TextValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is string)
                return value;

            if(value is double)
                return value;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
