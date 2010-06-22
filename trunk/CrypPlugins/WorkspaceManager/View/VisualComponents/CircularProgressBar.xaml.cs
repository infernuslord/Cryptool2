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
using System.Windows.Media.Animation;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for CircularProgressBar.xaml
    /// </summary>
    public partial class CircularProgressBar : UserControl
    {
        private double percentage;
        public double Percentage { 
            set 
            {
                percentage = value;
                SetPieChart(PercentagePath, PercentageArc, percentage);
            }
            get 
            {
                return percentage;
            }
        }

        public CircularProgressBar()
        {
            InitializeComponent();
        }

        private Storyboard setColorinAnimation(Color color)
        {
            Storyboard storyboard = Resources["ColorAnimation"] as Storyboard;
            ColorAnimation coloranimation = storyboard.Children[0] as ColorAnimation;
            coloranimation.To = color;
            return storyboard;
        }

        private void SetPieChart(Path percentagePath, ArcSegment arcSeg, double percentage)
        {
            double percToUse = percentage;
            if (percentage > 1)
                percToUse = percToUse / 100.0;
            if (percToUse < 0.35)
            {
                percentagePath.BeginStoryboard(setColorinAnimation(Colors.Red));
            }
            else if (percToUse >= 0.35 && percToUse < 0.75)
            {
                percentagePath.BeginStoryboard(setColorinAnimation(Colors.Yellow));
            }
            else
            {
                percentagePath.BeginStoryboard(setColorinAnimation(Colors.ForestGreen));
            }
            double angleFromPerc = 360 * percToUse;
            double angleInRadians = (Math.PI / 180) * angleFromPerc;

            Point endpoint = new Point();
            endpoint.X = (arcSeg.Size.Width) * Math.Cos(angleInRadians) + arcSeg.Point.X;
            endpoint.Y = (arcSeg.Size.Height) * Math.Sin(angleInRadians) + arcSeg.Point.Y;

            arcSeg.IsLargeArc = angleFromPerc > 180;
            arcSeg.Point = endpoint;
        }
    }
}
