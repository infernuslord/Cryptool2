using System;
using System.Collections.Generic;
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

namespace Cryptool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaction logic for LabelledIntegerDisplay.xaml
    /// </summary>
    public partial class LabelledIntegerDisplay : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(uint), typeof(LabelledIntegerDisplay), null);
        public uint Value { get { return (uint)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }


        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(LabelledIntegerDisplay), null);
        public string Caption { get { return (string)GetValue(CaptionProperty); } set { SetValue(CaptionProperty, value); } }

        public LabelledIntegerDisplay()
        {
            this.InitializeComponent();

            Width = double.NaN;
            Height = double.NaN;
        }
    }
}