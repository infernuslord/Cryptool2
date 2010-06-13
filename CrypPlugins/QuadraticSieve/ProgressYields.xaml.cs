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
    /// Interaction logic for ProgressYields.xaml
    /// </summary>
    public partial class ProgressYields : UserControl
    {
        private int ourID;
        private Random random = new Random();
        private Dictionary<int, SolidColorBrush> colorMap = new Dictionary<int, SolidColorBrush>();     //maps user ids to their colors
        private int peerAmount;

        public delegate void AmountOfClientsChangedHandler(int amount);
        public event AmountOfClientsChangedHandler AmountOfClientsChanged;

        public void Set(int i, int id, string name)
        {
            if (root.Children.Count <= i)   //if no rect exists for this yield yet
            {
                //Create some rects to fill the gap:
                for (int c = root.Children.Count; c < i; c++)
                    createYieldRect(c, 0, name);
                //create the rect:
                createYieldRect(i, id, name);
            }
            else
            {
                SetRectToStatus(root.Children[i] as Rectangle, id, name);
            }
        }

        public void Clear()
        {
            root.Children.Clear();
            peerAmount = 0;
        }

        public void setOurID(int id)
        {
            ourID = id;
        }

        private void SetRectToStatus(Rectangle rectangle, int uploaderID, string uploaderName)
        {
            ToolTip tooltip = new ToolTip();
            if (uploaderID == ourID)
            {
                rectangle.Fill = Brushes.Green;
                tooltip.Content = "This yield was sieved by us";
            }
            else if (uploaderID == 0)
            {
                rectangle.Fill = Brushes.Black;
                tooltip.Content = "This yield was sieved by an unknown user and we didn't load it yet";
            }
            else if (uploaderID == -1)
            {
                rectangle.Fill = Brushes.White;     //TODO: Proper representation here                
                tooltip.Content = "This yield got lost";
            }
            else
            {
                rectangle.Fill = GetColor(uploaderID);
                if (uploaderName == null)
                    uploaderName = "other user";
                tooltip.Content = "This yield was sieved by " + uploaderName + " but we loaded it";
            }            
            rectangle.ToolTip = tooltip;
        }

        private Brush GetColor(int uploaderID)
        {
            if (!colorMap.ContainsKey(uploaderID))
            {
                peerAmount++;
                AmountOfClientsChanged(peerAmount);
                SolidColorBrush color = new SolidColorBrush();
                color.Color = GenerateRandomColor();
                colorMap[uploaderID] = color;
                return color;
            }
            else
                return colorMap[uploaderID];
        }

        private Color GenerateRandomColor()
        {
            Color gen;
            bool ok;
            do
            {
                ok = true;
                gen = Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));

                //We want a color that doesn't look too similar to the other colors used:
                if (SimilarColors(gen, Color.FromRgb(0, 0, 0)))             //Check Black similarity
                    ok = false;
                else if (SimilarColors(gen, Color.FromRgb(255, 255, 255)))  //Check White similarity
                    ok = false;
                else                                                        //Check similarity to all other colors
                {
                    foreach (var b in colorMap.Values)
                    {
                        if (SimilarColors(b.Color, Color.FromRgb(255, 255, 255)))
                        {
                            ok = false;
                            break;
                        }
                    }
                }
            } while (!ok);
            return gen;
        }

        private bool SimilarColors(Color gen, Color color)
        {
            const int toleratedDifference = 15;
            int diffR = Math.Abs(gen.R - color.R);
            int diffG = Math.Abs(gen.G - color.G);
            int diffB = Math.Abs(gen.B - color.B);
            return (diffR < toleratedDifference) && (diffG < toleratedDifference) && (diffB < toleratedDifference);
        }

        private void createYieldRect(int c, int id, string name)
        {
            Rectangle rect = new Rectangle();                        
            rect.Width = 10;
            rect.Height = rect.Width;
            rect.Stroke = Brushes.White;
            rect.StrokeThickness = 0.1;
            SetRectToStatus(rect, id, name);
            root.Children.Add(rect);
        }

        public ProgressYields()
        {
            InitializeComponent();
            root.Children.Clear();
        }
    }
}
