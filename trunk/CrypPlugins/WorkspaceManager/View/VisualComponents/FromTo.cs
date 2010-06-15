using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WorkspaceManager.View.VisualComponents
{
    class FromTo
    {
        public Point From { get; private set; }
        public Point To { get; private set; }
        public SortedSet<Point> Intersection { get; private set; }

        public FromTo(Point from, Point to)
        {
            this.From = from;
            this.To = to;
            this.Intersection = new SortedSet<Point>(new InLineSorter(From, To));
        }

    }

    class InLineSorter : IComparer<Point>
    {
        private enum DirSort
        {
            X_DESC,
            Y_DESC,
            X_ASC,
            Y_ASC
        };

        private DirSort dirSort;

        public InLineSorter(Point From, Point To)
        {
            if (From.X == To.X)
            {
                if (From.Y > To.Y)
                    dirSort = DirSort.Y_ASC;
                else
                    dirSort = DirSort.Y_DESC;
            }
            else if (From.Y == To.Y)
            {
                if (From.X > To.X)
                    dirSort = DirSort.X_ASC;
                else
                    dirSort = DirSort.X_DESC;
            }
            else
                throw new Exception("90° only");
        }

        // Returns:
        //     Value               Meaning
        //  Less than zero      a is less than b
        //  Zero                a equals b
        //  Greater than zero   a is greater than b
        public int Compare(Point a, Point b)
        {
            switch (dirSort)
            {
                case DirSort.Y_DESC:
                    return a.Y.CompareTo(b.Y);
                case DirSort.Y_ASC:
                    return b.Y.CompareTo(a.Y);
                case DirSort.X_DESC:
                    return a.X.CompareTo(b.X);
                case DirSort.X_ASC:
                    return b.X.CompareTo(a.X);
                default:
                    throw new Exception("error");
            }
        }
    }
}
