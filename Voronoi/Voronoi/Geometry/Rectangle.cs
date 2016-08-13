using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coneil.Math.Voronoi.Geometry
{
    // Flash coord space
    // Orientation in upper left, x increasing right, y increasing down
    public struct Rectangle
    {
        public double X;
        public double Y;

        public double Width;
        public double Height;

        public double Left { get { return X; } }
        public double Right { get { return X + Width; } }
        public double Top { get { return Y; } }
        public double Bottom { get { return Y + Height; } }

        public Rectangle(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
