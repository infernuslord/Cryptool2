using System.Linq;
using System.Windows.Documents;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities.Arrows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LatticeCrypto.ViewModels
{
    public class CvpViewModel : SvpGaussViewModel
    {
        public void FindClosestVector(Boolean writeHistory)
        {
            //Anmerkung: Man könnte für das Finden eines Closest Vectors die Methode von Babai benutzen.
            //Hierbei wird der Punkt als Linearkombination mit den reduzierten Vektoren ausgedrückt und am
            //Ende müssen die Skalare auf ganze Zahlen gerundet werden. Da aber dadurch zwischenzeitlich
            //mit Dezimalzahlen gearbeitet werden muss, ist dies hier schwierig, da mit BigInteger gearbeitet
            //wird. Daher wird der einfachere, pragmatischere Weg eingeschlagen: Es wird über die Parallelo-
            //gramme iteriert und schließlich der Eckpunkt gesucht, der dem Punkt am nähsten liegt.
            //Da die Anzahl der Parallelogramme begrenzt ist (z.B. 1.000) geht dies recht schnell.

            if (TargetVectorX == null || TargetVectorY == null)
            {
                closestVector = null;
                closestVectorArrow = null;
                return;
            }

            double targetX = y_line.X1 + (double)TargetVectorX * PixelsPerPoint * scalingFactorX;
            double targetY = x_line.Y1 + (double)TargetVectorY * PixelsPerPoint * scalingFactorY;

            closestVector = new Ellipse
            {
                Width = 10 + PixelsPerPoint / 5,
                Height = 10 + PixelsPerPoint / 5,
                Fill = Brushes.DarkOrange,
                ToolTip = "X = " + TargetVectorX+ ", Y = " + TargetVectorY
            };
            Canvas.SetLeft(closestVector, targetX - closestVector.Width / 2);
            Canvas.SetBottom(closestVector, targetY - closestVector.Height / 2);

            //Vorauswahl
            List<Polygon> listPolygonsSmaller = listPolygons.FindAll(x => (new List<Point>(x.Points)).Exists(y => y.X >= targetX)).FindAll(x => (new List<Point>(x.Points)).Exists(y => y.X <= targetX)).FindAll(x => (new List<Point>(x.Points)).Exists(y => y.Y >= 2 * x_line.Y1 - targetY)).FindAll(x => (new List<Point>(x.Points)).Exists(y => y.Y <= 2 * x_line.Y1 - targetY));
            Point closestVectorPoint = new Point();
            double closestDistance = Double.MaxValue;
            bool vectorFound = false;

            //Suche in den übrig gebliebenen Polygonen
            foreach (Polygon polygon in listPolygonsSmaller.Where(polygon => IsPointInPolygon(polygon.Points, new Point(targetX, canvas.ActualHeight - targetY)) || listPolygonsSmaller.Count <= 1))
            {
                foreach (Point point in polygon.Points)
                {
                    double distance = Math.Sqrt(Math.Pow(point.X - targetX, 2) + Math.Pow(point.Y - (canvas.ActualHeight - targetY), 2));
                    if (distance >= closestDistance) continue;
                    closestDistance = distance;
                    closestVectorPoint = point;
                    vectorFound = true;
                }
                break;
            }

            if (vectorFound)
                closestVectorArrow = new ArrowLine { X1 = closestVectorPoint.X, X2 = targetX, Y1 = closestVectorPoint.Y, Y2 = (canvas.ActualHeight - targetY), Stroke = Brushes.DarkOrange, StrokeThickness = 7, ArrowAngle = 65, ArrowLength = 25 + PixelsPerPoint / 10 };

            if (!writeHistory) return;
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonFindClosestVector + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelTargetPoint)));
            paragraph.Inlines.Add(" {" + TargetVectorX + ", " + TargetVectorY + "}\r\n");
            paragraph.Inlines.Add(new Bold(new Run("Closest Vector:")));
            paragraph.Inlines.Add(" {" + (closestVectorPoint.X - y_line.X1) / pixelsPerPoint + ", " + (closestVectorPoint.Y - x_line.Y1) / pixelsPerPoint + "}\r\n");
            paragraph.Inlines.Add(new Bold(new Run("Closest Distance:")));
            paragraph.Inlines.Add(" " + String.Format("{0:f}", Math.Sqrt(closestDistance / PixelsPerPoint)) + "\r\n");
            History.Document.Blocks.Add(paragraph);
        }

        public bool IsPointInPolygon(PointCollection points, Point point)
        {
            var j = points.Count - 1;
            var oddNodes = false;

            for (var i = 0; i < points.Count; i++)
            {
                if (points[i].Y < point.Y && points[j].Y >= point.Y || points[j].Y < point.Y && points[i].Y >= point.Y)
                {
                    if (points[i].X + (point.Y - points[i].Y) / (points[j].Y - points[i].Y) * (points[j].X - points[i].X) < point.X)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }
    }
}
