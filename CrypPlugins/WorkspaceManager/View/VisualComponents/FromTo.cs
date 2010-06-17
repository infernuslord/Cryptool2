using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WorkspaceManager.View.VisualComponents
{
    enum DirSort
    {
        X_DESC,
        Y_DESC,
        X_ASC,
        Y_ASC
    };

    class FromTo
    {
        public DirSort DirSort { get; private set; }
        public Point From { get; private set; }
        public Point To { get; private set; }
        public SortedSet<Point> Intersection { get; private set; }

        public FromTo(Point from, Point to)
        {
            this.From = from;
            this.To = to;
            if (From.X == To.X)
            {
                if (From.Y > To.Y)
                    DirSort = DirSort.Y_DESC;
                else
                    DirSort = DirSort.Y_ASC;
            }
            else if (From.Y == To.Y)
            {
                if (From.X > To.X)
                    DirSort = DirSort.X_DESC;
                else
                    DirSort = DirSort.X_ASC;
            }
            else
                throw new Exception("90° only");
            this.Intersection = new SortedSet<Point>(new InLineSorter(DirSort));
        }

        public override string ToString()
        {
            return "From"+From.ToString() + " " +"To"+ To.ToString();
        }

    }

    class InLineSorter : IComparer<Point>
    {
        private DirSort dirSort;

        public InLineSorter(DirSort dirsort)
        {
            this.dirSort = dirsort;
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
                case DirSort.Y_ASC:
                    return a.Y.CompareTo(b.Y);
                case DirSort.Y_DESC:
                    return b.Y.CompareTo(a.Y);
                case DirSort.X_ASC:
                    return a.X.CompareTo(b.X);
                case DirSort.X_DESC:
                    return b.X.CompareTo(a.X);
                default:
                    throw new Exception("error");
            }
        }
    }
}
