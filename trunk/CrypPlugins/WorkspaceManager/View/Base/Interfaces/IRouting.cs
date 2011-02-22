using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WorkspaceManager.View.Base.Interfaces
{
    interface IRouting
    {
        ObjectSize ObjectSize { get; }
        Point Position { get; set; }
        Point[] RoutingPoints{ get; }
    }

    public class ObjectSize 
    {
        public double X { get; set; }
        public double Y { get; set; }

        public ObjectSize(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
