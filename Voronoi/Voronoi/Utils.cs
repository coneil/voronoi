using System;
using System.Collections.Generic;
using System.Linq;
using coneil.Math.Voronoi.Geometry;

namespace coneil.Math.Voronoi
{
    internal sealed static class Utils
    {
        public static List<LineSegment> GetDelaunayLinesForEdges(List<Edge> edges)
        {
            return edges.Select(x => x.DelaunayLine).ToList();
        }

        public static List<Edge> SelectEdgesForSitePoint(Point coord, List<Edge> edgesToTest)
        {
            return edgesToTest.Where(x => ((x.LeftSite != null && x.LeftSite.Coord.Equals(coord)) || (x.RightSite != null && x.RightSite.Coord.Equals(coord)))).ToList();
        }

        public static List<LineSegment> GetVisibleLineSegments(List<Edge> edges)
        {
            return edges.Where(x => x.Visible).Select(x => new LineSegment(x.ClippedVertices[LR.LEFT], x.ClippedVertices[LR.RIGHT])).ToList();
        }
    }
}
