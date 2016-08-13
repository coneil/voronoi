using System;

namespace coneil.Math.Voronoi.Geometry
{
    public struct Circle
    {
        public Point Center;
        public double Radius;

        public Circle(double centerX, double centerY, double radius)
        {
            Center = new Point(centerX, centerY);
            Radius = radius;
        }

        public Circle(Point center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public override string ToString()
        {
            return "Circle (center: " + Center + "; radius: " + Radius + ")";
        }
    }
}
