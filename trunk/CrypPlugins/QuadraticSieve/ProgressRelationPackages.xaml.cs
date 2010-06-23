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
    /// <summary>
    /// Interaction logic for ProgressRelationPackages.xaml
    /// </summary>
    public partial class ProgressRelationPackages : UserControl
    {
        private int ourID;
        private ScrollViewer scrollViewer;

        public void Set(int i, int id, string name)
        {
            if (root.Children.Count <= i)   //if no shape exists for this relation package yet
            {
                //Create some shapes to fill the gap:
                for (int c = root.Children.Count; c < i; c++)
                    CreateRelationPackageShape(c, 0, name);
                //create the rect:
                CreateRelationPackageShape(i, id, name);
            }
            else
            {
                SetShapeToStatus(i, id, name);
            }
        }

        public void Clear()
        {
            root.Children.Clear();
        }

        public void setOurID(int id)
        {
            ourID = id;
        }

        private void SetShapeToStatus(int index, int uploaderID, string uploaderName)
        {
            ToolTip tooltip = new ToolTip();
            Shape shape = root.Children[index] as Shape;

            if (uploaderID == ourID)
            {
                if (!(shape is Ellipse))
                {
                    root.Children.Remove(shape);
                    shape = GetRelationPackageShape(uploaderID);                    
                    root.Children.Insert(index, shape);
                }
                shape.Fill = GetColor(uploaderID);
                tooltip.Content = "This relation package was sieved by us";
            }
            else if (uploaderID == 0)
            {
                shape.Fill = Brushes.Black;
                tooltip.Content = "This relation package was sieved by an unknown user and we didn't load it yet";
            }
            else if (uploaderID == -1)
            {
                if (!(shape is Path))
                {
                    root.Children.Remove(shape);
                    shape = GetRelationPackageShape(-1);
                    root.Children.Insert(index, shape);
                }
                shape.Fill = Brushes.Black;
                tooltip.Content = "This relation package got lost";
            }
            else
            {
                shape.Fill = GetColor(uploaderID);
                if (uploaderName == null)
                    uploaderName = "other user";
                tooltip.Content = "This relation package was sieved by " + uploaderName + " but we loaded it";
            }            
            shape.ToolTip = tooltip;
        }

        private Brush GetColor(int uploaderID)
        {            
            SolidColorBrush color = new SolidColorBrush();
            Random random = new Random(uploaderID);

            bool ok;
            do
            {
                ok = true;
                color.Color = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

                if (SimilarColors(color.Color, Color.FromRgb(0, 0, 0)))             //Check Black similarity
                    ok = false;
                else if (SimilarColors(color.Color, Color.FromRgb(255, 255, 255)))  //Check White similarity
                    ok = false;

            } while (!ok);

            return color;        
        }

        private bool SimilarColors(Color col1, Color col2)
        {
            const int toleratedDifference = 15;
            int diffR = Math.Abs(col1.R - col2.R);
            int diffG = Math.Abs(col1.G - col2.G);
            int diffB = Math.Abs(col1.B - col2.B);
            return (diffR < toleratedDifference) && (diffG < toleratedDifference) && (diffB < toleratedDifference);
        }

        private void CreateRelationPackageShape(int c, int id, string name)
        {
            Shape shape = GetRelationPackageShape(id);            
            root.Children.Add(shape);
            SetShapeToStatus(root.Children.Count-1, id, name);
            scrollViewer.ScrollToBottom();
        }

        private Shape GetRelationPackageShape(int id)
        {
            Shape shape;
            if (id == ourID)
                shape = new Ellipse();
            else if (id == -1)
                shape = new Path();
            else
                shape = new Rectangle();

            shape.Width = 10;
            shape.Height = shape.Width;
            shape.Stroke = Brushes.White;
            shape.StrokeThickness = 0.1;

            if (id == -1)       //Draw the null shape
            {
                Path path = shape as Path;
                RectangleGeometry rect = new RectangleGeometry();
                rect.Rect = new Rect(0, 0, 10, 10);
                LineGeometry line1 = new LineGeometry();
                line1.StartPoint = new Point(0, 0);
                line1.EndPoint = new Point(shape.Width, shape.Height);
                LineGeometry line2 = new LineGeometry();
                line2.StartPoint = new Point(shape.Width, 0);
                line2.EndPoint = new Point(0, shape.Height);
                GeometryGroup group = new GeometryGroup();
                group.Children.Add(rect);
                group.Children.Add(line1);
                group.Children.Add(line2);
                path.StrokeThickness = 1;
                path.Data = group;
            }

            return shape;
        }

        public ProgressRelationPackages(ScrollViewer scrollViewer)
        {
            InitializeComponent();
            root.Children.Clear();
            this.scrollViewer = scrollViewer;
        }
    }
}
