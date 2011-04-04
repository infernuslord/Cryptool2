using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cryptool.PluginBase.Attributes;
using WorkspaceManager.Model;
using WorkspaceManager.View.VisualComponents;

namespace WorkspaceManager
{
    /// <summary>
    /// Interaction logic for WorkspaceManagerSettingsTab.xaml
    /// </summary>
    [Localization("WorkspaceManager.Properties.Resources")]
    [SettingsTab("WorkspaceManagerSettings", "/MainSettings/", 1.1)]
    public partial class WorkspaceManagerSettingsTab : UserControl
    {
        public WorkspaceManagerSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            InitializeColors();
        }

        private void InitializeColors()
        {
            IntegerColor.Fill = new SolidColorBrush(ColorHelper.IntegerColor);
            ByteColor.Fill = new SolidColorBrush(ColorHelper.ByteColor);
            DoubleColor.Fill = new SolidColorBrush(ColorHelper.DoubleColor);
            BoolColor.Fill = new SolidColorBrush(ColorHelper.BoolColor);
            StreamColor.Fill = new SolidColorBrush(ColorHelper.StreamColor);
            StringColor.Fill = new SolidColorBrush(ColorHelper.StringColor);
            ObjectColor.Fill = new SolidColorBrush(ColorHelper.ObjectColor);
            BigIntegerColor.Fill = new SolidColorBrush(ColorHelper.BigIntegerColor);
            DefaultColor.Fill = new SolidColorBrush(ColorHelper.DefaultColor);

            AsymmetricColor.Fill = new SolidColorBrush(ColorHelper.AsymmetricColor);
            ClassicColor.Fill = new SolidColorBrush(ColorHelper.ClassicColor);
            SymmetricBlockColor.Fill = new SolidColorBrush(ColorHelper.SymmetricBlockColor);
            SymmetricStreamColor.Fill = new SolidColorBrush(ColorHelper.SymmetricStreamColor);
            HybridColor.Fill = new SolidColorBrush(ColorHelper.HybridColor);
            GeneratorColor.Fill = new SolidColorBrush(ColorHelper.GeneratorColor);
            HashColor.Fill = new SolidColorBrush(ColorHelper.HashColor);
            StatisticColor.Fill = new SolidColorBrush(ColorHelper.StatisticColor);
            AnalysisMiscColor.Fill = new SolidColorBrush(ColorHelper.AnalysisMiscColor);
        }
        
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs eventArgs)
        {            
            if (sender is Rectangle)
            {
                eventArgs.Handled = true;
                var rect = (Rectangle) sender;
                var colorPickPopUp = new ColorPickPopUp(rect);
                colorPickPopUp.Placement = PlacementMode.Bottom;
                colorPickPopUp.PlacementTarget = rect;
                colorPickPopUp.IsOpen = true;  
                colorPickPopUp.ColorPickerColorChanged+=new EventHandler<EventArgs>(colorPickPopUp_ColorPickerColorChanged);              
            }
        }

        private void colorPickPopUp_ColorPickerColorChanged(object sender, EventArgs args)
        {
            if(!(sender is Rectangle) || !(((Rectangle)sender).Fill is SolidColorBrush))
            {
                return;
            }

            if (sender == IntegerColor)
            {
                ColorHelper.IntegerColor = ((SolidColorBrush)IntegerColor.Fill).Color;
            }
            else if (sender == ByteColor)
            {
                ColorHelper.ByteColor = ((SolidColorBrush)ByteColor.Fill).Color;
            }
            else if (sender == DoubleColor)
            {
                ColorHelper.DoubleColor = ((SolidColorBrush)DoubleColor.Fill).Color;
            }
            else if (sender == BoolColor)
            {
                ColorHelper.BoolColor = ((SolidColorBrush)BoolColor.Fill).Color;
            }
            else if (sender == StreamColor)
            {
                ColorHelper.StreamColor = ((SolidColorBrush)StreamColor.Fill).Color;
            }
            else if (sender == StringColor)
            {
                ColorHelper.StringColor = ((SolidColorBrush)StringColor.Fill).Color;
            }
            else if (sender == ObjectColor)
            {
                ColorHelper.ObjectColor = ((SolidColorBrush)ObjectColor.Fill).Color;
            }
            else if (sender == BigIntegerColor)
            {
                ColorHelper.BigIntegerColor = ((SolidColorBrush)BigIntegerColor.Fill).Color;
            }
            else if (sender == DefaultColor)
            {
                ColorHelper.DefaultColor = ((SolidColorBrush)DefaultColor.Fill).Color;
            }
            else if (sender == AsymmetricColor)
            {
                ColorHelper.AsymmetricColor = ((SolidColorBrush)AsymmetricColor.Fill).Color;
            }
            else if (sender == ClassicColor)
            {
                ColorHelper.ClassicColor = ((SolidColorBrush)ClassicColor.Fill).Color;
            }
            else if (sender == SymmetricBlockColor)
            {
                ColorHelper.SymmetricBlockColor = ((SolidColorBrush)SymmetricBlockColor.Fill).Color;
            }
            else if (sender == SymmetricStreamColor)
            {
                ColorHelper.SymmetricStreamColor = ((SolidColorBrush)SymmetricStreamColor.Fill).Color;
            }
            else if (sender == HybridColor)
            {
                ColorHelper.HybridColor = ((SolidColorBrush)HybridColor.Fill).Color;
            }
            else if (sender == GeneratorColor)
            {
                ColorHelper.GeneratorColor = ((SolidColorBrush)GeneratorColor.Fill).Color;
            }
            else if (sender == HashColor)
            {
                ColorHelper.HashColor = ((SolidColorBrush)HashColor.Fill).Color;
            }
            else if (sender == StatisticColor)
            {
                ColorHelper.StatisticColor = ((SolidColorBrush)StatisticColor.Fill).Color;
            }
            else if (sender == StatisticColor)
            {
                ColorHelper.AnalysisMiscColor = ((SolidColorBrush)AnalysisMiscColor.Fill).Color;
            }
        }

        private void ResetColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorHelper.SetDefaultColors();
            this.InitializeColors();
        }
    }
}
