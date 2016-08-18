using System;

namespace coneil.Math.Voronoi.Geometry
{
    public struct Point
    {
        public double X;
        public double Y;
        private bool _initialized;

        public Point(double x, double y)
        {
            X = x;
            Y = y;
            _initialized = true;
        }

        public bool Equals(Point p)
        {
            return p.X == X && p.Y == Y;
        }

        public bool AtInfinity() { return double.IsInfinity(X) || double.IsInfinity(Y); }

        public bool IsUnassigned()
        {
            return !_initialized;
        }

        public void Reset()
        {
            X = double.MinValue;
            Y = double.MinValue;
            _initialized = false;
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