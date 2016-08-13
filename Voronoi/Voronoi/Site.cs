using System;
using System.Collections.Generic;
using System.Linq;
using coneil.Math.Voronoi.Geometry;

namespace coneil.Math.Voronoi
{
    public class Site
    {
        private static List<Site> s_pool = new List<Site>();

        private static int Sort(Site s1, Site s2)
        {
            int returnValue = 0;

            // TODO
            // int returnValue = Voronoi.compareByYThenX(s1, s2);

            if(returnValue == -1)
            {
                if(s1._siteIndex > s2._siteIndex)
                {
                    int temp = s1._siteIndex;
                    s1._siteIndex = s2._siteIndex;
                    s2._siteIndex = temp;
                }
            }
            else if(returnValue == 1)
            {
                if(s2._siteIndex > s1._siteIndex)
                {
                    int temp = s1._siteIndex;
                    s1._siteIndex = s2._siteIndex;
                    s2._siteIndex = temp;
                }
            }

            return returnValue;
        }

        private const double EPSILON = 0.005;

        internal Point Coord { get; private set; }
        internal uint Color { get; private set; }
        internal double Weight { get; private set; }
        internal Edge[] Edges { get { return _edges.ToArray() } }

        private int _siteIndex;
        private List<Edge> _edges;
        private List<LR> _edgeOrientations;
        private List<Point> _region;

        private bool CloseEnough(Point p0, Point p1)
        {
            return Point.Distance(p0, p1) < EPSILON;
        }

        internal void AddEdge(Edge e)
        {
            _edges.Add(e);
        }

        internal void Move(Point to)
        {
            Clear();
            Coord = to;
        }

        internal Edge GetNearestEdge()
        {
            _edges.OrderBy(x => x.SitesDistance());
            return _edges[0];
        }

        internal List<Site> GetNeighborSites()
        {
            if(_edges == null || _edges.Count == 0)
                return new List<Site>();

            if(_edgeOrientations == null)
                ReorderEdges();

            List<Site> neighbors = new List<Site>();
            foreach(var edge in _edges)
                neighbors.Add(NeighborSiteFor(edge));

            return neighbors;
        }

        internal Site NeighborSiteFor(Edge edge)
        {
            if(this == edge.LeftSite)
                return edge.RightSite;

            if(this == edge.RightSite)
                return edge.LeftSite;

            return null;
        }

        internal void ReorderEdges()
        {
            // TODO
        }

        internal List<Point> Region(Rectangle clippingBounds)
        {
            if(_edges == null || _edges.Count == 0)
                return new List<Point>();

            if(_edgeOrientations == null)
            {
                ReorderEdges();
                _region = ClipToBounds(clippingBounds);
                if(new Polygon(_region).Winding == Winding.CLOCKWISE)
                    _region.Reverse();
            }

            return _region;
        }

        List<Point> ClipToBounds(Rectangle bounds)
        {
            var points = new List<Point>();

            Edge e = _edges.FirstOrDefault(x => x.Visible);
            if(e == null)
                return points;

            int index = _edges.IndexOf(e);
            LR orientation = _edgeOrientations[index];
            LR opposite = orientation == LR.RIGHT ? LR.LEFT : LR.RIGHT;
            points.Add(e.ClippedVertices[orientation]);
            points.Add(e.ClippedVertices[opposite]);

            for(int j = index + 1; j < _edges.Count; j++)
            {
                e = _edges[j];
                if(!e.Visible) continue;

                Connect(points, j, bounds);
            }

            Connect(points, index, bounds, true);

            return points;
        }

