using System;
using System.Collections.Generic;

namespace coneil.Math.Voronoi.Geometry
{
    public enum Winding { NONE, CLOCKWISE, COUNTERCLOCKWISE }

    public sealed class Polygon
    {
        public double Area { get; private set; }

        public Winding Winding { get; private set; }

        public Polygon(List<Point> vertices)
        {
            Process(vertices.ToArray());
        }

        public Polygon(Point[] vertices)
        {
            Process(vertices);
        }

        void Process(Point[] vertices)
        {
            double signedDouble = GetSignedDoubleArea(vertices);

            Area = System.Math.Abs(signedDouble * 0.5f);

            if(signedDouble < 0)
                Winding = Geometry.Winding.CLOCKWISE;
            else if(signedDouble > 0)
                Winding = Geometry.Winding.COUNTERCLOCKWISE;
            else
                Winding = Geometry.Winding.NONE;
        }

        double GetSignedDoubleArea(Point[] vertices)
        {
            double result = 0;
            int count = vertices.Length;
            for(int i = 0; i < count; i++)
            {
                int nextIndex = (i + 1) % count;
                Point p = vertices[i];
                Point next = vertices[nextIndex];
                result += p.X * next.Y - next.X * p.Y;
            }
            return result;
        }
    }
}
