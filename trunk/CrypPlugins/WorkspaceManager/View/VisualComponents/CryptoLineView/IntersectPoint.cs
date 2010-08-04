using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WorkspaceManager.View.VisualComponents
{
    public enum IntersectPointMode
    {
        InnerIntersect,
        NormalIntersect
    };

    public class IntersectPoint
    {
        public Point Point { get; set; }
        public IntersectPointMode Mode { get; set; }

        public IntersectPoint(Point point)
        {
            this.Mode = IntersectPointMode.NormalIntersect;
            this.Point = point;
        }
    }
}
