using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace WorkspaceManager.View.VisualComponents
{
    public enum DirSort
    {
        X_DESC,
        Y_DESC,
        X_ASC,
        Y_ASC
    };

    public class FromTo: INotifyPropertyChanged
    {
        private Point from, to;

        public DirSort DirSort { get; private set; }
        public Point From 
        { 
            get 
            {
                return from;
            } 
            set 
            {
                from = value;
            } 
        }

        public Point To
        {
            get
            {
                return to;
            }
            set
            {
                to = value;
                OnPropertyChanged("To");
            }
        }
        public SortedSet<IntersectPoint> Intersection { get; private set; }

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
            this.Intersection = new SortedSet<IntersectPoint>(new InLineSorter(DirSort));
        }

        public override string ToString()
        {
            return "From"+From.ToString() + " " +"To"+ To.ToString();
        }


        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class InLineSorter : IComparer<IntersectPoint>
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
        public int Compare(IntersectPoint a, IntersectPoint b)
        {
            switch (dirSort)
            {
                case DirSort.Y_ASC:
                    return a.Point.Y.CompareTo(b.Point.Y);
                case DirSort.Y_DESC:
                    return b.Point.Y.CompareTo(a.Point.Y);
                case DirSort.X_ASC:
                    return a.Point.X.CompareTo(b.Point.X);
                case DirSort.X_DESC:
                    return b.Point.X.CompareTo(a.Point.X);
                default:
                    throw new Exception("error");
            }
        }
    }
}
