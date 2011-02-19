using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace WorkspaceManager.View.Base.Interfaces
{
    interface IRouting
    {
        public ObjectSize ObjectSize { get; set; }
        public Point Position { get; private set; }
        public Point[] RoutingPoints { get; }
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
