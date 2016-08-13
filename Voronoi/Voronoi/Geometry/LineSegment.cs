using System;

namespace coneil.Math.Voronoi.Geometry
{
    public struct LineSegment
    {
        public Point P0;
        public Point P1;

        public LineSegment(Point p0, Point p1)
        {
            P0 = p0;
            P1 = p1;
        }

        public bool IsUnassigned()
        {
            return P0.IsUnassigned() && P1.IsUnassigned();
        }
    }
}
