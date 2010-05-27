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

namespace Cryptool.Plugins.QuadraticSieve
{
    public enum YieldStatus {Ours, OthersNotLoaded, OthersLoaded};

    /// <summary>
    /// Interaction logic for ProgressYields.xaml
    /// </summary>
    public partial class ProgressYields : UserControl
    {
        public YieldStatus this[int i]
        {
            set 
            {
                if (root.Children.Count <= i)   //if no rect exists for this yield yet
                {
                    //Create some rects to fill the gap:
                    for (int c = root.Children.Count; c < i; c++)
                        createYieldRect(c, YieldStatus.OthersNotLoaded);
                    //create the rect:
                    createYieldRect(i, value);
                }
                else
                {
                    SetRectToStatus(root.Children[i] as Rectangle, value);                    
                }
            }
        }

        public void Clear()
        {
            root.Children.Clear();
        }

        private void SetRectToStatus(Rectangle rectangle, YieldStatus value)
        {
            ToolTip tooltip = new ToolTip();
            switch (value)
            {
                case YieldStatus.Ours:
                    rectangle.Fill = Brushes.Green;
                    tooltip.Content = "This yield was sieved by us";
                    break;
                case YieldStatus.OthersLoaded:
                    rectangle.Fill = Brushes.Yellow;
                    tooltip.Content = "This yield was sieved by others but we loaded it";
                    break;
                case YieldStatus.OthersNotLoaded:
                    rectangle.Fill = Brushes.Red;
                    tooltip.Content = "This yield was sieved by others but we didn't loaded it yet";
                    break;
            }
            rectangle.ToolTip = tooltip;
        }

        private void createYieldRect(int c, YieldStatus yieldStatus)
        {
            Rectangle rect = new Rectangle();            
            //rect.Margin = JobMargin;
            rect.Width = 10;
            rect.Height = rect.Width;
            rect.Stroke = Brushes.White;
            rect.StrokeThickness = 0.1;
            SetRectToStatus(rect, yieldStatus);
            root.Children.Add(rect);
        }

        public ProgressYields()
        {
            InitializeComponent();

            root.Children.Clear();
        }
    }
}