        void Connect(List<Point> points, int index, Rectangle bounds, bool closingUp = false)
        {
            Point rightPoint = points[points.Count - 1];
            Edge newEdge = _edges[index];
            LR newOrientation = _edgeOrientations[index];

            Point newPoint = newEdge.ClippedVertices[newOrientation];
            if(!CloseEnough(rightPoint, newPoint))
            {
                // Since the points don't overlap, it's assumed they were clipped at the bounds
                // Check if they're on the same border.
                if(!rightPoint.Equals(newPoint))
                {
                    // Different borders. Add interim corners.
                    int rightCheck = BoundsCheck.Check(rightPoint, bounds);
                    int newCheck = BoundsCheck.Check(newPoint, bounds);
                    double px, py;
                    if((rightCheck & BoundsCheck.RIGHT) > 0)
                    {
                        px = bounds.Right;
                        if((newCheck & BoundsCheck.BOTTOM) > 0)
                        {
                            py = bounds.Bottom;
                            points.Add(new Point(px, py));
                        }
                        else if((newCheck & BoundsCheck.TOP) > 0)
                        {
                            py = bounds.Top;
                            points.Add(new Point(px, py));
                        }
                        else if((newCheck & BoundsCheck.LEFT) > 0)
                        {
                            if(rightPoint.Y - bounds.Y + newPoint.Y - bounds.Y < bounds.Height)
                            {
                                py = bounds.Top;
                            }
                            else
                            {
                                py = bounds.Bottom;
                            }
                            points.Add(new Point(px, py));
                            points.Add(new Point(bounds.Left, py));
                        }
                    }
                    else if((rightCheck & BoundsCheck.LEFT) > 0)
                    {
                        px = bounds.Left;
                        if((newCheck & BoundsCheck.BOTTOM) > 0)
                        {
                            py = bounds.Bottom;
                            points.Add(new Point(px, py));
                        }
                        else if((newCheck & BoundsCheck.TOP) > 0)
                        {
                            py = bounds.Top;
                            points.Add(new Point(px, py));
                        }
                        else if((newCheck & BoundsCheck.RIGHT) > 0)
                        {
                            if(rightPoint.Y - bounds.Y + newPoint.Y - bounds.Y < bounds.Height)
                            {
                                py = bounds.Top;
                            }
                            else
                            {
                                py = bounds.Bottom;
                            }
                            points.Add(new Point(px, py));
                            points.Add(new Point(bounds.Right, py));
                        }
                    }
                    else if((rightCheck & BoundsCheck.TOP) > 0)
                    {
                        py = bounds.Top;
                        if((newCheck & BoundsCheck.RIGHT) > 0)
                        {
                            px = bounds.Right;
                            points.Add(new Point(px, py));
                        }
                        else if((newCheck & BoundsCheck.LEFT) > 0)
                        {
                            px = bounds.Left;
                            points.Add(new Point(px, py));
                        }
                        else if((newCheck & BoundsCheck.BOTTOM) > 0)
                        {
                            if(rightPoint.X - bounds.X + newPoint.X - bounds.X < bounds.Width)
                            {
                                px = bounds.Left;
                            }
                            else
                            {
                                px = bounds.Right;
                            }
                            points.Add(new Point(px, py));
                            points.Add(new Point(px, bounds.Bottom));
                        }
                    }
                    else if((rightCheck & BoundsCheck.BOTTOM) > 0)
                    {
                        py = bounds.Bottom;
                        if((newCheck & BoundsCheck.RIGHT) > 0)
                        {
                            px = bounds.Right;
                            points.Add(new Point(px, py));
                        }
                        else if((newCheck & BoundsCheck.LEFT) > 0)
                        {
                            px = bounds.Left;
                            points.Add(new Point(px, py));
                        }
                        else if((newCheck & BoundsCheck.TOP) > 0)
                        {
                            if(rightPoint.X - bounds.X + newPoint.X - bounds.X < bounds.Width)
                            {
                                px = bounds.Left;
                            }
                            else
                            {
                                px = bounds.Right;
                            }
                            points.Add(new Point(px, py));
                            points.Add(new Point(px, bounds.Top));
                        }
                    }
                }
                if(closingUp)
                {
                    return;
                }
            }
            LR other = newOrientation == LR.RIGHT ? LR.LEFT : LR.RIGHT;
            Point newRightPoint = newEdge.ClippedVertices[other];
            if(!CloseEnough(points[0], newRightPoint))
            {
                points.Add(newRightPoint);
            }
        }

        #region CONSTRUCT/DESTRUCT
        public static Site Create(Point p, int index, double weight, uint color)
        {
            Site s;
            if(s_pool.Count > 0)
            {
                s = s_pool[s_pool.Count - 1];
                s_pool.RemoveAt(s_pool.Count - 1);
                s.Init(p, index, weight, color);
            }
            else
            {
                s = new Site(p, index, weight, color);
            }

            return s;
        }

        private Site(Point p, int index, double weight, uint color)
        {
            Init(p, index, weight, color);
        }

        public void Init(Point p, int index, double weight, uint color)
        {
            Coord = p;
            _siteIndex = index;
            Weight = weight;
            Color = color;
            _edges = new List<Edge>();
        }

        public void Dispose()
        {
            Coord.Reset();
            Clear();
            s_pool.Add(this);
        }

        void Clear()
        {
            if(_edges != null)
                _edges.Clear();

            if(_edgeOrientations != null)
                _edgeOrientations.Clear();

            if(_region != null)
                _region.Clear();
        }
        #endregion
    }

    sealed static class BoundsCheck
    {
        public static const int TOP = 1;
        public static const int BOTTOM = 2;
        public static const int LEFT = 4;
        public static const int RIGHT = 8;

        public static int Check(Point point, Rectangle bounds)
        {
            int value = 0;
            if(point.X == bounds.Left)
            {
                value |= LEFT;
            }
            if(point.X == bounds.Right)
            {
                value |= RIGHT;
            }
            if(point.Y == bounds.Top)
            {
                value |= TOP;
            }
            if(point.Y == bounds.Bottom)
            {
                value |= BOTTOM;
            }
            return value;
        }
    }
}
