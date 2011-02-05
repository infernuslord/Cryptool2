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
using System.ComponentModel;
using CTWin.Components.Misc;

namespace CTWin.Components
{
    /// <summary>
    /// Interaction logic for CTWinPlugInDragDropItem.xaml
    /// </summary>
    public partial class CTWinPlugInDragDropItem : UserControl
    {
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register(
            "Model",
            typeof(ChildDataModel),
            typeof(CTWinPlugInDragDropItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null));

        [TypeConverter(typeof(ChildDataModel))]
        public ChildDataModel Model
        {
            get { return (ChildDataModel)GetValue(ModelProperty); }
            set
            {
                SetValue(ModelProperty, value);
            }
        }

        public static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.Register(
            "IsPressed",
            typeof(bool),
            typeof(CTWinPlugInDragDropItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, null));

        [TypeConverter(typeof(bool))]
        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set
            {
                SetValue(IsPressedProperty, value);
            }
        }

        public CTWinPlugInDragDropItem()
        {
            InitializeComponent();
        }

        private void MouseLeftButtonDownEvent(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                IsPressed = true;
                return;
            }

            if (e.ButtonState == MouseButtonState.Released)
            {
                IsPressed = false;
                return;
            }
        }
        private void MouseLeaveEvent(object sender, MouseEventArgs e)
        {
            IsPressed = false;
        }
    }
}
