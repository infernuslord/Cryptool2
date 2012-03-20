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
using Microsoft.Windows.Controls;
using WorkspaceManager.View.Visuals;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for TextEditPanel.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class TextEditPanel : UserControl
    {
        private EditorVisual editor;

        public TextEditPanel()
        {
            DataContextChanged += new DependencyPropertyChangedEventHandler(TextEditPanel_DataContextChanged);
            InitializeComponent();
        }

        void TextEditPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is EditorVisual)
            {
                editor = (EditorVisual)e.NewValue;
                editor.SelectedTextChanged += (x, s) =>
                {
                    if (editor.SelectedText == null)
                        return;

                    //Binding bind = new Binding();
                    //bind.Converter = new ColorToBrushConverter();
                    //bind.Mode = BindingMode.TwoWay;
                    //bind.Source = editor.SelectedText.mainRTB;
                    //bind.Path = new PropertyPath(System.Windows.Controls.RichTextBox.BackgroundProperty);
                    //CrPicker.SetBinding(ColorPicker.SelectedColorProperty, bind);

                    CrPicker.SelectedColor = editor.SelectedText.Color.Color;
                };
            }
        }

        private void CrPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            if (editor.SelectedText == null)
                return;

            editor.SelectedText.Color = new SolidColorBrush(e.NewValue);
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color c;
            SolidColorBrush b;
            if (value is Color)
            {
                c = (Color)value;
                b = new SolidColorBrush(c);
                return b;
            }
            if (value is SolidColorBrush)
            {
                b = (SolidColorBrush)value;
                c = b.Color;
                return c;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            return null;
        }
    }

}
