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

namespace WpfApplication1
{
    /// <summary>
    /// Interaktionslogik für UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public Brush this[int i]
        {
            get
            {
                if (!(root.Children == null))
                    return (root.Children[i] as Rectangle).Fill;
                else
                    return null;
            }
            set
            {
                if (!(root.Children == null))
                {
                    (root.Children[i] as Rectangle).Fill = value;
                }
            }
        }

        private Thickness jobMargin = new Thickness(1, 1, 1, 1);
        public Thickness JobMargin 
        { 
            get 
            { 
                return jobMargin; 
            } 
            set 
            {
                if (value.Bottom.Equals(value.Top) && value.Top.Equals(value.Left) && value.Left.Equals(value.Right))
                    jobMargin = value;
                else
                    return;
                foreach(Rectangle element in root.Children)
                {
                    element.Margin = jobMargin;
                }
            } 
        }
        private int count = 1;
        public int JobCount
        {
            get { return count; }
            set
            {
                count = value;
                Rectangle rect;
                for (int i = 0; i < count; i++)
                {
                    rect = new Rectangle();
                    rect.Fill = Brushes.Gray;
                    rect.Margin = JobMargin;
                    rect.Width = dynX(rect.Margin.Left);
                    rect.Height = rect.Width;
                    root.Children.Add(rect);
                }
            }
        }

        public UserControl1()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(UserControl1_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(UserControl1_SizeChanged);
        }

        void UserControl1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (Rectangle element in root.Children)
            {

                element.Width = dynX(element.Margin.Left);
                element.Height = element.Width;
            }
        }

        private double dynX(double margin)
        {
            double x = Math.Sqrt(this.ActualWidth * this.ActualHeight / count) + 2 * margin;
            if (x > 0)
                return x;
            else
                return x -= 2 * margin; 
        }

        void UserControl1_Loaded(object sender, RoutedEventArgs e)
        {
            JobCount = 1;
        }
    }
}