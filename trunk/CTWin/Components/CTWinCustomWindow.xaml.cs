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
using System.Windows.Markup;
using System.ComponentModel;

namespace CTWin.Components
{
    /// <summary>
    /// Interaction logic for CTWinCustomWindow.xaml
    /// </summary>
    public partial class CTWinCustomWindow : UserControl
    {
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
            "Description",
            typeof(string),
            typeof(CTWinCustomWindow),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        public static readonly DependencyProperty IsSmallProperty =
            DependencyProperty.Register(
            "IsSmall",
            typeof(bool),
            typeof(CTWinCustomWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, null));

        [TypeConverter(typeof(bool))]
        public bool IsSmall
        {
            get { return (bool)GetValue(IsSmallProperty); }
            set
            {
                SetValue(IsSmallProperty, value);
            }
        }

        public CTWinCustomWindow()
        {
            InitializeComponent();
        }

        #region Callbacks

        #endregion
    }
}
