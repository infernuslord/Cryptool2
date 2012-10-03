using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cryptool.CrypTutorials.Visuals
{
    public class AnimWrapPanel : WrapPanel
    {

        protected override Size ArrangeOverride(Size finalSize)
        {
            int i = 0;
            foreach (UIElement child in Children)
            {
                var c = child as FrameworkElement;
                c.HorizontalAlignment = i % 2 == 0 ? HorizontalAlignment.Left : HorizontalAlignment.Right;
                i++;
            }

            return base.ArrangeOverride(finalSize);
        }
    }
}
