using System;

namespace coneil.Math.Voronoi.Geometry
{
    public struct Point
    {
        public double X = double.MinValue;
        public double Y = double.MinValue;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Point p)
        {
            return p.X == X && p.Y == Y;
        }

        public bool IsUnassigned()
        {
            return X == double.MinValue && Y == double.MinValue;
        }

        public void Reset()
        {
            X = double.MinValue;
            Y = double.MinValue;
        }

        public static double Distance(Point p0, Point p1)
        {
            var x = p0.X - p1.X;
            var y = p0.Y - p1.Y;
            return System.Math.Sqrt(x * x + y * y);
        }

        public override string ToString()
        {
            return "X: " + X + ", Y: " + Y;
        }
    }
}