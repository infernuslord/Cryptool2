using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using WorkspaceManager.View.BinVisual;

namespace WorkspaceManager.View.VisualComponents
{
    public class ModifiedCanvas : Canvas
    {
        protected override Size MeasureOverride(Size constraint)
        {
            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            double maxHeight = 0;
            double maxWidth = 0;
            double left;
            double top;

            foreach (UIElement element in base.InternalChildren)
            {
                if (element != null)
                {
                    element.Measure(availableSize);
                    if (element is BinComponentVisual)
                    {
                        BinComponentVisual b = (BinComponentVisual)element;
                        left = b.Position.X;
                        top = b.Position.Y;
                        left += element.DesiredSize.Width;
                        top += element.DesiredSize.Height;

                        maxWidth = maxWidth < left ? left : maxWidth;
                        maxHeight = maxHeight < top ? top : maxHeight;
                    }
                    else
                    {
                        if (element is CryptoLineView)
                            Canvas.SetZIndex(element, -1);
                        if (element is BinImageVisual)
                            Canvas.SetZIndex(element, -2);
                        left = element.DesiredSize.Width;
                        top = element.DesiredSize.Height;
                        maxWidth = maxWidth < left ? left : maxWidth;
                        maxHeight = maxHeight < top ? top : maxHeight;
                    }
                }
            }
            return new Size { Height = maxHeight, Width = maxWidth };
        }
    }
}
